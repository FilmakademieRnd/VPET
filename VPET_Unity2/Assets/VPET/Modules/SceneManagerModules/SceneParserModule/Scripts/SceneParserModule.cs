using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class SceneParserModule
    {
        private Transform scene;

        private byte[] headerByteData;
        private byte[] nodesByteData;
        private byte[] objectsByteData;
        private byte[] charactersByteData;
        private byte[] texturesByteData;
        private byte[] materialsByteData;

        private VpetHeader vpetHeader;

        private int m_lodLowLayer;
        private int m_lodHighLayer;
        private int m_lodMixedLayer;

        private int m_globalID;

        public SceneParserModule()
        {
            scene = GameObject.Find("Scene").transform;

            m_lodLowLayer = LayerMask.NameToLayer("LodLow");
            m_lodHighLayer = LayerMask.NameToLayer("LodHigh");
            m_lodMixedLayer = LayerMask.NameToLayer("LodMixed");

            m_globalID = 0;
        }

        public void ParseScene(bool getLowLayer = true, bool getHighLayer = false, bool getMixedLayer = true)
        {
            vpetHeader = new VpetHeader();
            vpetHeader.lightIntensityFactor = 1f;
            vpetHeader.textureBinaryType = 1;

            List<SceneNode> nodeList = new List<SceneNode>(); ;
            List<ObjectPackage> objectList = new List<ObjectPackage>();
            List<CharacterPackage> characterList = new List<CharacterPackage>();
            List<TexturePackage> textureList = new List<TexturePackage>();
            List<MaterialPackage> materialList = new List<MaterialPackage>();

            List<GameObject> gameObjects = new List<GameObject>();
            recursiveGameObjectIdExtract(scene.parent.GetChild(0), ref gameObjects, getLowLayer, getHighLayer, getMixedLayer);

            foreach (GameObject gameObject in gameObjects)
            {
                SceneNode node = new SceneNode();
                Transform trans = gameObject.transform;
                if (trans.GetComponent<Light>() != null)
                    node = ParseLight(trans.GetComponent<Light>());
                else if (trans.GetComponent<Camera>() != null)
                    node = ParseCamera(trans.GetComponent<Camera>());
                else if (trans.GetComponent<MeshFilter>() != null)
                    node = ParseMesh(trans);
                else if (trans.GetComponent<SkinnedMeshRenderer>() != null)
                    node = ParseSkinnedMesh(trans);

                Animator animator = trans.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.logWarnings = false;
                    processCharacter(animator);
                }

                node.position = new float[3] { trans.localPosition.x, trans.localPosition.y, trans.localPosition.z };
                node.scale = new float[3] { trans.localScale.x, trans.localScale.y, trans.localScale.z };
                node.rotation = new float[4] { trans.localRotation.x, trans.localRotation.y, trans.localRotation.z, trans.localRotation.w };
                node.name = new byte[256];
                byte[] tmpName = Encoding.ASCII.GetBytes(trans.name);
                Buffer.BlockCopy(tmpName, 0, node.name, 0, Math.Min(tmpName.Length, 256));

                node.childCount = trans.childCount;

                if (trans.name != "root")
                    nodeList.Add(node);
            }

            // create byte arrays
            headerByteData = StructureToByteArray(vpetHeader);
            getNodesByteArray();
            getObjectsByteArray();
            getCharacterByteArray();
            getTexturesByteArray();
            getMaterialsByteArray();
        }

        private SceneNode ParseLight(Light light)
        {
            SceneNodeLight nodeLight = new SceneNodeLight();

            nodeLight.intensity = light.intensity;
            nodeLight.color = new float[3] { light.color.r, light.color.g, light.color.b };
            nodeLight.lightType = light.type;
            nodeLight.angle = light.spotAngle;
            nodeLight.range = light.range;
            return nodeLight;
        }

        private SceneNode ParseCamera(Camera camera)
        {
            SceneNodeCam nodeCamera = new SceneNodeCam();

            nodeCamera.fov = camera.fieldOfView;
            nodeCamera.near = camera.nearClipPlane;
            nodeCamera.far = camera.farClipPlane;
            return nodeCamera;
        }

        private SceneNode ParseMesh(Transform location)
        {
            SceneNodeGeo nodeGeo = new SceneNodeGeo();
            nodeGeo.color = new float[4] { 0, 0, 0, 1 };
            nodeGeo.geoId = processGeometry(location.GetComponent<MeshFilter>().sharedMesh);

            if (location.GetComponent<Renderer>() != null)
            {
                addMaterial(nodeGeo, location.GetComponent<Renderer>().material);
            }

            return nodeGeo;
        }

        private SceneNode ParseSkinnedMesh(Transform location)
        {
            SceneNodeSkinnedGeo nodeGeo = new SceneNodeSkinnedGeo();

            SkinnedMeshRenderer sRenderer = location.GetComponent<SkinnedMeshRenderer>();

            nodeGeo.color = new float[4] { 0, 0, 0, 1 };
            nodeGeo.geoId = processGeometry(sRenderer.sharedMesh);
            nodeGeo.rootBoneID = gameObjectList.IndexOf(sRenderer.rootBone.gameObject);
            nodeGeo.boundCenter = new float[3] { sRenderer.localBounds.center.x, sRenderer.localBounds.center.y, sRenderer.localBounds.center.z };
            nodeGeo.boundExtents = new float[3] { sRenderer.localBounds.extents.x, sRenderer.localBounds.extents.y, sRenderer.localBounds.extents.z };
            nodeGeo.bindPoseLength = sRenderer.sharedMesh.bindposes.Length;
            //Debug.Log("Bindpose Length: " + sRenderer.sharedMesh.bindposes.Length.ToString());
            nodeGeo.bindPoses = new float[nodeGeo.bindPoseLength * 16];

            for (int i = 0; i < nodeGeo.bindPoseLength; i++)
            {
                nodeGeo.bindPoses[i * 16] = sRenderer.sharedMesh.bindposes[i].m00;
                nodeGeo.bindPoses[i * 16 + 1] = sRenderer.sharedMesh.bindposes[i].m01;
                nodeGeo.bindPoses[i * 16 + 2] = sRenderer.sharedMesh.bindposes[i].m02;
                nodeGeo.bindPoses[i * 16 + 3] = sRenderer.sharedMesh.bindposes[i].m03;
                nodeGeo.bindPoses[i * 16 + 4] = sRenderer.sharedMesh.bindposes[i].m10;
                nodeGeo.bindPoses[i * 16 + 5] = sRenderer.sharedMesh.bindposes[i].m11;
                nodeGeo.bindPoses[i * 16 + 6] = sRenderer.sharedMesh.bindposes[i].m12;
                nodeGeo.bindPoses[i * 16 + 7] = sRenderer.sharedMesh.bindposes[i].m13;
                nodeGeo.bindPoses[i * 16 + 8] = sRenderer.sharedMesh.bindposes[i].m20;
                nodeGeo.bindPoses[i * 16 + 9] = sRenderer.sharedMesh.bindposes[i].m21;
                nodeGeo.bindPoses[i * 16 + 10] = sRenderer.sharedMesh.bindposes[i].m22;
                nodeGeo.bindPoses[i * 16 + 11] = sRenderer.sharedMesh.bindposes[i].m23;
                nodeGeo.bindPoses[i * 16 + 12] = sRenderer.sharedMesh.bindposes[i].m30;
                nodeGeo.bindPoses[i * 16 + 13] = sRenderer.sharedMesh.bindposes[i].m31;
                nodeGeo.bindPoses[i * 16 + 14] = sRenderer.sharedMesh.bindposes[i].m32;
                nodeGeo.bindPoses[i * 16 + 15] = sRenderer.sharedMesh.bindposes[i].m33;
            }

            nodeGeo.skinnedMeshBoneIDs = Enumerable.Repeat(-1, 99).ToArray<int>();

            for (int i = 0; i < sRenderer.bones.Length; i++)
            {
                nodeGeo.skinnedMeshBoneIDs[i] = gameObjectList.IndexOf(sRenderer.bones[i].gameObject);
            }

            addMaterial(nodeGeo, sRenderer.material);

            return nodeGeo;
        }

        private void addMaterial(SceneNodeGeo node, Material material)
        {
            if (material = null)
                node.textureId = -1;
            else
            {
                // if materials's shader is not standard, add this material to material package. 
                // Currently this will only get the material name and try to load it on client side. If this fails, it will fallback to Standard.
                node.materialId = processMaterial(material);

                if (material.HasProperty("_Color"))
                {
                    node.color = new float[4] { material.color.r, material.color.g, material.color.b, material.color.a };
                }

                if (material.HasProperty("_Glossiness"))
                    node.roughness = material.GetFloat("_Glossiness");

                if (material.mainTexture != null)
                {
                    Texture2D mainTex = (Texture2D)material.mainTexture;
                    node.textureId = processTexture(mainTex);
                }
                else
                {
                    node.textureId = -1;
                }
            }
        }

        //!
        //! Recursively iterates over all scene elements
        //!
        private void recursiveGameObjectIdExtract(Transform location, ref List<GameObject> gameObjects, bool getLowLayer, bool getHighLayer, bool getMixedLayer)
        {
            // fill game object list
            gameObjects.Add(location.gameObject);

            foreach (Transform child in location)
                if (child.gameObject.activeSelf &&
                    ((location.gameObject.layer == m_lodLowLayer && getLowLayer) ||
                    (location.gameObject.layer == m_lodHighLayer && getHighLayer) ||
                    (location.gameObject.layer == m_lodMixedLayer && getMixedLayer)))
                    recursiveGameObjectIdExtract(child, ref gameObjects, getLowLayer, getHighLayer, getMixedLayer);
        }

        /*
         * [REVIEW]
        MOVE THIS TO FUTURE SERVERADAPTER

        //!
        //! Recursively iterates over all scene elements & adds their ID to sceneObjectRefList
        //!
        private void recursiveIdExtract(Transform location)
        {
            if (location.GetComponent<SceneObject>())
            {
                serverAdapter.sceneObjectRefList[location.GetComponent<SceneObject>().id] = location.GetComponent<SceneObject>();
            }

            foreach (Transform child in location)
                if (child.gameObject.activeSelf)
                    recursiveIdExtract(child);
        }
         
         */


        //! 
        //! Template function for serialising arbitrary structures in to byte streams.
        //! 
        //! @param obj The object to be serialised.
        //!
        private static byte[] StructureToByteArray(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }
    }
}

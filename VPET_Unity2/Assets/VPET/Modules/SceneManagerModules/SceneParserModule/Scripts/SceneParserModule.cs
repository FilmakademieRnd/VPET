using System;
using System.Runtime.InteropServices;
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

        public bool ParseScene(bool getLowLayer = true, bool getHighLayer = false, bool getMixedLayer = true)
        {
            vpetHeader = new VpetHeader();
            vpetHeader.lightIntensityFactor = 1f;
            vpetHeader.textureBinaryType = 1;

            List<SceneNode> nodeList = new List<SceneNode>(); ;
            List<ObjectPackage> objectList = new List<ObjectPackage>();
            List<CharacterPackage> characterList = new List<CharacterPackage>();
            List<TexturePackage> textureList = new List<TexturePackage>();
            List<MaterialPackage> materialList = new List<MaterialPackage>();

            iterLocation(scene.parent, getLowLayer, getHighLayer, getMixedLayer);

            // create byte arrays
            headerByteData = StructureToByteArray(vpetHeader);
            getNodesByteArray();
            getObjectsByteArray();
            getCharacterByteArray();
            getTexturesByteArray();
            getMaterialsByteArray();

            return false;
        }

        //!
        //! Recursively iterate over scene and prepare data to be send to clients
        //!
        private bool iterLocation(Transform location, bool getLowLayer, bool getHighLayer, bool getMixedLayer)
        {
            if (!((location.gameObject.layer == m_lodLowLayer && getLowLayer) ||
                  (location.gameObject.layer == m_lodHighLayer && getHighLayer) ||
                  (location.gameObject.layer == m_lodMixedLayer && getMixedLayer)))
                return false;

            SceneNode node = new SceneNode();

            if (location.GetComponent<Light>() != null)
                node = ParseLight(location.GetComponent<Light>());
            else if (location.GetComponent<Camera>() != null)
                node = ParseCamera(location.GetComponent<Camera>());
            else if (location.GetComponent<MeshFilter>() != null)
                node = ParseMesh(location);
            else if (location.GetComponent<SkinnedMeshRenderer>() != null)
                node = ParseSkinnedMesh(location);


            return true;
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
            Material mat = location.GetComponent<Renderer>().sharedMaterial;

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

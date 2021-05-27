/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneDataHandler.cs"
//! @brief implementation scene data deserialisation
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 19.05.2021

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Class handling Unity scene parsing and serialisation.
    //!
    public class SceneParserModule : SceneManagerModule
    {
        //!
        //! The scenes root transform.
        //!
        private Transform scene;
        //!
        //! The layer ID tacked as LOD low.
        //!
        private int m_lodLowLayer;
        //!
        //! The layer ID tacked as LOD high.
        //!
        private int m_lodHighLayer;
        //!
        //! The layer ID tacked as LOD mixed.
        //!
        private int m_lodMixedLayer;

        //!
        //! Constructor
        //!
        public SceneParserModule(string name) : base(name)
        {
            name = base.name;
            scene = GameObject.Find("Scene").transform;

            m_lodLowLayer = LayerMask.NameToLayer("LodLow");
            m_lodHighLayer = LayerMask.NameToLayer("LodHigh");
            m_lodMixedLayer = LayerMask.NameToLayer("LodMixed");
        }

        //!
        //! Parses the Unity scene filtered by LOD layers and creates binary streams from it.
        //!
        //! @param getLowLayer Gather only scene elements from LOD low layer.
        //! @param getHighLayer Gather only scene elements from LOD high layer.
        //! @param getMixedLayer Gather only scene elements from LOD mixed layer.
        //!
        public void ParseScene(bool getLowLayer = true, bool getHighLayer = false, bool getMixedLayer = true)
        {
            SceneManager.SceneDataHandler.SceneData sceneData = new SceneManager.SceneDataHandler.SceneData();

            sceneData.header = new SceneManager.VpetHeader();
            sceneData.header.lightIntensityFactor = 1f;
            sceneData.header.textureBinaryType = 0;

            List<GameObject> gameObjects = new List<GameObject>();
            recursiveGameObjectIdExtract(scene.parent.GetChild(0), ref gameObjects, getLowLayer, getHighLayer, getMixedLayer);

            foreach (GameObject gameObject in gameObjects)
            {
                SceneManager.SceneNode node = new SceneManager.SceneNode();
                Transform trans = gameObject.transform;
                if (trans.GetComponent<Light>() != null)
                    node = ParseLight(trans.GetComponent<Light>());
                else if (trans.GetComponent<Camera>() != null)
                    node = ParseCamera(trans.GetComponent<Camera>());
                else if (trans.GetComponent<MeshFilter>() != null)
                    node = ParseMesh(trans, ref sceneData);
                else if (trans.GetComponent<SkinnedMeshRenderer>() != null)
                    node = ParseSkinnedMesh(trans, ref gameObjects , ref sceneData);

                Animator animator = trans.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.logWarnings = false;
                    processCharacter(animator, ref gameObjects, ref sceneData.characterList);
                }

                node.position = new float[3] { trans.localPosition.x, trans.localPosition.y, trans.localPosition.z };
                node.scale = new float[3] { trans.localScale.x, trans.localScale.y, trans.localScale.z };
                node.rotation = new float[4] { trans.localRotation.x, trans.localRotation.y, trans.localRotation.z, trans.localRotation.w };
                node.name = new byte[256];
                byte[] tmpName = Encoding.ASCII.GetBytes(trans.name);
                Buffer.BlockCopy(tmpName, 0, node.name, 0, Math.Min(tmpName.Length, 256));

                node.childCount = trans.childCount;

                if (trans.name != "root")
                    sceneData.nodeList.Add(node);
            }

            
        }

        //!
        //! Function for creating a VPET light node out of an Unity light object.
        //!
        //! @param light The Unity light for which a VPET node will be created.
        //! @return The created light node.
        //!
        private SceneManager.SceneNode ParseLight(Light light)
        {
            SceneManager.SceneNodeLight nodeLight = new SceneManager.SceneNodeLight();

            nodeLight.intensity = light.intensity;
            nodeLight.color = new float[3] { light.color.r, light.color.g, light.color.b };
            nodeLight.lightType = light.type;
            nodeLight.angle = light.spotAngle;
            nodeLight.range = light.range;
            return nodeLight;
        }

        //!
        //! Function for creating a VPET camera node out of an Unity camera object.
        //!
        //! @param camera The Unity light for which a VPET node will be created.
        //! @return The created camera node.
        //!
        private SceneManager.SceneNode ParseCamera(Camera camera)
        {
            SceneManager.SceneNodeCam nodeCamera = new SceneManager.SceneNodeCam();

            nodeCamera.fov = camera.fieldOfView;
            nodeCamera.near = camera.nearClipPlane;
            nodeCamera.far = camera.farClipPlane;
            return nodeCamera;
        }

        //!
        //! Function for creating a VPET geo node out of an Unity object.
        //!
        //! @param transform The transform from the Unity mesh object.
        //! @param sceneData A reference to the structure that will store the serialised scene data. 
        //! @return Created scene node.
        //!
        private SceneManager.SceneNode ParseMesh(Transform location, ref SceneManager.SceneDataHandler.SceneData sceneData)
        {
            SceneManager.SceneNodeGeo nodeGeo = new SceneManager.SceneNodeGeo();
            nodeGeo.color = new float[4] { 0, 0, 0, 1 };
            nodeGeo.geoId = processGeometry(location.GetComponent<MeshFilter>().sharedMesh, ref sceneData);

            if (location.GetComponent<Renderer>() != null)
            {
                processMaterial(nodeGeo, location.GetComponent<Renderer>().material, ref sceneData);
            }

            return nodeGeo;
        }

        //!
        //! Function for creating a VPET skinned geo node out of an skinned Unity object.
        //!
        //! @param transform The transform from the Unity mesh object.
        //! @param gameObjectList A reference to the list of the traversed Unity Game Object tree.       
        //! @param sceneData A reference to the structure that will store the serialised scene data. 
        //! @return Created scene node.
        //!
        private SceneManager.SceneNode ParseSkinnedMesh(Transform location, ref List<GameObject> gameObjectList, ref SceneManager.SceneDataHandler.SceneData sceneData)
        {
            SceneManager.SceneNodeSkinnedGeo nodeGeo = new SceneManager.SceneNodeSkinnedGeo();

            SkinnedMeshRenderer sRenderer = location.GetComponent<SkinnedMeshRenderer>();

            nodeGeo.color = new float[4] { 0, 0, 0, 1 };
            nodeGeo.geoId = processGeometry(sRenderer.sharedMesh, ref sceneData);
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

            processMaterial(nodeGeo, sRenderer.material, ref sceneData);

            return nodeGeo;
        }

        //!
        //! Fills a given list with Unity Game Objects by traversing the scene object tree.
        //!
        //! @location The transform of the Unity Game Object to start at.
        //! @gameObjects The list to be filld with the Game Objects.
        //! @param getLowLayer Gather only scene elements from LOD low layer.
        //! @param getHighLayer Gather only scene elements from LOD high layer.
        //! @param getMixedLayer Gather only scene elements from LOD mixed layer.
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

        //!
        //! Function that adds a VPET nods material properties.
        //!
        //! @param node The node to which the properties will be added.
        //! @material The Unity material containing the properties to be added.
        //! @param sceneData A reference to the structure that will store the serialised scene data. 
        //!
        private void processMaterial(SceneManager.SceneNodeGeo node, Material material, ref SceneManager.SceneDataHandler.SceneData sceneData)
        {
            if (material = null)
                node.textureId = -1;
            else
            {
                // if materials's shader is not standard, add this material to material package. 
                // Currently this will only get the material name and try to load it on client side. If this fails, it will fallback to Standard.
                node.materialId = processMaterial(material, ref sceneData.materialList);

                if (material.HasProperty("_Color"))
                {
                    node.color = new float[4] { material.color.r, material.color.g, material.color.b, material.color.a };
                }

                if (material.HasProperty("_Glossiness"))
                    node.roughness = material.GetFloat("_Glossiness");

                if (material.mainTexture != null)
                {
                    Texture2D mainTex = (Texture2D)material.mainTexture;
                    node.textureId = processTexture(mainTex, ref sceneData.textureList);
                }
                else
                {
                    node.textureId = -1;
                }
            }
        }

        //!
        //! Serialises a Unity mesh into a VPET Object Package and adds it to the given 
        //! list, if it does not already contain the mesh. Otherweise returns the reference ID of the
        //! already existing mesh. 
        //!
        //! @mesh The mesh to be serialized
        //! @param sceneData A reference to the structure that will store the serialised scene data. 
        //! @return The reference ID for mesh. 
        //!
        private int processGeometry(Mesh mesh, ref SceneManager.SceneDataHandler.SceneData sceneData)
        {
            for (int i = 0; i < sceneData.objectList.Count; i++)
            {
                if (sceneData.objectList[i].mesh == mesh)
                {
                    return i;
                }
            }

            SceneManager.ObjectPackage objPack = new SceneManager.ObjectPackage();
            // vertices, normals, uvs, weights
            objPack.vSize = mesh.vertexCount;
            objPack.nSize = mesh.normals.Length;
            objPack.uvSize = mesh.uv.Length;
            objPack.bWSize = mesh.boneWeights.Length;
            objPack.vertices = new float[objPack.vSize * 3];
            objPack.normals = new float[objPack.nSize * 3];
            objPack.uvs = new float[objPack.uvSize * 2];
            objPack.boneWeights = new float[objPack.bWSize * 4];
            objPack.boneIndices = new int[objPack.bWSize * 4];

            Vector3[] mVertices = mesh.vertices;
            Vector3[] mNormals = mesh.normals;
            Vector2[] mUV = mesh.uv;
            BoneWeight[] mWeights = mesh.boneWeights;

            // v
            for (int i = 0; i < objPack.vSize; i++)
            {
                objPack.vertices[i * 3 + 0] = mVertices[i].x;
                objPack.vertices[i * 3 + 1] = mVertices[i].y;
                objPack.vertices[i * 3 + 2] = mVertices[i].z;
            }

            // n
            for (int i = 0; i < objPack.nSize; i++)
            {
                objPack.normals[i * 3 + 0] = mNormals[i].x;
                objPack.normals[i * 3 + 1] = mNormals[i].y;
                objPack.normals[i * 3 + 2] = mNormals[i].z;
            }

            // uv
            for (int i = 0; i < objPack.uvSize; i++)
            {
                objPack.uvs[i * 2 + 0] = mUV[i].x;
                objPack.uvs[i * 2 + 1] = mUV[i].y;
            }

            // vertex indices
            objPack.iSize = mesh.triangles.Length;
            objPack.indices = mesh.triangles;

            // bone weights
            for (int i = 0; i < objPack.bWSize; i++)
            {
                objPack.boneWeights[i * 4 + 0] = mWeights[i].weight0;
                objPack.boneWeights[i * 4 + 1] = mWeights[i].weight1;
                objPack.boneWeights[i * 4 + 2] = mWeights[i].weight2;
                objPack.boneWeights[i * 4 + 3] = mWeights[i].weight3;
            }

            // bone indices
            for (int i = 0; i < objPack.bWSize; i++)
            {
                objPack.boneIndices[i * 4 + 0] = mWeights[i].boneIndex0;
                objPack.boneIndices[i * 4 + 1] = mWeights[i].boneIndex1;
                objPack.boneIndices[i * 4 + 2] = mWeights[i].boneIndex2;
                objPack.boneIndices[i * 4 + 3] = mWeights[i].boneIndex3;
            }

            objPack.mesh = mesh;
            sceneData.objectList.Add(objPack);

            return sceneData.objectList.Count - 1;
        }

        //!
        //! Serialises a Unity material into a VPET Material Package and adds it to the given 
        //! list, if it does not already contain the material. Otherwise returns the reference ID of the
        //! already existing material. 
        //!
        //! @mat The material to be serialized
        //! @materialList The list to add the serialised material to.
        //! @return The reference ID for material. 
        //!
        private int processMaterial(Material mat, ref List<SceneManager.MaterialPackage> materialList)
        {
            if (mat == null || mat.shader == null)
                return -1;

            // already stored ?
            for (int i = 0; i < materialList.Count; i++)
            {
                if (materialList[i].mat.GetInstanceID() == mat.GetInstanceID())
                {
                    return i;
                }
            }

            // create
            SceneManager.MaterialPackage matPack = new SceneManager.MaterialPackage();
            matPack.mat = mat;
            matPack.name = mat.name;

            // if material within Resources/VPET than use load material
            string matName = mat.name.Replace("(Instance)", "").Trim();
            Material _mat = Resources.Load(string.Format("VPET/Materials/{0}", matName), typeof(Material)) as Material;
            if (_mat)
            {
                Helpers.Log("mat tyoe" + 1 + " material " + mat.name);
                matPack.type = 1;
                matPack.src = matName;
            }
            else
            {
                Helpers.Log("mat tyoe" + 2 + " shader " + mat.shader.name);
                matPack.type = 2;
                matPack.src = mat.shader.name;
            }

            materialList.Add(matPack);
            return materialList.Count - 1;
        }

        //!
        //! Serialises a Unity texture into a VPET Texture Package and adds it to the given 
        //! list, if it does not already contain the texture. Otherwise returns the reference ID of the
        //! already existing texture. 
        //!
        //! @texture The texture to be serialized
        //! @textureList The list to add the serialised texture to.
        //! @return The reference ID for texture. 
        //!
        private int processTexture(Texture2D texture, ref List<SceneManager.TexturePackage> textureList)
        {
            for (int i = 0; i < textureList.Count; i++)
            {
                if (textureList[i].texture == texture)
                {
                    return i;
                }
            }

            SceneManager.TexturePackage texPack = new SceneManager.TexturePackage();

            texPack.width = texture.width;
            texPack.height = texture.height;
            texPack.format = texture.format;

            texPack.colorMapData = texture.GetRawTextureData();
            texPack.colorMapDataSize = texPack.colorMapData.Length;

            texPack.texture = texture;

            textureList.Add(texPack);

            return textureList.Count - 1;
        }

        //!
        //! Serialises a Unity skinned object into a VPET Character Package and adds it to the given list.
        //!
        //! @animator The Unity animator containing the skinned object.
        //! @characterList The list to add the serialised skinned object to.
        //! @param gameObjectList A reference to the list of the traversed Unity Game Object tree. 
        //!
        private void processCharacter(Animator animator, ref List<GameObject> gameObjectList, ref List<SceneManager.CharacterPackage> characterList)
        {
            SceneManager.CharacterPackage chrPack = new SceneManager.CharacterPackage();
            chrPack.rootId = gameObjectList.IndexOf(animator.transform.gameObject);

            HumanBone[] boneArray = animator.avatar.humanDescription.human;
            chrPack.bMSize = Enum.GetNames(typeof(HumanBodyBones)).Length;
            chrPack.boneMapping = Enumerable.Repeat(-1, chrPack.bMSize).ToArray<int>();

            for (int i = 0; i < boneArray.Length; i++)
            {
                if (boneArray[i].boneName != null)
                {
                    string enumName = boneArray[i].humanName.Replace(" ", "");
                    HumanBodyBones enumNum;
                    Enum.TryParse<HumanBodyBones>(enumName, true, out enumNum);
                    Transform boneTransform = Helpers.FindDeepChild(animator.transform, boneArray[i].boneName);
                    chrPack.boneMapping[(int)enumNum] = gameObjectList.IndexOf(boneTransform.gameObject);
                }
            }

            SkeletonBone[] skeletonArray = animator.avatar.humanDescription.skeleton;
            chrPack.sSize = skeletonArray.Length;
            chrPack.skeletonMapping = new int[chrPack.sSize];
            chrPack.bonePosition = new float[chrPack.sSize * 3];
            chrPack.boneRotation = new float[chrPack.sSize * 4];
            chrPack.boneScale = new float[chrPack.sSize * 3];

            for (int i = 0; i < skeletonArray.Length; i++)
            {
                chrPack.skeletonMapping[i] = gameObjectList.IndexOf(GameObject.Find(skeletonArray[i].name));

                chrPack.bonePosition[i * 3] = skeletonArray[i].position.x;
                chrPack.bonePosition[i * 3 + 1] = skeletonArray[i].position.y;
                chrPack.bonePosition[i * 3 + 2] = skeletonArray[i].position.z;

                chrPack.boneRotation[i * 4] = skeletonArray[i].rotation.x;
                chrPack.boneRotation[i * 4 + 1] = skeletonArray[i].rotation.y;
                chrPack.boneRotation[i * 4 + 2] = skeletonArray[i].rotation.z;
                chrPack.boneRotation[i * 4 + 3] = skeletonArray[i].rotation.w;

                chrPack.boneScale[i * 3] = skeletonArray[i].scale.x;
                chrPack.boneScale[i * 3 + 1] = skeletonArray[i].scale.y;
                chrPack.boneScale[i * 3 + 2] = skeletonArray[i].scale.z;
            }

            characterList.Add(chrPack);
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
    }
}

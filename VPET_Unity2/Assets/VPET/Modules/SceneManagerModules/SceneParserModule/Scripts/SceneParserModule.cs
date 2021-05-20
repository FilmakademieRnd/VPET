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
under grant agreement no 780470, 2018-2020
 
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
    public class SceneParserModule
    {
        //!
        //! Data type for storing scene object information.
        //!
        private struct SceneData
        {
            public List<SceneNode> nodeList;
            public List<ObjectPackage> objectList;
            public List<CharacterPackage> characterList;
            public List<TexturePackage> textureList;
            public List<MaterialPackage> materialList;
        }

        //!
        //! The scenes root transform.
        //!
        private Transform scene;

        //!
        //! The list containing the serialised header.
        //!
        private byte[] m_headerByteData;
        //!
        //! Getter function returning a reference to the byte array  
        //! containing the serialised header data.
        //!
        //! @return A reference to the serialised header data.
        //!
        public ref byte[] headerByteData
        {
            get { return ref m_headerByteData; }
        }
        //!
        //! The list containing the serialised nodes.
        //!
        private byte[] m_nodesByteData;
        //!
        //! Getter function returning a reference to the byte array  
        //! containing the serialised nodes data.
        //!
        //! @return A reference to the serialised nodes data.
        //!
        public ref byte[] nodesByteData
        {
            get { return ref m_nodesByteData; }
        }
        //!
        //! The list containing the serialised meshes.
        //!
        private byte[] m_objectsByteData;
        //!
        //! Getter function returning a reference to the byte array  
        //! containing the serialised objects data.
        //!
        //! @return A reference to the serialised objects data.
        //!
        public ref byte[] objectsByteData
        {
            get { return ref m_objectsByteData; }
        }
        //!
        //! The list containing the serialised skinned meshes.
        //!
        private byte[] m_charactersByteData;
        //!
        //! Getter function returning a reference to the byte array  
        //! containing the serialised characters data.
        //!
        //! @return A reference to the serialised characters data.
        //!
        public ref byte[] charactersByteData
        {
            get { return ref m_charactersByteData; }
        }
        //!
        //! The list containing the serialised textures.
        //!
        private byte[] m_texturesByteData;
        //!
        //! Getter function returning a reference to the byte array  
        //! containing the serialised textures data.
        //!
        //! @return A reference to the serialised textures data.
        //!
        public ref byte[] texturesByteData
        {
            get { return ref m_texturesByteData; }
        }
        //!
        //! The list containing the serialised materials.
        //!
        private byte[] m_materialsByteData;
        //!
        //! Getter function returning a reference to the byte array  
        //! containing the serialised materials data.
        //!
        //! @return A reference to the serialised materials data.
        //!
        public ref byte[] materialsByteData
        {
            get { return ref m_materialsByteData; }
        }
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
        public SceneParserModule()
        {
            scene = GameObject.Find("Scene").transform;

            m_lodLowLayer = LayerMask.NameToLayer("LodLow");
            m_lodHighLayer = LayerMask.NameToLayer("LodHigh");
            m_lodMixedLayer = LayerMask.NameToLayer("LodMixed");
        }

        //!
        //! Parses the Unity scene filtered by LOD layers and creates binary streams from it.
        //!
        //! @param getLowLayer Gether only scene elements from LOD low layer.
        //! @param getLowLayer Gether only scene elements from LOD low layer.
        //! @param getLowLayer Gether only scene elements from LOD low layer.
        //!
        public void ParseScene(bool getLowLayer = true, bool getHighLayer = false, bool getMixedLayer = true)
        {
            VpetHeader vpetHeader = new VpetHeader();
            vpetHeader.lightIntensityFactor = 1f;

            SceneData sceneData = new SceneData();

            sceneData.nodeList = new List<SceneNode>();
            sceneData.objectList = new List<ObjectPackage>();
            sceneData.characterList = new List<CharacterPackage>();
            sceneData.textureList = new List<TexturePackage>();
            sceneData.materialList = new List<MaterialPackage>();

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

            // create byte arrays and clear buffers
            m_headerByteData = StructureToByteArray(vpetHeader);
            getNodesByteArray(ref sceneData.nodeList);
            sceneData.nodeList.Clear();

            getObjectsByteArray(ref sceneData.objectList);
            sceneData.objectList.Clear();

            getCharacterByteArray(ref sceneData.characterList);
            sceneData.characterList.Clear();

            getTexturesByteArray(ref sceneData.textureList);
            sceneData.textureList.Clear();

            getMaterialsByteArray(ref sceneData.materialList);
            sceneData.materialList.Clear();
        }

        //!
        //! Function for creating a VPET light node out of an Unity light object.
        //!
        //! @param light The Unity light for which a VPET node will be created.
        //! @return The created light node.
        //!
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

        //!
        //! Function for creating a VPET camera node out of an Unity camera object.
        //!
        //! @param camera The Unity light for which a VPET node will be created.
        //! @return The created camera node.
        //!
        private SceneNode ParseCamera(Camera camera)
        {
            SceneNodeCam nodeCamera = new SceneNodeCam();

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
        private SceneNode ParseMesh(Transform location, ref SceneData sceneData)
        {
            SceneNodeGeo nodeGeo = new SceneNodeGeo();
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
        private SceneNode ParseSkinnedMesh(Transform location, ref List<GameObject> gameObjectList, ref SceneData sceneData)
        {
            SceneNodeSkinnedGeo nodeGeo = new SceneNodeSkinnedGeo();

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
        //! @param getLowLayer Gether only scene elements from LOD low layer.
        //! @param getHighLayer Gether only scene elements from LOD high layer.
        //! @param getMixedLayer Gether only scene elements from LOD mixed layer.
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
        private void processMaterial(SceneNodeGeo node, Material material, ref SceneData sceneData)
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
        private int processGeometry(Mesh mesh, ref SceneData sceneData)
        {
            for (int i = 0; i < sceneData.objectList.Count; i++)
            {
                if (sceneData.objectList[i].mesh == mesh)
                {
                    return i;
                }
            }

            ObjectPackage objPack = new ObjectPackage();
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
        private int processMaterial(Material mat, ref List<MaterialPackage> materialList)
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
            MaterialPackage matPack = new MaterialPackage();
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
        private int processTexture(Texture2D texture, ref List<TexturePackage> textureList)
        {
            for (int i = 0; i < textureList.Count; i++)
            {
                if (textureList[i].texture == texture)
                {
                    return i;
                }
            }

            TexturePackage texPack = new TexturePackage();

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
        private void processCharacter(Animator animator, ref List<GameObject> gameObjectList, ref List<CharacterPackage> characterList)
        {
            CharacterPackage chrPack = new CharacterPackage();
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

        //!
        //! Function that concatinates all serialised VPET nodes to a byte array.
        //!
        //! @nodeList The list that contains the serialised nodes to be concatinated.
        //!
        private void getNodesByteArray(ref List<SceneNode> nodeList)
        {
            m_nodesByteData = new byte[0];
            foreach (SceneNode node in nodeList)
            {
                byte[] nodeBinary;
                byte[] nodeTypeBinary;
                if (node.GetType() == typeof(SceneNodeGeo))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GEO);
                    SceneNodeGeo nodeGeo = (SceneNodeGeo)Convert.ChangeType(node, typeof(SceneNodeGeo));
                    nodeBinary = StructureToByteArray(nodeGeo);
                }
                else if (node.GetType() == typeof(SceneNodeSkinnedGeo))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.SKINNEDMESH);
                    SceneNodeSkinnedGeo nodeskinnedGeo = (SceneNodeSkinnedGeo)Convert.ChangeType(node, typeof(SceneNodeSkinnedGeo));
                    nodeBinary = StructureToByteArray(nodeskinnedGeo);
                }
                else if (node.GetType() == typeof(SceneNodeLight))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.LIGHT);
                    SceneNodeLight nodeLight = (SceneNodeLight)Convert.ChangeType(node, typeof(SceneNodeLight));
                    nodeBinary = StructureToByteArray(nodeLight);
                }
                else if (node.GetType() == typeof(SceneNodeCam))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.CAMERA);
                    SceneNodeCam nodeCam = (SceneNodeCam)Convert.ChangeType(node, typeof(SceneNodeCam));
                    nodeBinary = StructureToByteArray(nodeCam);
                }
                else
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GROUP);
                    nodeBinary = StructureToByteArray(node);
                }

                // concate arrays
                m_nodesByteData = Concat<byte>(m_nodesByteData, nodeTypeBinary);
                m_nodesByteData = Concat<byte>(m_nodesByteData, nodeBinary);
            }
        }

        //!
        //! Function that concatinates all serialised VPET meshes to a byte array.
        //!
        //! @objectList The list that contains the serialised meshes to be concatinated.
        //!
        private void getObjectsByteArray(ref List<ObjectPackage> objectList)
        {
            m_objectsByteData = new byte[0];

            foreach (ObjectPackage objPack in objectList)
            {
                byte[] objByteData = new byte[5 * SceneDataHandler.size_int +
                                                    objPack.vSize * 3 * SceneDataHandler.size_float +
                                                    objPack.iSize * SceneDataHandler.size_int +
                                                    objPack.nSize * 3 * SceneDataHandler.size_float +
                                                    objPack.uvSize * 2 * SceneDataHandler.size_float +
                                                    objPack.bWSize * 4 * SceneDataHandler.size_float +
                                                    objPack.bWSize * 4 * SceneDataHandler.size_int];
                int dstIdx = 0;
                // vertices
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.vSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.vertices, 0, objByteData, dstIdx, objPack.vSize * 3 * SceneDataHandler.size_float);
                dstIdx += objPack.vSize * 3 * SceneDataHandler.size_float;
                // indices
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.iSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.indices, 0, objByteData, dstIdx, objPack.iSize * SceneDataHandler.size_int);
                dstIdx += objPack.iSize * SceneDataHandler.size_int;
                // normals
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.nSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.normals, 0, objByteData, dstIdx, objPack.nSize * 3 * SceneDataHandler.size_float);
                dstIdx += objPack.nSize * 3 * SceneDataHandler.size_float;
                // uvs
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.uvSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.uvs, 0, objByteData, dstIdx, objPack.uvSize * 2 * SceneDataHandler.size_float);
                dstIdx += objPack.uvSize * 2 * SceneDataHandler.size_float;
                // bone weights
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.bWSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.boneWeights, 0, objByteData, dstIdx, objPack.bWSize * 4 * SceneDataHandler.size_float);
                dstIdx += objPack.bWSize * 4 * SceneDataHandler.size_float;
                // bone indices
                Buffer.BlockCopy(objPack.boneIndices, 0, objByteData, dstIdx, objPack.bWSize * 4 * SceneDataHandler.size_int);
                dstIdx += objPack.bWSize * 4 * SceneDataHandler.size_int;

                // concate
                m_objectsByteData = Concat<byte>(m_objectsByteData, objByteData);
            }
        }

        //!
        //! Function that concatinates all serialised VPET skinned meshes to a byte array.
        //!
        //! @characterList The list that contains the serialised skinned meshes to be concatinated.
        //!
        private void getCharacterByteArray(ref List<CharacterPackage> characterList)
        {
            m_charactersByteData = new byte[0];
            foreach (CharacterPackage chrPack in characterList)
            {
                byte[] characterByteData = new byte[SceneDataHandler.size_int * 3 +
                                                chrPack.boneMapping.Length * SceneDataHandler.size_int +
                                                chrPack.skeletonMapping.Length * SceneDataHandler.size_int +
                                                chrPack.sSize * SceneDataHandler.size_float * 10];
                int dstIdx = 0;
                // bone mapping size
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.bMSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // skeleton mapping size
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.sSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // root dag id
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.rootId), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                Buffer.BlockCopy(chrPack.boneMapping, 0, characterByteData, dstIdx, chrPack.bMSize * SceneDataHandler.size_int);
                dstIdx += chrPack.bMSize * SceneDataHandler.size_int;

                // skeleton Mapping
                Buffer.BlockCopy(chrPack.skeletonMapping, 0, characterByteData, dstIdx, chrPack.sSize * SceneDataHandler.size_int);
                dstIdx += chrPack.sSize * SceneDataHandler.size_int;

                //skelton bone positions
                Buffer.BlockCopy(chrPack.bonePosition, 0, characterByteData, dstIdx, chrPack.sSize * 3 * SceneDataHandler.size_float);
                dstIdx += chrPack.sSize * 3 * SceneDataHandler.size_float;

                //skelton bone rotations
                Buffer.BlockCopy(chrPack.boneRotation, 0, characterByteData, dstIdx, chrPack.sSize * 4 * SceneDataHandler.size_float);
                dstIdx += chrPack.sSize * 4 * SceneDataHandler.size_float;

                //skelton bone scales
                Buffer.BlockCopy(chrPack.boneScale, 0, characterByteData, dstIdx, chrPack.sSize * 3 * SceneDataHandler.size_float);
                dstIdx += chrPack.sSize * 3 * SceneDataHandler.size_float;

                // concate
                m_charactersByteData = Concat<byte>(m_charactersByteData, characterByteData);
            }
        }

        //!
        //! Function that concatinates all serialised VPET textures to a byte array.
        //!
        //! @textureList The list that contains the serialised textures to be concatinated.
        //!
        private void getTexturesByteArray(ref List<TexturePackage> textureList)
        {
            m_texturesByteData = new byte[0];

            foreach (TexturePackage texPack in textureList)
            {
                byte[] texByteData = new byte[4 * SceneDataHandler.size_int + texPack.colorMapDataSize];
                int dstIdx = 0;
                // width
                Buffer.BlockCopy(BitConverter.GetBytes(texPack.width), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                // height
                Buffer.BlockCopy(BitConverter.GetBytes(texPack.height), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                // format
                Buffer.BlockCopy(BitConverter.GetBytes((int)texPack.format), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                // pixel data
                Buffer.BlockCopy(BitConverter.GetBytes(texPack.colorMapDataSize), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(texPack.colorMapData, 0, texByteData, dstIdx, texPack.colorMapDataSize);
                dstIdx += texPack.colorMapDataSize;

                // concate
                m_texturesByteData = Concat<byte>(m_texturesByteData, texByteData);
            }
        }

        //!
        //! Function that concatinates all serialised VPET materials to a byte array.
        //!
        //! @materialList The list that contains the serialised materials to be concatinated.
        //!
        private void getMaterialsByteArray(ref List<MaterialPackage> materialList)
        {
            m_materialsByteData = new byte[0];

            foreach (MaterialPackage matPack in materialList)
            {
                byte[] matByteData = new byte[SceneDataHandler.size_int + SceneDataHandler.size_int + matPack.name.Length + SceneDataHandler.size_int + matPack.src.Length];
                int dstIdx = 0;

                // type
                Buffer.BlockCopy(BitConverter.GetBytes(matPack.type), 0, matByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // name length
                Buffer.BlockCopy(BitConverter.GetBytes(matPack.name.Length), 0, matByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // name
                byte[] nameByte = Encoding.ASCII.GetBytes(matPack.name);
                Buffer.BlockCopy(nameByte, 0, matByteData, dstIdx, matPack.name.Length);
                dstIdx += matPack.name.Length;

                // src length
                Buffer.BlockCopy(BitConverter.GetBytes(matPack.src.Length), 0, matByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // src
                nameByte = Encoding.ASCII.GetBytes(matPack.src);
                Buffer.BlockCopy(nameByte, 0, matByteData, dstIdx, matPack.src.Length);
                dstIdx += matPack.src.Length;

                // concate
                m_materialsByteData = Concat<byte>(m_materialsByteData, matByteData);
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
        //! Template function for concatination of arrays. 
        //!
        //! @param first The array field to be appended to.
        //! @param arrays The arrays to be append.
        //!
        private static T[] Concat<T>(T[] first, params T[][] arrays)
        {
            int length = first.Length;
            foreach (T[] array in arrays)
            {
                length += array.Length;
            }
            T[] result = new T[length];

            length = first.Length;
            Array.Copy(first, 0, result, 0, first.Length);
            foreach (T[] array in arrays)
            {
                Array.Copy(array, 0, result, length, array.Length);
                length += array.Length;
            }
            return result;
        }

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

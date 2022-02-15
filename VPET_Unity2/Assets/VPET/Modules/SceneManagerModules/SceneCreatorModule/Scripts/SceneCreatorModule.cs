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
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneCreatorModule.cs"
//! @brief implementation of VPET scene creator module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 03.02.2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace vpet
{
    //!
    //! implementation of VPET scene creator module
    //!
    public class SceneCreatorModule : SceneManagerModule
    {
        //! The list storing all Unity gameObjects in scene.
        private List<GameObject> gameObjectList = new List<GameObject>();

        //! The list storing Unity materials in scene.
        private List<Material> SceneMaterialList = new List<Material>();

        //! The list storing Unity textures in scene.
        private List<Texture2D> SceneTextureList = new List<Texture2D>();

        //! The list storing Unity meshes in scene.
        private List<Mesh> SceneMeshList = new List<Mesh>();


        //!
        //! Constructor
        //! Creates an reference to the network manager and connects the scene creation method to the scene received event in the network requester.
        //!
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public SceneCreatorModule(string name, Core core) : base(name, core)
        {
            if (core.isServer)
                load = false;
        }

        //!
        //! Destructor, cleaning up event registrations. 
        //!
        ~SceneCreatorModule()
        {
            if (!m_core.isServer)
            {
                NetworkManager networkManager = m_core.getManager<NetworkManager>();
                SceneReceiverModule sceneReceiverModule = networkManager.getModule<SceneReceiverModule>();
                sceneReceiverModule.m_sceneReceived -= CreateScene;
            }
        }

        protected override void Init(object sender, EventArgs e)
        {
            NetworkManager networkManager = m_core.getManager<NetworkManager>();
            SceneReceiverModule sceneReceiverModule = networkManager.getModule<SceneReceiverModule>();
            sceneReceiverModule.m_sceneReceived += CreateScene;
        }

        //!
        //! Function that creates the Unity scene content.
        //!
        public void CreateScene(object o, EventArgs e)
        {
            SceneManager.SceneDataHandler sceneDataHandler = ((SceneManager)manager).sceneDataHandler;
            SceneManager.SceneDataHandler.SceneData sceneData = sceneDataHandler.getSceneData();



            Helpers.Log(string.Format("Build scene from: {0} objects, {1} textures, {2} materials, {3} nodes", sceneData.objectList.Count, sceneData.textureList.Count, sceneData.materialList.Count, sceneData.nodeList.Count));

            createMaterials(ref sceneData);

            if (manager.settings.loadTextures)
                createTextures(ref sceneData);

            createMeshes(ref sceneData);

            createSceneGraphIter(ref sceneData, manager.scnRoot.transform);

            createSkinnedMeshes(ref sceneData, manager.scnRoot.transform);

            sceneDataHandler.clearSceneByteData();
            clearData();

            manager.emitSceneReady();
        }

        //!
        //! Function that creates the materials in the Unity scene.
        //!
        //! @param sceneData A reference to the VPET sceneData.
        //!
        private void createMaterials(ref SceneManager.SceneDataHandler.SceneData sceneData)
        {
            foreach (SceneManager.MaterialPackage matPack in sceneData.materialList)
            {
                if (matPack.type == 1)
                {
                    Material mat = Resources.Load(string.Format("VPET/Materials/{0}", matPack.src), typeof(Material)) as Material;
                    if (mat)
                        SceneMaterialList.Add(mat);
                    else
                    {
                        Debug.LogWarning(string.Format("[{0} createMaterials]: Cant find Resource: {1}. Create Standard.", this.GetType(), matPack.src));
                        Material _mat = new Material(Shader.Find("Standard"));
                        _mat.name = matPack.name;
                        SceneMaterialList.Add(_mat);
                    }
                }
                else if (matPack.type == 2)
                {
                    Debug.Log(matPack.src);
                    Material mat = new Material(Shader.Find(matPack.src));
                    mat.name = matPack.name;
                    SceneMaterialList.Add(mat);
                }
            }
        }

        //!
        //! Function that creates the textures in the Unity scene.
        //!
        //! @param sceneData A reference to the VPET sceneData.
        //!
        private void createTextures(ref SceneManager.SceneDataHandler.SceneData sceneData)
        {
            foreach (SceneManager.TexturePackage texPack in sceneData.textureList)
            {
                if (sceneData.header.textureBinaryType == 1)
                {
                    Texture2D tex_2d = new Texture2D(texPack.width, texPack.height, texPack.format, false);
                    tex_2d.LoadRawTextureData(texPack.colorMapData);
                    tex_2d.Apply();
                    SceneTextureList.Add(tex_2d);
                }
                else
                {
#if UNITY_IPHONE
                    Texture2D tex_2d = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);
#else
                    Texture2D tex_2d = new Texture2D(16, 16, TextureFormat.DXT5Crunched, false);
#endif
                    tex_2d.LoadImage(texPack.colorMapData);
                    SceneTextureList.Add(tex_2d);
                }

            }
        }

        //!
        //! Function that creates the meshes in the Unity scene.
        //!
        //! @param sceneData A reference to the VPET sceneData.
        //!
        private void createMeshes(ref SceneManager.SceneDataHandler.SceneData sceneData)
        {
            foreach (SceneManager.ObjectPackage objPack in sceneData.objectList)
            {
                Vector3[] vertices = new Vector3[objPack.vSize];
                Vector3[] normals = new Vector3[objPack.nSize];
                Vector2[] uv = new Vector2[objPack.uvSize];
                BoneWeight[] weights = new BoneWeight[objPack.bWSize];

                for (int i = 0; i < objPack.bWSize; i++)
                {
                    BoneWeight b = new BoneWeight();
                    b.weight0 = objPack.boneWeights[i * 4 + 0];
                    b.weight1 = objPack.boneWeights[i * 4 + 1];
                    b.weight2 = objPack.boneWeights[i * 4 + 2];
                    b.weight3 = objPack.boneWeights[i * 4 + 3];
                    b.boneIndex0 = objPack.boneIndices[i * 4 + 0];
                    b.boneIndex1 = objPack.boneIndices[i * 4 + 1];
                    b.boneIndex2 = objPack.boneIndices[i * 4 + 2];
                    b.boneIndex3 = objPack.boneIndices[i * 4 + 3];
                    weights[i] = b;
                }

                for (int i = 0; i < objPack.vSize; i++)
                {
                    Vector3 v = new Vector3(objPack.vertices[i * 3 + 0], objPack.vertices[i * 3 + 1], objPack.vertices[i * 3 + 2]);
                    vertices[i] = v;
                }

                for (int i = 0; i < objPack.nSize; i++)
                {
                    Vector3 v = new Vector3(objPack.normals[i * 3 + 0], objPack.normals[i * 3 + 1], objPack.normals[i * 3 + 2]);
                    normals[i] = v;
                }

                for (int i = 0; i < objPack.uvSize; i++)
                {
                    Vector2 v2 = new Vector2(objPack.uvs[i * 2 + 0], objPack.uvs[i * 2 + 1]);
                    uv[i] = v2;
                }

                Mesh mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = uv;
                mesh.triangles = objPack.indices;
                mesh.boneWeights = weights;

                SceneMeshList.Add(mesh);
            }
        }

        // [REVIEW]
        //!
        //! Function that recusively creates the gameObjects in the Unity scene.
        //!
        //! @param sceneData A reference to the VPET sceneData.
        //! @param parent The transform of the parant node.
        //! @param idx The index for referencing into the node list.
        //!
        private int createSceneGraphIter(ref SceneManager.SceneDataHandler.SceneData sceneData, Transform parent, int idx = 0)
        {
            SceneManager.SceneNode node = sceneData.nodeList[idx];

            // process all registered build callbacks
            GameObject obj = CreateObject(node, parent);

            gameObjectList.Add(obj);

            // recursive call
            int idxChild = idx;
            for (int k = 1; k <= node.childCount; k++)
            {
                idxChild = createSceneGraphIter(ref sceneData, obj.transform, idxChild + 1);
            }

            // if there are more nodes on one level
            if (idxChild + 1 < sceneData.nodeList.Count)
                idxChild = createSceneGraphIter(ref sceneData, parent, idxChild + 1);

            return idxChild;
        }

        //!
        //! Function that recusively creates the gameObjects in the Unity scene.
        //!
        //! @param sceneData A reference to the VPET sceneData.
        //! @param root The transform of the root object.
        //!
        private void createSkinnedMeshes(ref SceneManager.SceneDataHandler.SceneData sceneData, Transform root)
        {
            List<SceneManager.CharacterPackage> characterList = sceneData.characterList;

            if (characterList.Count > 0)
                createSkinnedRendererIter(ref sceneData, root);

            //setup characters
            foreach (SceneManager.CharacterPackage cp in characterList)
            {
                GameObject obj = gameObjectList[cp.rootId];
                Transform parentBackup = obj.transform.parent;
                obj.transform.parent = GameObject.Find("Scene").transform.parent;
                HumanBone[] human = new HumanBone[cp.bMSize];
                for (int i = 0; i < human.Length; i++)
                {
                    int boneMapping = cp.boneMapping[i];
                    if (boneMapping == -1)
                        continue;
                    GameObject boneObj = gameObjectList[boneMapping];
                    human[i].boneName = boneObj.name;
                    human[i].humanName = ((HumanBodyBones)i).ToString();
                    human[i].limit.useDefaultValues = true;
                }
                SkeletonBone[] skeleton = new SkeletonBone[cp.sSize];
                skeleton[0].name = obj.name;
                skeleton[0].position = new Vector3(cp.bonePosition[0], cp.bonePosition[1], cp.bonePosition[2]);
                skeleton[0].rotation = new Quaternion(cp.boneRotation[0], cp.boneRotation[1], cp.boneRotation[2], cp.boneRotation[3]);
                skeleton[0].scale = new Vector3(cp.boneScale[0], cp.boneScale[1], cp.boneScale[2]);

                for (int i = 1; i < cp.skeletonMapping.Length; i++)
                {
                    if (cp.skeletonMapping[i] != -1)
                    {
                        skeleton[i].name = gameObjectList[cp.skeletonMapping[i]].name;
                        skeleton[i].position = new Vector3(cp.bonePosition[i * 3], cp.bonePosition[i * 3 + 1], cp.bonePosition[i * 3 + 2]);
                        skeleton[i].rotation = new Quaternion(cp.boneRotation[i * 4], cp.boneRotation[i * 4 + 1], cp.boneRotation[i * 4 + 2], cp.boneRotation[i * 4 + 3]);
                        skeleton[i].scale = new Vector3(cp.boneScale[i * 3], cp.boneScale[i * 3 + 1], cp.boneScale[i * 3 + 2]);
                    }
                }
                HumanDescription humanDescription = new HumanDescription();
                humanDescription.human = human;
                humanDescription.skeleton = skeleton;
                humanDescription.upperArmTwist = 0.5f;
                humanDescription.lowerArmTwist = 0.5f;
                humanDescription.upperLegTwist = 0.5f;
                humanDescription.lowerLegTwist = 0.5f;
                humanDescription.armStretch = 0.05f;
                humanDescription.legStretch = 0.05f;
                humanDescription.feetSpacing = 0.0f;
                humanDescription.hasTranslationDoF = false;

                Avatar avatar = AvatarBuilder.BuildHumanAvatar(obj, humanDescription);
                if (avatar.isValid == false || avatar.isHuman == false)
                {
                    Helpers.Log(GetType().FullName + ": Unable to create source Avatar for retargeting. Check that your Skeleton Asset Name and Bone Naming Convention are configured correctly.", Helpers.logMsgType.ERROR);
                    return;
                }
                avatar.name = obj.name;
                Animator animator = obj.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.applyRootMotion = true;

                //[REVIEW]
                //.runtimeAnimatorController = (RuntimeAnimatorController)Instantiate(Resources.Load("VPET/Prefabs/AnimatorController"));
                //obj.AddComponent<CharacterAnimationController>();

                obj.transform.parent = parentBackup;
            }

        }

        //!
        //! Creates an GameObject from an VPET SceneNode beneath the parent transform.
        //! @param node VPET SceneNode to be parsed.
        //! @param parentTransform Unity parent transform of the GameObject to-be created.
        //! @return The created GameObject.
        //!
        private GameObject CreateObject(SceneManager.SceneNode node, Transform parentTransform)
        {
            GameObject objMain;

            // Transform / convert handiness
            Vector3 pos = new Vector3(node.position[0], node.position[1], node.position[2]);

            // Rotation / convert handiness
            Quaternion rot = new Quaternion(node.rotation[0], node.rotation[1], node.rotation[2], node.rotation[3]);

            // Scale
            Vector3 scl = new Vector3(node.scale[0], node.scale[1], node.scale[2]);

            if (!parentTransform.Find(Encoding.ASCII.GetString(node.name)))
            {
                // set up object basics
                objMain = new GameObject();
                objMain.name = Encoding.ASCII.GetString(node.name);

                objMain.transform.localPosition = pos;
                objMain.transform.localRotation = rot;
                objMain.transform.localScale = scl;

                if (node.GetType() == typeof(SceneManager.SceneNodeGeo) || node.GetType() == typeof(SceneManager.SceneNodeSkinnedGeo))
                {
                    SceneManager.SceneNodeGeo nodeGeo = (SceneManager.SceneNodeGeo)node;
                    // Material Properties and Textures
                    Material mat;
                    // assign material from material list
                    if (nodeGeo.materialId > -1 && nodeGeo.materialId < SceneMaterialList.Count)
                    {
                        mat = SceneMaterialList[nodeGeo.materialId];
                    }
                    else // or set standard
                    {
                        mat = new Material(Shader.Find("Standard"));
                    }

                    // map properties
                    if (manager.settings.loadSampleScene)
                    {
                        MapMaterialProperties(mat, nodeGeo);
                    }

                    // Add Material
                    Renderer renderer;
                    if (nodeGeo.GetType() == typeof(SceneManager.SceneNodeSkinnedGeo))
                        renderer = objMain.AddComponent<SkinnedMeshRenderer>();
                    else
                        renderer = objMain.AddComponent<MeshRenderer>();

                    renderer.material = mat;

                    // Add Mesh
                    if (nodeGeo.geoId > -1 && nodeGeo.geoId < SceneMeshList.Count)
                    {
                        Mesh mesh = SceneMeshList[nodeGeo.geoId];

                        manager.sceneBoundsMax = Vector3.Max(manager.sceneBoundsMax, renderer.bounds.max);
                        manager.sceneBoundsMin = Vector3.Min(manager.sceneBoundsMin, renderer.bounds.min);

                        if (node.GetType() == typeof(SceneManager.SceneNodeSkinnedGeo))
                        {
                            SkinnedMeshRenderer sRenderer = (SkinnedMeshRenderer)renderer;
                            SceneManager.SceneNodeSkinnedGeo sNodeGeo = (SceneManager.SceneNodeSkinnedGeo)node;
                            Bounds bounds = new Bounds(new Vector3(sNodeGeo.boundCenter[0], sNodeGeo.boundCenter[1], sNodeGeo.boundCenter[2]),
                                                   new Vector3(sNodeGeo.boundExtents[0] * 2f, sNodeGeo.boundExtents[1] * 2f, sNodeGeo.boundExtents[2] * 2f));
                            sRenderer.localBounds = bounds;
                            Matrix4x4[] bindposes = new Matrix4x4[sNodeGeo.bindPoseLength];
                            for (int i = 0; i < sNodeGeo.bindPoseLength; i++)
                            {
                                bindposes[i] = new Matrix4x4();
                                bindposes[i][0, 0] = sNodeGeo.bindPoses[i * 16];
                                bindposes[i][0, 1] = sNodeGeo.bindPoses[i * 16 + 1];
                                bindposes[i][0, 2] = sNodeGeo.bindPoses[i * 16 + 2];
                                bindposes[i][0, 3] = sNodeGeo.bindPoses[i * 16 + 3];
                                bindposes[i][1, 0] = sNodeGeo.bindPoses[i * 16 + 4];
                                bindposes[i][1, 1] = sNodeGeo.bindPoses[i * 16 + 5];
                                bindposes[i][1, 2] = sNodeGeo.bindPoses[i * 16 + 6];
                                bindposes[i][1, 3] = sNodeGeo.bindPoses[i * 16 + 7];
                                bindposes[i][2, 0] = sNodeGeo.bindPoses[i * 16 + 8];
                                bindposes[i][2, 1] = sNodeGeo.bindPoses[i * 16 + 9];
                                bindposes[i][2, 2] = sNodeGeo.bindPoses[i * 16 + 10];
                                bindposes[i][2, 3] = sNodeGeo.bindPoses[i * 16 + 11];
                                bindposes[i][3, 0] = sNodeGeo.bindPoses[i * 16 + 12];
                                bindposes[i][3, 1] = sNodeGeo.bindPoses[i * 16 + 13];
                                bindposes[i][3, 2] = sNodeGeo.bindPoses[i * 16 + 14];
                                bindposes[i][3, 3] = sNodeGeo.bindPoses[i * 16 + 15];
                            }
                            mesh.bindposes = bindposes;
                            sRenderer.sharedMesh = mesh;

                        }
                        else
                        {
                            objMain.AddComponent<MeshFilter>();
                            objMain.GetComponent<MeshFilter>().mesh = mesh;
                        }
                    }

                    if (nodeGeo.editable)
                    {
                        SceneObject sco = objMain.AddComponent<SceneObject>();
                        manager.sceneObjects.Add(sco);
                    }
                }
                else if (node.GetType() == typeof(SceneManager.SceneNodeLight))
                {
                    SceneManager.SceneNodeLight nodeLight = (SceneManager.SceneNodeLight)node;

                    Light lightComponent = objMain.AddComponent<Light>();

                    lightComponent.type = nodeLight.lightType;
                    lightComponent.color = new Color(nodeLight.color[0], nodeLight.color[1], nodeLight.color[2]);
                    lightComponent.intensity = nodeLight.intensity * manager.settings.lightIntensityFactor;
                    lightComponent.spotAngle = nodeLight.angle;
                    if (lightComponent.type == LightType.Directional)
                    {
                        lightComponent.shadows = LightShadows.Soft;
                        lightComponent.shadowStrength = 0.8f;
                    }
                    else
                        lightComponent.shadows = LightShadows.None;
                    lightComponent.shadowBias = 0f;
                    lightComponent.shadowNormalBias = 1f;
                    lightComponent.range = nodeLight.range * manager.settings.sceneScale;

                    Debug.Log("Create Light: " + nodeLight.name + " of type: " + nodeLight.lightType.ToString() + " Intensity: " + nodeLight.intensity + " Pos: " + pos);

                    // Add light specific settings
                    if (nodeLight.lightType == LightType.Directional)
                    {
                    }
                    else if (nodeLight.lightType == LightType.Spot)
                    {
                        lightComponent.range *= 2;
                        //objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);
                    }
                    else if (nodeLight.lightType == LightType.Area)
                    {
                        // TODO: use are lights when supported in unity
                        lightComponent.spotAngle = 170;
                        lightComponent.range *= 4;
                    }
                    else
                    {
                        Debug.Log("Unknown Light Type in NodeBuilderBasic::CreateLight");
                    }

                    if (nodeLight.editable)
                    {
                        SceneObjectLight sco;
                        switch (lightComponent.type)
                        {
                            case LightType.Point:
                                sco = objMain.AddComponent<SceneObjectPointLight>();
                                break;
                            case LightType.Directional:
                                sco = objMain.AddComponent<SceneObjectDirectionalLight>();
                                break;
                            case LightType.Spot:
                                sco = objMain.AddComponent<SceneObjectSpotLight>();
                                break;
                            case LightType.Area:
                                sco = objMain.AddComponent<SceneObjectAreaLight>();
                                break;
                            default:
                                sco = objMain.AddComponent<SceneObjectLight>();
                                break;
                        }
                        manager.sceneLightList.Add(sco);
                        manager.sceneObjects.Add(sco);
                    }
                }
                else if (node.GetType() == typeof(SceneManager.SceneNodeCam))
                {
                    SceneManager.SceneNodeCam nodeCam = (SceneManager.SceneNodeCam)node;

                    Camera camera = objMain.AddComponent<Camera>();

                    camera.fieldOfView = nodeCam.fov;
                    camera.aspect = nodeCam.aspect;
                    camera.nearClipPlane = nodeCam.near;
                    camera.farClipPlane = nodeCam.far;
                    //camera.focalDistance = nodeCam.focalDist;     // not available in unity
                    //camera.aperture = nodeCam.aperture;       // not available in unity

                    if (nodeCam.editable)
                    {
                        SceneObjectCamera sco = objMain.AddComponent<SceneObjectCamera>();

                        manager.sceneCameraList.Add(sco);
                        manager.sceneObjects.Add(sco);
                    }
                }

                Vector3 sceneExtends = manager.sceneBoundsMax - manager.sceneBoundsMin;
                manager.maxExtend = Mathf.Max(Mathf.Max(sceneExtends.x, sceneExtends.y), sceneExtends.z);

                //place object
                objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
            }
            else
            {
                objMain = parentTransform.Find(Encoding.ASCII.GetString(node.name)).gameObject;
            }

            return objMain;

        }

        //! 
        //! [REVIEW]
        //!
        public static void MapMaterialProperties(Material material, SceneManager.SceneNodeGeo nodeGeo)
        {
            /*
            //available parameters in this physically based standard shader:
            // _Color                   diffuse color (color including alpha)
            // _MainTex                 diffuse texture (2D texture)
            // _MainTex_ST
            // _Cutoff                  alpha cutoff
            // _Glossiness              smoothness of surface
            // _Metallic                matallic look of the material
            // _MetallicGlossMap        metallic texture (2D texture)
            // _BumpScale               scale of the bump map (float)
            // _BumpMap                 bumpmap (2D texture)
            // _Parallax                scale of height map
            // _ParallaxMap             height map (2D texture)
            // _OcclusionStrength       scale of occlusion
            // _OcclusionMap            occlusionMap (2D texture)
            // _EmissionColor           color of emission (color without alpha)
            // _EmissionMap             emission strength map (2D texture)
            // _DetailMask              detail mask (2D texture)
            // _DetailAlbedoMap         detail diffuse texture (2D texture)
            // _DetailAlbedoMap_ST
            // _DetailNormalMap
            // _DetailNormalMapScale    scale of detail normal map (float)
            // _DetailAlbedoMap         detail normal map (2D texture)
            // _UVSec                   UV Set for secondary textures (float)
            // _Mode                    rendering mode (float) 0 -> Opaque , 1 -> Cutout , 2 -> Transparent
            // _SrcBlend                source blend mode (enum is UnityEngine.Rendering.BlendMode)
            // _DstBlend                destination blend mode (enum is UnityEngine.Rendering.BlendMode)
            // test texture
            // WWW www = new WWW("file://F:/XML3D_Examples/tex/casual08a.jpg");
            // Texture2D texture = www.texture;
            foreach (KeyValuePair<string, KeyValuePair<string, Type>> pair in VPETSettings.ShaderPropertyMap)
            {
                FieldInfo fieldInfo = nodeGeo.GetType().GetField(pair.Value.Key, BindingFlags.Instance | BindingFlags.Public);
                Type propertyType = pair.Value.Value;

                if (material.HasProperty(pair.Key) && fieldInfo != null)
                {
                    if (propertyType == typeof(int))
                    {
                        material.SetInt(pair.Key, (int)Convert.ChangeType(fieldInfo.GetValue(nodeGeo), propertyType));
                    }
                    else if (propertyType == typeof(float))
                    {
                        material.SetFloat(pair.Key, (float)Convert.ChangeType(fieldInfo.GetValue(nodeGeo), propertyType));
                    }
                    else if (propertyType == typeof(Color))
                    {
                        float[] v = (float[])fieldInfo.GetValue(nodeGeo);
                        float a = v.Length > 3 ? v[3] : 1.0f;
                        Color c = new Color(v[0], v[1], v[2], a);
                        string name = Encoding.UTF8.GetString(nodeGeo.name, 0, nodeGeo.name.Length);
                        material.SetColor(pair.Key, c);

                        if (a < 1.0f)
                        {
                            // set rendering mode
                            material.SetFloat("_Mode", 1);
                            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.SetInt("_ZWrite", 0);
                            material.DisableKeyword("_ALPHATEST_ON");
                            material.DisableKeyword("_ALPHABLEND_ON");
                            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.renderQueue = 3000;
                        }
                    }
                    else if (propertyType == typeof(Texture))
                    {
                        int id = (int)Convert.ChangeType(fieldInfo.GetValue(nodeGeo), typeof(int));

                        if (id > -1 && id < SceneLoader.SceneTextureList.Count)
                        {
                            Texture2D texRef = SceneLoader.SceneTextureList[nodeGeo.textureId];

                            material.SetTexture(pair.Key, texRef);

                            // set materials render mode to fate to senable alpha blending
                            // TODO these values should be part of the geo node or material package !?
                            if (Textures.hasAlpha(texRef))
                            {
                                // set rendering mode
                                material.SetFloat("_Mode", 1);
                                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                                material.SetInt("_ZWrite", 0);
                                material.DisableKeyword("_ALPHATEST_ON");
                                material.DisableKeyword("_ALPHABLEND_ON");
                                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                                material.renderQueue = 3000;
                            }
                        }

                    }
                    else
                    {
                        Debug.LogWarning("Can not map material property " + pair.Key);
                    }


                    // TODO implement the rest
                    // .
                    // .
                    // .
                }

            }*/

        }

        //!
        //! Function that recusively adds bone transforms to renderers of SkinnedMesh objects.
        //!
        //! @param sceneData A reference to the VPET sceneData.
        //! @param parent the parent Unity transform.
        //! @param idx The index for referencing into the node list.
        //!
        private int createSkinnedRendererIter(ref SceneManager.SceneDataHandler.SceneData sceneData, Transform parent, int idx = 0)
        {

            SceneManager.SceneNodeSkinnedGeo node = (SceneManager.SceneNodeSkinnedGeo)sceneData.nodeList[idx];

            if (node == null)
                return -1;

            Transform trans = parent.Find(Encoding.ASCII.GetString(node.name));

            SkinnedMeshRenderer renderer = trans.gameObject.GetComponent<SkinnedMeshRenderer>();

            if (renderer)
            {
                renderer.rootBone = trans;
                Transform[] meshBones = new Transform[node.skinnedMeshBoneIDs.Length];
                for (int i = 0; i < node.skinnedMeshBoneIDs.Length; i++)
                {
                    if (node.skinnedMeshBoneIDs[i] != -1)
                        meshBones[i] = gameObjectList[node.skinnedMeshBoneIDs[i]].transform;
                }
                renderer.bones = meshBones;
            }

            // recursive call
            int idxChild = idx;
            for (int k = 1; k <= node.childCount; k++)
            {
                idxChild = createSkinnedRendererIter(ref sceneData, trans, idxChild + 1);
            }

            return idxChild;
        }

        //!
        //! Function that deletes all Unity scene content and clears the VPET scene object lists.
        //!
        public void clearData()
        {
            SceneMaterialList.Clear();
            SceneTextureList.Clear();
            SceneMeshList.Clear();
            gameObjectList.Clear();
        }

    }
}

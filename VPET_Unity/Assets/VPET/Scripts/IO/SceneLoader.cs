/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

//!
//! Script creating the scene received from the server (providing XML3D) within Unity 
//!
namespace vpet
{

    public class SceneLoader : MonoBehaviour
    {
        //!
        //! name of the parent gameobject, all objects go underneath it
        //!
        public string sceneParent = "Scene";


        private GameObject scnPrtGO;
#if TRUNK
        public static List<Material> SceneMaterialList = new List<Material>();
#endif
        public static List<Texture2D> SceneTextureList = new List<Texture2D>();
        public static List<Mesh[]> SceneMeshList = new List<Mesh[]>();
        public static List<GameObject> SceneEditableObjects = new List<GameObject>();
        public static List<GameObject> SelectableLights = new List<GameObject>();
        public static List<GameObject> SceneCameraList = new List<GameObject>();
        public static List<GameObject> gameObjectList = new List<GameObject>();

        public static GameObject scnRoot;

        //!
        //! list of skinnedMeshRootBone Objects
        //!
        private List<Tuple<Renderer, GameObject, int[]>> skinnedMeshRootBones;

        private List<GameObject> geometryPassiveList = new List<GameObject>();
        private SceneDataHandler sceneDataHandler;




        public SceneDataHandler SceneDataHandler
        {
            get { return sceneDataHandler; }
        }

        public delegate GameObject NodeBuilderDelegate(ref SceneNode n, Transform t, GameObject o, bool resetID, ref List<Tuple<Renderer, GameObject, int[]>> skinnedMeshRootBones);
        public static List<NodeBuilderDelegate> nodeBuilderDelegateList = new List<NodeBuilderDelegate>();

        public static void RegisterDelegate(NodeBuilderDelegate call)
        {
            if (!nodeBuilderDelegateList.Contains(call))
                nodeBuilderDelegateList.Add(call);
        }


        void Awake()
        {
            sceneDataHandler = new SceneDataHandler();
            sceneDataHandler.initializeLists();
        }

        void Start()
        {
            // create scene parent if not there
            scnPrtGO = GameObject.Find(sceneParent);
            if (scnPrtGO == null)
            {
                scnPrtGO = new GameObject(sceneParent);
            }

            scnRoot = scnPrtGO.transform.Find("root").gameObject;
            if (scnRoot == null)
            {
                scnRoot = new GameObject("root");
                scnRoot.transform.parent = scnPrtGO.transform;
            }
        }

        public void ResetScene()
        {
            SceneEditableObjects.Clear();
#if TRUNK
            SceneMaterialList.Clear();
#endif
            SceneTextureList.Clear();
            SceneMeshList.Clear();
            SceneCameraList.Clear();
            geometryPassiveList.Clear();
            SelectableLights.Clear();
            gameObjectList.Clear();

            if (scnRoot != null)
            {
                foreach (Transform child in scnRoot.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        public void createSceneGraph()
        {
            VPETSettings.Instance.sceneBoundsMax = Vector3.negativeInfinity;
            VPETSettings.Instance.sceneBoundsMin = Vector3.positiveInfinity;

#if TRUNK
            print(string.Format("Build scene from: {0} objects, {1} textures, {2} materials, {3} nodes", sceneDataHandler.ObjectList.Count, sceneDataHandler.TextureList.Count, sceneDataHandler.MaterialList.Count, sceneDataHandler.NodeList.Count));
#else
            print(string.Format("Build scene from: {0} objects, {1} textures, {2} nodes", sceneDataHandler.ObjectList.Count, sceneDataHandler.TextureList.Count, sceneDataHandler.NodeList.Count));
#endif

#if TRUNK
            createMaterials();
#endif

            // create textures
            if (VPETSettings.Instance.doLoadTextures)
            {
                createTextures();
            }

            // create meshes
            createMeshes();

            //initiialize skinnedMeshRootBones
            skinnedMeshRootBones = new List<Tuple<Renderer, GameObject, int[]>>();

            // iterate nodes
            createSceneGraphIter(scnRoot.transform, 0);

            List<CharacterPackage> characterList = sceneDataHandler.CharacterList;
            
            //setup skinned mesh root bones
            foreach (Tuple<Renderer, GameObject, int[]> t in skinnedMeshRootBones)
            {
                ((SkinnedMeshRenderer)t.Item1).rootBone = t.Item2.transform;
                Transform[] meshBones = new Transform[t.Item3.Length];
                for (int i = 0; i < t.Item3.Length; i++)
                {
                    if (t.Item3[i] != -1)
                        meshBones[i] = SceneLoader.gameObjectList[t.Item3[i]].transform;
                }
                ((SkinnedMeshRenderer)t.Item1).bones = meshBones;

            }
            skinnedMeshRootBones.Clear();

            Debug.Log("Found " + characterList.Count + " characters in scene.");

            //setup characters
            foreach (CharacterPackage cp in characterList)
            {
                GameObject obj = SceneLoader.gameObjectList[cp.rootId];
                Transform parentBackup = obj.transform.parent;
                obj.transform.parent = GameObject.Find("Scene").transform.parent;
                HumanBone[] human = new HumanBone[cp.bMSize];
                for (int i = 0; i < human.Length; i++)
                {
                    int boneMapping = cp.boneMapping[i];
                    if (boneMapping == -1)
                        continue;
                    GameObject boneObj = SceneLoader.gameObjectList[boneMapping];
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
                        skeleton[i].name = SceneLoader.gameObjectList[cp.skeletonMapping[i]].name;
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
                    Debug.LogError(GetType().FullName + ": Unable to create source Avatar for retargeting. Check that your Skeleton Asset Name and Bone Naming Convention are configured correctly.", this);
                    this.enabled = false;
                    return;
                }
                avatar.name = obj.name;
                Animator animator = obj.AddComponent<Animator>();
                animator.avatar = avatar;
                animator.applyRootMotion = true;

                animator.runtimeAnimatorController = (RuntimeAnimatorController)Instantiate(Resources.Load("VPET/Prefabs/AnimatorController"));
                obj.AddComponent<CharacterAnimationController>();

                obj.transform.parent = parentBackup;
            }

            // set default orthographic size
            Camera.main.orthographicSize = 1000 * VPETSettings.Instance.sceneScale;
        }

        public bool isEditable(GameObject targetObj)
        {
            foreach (GameObject g in SceneEditableObjects)
            {
                if (g == targetObj)
                    return true;
            }
            return false;
        }


        private int createSceneGraphIter(Transform parent, int idx)
        {

            GameObject obj = null; // = new GameObject( scnObjKtn.rawNodeList[idx].name );

            SceneNode node = sceneDataHandler.NodeList[idx];

            // process all registered build callbacks
            foreach (NodeBuilderDelegate nodeBuilderDelegate in nodeBuilderDelegateList)
            {
                GameObject _obj = nodeBuilderDelegate(ref node, parent, obj, (sceneDataHandler.NodeList.Count == 0), ref skinnedMeshRootBones);
                if (_obj != null)
                    obj = _obj;
            }

            gameObjectList.Add(obj);

            // add scene object to editable 
            if (node.editable)
            {
                SceneEditableObjects.Add(obj);
            }

            // recursive call
            int idxChild = idx;
            for (int k = 1; k <= node.childCount; k++)
            {
                idxChild = createSceneGraphIter(obj.transform, idxChild + 1);
            }

            return idxChild;
        }

#if TRUNK
        private void createMaterials()
        {
            foreach (MaterialPackage matPack in sceneDataHandler.MaterialList)
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
#endif

        private void createTextures()
        {
            foreach (TexturePackage texPack in sceneDataHandler.TextureList)
            {
                if (sceneDataHandler.TextureBinaryType == 1)
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
        //! function ??
        //! @param  ??   ??
        //!    
        private void createMeshes()
        {

            foreach (ObjectPackage objPack in sceneDataHandler.ObjectList)
            {
                Vector3[] vertices = new Vector3[objPack.vSize];
                Vector3[] normals = new Vector3[objPack.nSize];
                Vector2[] uv = new Vector2[objPack.uvSize];
                BoneWeight[] weights = new BoneWeight[objPack.bWSize];


                // TODO Handiness, see below!?
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

                createSplitMesh(vertices, normals, uv, objPack.indices, weights);
            }
        }


        //! function create mesh at the given gameobject and split if necessary
        //! @param  obj             gameobject to work on
        //! @param  vertices        
        //! @param  normals        
        //! @param  uvs        
        //! @param  triangles        
        //! @param  material        
        private void createSplitMesh(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles, BoneWeight[] weights)
        {
            List<Mesh> meshList = new List<Mesh>();

            //Note to commented part below:
            //Splitting of meshes disabled as we are using 32bit index buffers now!

            // TODO: review
            // if more than 65K vertices, split the mesh in submeshs
            /*if (vertices.Length > 6500000)
            {
                // print( String.Format( "Split object: {0}", obj.name ) );

                int triIndex = 0;
                int triIndexMax = triangles.Length - 1;
                int triIndexOffset = 63000;
                int subObjCount = 0;

                while (triIndex < triIndexMax)
                {
                    subObjCount++;

                    List<Vector3> subVertices = new List<Vector3>();
                    List<Vector3> subNormals = new List<Vector3>();
                    List<Vector2> subUVs = new List<Vector2>();
                    List<BoneWeight> subWeights = new List<BoneWeight>();
                    List<int> subTriangles = new List<int>();

                    int[] mapVertexIndices = new int[vertices.Length];
                    for (int i = 0; i < mapVertexIndices.Length; i++)
                    {
                        mapVertexIndices[i] = -1;
                    }

                    for (int i = 0; i < triIndexOffset; i++)
                    {
                        int idx = triangles[triIndex + i];
                        if (mapVertexIndices[idx] != -1)
                        {
                            subTriangles.Add(mapVertexIndices[idx]);
                        }
                        else
                        {
                            subVertices.Add(vertices[idx]);
                            subNormals.Add(normals[idx]);
                            subUVs.Add(uvs[idx]);
                            subWeights.Add(weights[idx]);
                            subTriangles.Add(subVertices.Count - 1);
                            mapVertexIndices[idx] = subVertices.Count - 1;
                        }
                    }


                    // print( "create :" + subObj.name + " triIndex " + triIndex  + " triOffset " + triIndexOffset + " numVertices " + subVertices.Count);

                    Mesh mesh = new Mesh();
                    mesh.Clear();
                    mesh.vertices = subVertices.ToArray();
                    mesh.normals = subNormals.ToArray();
                    mesh.uv = subUVs.ToArray();
                    mesh.boneWeights = subWeights.ToArray();
                    mesh.triangles = subTriangles.ToArray();
                    meshList.Add(mesh);


                    triIndex += triIndexOffset;

                    if (triIndex + triIndexOffset > triIndexMax)
                    {
                        triIndexOffset = triIndexMax - triIndex + 1;
                    }

                }
            }
            else*/
            {
                Mesh mesh = new Mesh();
                mesh.Clear();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = uvs;
                mesh.triangles = triangles;
                mesh.boneWeights = weights;
                meshList.Add(mesh);
                // mesh.RecalculateNormals();
                // mesh.RecalculateBounds();
            }

            SceneMeshList.Add(meshList.ToArray());
        }

        public void HideGeometry()
        {
            if (geometryPassiveList.Count == 0 && scnRoot.transform.childCount > 0)
            {
                getGeometryIter(scnRoot.transform);
            }

            foreach (GameObject g in geometryPassiveList)
            {
                g.SetActive(false);
            }
        }

        public void ShowGeometry()
        {
            foreach (GameObject g in geometryPassiveList)
            {
                g.SetActive(true);
            }
        }

        private void getGeometryIter(Transform t)
        {
            if (t.GetComponentInChildren<SceneObject>() == null && t.GetComponentInChildren<Light>() == null && t.GetComponentInChildren<Camera>() == null)
            {
                geometryPassiveList.Add(t.gameObject);
            }
            else if (t.GetComponent<SceneObject>() == null && t.GetComponent<Light>() == null && t.GetComponent<Camera>() == null)
            {
                foreach (Transform child in t)
                {
                    getGeometryIter(child);
                }
            }
        }

        public bool HasHiddenGeo
        {
            get
            {
                if (geometryPassiveList.Count < 1 || geometryPassiveList[0].activeSelf)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static void MapMaterialProperties(Material material, SceneNodeGeo nodeGeo)
        {
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

            }

        }


    }
}
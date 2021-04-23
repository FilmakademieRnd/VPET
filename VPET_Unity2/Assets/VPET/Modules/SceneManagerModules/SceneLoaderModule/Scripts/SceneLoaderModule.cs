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

//! @file "SceneLoaderModule.cs"
//! @brief implementation of VPET scene loader module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.04.2021

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! implementation of unity scene loader module
    //!
    public class SceneLoaderModule : Module
    {
        //! The list storing Unity materials in scene.
        public static List<Material> SceneMaterialList = new List<Material>();
        
        //! The list storing Unity textures in scene.
        public static List<Texture2D> SceneTextureList = new List<Texture2D>();
       
        //! The list storing Unity meshes in scene.
        public static List<Mesh> SceneMeshList = new List<Mesh>();
        
        // [REVIEW]
        //! The list storing editable Unity game objects in scene.
        public static List<GameObject> SceneEditableObjects = new List<GameObject>();
        
        //! The list storing selectable Unity lights in scene.
        public static List<GameObject> SelectableLights = new List<GameObject>();
        
        //! The list storing Unity cameras in scene.
        public static List<GameObject> SceneCameraList = new List<GameObject>();
       
        //! The list storing all Unity gameObjects in scene.
        public static List<GameObject> gameObjectList = new List<GameObject>();

        public static GameObject scnRoot;

        //!
        //! constructor
        //! @param   name    Name of this module
        //!
        public SceneLoaderModule(string name) : base(name) => name = base.name;

        // [REVIEW]
        //! to be replaced
        public void Test()
        {
            UnitySceneLoaderModule m = (UnitySceneLoaderModule)manager.getModule(typeof(UnitySceneLoaderModule));        

        }

        //!
        //! Function that creates the Unity scene content.
        //!
        public void LoadScene()
        {
            SceneDataHandler sceneDataHandler = new SceneDataHandler();

            // create scene parent if not there
            GameObject scnPrtGO = GameObject.Find("Scene");
            if (scnPrtGO == null)
            {
                scnPrtGO = new GameObject("Scene");
            }

            GameObject scnRoot = scnPrtGO.transform.Find("root").gameObject;
            if (scnRoot == null)
            {
                scnRoot = new GameObject("root");
                scnRoot.transform.parent = scnPrtGO.transform;
            }

            // [REVIEW]
            //VPETSettings.Instance.sceneBoundsMax = Vector3.negativeInfinity;
            //VPETSettings.Instance.sceneBoundsMin = Vector3.positiveInfinity;

            Helpers.Log(string.Format("Build scene from: {0} objects, {1} textures, {2} materials, {3} nodes", sceneDataHandler.ObjectList.Count, sceneDataHandler.TextureList.Count, sceneDataHandler.MaterialList.Count, sceneDataHandler.NodeList.Count));

            createMaterials(ref sceneDataHandler);
            
            // [REVIEW]
            //if (VPETSettings.Instance.doLoadTextures)
            createTextures(ref sceneDataHandler);

            createMeshes(ref sceneDataHandler);

            //initiialize skinnedMeshRootBones
            List<Tuple<Renderer, GameObject, int[]>> skinnedMeshRootBones = new List<Tuple<Renderer, GameObject, int[]>>();

            createSceneGraphIter(ref sceneDataHandler, ref skinnedMeshRootBones, scnRoot.transform);

            createSkinnedMeshes(ref sceneDataHandler, ref skinnedMeshRootBones);
        }

        //!
        //! Function that creates the materials in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //!
        private void createMaterials(ref SceneDataHandler sceneDataHandler)
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

        //!
        //! Function that creates the textures in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //!
        private void createTextures(ref SceneDataHandler sceneDataHandler)
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
        //! Function that creates the meshes in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //!
        private void createMeshes(ref SceneDataHandler sceneDataHandler)
        {
            foreach (ObjectPackage objPack in sceneDataHandler.ObjectList)
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
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //! @param skinnedMeshRootBones A reference to a list containing the skinned mesh root bones.
        //! @param parent the parent Unity transform.
        //! @param idx The index for referencing into the node list.
        //!
        private int createSceneGraphIter(ref SceneDataHandler sceneDataHandler, ref List<Tuple<Renderer, GameObject, int[]>> skinnedMeshRootBones, Transform parent, int idx = 0)
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

        // [REVIEW]
        //!
        //! Function that recusively creates the gameObjects in the Unity scene.
        //!
        //! @param sceneDataHandler A reference to the actual VPET sceneDataHandler.
        //! @param skinnedMeshRootBones A reference to a list containing the skinned mesh root bones.
        //!
        private void createSkinnedMeshes(ref SceneDataHandler sceneDataHandler, ref List<Tuple<Renderer, GameObject, int[]>> skinnedMeshRootBones)
        {
            List<CharacterPackage> characterList = sceneDataHandler.CharacterList;

            foreach (Tuple<Renderer, GameObject, int[]> t in skinnedMeshRootBones)
            {
                ((SkinnedMeshRenderer)t.Item1).rootBone = t.Item2.transform;
                Transform[] meshBones = new Transform[t.Item3.Length];
                for (int i = 0; i < t.Item3.Length; i++)
                {
                    if (t.Item3[i] != -1)
                        meshBones[i] = gameObjectList[t.Item3[i]].transform;
                }
                ((SkinnedMeshRenderer)t.Item1).bones = meshBones;
            }

            //setup characters
            foreach (CharacterPackage cp in characterList)
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

        }

        //!
        //! Function that deletes all Unity scene content and clears the VPET scene object lists.
        //!
        public void ResetScene()
        {
            SceneEditableObjects.Clear();
            SceneMaterialList.Clear();
            SceneTextureList.Clear();
            SceneMeshList.Clear();
            SceneCameraList.Clear();
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

    }
}

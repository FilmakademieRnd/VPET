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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System;
using System.IO;


namespace vpet
{

    public class SceneDistribution : MonoBehaviour
    {
        [Header("Scene")]
        public GameObject sceneRoot;
        public bool doDistribute = true;
        public string port = "5565";
        public bool doGatherOnRequest = false;
        public bool doAssignSceneObjects = false;

        [Header("Cache File")]
        public bool WriteCacheFile = false;
        public string sceneFileName;

        [Header("Recording")]
        public string recordPath = "Records";
        public string sceneName = "";
        public string shotName = "";
        public string takeName = "";

        private List<SceneNode> nodeList;
        private List<ObjectPackage> objectList;
        private List<CharacterPackage> characterList;
        private List<TexturePackage> textureList;
        private Thread serverThread;
        private bool isRunning = false;

        private byte[] headerByteData;
        private byte[] nodesByteData;
        private byte[] objectsByteData;
        private byte[] charactersByteData;
        private byte[] texturesByteData;
#if TRUNK
        private List<MaterialPackage> materialList;
        private byte[] materialsByteData;
#endif
        private int textureBinaryType = 1; // unity raw data

        private VpetHeader vpetHeader;

        private int lodLowLayer;
        private int lodHighLayer;
        private int lodMixedLayer;

        private int globalID;

        private Transform scene;

        private ServerAdapter serverAdapter;

        private void Awake()
        {
            if (sceneRoot == null)
                sceneRoot = GameObject.Find("root");

            serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();

            if (sceneRoot == null) Debug.LogError(string.Format("{0}: Cant find Scene Root: 'root'.", this.GetType()));

            lodLowLayer = LayerMask.NameToLayer("LodLow");
            lodHighLayer = LayerMask.NameToLayer("LodHigh");
            lodMixedLayer = LayerMask.NameToLayer("LodMixed");

            VPETRegister.RegisterObjectSender();
            serverAdapter.initServerAdapterTransfer();
        }

        // Use this for initialization
        void Start()
        {
            scene = GameObject.Find("Scene").transform;
            Application.targetFrameRate = 60;
            globalID = 0;
            if (doDistribute && sceneRoot != null)
            {
                vpetHeader = new VpetHeader();
                vpetHeader.lightIntensityFactor = 1f;
                vpetHeader.textureBinaryType = 1;

                gatherSceneData();

                // write binary to file
                if (WriteCacheFile && sceneFileName != "")
                {
                    writeBinary(headerByteData, "header");
                    writeBinary(nodesByteData, "nodes");
                    writeBinary(objectsByteData, "objects");
                    writeBinary(charactersByteData, "characters");
                    writeBinary(texturesByteData, "textures");
#if TRUNK
                    writeBinary(materialsByteData, "materials");
#endif
                }

                if (serverThread == null)
                {
                    print("Start distribution thread");
                    isRunning = true;
                    serverThread = new Thread(new ThreadStart(dataServer));
                    serverThread.Start();
                    print("Distribution thread started");
                }
                else
                {
                    Debug.LogWarning(string.Format("{0}: DistributionServerThread already running", this.GetType()));
                }
            }
        }



        private void dataServer()
        {
            AsyncIO.ForceDotNet.Force();
            using (var dataSender = new ResponseSocket())
            {
                dataSender.Bind("tcp://" + VPETSettings.Instance.serverIP + ":5565");
                Debug.Log("Enter while.. ");

                while (isRunning)
                {
                    string message = "";
                    dataSender.TryReceiveFrameString(out message);
                    //print ("Got request message: " + message);

                    // re-run scene iteration if true
                    if (doGatherOnRequest)
                        gatherSceneData();

                    switch (message)
                    {
                        case "header":
                            print("Send Header.. ");
                            dataSender.SendFrame(headerByteData);
                            print(string.Format(".. Nodes ({0} bytes) sent ", headerByteData.Length));
                            break;
                        case "nodes":
                            print("Send Nodes.. ");
                            dataSender.SendFrame(nodesByteData);
                            print(string.Format(".. Nodes ({0} bytes) sent ", nodesByteData.Length));
                            break;
                        case "objects":
                            print("Send Objects.. ");
                            dataSender.SendFrame(objectsByteData);
                            print(string.Format(".. Objects ({0} bytes) sent ", objectsByteData.Length));
                            break;
                        case "characters":
                            print("Send Characters.. ");
                            dataSender.SendFrame(charactersByteData);
                            print(string.Format(".. Characters ({0} bytes) sent ", charactersByteData.Length));
                            break;
                        case "textures":
                            print("Send Textures.. ");
                            dataSender.SendFrame(texturesByteData);
                            print(string.Format(".. Textures ({0} bytes) sent ", texturesByteData.Length));
                            break;
#if TRUNK
                        case "materials":
                            print("Send Materials.. ");
                            dataSender.SendFrame(materialsByteData);
                            print(string.Format(".. Materials ({0} bytes) sent ", materialsByteData.Length));
                            break;
#endif
                        default:
                            break;
                    }

                }

                // TODO: check first if closed
                try
                {
                    dataSender.Disconnect("tcp://" + VPETSettings.Instance.serverIP + ":5565");
                    dataSender.Dispose();
                    dataSender.Close();
                }
                finally
                {
                    NetMQConfig.Cleanup(false);
                }

            }
        }



        /*
        private bool hasLodLowChild( Transform t )
        {
            if (t.gameObject.layer == lodLowLayer)
                return true;

            foreach( Transform c in t )
            {
                if ( hasLodLowChild(c) )
                {
                    return true;
                }
            }

            return false;
        }
        */

        private void gatherSceneData()
        {
            nodeList = new List<SceneNode>();
            objectList = new List<ObjectPackage>();
            characterList = new List<CharacterPackage>();
            textureList = new List<TexturePackage>();
#if TRUNK
            materialList = new List<MaterialPackage>();
#endif

            iterLocation(sceneRoot.transform);

            serverAdapter.sceneObjectRefList = new SceneObject[globalID];

            recursiveIdExtract(sceneRoot.transform);

            Debug.Log(string.Format("{0}: Collected number nodes: {1}", this.GetType(), nodeList.Count));
            Debug.Log(string.Format("{0}: Collected number objects: {1}", this.GetType(), objectList.Count));
            Debug.Log(string.Format("{0}: Collected number skinned charcaters: {1}", this.GetType(), characterList.Count));
            Debug.Log(string.Format("{0}: Collected number textures: {1}", this.GetType(), textureList.Count));
#if TRUNK
            Debug.Log(string.Format("{0}: Collected number materials: {1}", this.GetType(), materialList.Count));
#endif
            // create byte arrays
            headerByteData = SceneDataHandler.StructureToByteArray(vpetHeader);
            getNodesByteArray();
            getObjectsByteArray();
            getCharacterByteArray();
            getTexturesByteArray();
#if TRUNK
            getMaterialsByteArray();
#endif
            Debug.Log(string.Format("{0}: HeaderByteArray size: {1}", this.GetType(), headerByteData.Length));
            Debug.Log(string.Format("{0}: NodeByteArray size: {1}", this.GetType(), nodesByteData.Length));
            Debug.Log(string.Format("{0}: ObjectsByteArray size: {1}", this.GetType(), objectsByteData.Length));
            Debug.Log(string.Format("{0}: CharacterByteArray size: {1}", this.GetType(), charactersByteData.Length));
            Debug.Log(string.Format("{0}: TexturesByteArray size: {1}", this.GetType(), texturesByteData.Length));
#if TRUNK
            Debug.Log(string.Format("{0}: MaterialsByteArray size: {1}", this.GetType(), materialsByteData.Length));
#endif
        }

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

        //!
        //! Recursively iterate over scene and prepare data to be send to clients
        //!
        private bool iterLocation(Transform location)
        {
            // check LOD and retur if not match
            if (location.gameObject.layer != lodLowLayer && location.gameObject.layer != lodMixedLayer)
                return false;

            SceneNode node = new SceneNode();

            // print("Location: " + location);

            if (location.GetComponent<Light>() != null)
            {
                SceneNodeLight nodeLight = new SceneNodeLight();

                Light light = location.GetComponent<Light>();
                nodeLight.intensity = light.intensity;
                nodeLight.color = new float[3] { light.color.r, light.color.g, light.color.b };
                nodeLight.lightType = light.type;
                nodeLight.exposure = 0;
                nodeLight.angle = light.spotAngle;
                nodeLight.range = light.range;

                node = nodeLight;
                node.editable = true;
                SceneObject sObj = location.gameObject.AddComponent<SceneObject>();
                sObj.id = globalID;
                globalID++;
            }
            else if (location.GetComponent<Camera>() != null)
            {
                SceneNodeCam nodeCamera = new SceneNodeCam();

                Camera camera = location.GetComponent<Camera>();
                nodeCamera.fov = camera.fieldOfView;
                nodeCamera.near = camera.nearClipPlane;
                nodeCamera.far = camera.farClipPlane;
                node = nodeCamera;

                node.editable = true;
                SceneObject sObj = location.gameObject.AddComponent<SceneObject>();
                sObj.id = globalID;
                globalID++;
            }
            else if (location.GetComponent<MeshFilter>() != null)
            {
                SceneNodeGeo nodeGeo = new SceneNodeGeo();
                nodeGeo.color = new float[4] { 0, 0, 0, 0 };
                nodeGeo.geoId = processGeometry(location.GetComponent<MeshFilter>().sharedMesh);

                if (location.GetComponent<Renderer>() != null && location.GetComponent<Renderer>().sharedMaterial != null)
                {
                    Material mat = location.GetComponent<Renderer>().sharedMaterial;
#if TRUNK
                    // if materials's shader is not standard, add this material to material package. 
                    // Currently this will only get the material name and try to load it on client side. If this fails, it will fallback to Standard.
                    nodeGeo.materialId = processMaterial(location.GetComponent<Renderer>().sharedMaterial);
#endif
                    if (mat.HasProperty("_Color"))
                    {
                        nodeGeo.color = new float[4] { mat.color.r, mat.color.g, mat.color.b, mat.color.a };
                    }

                    if (mat.HasProperty("_Glossiness"))
                        nodeGeo.roughness = mat.GetFloat("_Glossiness");

                    if (mat.mainTexture != null)
                    {
                        Texture2D mainTex = (Texture2D)mat.mainTexture;
                        nodeGeo.textureId = processTexture(mainTex);
                    }
                    else
                    {
                        nodeGeo.textureId = -1;
                    }
                }
                else
                {
                    nodeGeo.textureId = -1;
                }

                node = nodeGeo;

                if (location.gameObject.tag == "editable")
                {
                    node.editable = true;
                    bool gotHighLod = false;
                    foreach (Transform child in location.parent)
                    {
                        if (child.name == location.name && child.gameObject.layer == lodHighLayer)
                        {
                            SceneObject sObj = child.gameObject.AddComponent<SceneObject>();
                            sObj.id = globalID;
                            globalID++;
                            gotHighLod = true;
                        }
                    }
                    if (!gotHighLod)
                    {
                        SceneObject sObj = location.gameObject.AddComponent<SceneObject>();
                        sObj.id = globalID;
                        globalID++;
                    }
                }
                else
                    node.editable = false;
            }
            else if (location.GetComponent<SkinnedMeshRenderer>() != null)
            {
                SkinnedMeshRenderer sRenderer = location.GetComponent<SkinnedMeshRenderer>();
                SceneNodeSkinnedGeo nodeGeo = new SceneNodeSkinnedGeo();
                Material mat = location.GetComponent<Renderer>().sharedMaterial;
#if TRUNK
                // if materials's shader is not standard, add this material to material package. 
                // Currently this will only get the material name and try to load it on client side. If this fails, it will fallback to Standard.
                nodeGeo.materialId = processMaterial(location.GetComponent<Renderer>().sharedMaterial);
#endif

                nodeGeo.color = new float[4] { 0, 0, 0, 0 };
                nodeGeo.geoId = processGeometry(sRenderer.sharedMesh);
                nodeGeo.rootBoneDagPath = vpet.Extensions.getPathString(sRenderer.rootBone, scene);
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

                List<string> skinnedMeshBones = new List<string>();
                foreach (Transform t in sRenderer.bones)
                    skinnedMeshBones.Add(Extensions.getPathString(t, scene));
                foreach (string s in skinnedMeshBones)
                {
                    nodeGeo.skinnedMeshBonesArray += s;
                    if (s != skinnedMeshBones[skinnedMeshBones.Count - 1])
                        nodeGeo.skinnedMeshBonesArray += '\r';
                }
                Debug.Log("nodeGeo.skinnedMeshBonesArray.length:" + nodeGeo.skinnedMeshBonesArray.Length.ToString());

                if (sRenderer.material != null)
                {
                    mat = sRenderer.material;

                    if (mat.HasProperty("_Color"))
                    {
                        nodeGeo.color = new float[4] { mat.color.r, mat.color.g, mat.color.b, mat.color.a };
                    }

                    if (mat.HasProperty("_Glossiness"))
                        nodeGeo.roughness = mat.GetFloat("_Glossiness");

                    if (mat.mainTexture != null)
                    {
                        Texture2D mainTex = (Texture2D)mat.mainTexture;
                        nodeGeo.textureId = processTexture(mainTex);
                    }
                    else
                    {
                        nodeGeo.textureId = -1;
                    }
                }
                else
                {
                    nodeGeo.textureId = -1;
                }

                node = nodeGeo;

                if (location.gameObject.tag == "editable")
                {
                    node.editable = true;
                    bool gotHighLod = false;
                    foreach (Transform child in location.parent)
                    {
                        if (child.name == location.name && child.gameObject.layer == lodHighLayer)
                        {
                            SceneObject sObj = child.gameObject.AddComponent<SceneObject>();
                            sObj.id = globalID;
                            globalID++;
                            gotHighLod = true;
                        }
                    }
                    if (!gotHighLod)
                    {
                        SceneObject sObj = location.gameObject.AddComponent<SceneObject>();
                        sObj.id = globalID;
                        globalID++;
                    }
                }
                else
                    node.editable = false;
            }
            else if (location.gameObject.tag == "editable")
            {
                node.editable = true;
                SceneObject sObj = location.gameObject.AddComponent<SceneObject>();
                sObj.id = globalID;
                globalID++;
            }


            Animator animator = location.GetComponent<Animator>();

            if (animator != null)
            {
                animator.logWarnings = false;
                processCharacter(animator);
            }

            //if (location.gameObject.tag == "editable")
            //{
            //    node.editable = true;
            //    SceneObject sObj = location.gameObject.AddComponent<SceneObject>();
            //    sObj.id = globalID;
            //    globalID++;
            //}
            //else
            //    node.editable = false;

            node.position = new float[3] { location.localPosition.x, location.localPosition.y, location.localPosition.z };
            node.scale = new float[3] { location.localScale.x, location.localScale.y, location.localScale.z };
            node.rotation = new float[4] { location.localRotation.x, location.localRotation.y, location.localRotation.z, location.localRotation.w };
            node.name = new byte[256];
            byte[] tmpName = Encoding.ASCII.GetBytes(location.name);
            for (int i = 0; i < tmpName.Length; i++)
            {
                node.name[i] = tmpName[i];
            }

            if (location.name != "root")
            {
                // print("Added: " + location.name);
                nodeList.Add(node);
            }

            // recursive children
            int childCounter = 0;
            if (location.childCount > 0)
            {
                foreach (Transform child in location)
                {
                    if (!child.gameObject.activeSelf)
                    {
                        continue;
                    }
                    if (iterLocation(child))
                    {
                        childCounter++;
                    }
                }
            }
            node.childCount = childCounter;

            if (doAssignSceneObjects)
            {
                if (location.gameObject.tag == "editable")
                {
#if UNITY_EDITOR
                    // add recorder
                    if (sceneName != "" && shotName != "" && takeName != "")
                    {
                        UnityAnimationRecorder animRecorder = location.gameObject.AddComponent<UnityAnimationRecorder>();
                        animRecorder.savePath = recordPath;
                        animRecorder.fileName = String.Format("{0}_{1}_{2}_{3}", sceneName, shotName, takeName, location.name);
                        animRecorder.showLogGUI = true;
                    }
#endif
                }
                else if (location.GetComponent<Light>() != null)
                {
                    // Add light prefab
                    GameObject lightUber = Resources.Load<GameObject>("VPET/Prefabs/UberLight");
                    GameObject _lightUberInstance = Instantiate(lightUber);
                    _lightUberInstance.name = lightUber.name;
                    _lightUberInstance.transform.SetParent(location, false);

                    SceneNodeLight nodeLight = (SceneNodeLight)Convert.ChangeType(node, typeof(SceneNodeLight));
                    Light lightComponent = _lightUberInstance.GetComponent<Light>();
                    lightComponent.type = nodeLight.lightType;
                    lightComponent.color = new Color(nodeLight.color[0], nodeLight.color[1], nodeLight.color[2]);
                    lightComponent.intensity = nodeLight.intensity * VPETSettings.Instance.lightIntensityFactor;
                    lightComponent.spotAngle = Mathf.Min(150, nodeLight.angle);
                    lightComponent.range = nodeLight.range;

                    location.GetComponent<Light>().enabled = false;

#if UNITY_EDITOR
                    // add recorder
                    if (sceneName != "" && shotName != "" && takeName != "")
                    {
                        UnityAnimationRecorder animRecorder = location.gameObject.AddComponent<UnityAnimationRecorder>();
                        animRecorder.savePath = recordPath;
                        animRecorder.fileName = String.Format("{0}_{1}_{2}_{3}", sceneName, shotName, takeName, location.name);
                        animRecorder.showLogGUI = true;
                    }
#endif


                }
                else if (location.GetComponent<Camera>() != null)
                {
                    // add camera dummy mesh
                    GameObject cameraObject = Resources.Load<GameObject>("VPET/Prefabs/cameraObject");
                    GameObject cameraInstance = Instantiate(cameraObject);
                    cameraInstance.SetActive(false);
                    cameraInstance.name = cameraObject.name;
                    cameraInstance.transform.SetParent(location.transform, false);
                    cameraInstance.transform.localScale = new Vector3(1, 1, 1);
                    cameraInstance.transform.localPosition = new Vector3(0, 0, -0.5f);

#if UNITY_EDITOR
                    // add recorder
                    if (sceneName != "" && shotName != "" && takeName != "")
                    {
                        UnityAnimationRecorder animRecorder = location.gameObject.AddComponent<UnityAnimationRecorder>();
                        animRecorder.savePath = recordPath;
                        animRecorder.fileName = String.Format("{0}_{1}_{2}_{3}", sceneName, shotName, takeName, location.name);
                        animRecorder.showLogGUI = true;
                    }
#endif

                }
            }
            return true;
        }

        private int processGeometry(Mesh mesh)
        {

            for (int i = 0; i < objectList.Count; i++)
            {
                if (objectList[i].mesh == mesh)
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
            objectList.Add(objPack);

            return objectList.Count - 1;
        }

        private void processCharacter(Animator animator)
        {
            CharacterPackage chrPack = new CharacterPackage();
            chrPack.rootDag = vpet.Extensions.getPathString(animator.transform, scene);
            chrPack.rootDagSize = chrPack.rootDag.Length;

            HumanBone[] boneArray = animator.avatar.humanDescription.human;
            chrPack.bMSize = Enum.GetNames(typeof(HumanBodyBones)).Length;
            chrPack.boneMapping = "";
            string []boneMapping = new String[chrPack.bMSize];

            for (int i = 0; i < boneArray.Length; i++)
            {
                if (boneArray[i].boneName != null)
                {
                    string enumName = boneArray[i].humanName.Replace(" ", "");
                    HumanBodyBones enumNum;
                    Enum.TryParse<HumanBodyBones>(enumName, true, out enumNum);
                    Transform boneTransform = vpet.Extensions.FindDeepChild(animator.transform, boneArray[i].boneName);
                    boneMapping[(int)enumNum] = vpet.Extensions.getPathString(boneTransform, scene);
                }
            }
            foreach (String pathString in boneMapping)
            {
                chrPack.boneMapping += pathString + "\n";
            }
            chrPack.boneMapping = chrPack.boneMapping.Remove(chrPack.boneMapping.Length-1);
            chrPack.mdagSize = chrPack.boneMapping.Length;

            SkeletonBone[] skeletonArray = animator.avatar.humanDescription.skeleton;
            chrPack.sSize = skeletonArray.Length;
            chrPack.skeletonMapping = "";
            chrPack.bonePosition = new float[chrPack.sSize * 3];
            chrPack.boneRotation = new float[chrPack.sSize * 4];
            chrPack.boneScale = new float[chrPack.sSize * 3];

            for (int i = 0; i < skeletonArray.Length; i++)
            {
                chrPack.skeletonMapping += skeletonArray[i].name + "\n";

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
            chrPack.skeletonMapping = chrPack.skeletonMapping.Remove(chrPack.skeletonMapping.Length - 1);
            chrPack.sdagSize = chrPack.skeletonMapping.Length;

            characterList.Add(chrPack);
        }


        private int processTexture(Texture2D texture)
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

#if TRUNK
        private int processMaterial(Material mat)
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
                print("mat tyoe" + 1 + " material " + mat.name);
                matPack.type = 1;
                matPack.src = matName;
            }
            else
            {
                print("mat tyoe" + 2 + " shader " + mat.shader.name);
                matPack.type = 2;
                matPack.src = mat.shader.name;
            }


            materialList.Add(matPack);
            return materialList.Count - 1;
        }
#endif

        private void getNodesByteArray()
        {
            nodesByteData = new byte[0];
            foreach (SceneNode node in nodeList)
            {
                byte[] nodeBinary;
                byte[] nodeTypeBinary;
                byte[] nodeLengthBinary;
                if (node.GetType() == typeof(SceneNodeGeo))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GEO);
                    SceneNodeGeo nodeGeo = (SceneNodeGeo)Convert.ChangeType(node, typeof(SceneNodeGeo));
                    nodeBinary = SceneDataHandler.StructureToByteArray(nodeGeo);
                }
                else if (node.GetType() == typeof(SceneNodeSkinnedGeo))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.SKINNEDMESH);
                    SceneNodeSkinnedGeo nodeskinnedGeo = (SceneNodeSkinnedGeo)Convert.ChangeType(node, typeof(SceneNodeSkinnedGeo));
                    nodeBinary = SceneDataHandler.StructureToByteArray(nodeskinnedGeo);
                }
                else if (node.GetType() == typeof(SceneNodeLight))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.LIGHT);
                    SceneNodeLight nodeLight = (SceneNodeLight)Convert.ChangeType(node, typeof(SceneNodeLight));
                    nodeBinary = SceneDataHandler.StructureToByteArray(nodeLight);
                }
                else if (node.GetType() == typeof(SceneNodeCam))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.CAMERA);
                    SceneNodeCam nodeCam = (SceneNodeCam)Convert.ChangeType(node, typeof(SceneNodeCam));
                    nodeBinary = SceneDataHandler.StructureToByteArray(nodeCam);
                }
                else
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GROUP);
                    nodeBinary = SceneDataHandler.StructureToByteArray(node);
                }
                nodeLengthBinary = BitConverter.GetBytes(nodeBinary.Length);

                // concate arrays
                nodesByteData = SceneDataHandler.Concat<byte>(nodesByteData, nodeLengthBinary);
                nodesByteData = SceneDataHandler.Concat<byte>(nodesByteData, nodeTypeBinary);
                nodesByteData = SceneDataHandler.Concat<byte>(nodesByteData, nodeBinary);
            }
        }

        private void getObjectsByteArray()
        {
            objectsByteData = new byte[0];

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
                objectsByteData = SceneDataHandler.Concat<byte>(objectsByteData, objByteData);

            }
        }

        private void getCharacterByteArray()
        {
            charactersByteData = new byte[0];
            foreach (CharacterPackage chrPack in characterList)
            {
                byte[] characterByteData = new byte[SceneDataHandler.size_int * 3 +
                                                chrPack.rootDag.Length +
                                                /* dagSizes */ + SceneDataHandler.size_int +
                                                chrPack.boneMapping.Length +
                                                /* sdagSizes */ + SceneDataHandler.size_int +
                                                chrPack.skeletonMapping.Length +
                                                chrPack.sSize * SceneDataHandler.size_float * 10];
                int dstIdx = 0;
                // bone mapping size
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.bMSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // skeleton mapping size
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.sSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // root dag size
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.rootDagSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // root dag path
                byte[] nameByte = Encoding.ASCII.GetBytes(chrPack.rootDag);
                Buffer.BlockCopy(nameByte, 0, characterByteData, dstIdx, chrPack.rootDag.Length);
                dstIdx += chrPack.rootDag.Length;

                // m dag size
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.mdagSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // bone Mapping
                nameByte = Encoding.ASCII.GetBytes(chrPack.boneMapping);
                Buffer.BlockCopy(nameByte, 0, characterByteData, dstIdx, chrPack.boneMapping.Length);
                dstIdx += chrPack.boneMapping.Length;
                //foreach (string dag in chrPack.boneMapping)
                //{
                //    if (dag != null)
                //    {
                //        byte[] dagByte = Encoding.ASCII.GetBytes(dag);
                //        Buffer.BlockCopy(dagByte, 0, characterByteData, dstIdx, dag.Length);
                //        dstIdx += dag.Length;
                //    }
                //}

                // s dag size
                Buffer.BlockCopy(BitConverter.GetBytes(chrPack.sdagSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;

                // skeleton Mapping
                nameByte = Encoding.ASCII.GetBytes(chrPack.skeletonMapping);
                Buffer.BlockCopy(nameByte, 0, characterByteData, dstIdx, chrPack.skeletonMapping.Length);
                dstIdx += chrPack.skeletonMapping.Length;
                //foreach (string sdag in chrPack.skeletonMapping)
                //{
                //    if (sdag != null)
                //    {
                //        byte[] sdagByte = Encoding.ASCII.GetBytes(sdag);
                //        Buffer.BlockCopy(sdagByte, 0, characterByteData, dstIdx, sdag.Length);
                //        dstIdx += sdag.Length;
                //    }
                //}

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
                charactersByteData = SceneDataHandler.Concat<byte>(charactersByteData, characterByteData);
            }
        }

        private void getTexturesByteArray()
        {
            texturesByteData = BitConverter.GetBytes(textureBinaryType);

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
                texturesByteData = SceneDataHandler.Concat<byte>(texturesByteData, texByteData);
            }
        }

#if TRUNK
        private void getMaterialsByteArray()
        {
            materialsByteData = new byte[0];

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
                materialsByteData = SceneDataHandler.Concat<byte>(materialsByteData, matByteData);

            }
        }
#endif


        private void writeBinary(byte[] data, string dataname)
        {
            string filesrc = "Assets/Resources/VPET/SceneDumps/" + sceneFileName + "_" + dataname + ".bytes"; ;
            print("Write binary data: " + filesrc);
            BinaryWriter writer = new BinaryWriter(File.Open(filesrc, FileMode.Create));
            writer.Write(data);
            writer.Close();
        }

        private void OnApplicationQuit()
        {
            isRunning = false;

            serverThread.Join();
            //NetMQConfig.Cleanup();
            serverThread.Abort();
        }


    }
}
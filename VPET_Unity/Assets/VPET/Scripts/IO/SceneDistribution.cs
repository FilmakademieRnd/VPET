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
        private List<TexturePackage> textureList;
        private Thread serverThread;
        private bool isRunning = false;

        private byte[] headerByteData;
        private byte[] nodesByteData;
        private byte[] objectsByteData;
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

        private void Awake()
        {
            if (sceneRoot == null)
                sceneRoot = GameObject.Find("root");

            if (sceneRoot == null) Debug.LogError(string.Format("{0}: Cant find Scene Root: 'root'.", this.GetType()));

            lodLowLayer = LayerMask.NameToLayer("LodLow");
            lodHighLayer = LayerMask.NameToLayer("LodHigh");
            lodMixedLayer = LayerMask.NameToLayer("LodMixed");

        }

        // Use this for initialization
        void Start()
        {
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
            using (var dataSender = new ResponseSocket ()) {
                dataSender.Bind ("tcp://*:5565");
                Debug.Log ("Enter while.. ");

                while (isRunning) {
                    string message = dataSender.ReceiveFrameString ();
                    print ("Got request message: " + message);

                    // re-run scene iteration if true
                    if (doGatherOnRequest)
                        gatherSceneData ();

                    switch (message) {
                    case "header":
                        print ("Send Header.. ");
                        dataSender.SendFrame (headerByteData);
                        print (string.Format (".. Nodes ({0} bytes) sent ", headerByteData.Length));
                        break;
                    case "nodes":
                        print ("Send Nodes.. ");
                        dataSender.SendFrame (nodesByteData);
                        print (string.Format (".. Nodes ({0} bytes) sent ", nodesByteData.Length));
                        break;
                    case "objects":
                        print ("Send Objects.. ");
                        dataSender.SendFrame (objectsByteData);
                        print (string.Format (".. Objects ({0} bytes) sent ", objectsByteData.Length));
                        break;
                    case "textures":
                        print ("Send Textures.. ");
                        dataSender.SendFrame (texturesByteData);
                        print (string.Format (".. Textures ({0} bytes) sent ", texturesByteData.Length));
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

                dataSender.Unbind ("tcp://127.0.0.1:5565");
                dataSender.Close();
                dataSender.Dispose();
            }
            //NetMQConfig.Cleanup();
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
            textureList = new List<TexturePackage>();
#if TRUNK
            materialList = new List<MaterialPackage>();
#endif

            iterLocation(sceneRoot.transform);

            Debug.Log(string.Format("{0}: Collected number nodes: {1}", this.GetType(), nodeList.Count));
            Debug.Log(string.Format("{0}: Collected number objects: {1}", this.GetType(), objectList.Count));
            Debug.Log(string.Format("{0}: Collected number textures: {1}", this.GetType(), textureList.Count));
#if TRUNK
            Debug.Log(string.Format("{0}: Collected number materials: {1}", this.GetType(), materialList.Count));
#endif
            // create byte arrays
            headerByteData = SceneDataHandler.StructureToByteArray<VpetHeader>(vpetHeader);
            getNodesByteArray();
            getObjectsByteArray();
            getTexturesByteArray();
#if TRUNK
            getMaterialsByteArray();
#endif
            Debug.Log(string.Format("{0}: HeaderByteArray size: {1}", this.GetType(), headerByteData.Length));
            Debug.Log(string.Format("{0}: NodeByteArray size: {1}", this.GetType(), nodesByteData.Length));
            Debug.Log(string.Format("{0}: ObjectsByteArray size: {1}", this.GetType(), objectsByteData.Length));
            Debug.Log(string.Format("{0}: TexturesByteArray size: {1}", this.GetType(), texturesByteData.Length));
#if TRUNK
            Debug.Log(string.Format("{0}: MaterialsByteArray size: {1}", this.GetType(), materialsByteData.Length));
#endif
        }

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
            }
			else if (location.GetComponent<Camera>() != null)
			{
				SceneNodeCam nodeCamera = new SceneNodeCam();

				Camera camera = location.GetComponent<Camera>();
				nodeCamera.fov = camera.fieldOfView;
				nodeCamera.near = camera.nearClipPlane;
				nodeCamera.far = camera.farClipPlane;
                node = nodeCamera;
			}
            else if (location.GetComponent<MeshFilter>() != null)
            {
                
                SceneNodeGeo nodeGeo = new SceneNodeGeo(); 
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
                        nodeGeo.color = new float[3] { mat.color.r, mat.color.g, mat.color.b };
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
            }
            else if (location.GetComponent<SkinnedMeshRenderer>() != null)
            {
                SceneNodeGeo nodeGeo = new SceneNodeGeo();
                nodeGeo.geoId = processGeometry(location.GetComponent<SkinnedMeshRenderer>().sharedMesh);

                if (location.GetComponent<SkinnedMeshRenderer>().material != null)
                {
                    Material mat = location.GetComponent<SkinnedMeshRenderer>().material;

                    if (mat.HasProperty("_Color"))
                    {
                        nodeGeo.color = new float[3] { mat.color.r, mat.color.g, mat.color.b };
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
            }
            else if ( location.parent.GetComponent<AnimatorObject>() != null )
            {
                SceneNodeMocap nodeMocap = new SceneNodeMocap();
                node = nodeMocap;
            }


            node.editable = (location.gameObject.tag == "editable");
            node.position = new float[3] { location.localPosition.x, location.localPosition.y, location.localPosition.z };
            node.scale = new float[3] { location.localScale.x, location.localScale.y, location.localScale.z };
            node.rotation = new float[4] { location.localRotation.x, location.localRotation.y, location.localRotation.z, location.localRotation.w };
            node.name = Encoding.ASCII.GetBytes(location.name);

            // print("Added: " + location.name);
            nodeList.Add(node);




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
	                SceneObjectServer scnObj = location.gameObject.AddComponent<SceneObjectServer>();
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

                    SceneObjectServer scnObj = location.gameObject.AddComponent<SceneObjectServer>();
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

                    SceneObjectServer scnObj = location.gameObject.AddComponent<SceneObjectServer>();
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
            
            for ( int i=0; i<objectList.Count; i++)
            {
                if (objectList[i].mesh == mesh)
                {
                    return i;
                }
            }
            
            ObjectPackage objPack = new ObjectPackage();

            // vertices, normals, uvs
            objPack.vSize = mesh.vertexCount;
            objPack.nSize = mesh.normals.Length;
            objPack.uvSize = mesh.uv.Length;
            objPack.vertices = new float[objPack.vSize * 3];
            objPack.normals = new float[objPack.nSize * 3];
            objPack.uvs = new float[objPack.uvSize * 2];

            Vector3[] mVertices = mesh.vertices;
            Vector3[] mNormals = mesh.normals;
            Vector2[] mUV = mesh.uv;

            // v
            for (int i = 0; i < objPack.vSize; i++)
            {
                objPack.vertices[i*3 + 0] = mVertices[i].x;
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

            // indices
            objPack.iSize = mesh.triangles.Length;
            objPack.indices = mesh.triangles;

            //print("Vertice Count: " + objPack.vertices.Length + " indice count: " + objPack.iSize + " normalscount: " + objPack.normals.Length + " uvs count: " + objPack.uvs.Length);

            objPack.mesh = mesh;

            objectList.Add(objPack);

            return objectList.Count - 1;
        }


        private int processTexture( Texture2D texture)
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
                print("mat tyoe" + 1+ " material " + mat.name);
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
                if (node.GetType() == typeof(SceneNodeGeo))
                {
                    nodeTypeBinary =  BitConverter.GetBytes((int)NodeType.GEO);
                    SceneNodeGeo nodeGeo = (SceneNodeGeo)Convert.ChangeType(node, typeof(SceneNodeGeo));
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeGeo>(nodeGeo);
                }
                else if (node.GetType() == typeof(SceneNodeLight))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.LIGHT);
                    SceneNodeLight nodeLight = (SceneNodeLight)Convert.ChangeType(node, typeof(SceneNodeLight));
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeLight>(nodeLight);
                }
                else if (node.GetType() == typeof(SceneNodeCam))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.CAMERA);
                    SceneNodeCam nodeCam = (SceneNodeCam)Convert.ChangeType(node, typeof(SceneNodeCam));
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeCam>(nodeCam);
                }
                else if (node.GetType() == typeof(SceneNodeMocap))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.MOCAP);
                    SceneNodeMocap nodeMocap = (SceneNodeMocap)Convert.ChangeType(node, typeof(SceneNodeMocap));
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeMocap>(nodeMocap);
                }
                else
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GROUP);
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNode>(node);
                }
                // concate arrays
                nodesByteData = SceneDataHandler.Concat<byte>(nodesByteData, nodeTypeBinary);
                nodesByteData = SceneDataHandler.Concat<byte>(nodesByteData, nodeBinary);
            }
        }

        private void getObjectsByteArray()
        {
            objectsByteData = new byte[0];

            foreach (ObjectPackage objPack in objectList)
            {
                byte[] objByteData = new byte[4 * SceneDataHandler.size_int + 
                                                    objPack.vSize*3 * SceneDataHandler.size_float + 
                                                    objPack.iSize * SceneDataHandler.size_int + 
                                                    objPack.nSize*3 * SceneDataHandler.size_float + 
                                                    objPack.uvSize*2 * SceneDataHandler.size_float];
                int dstIdx = 0;
                // vertices
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.vSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.vertices, 0, objByteData, dstIdx, objPack.vSize*3 * SceneDataHandler.size_float);
                dstIdx += objPack.vSize*3 * SceneDataHandler.size_float;
                // indices
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.iSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.indices, 0, objByteData, dstIdx, objPack.iSize * SceneDataHandler.size_int);
                dstIdx += objPack.iSize * SceneDataHandler.size_int;
                // normals
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.nSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.normals, 0, objByteData, dstIdx, objPack.nSize*3 * SceneDataHandler.size_float);
                dstIdx += objPack.nSize*3 * SceneDataHandler.size_float;
                // uvs
                Buffer.BlockCopy(BitConverter.GetBytes(objPack.uvSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                dstIdx += SceneDataHandler.size_int;
                Buffer.BlockCopy(objPack.uvs, 0, objByteData, dstIdx, objPack.uvSize*2 * SceneDataHandler.size_float);
                dstIdx += objPack.uvSize*2 * SceneDataHandler.size_float;

                // concate
                objectsByteData = SceneDataHandler.Concat<byte>(objectsByteData, objByteData);

            }
        }

        private void getTexturesByteArray()
        {
            texturesByteData = BitConverter.GetBytes(textureBinaryType);

            foreach (TexturePackage texPack in textureList)
            {
                byte[] texByteData = new byte[4*SceneDataHandler.size_int + texPack.colorMapDataSize];
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
            //NetMQConfig.Cleanup();
            serverThread.Abort();
        }


    }
}
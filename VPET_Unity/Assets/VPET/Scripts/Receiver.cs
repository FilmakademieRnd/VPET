/*
 * using NetMQ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Runtime.InteropServices;
using System;

using vpet;




public class Receiver : MonoBehaviour
{

    Thread receiverThread;
    List<ObjectPackage> objectList;
    List<TexturePackage> textureList;
    bool sceneTransferDirty = false;
    // Use this for initialization
    void Start ()
    {
        objectList = new List<ObjectPackage>();
        textureList = new List<TexturePackage>();
    }

    // Update is called once per frame
    void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Space))
        {
            if (receiverThread == null )
            {
                print("Start receiver thread");
                receiverThread = new Thread(new ThreadStart(dataReceiver));
                receiverThread.Start();
            }
        }

        if ( sceneTransferDirty )
        {

            foreach (ObjectPackage objPack in objectList)
            {

                Vector3[] vertices = new Vector3[objPack.vSize];
                Vector3[] normals = new Vector3[objPack.nSize];
                Vector2[] uv = new Vector2[objPack.uvSize];

                print("Vertice Count: " + vertices.Length + " indice count: " + objPack.indices.Length + " normalscount: " + normals.Length + " uvs count: " + uv.Length);
                print("Vertice Count: " + objPack.vertices.Length + " indice count: " + objPack.indices.Length + " normalscount: " + objPack.normals.Length + " uvs count: " + objPack.uvs.Length);

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 v = new Vector3(objPack.vertices[i * 3 + 0], objPack.vertices[i * 3 + 1], objPack.vertices[i * 3 + 2]);
                    vertices[i] = v;
                    v = new Vector3(objPack.normals[i * 3 + 0], objPack.normals[i * 3 + 1], objPack.normals[i * 3 + 2]);
                    normals[i] = v;
                    Vector2 v2 = new Vector2(objPack.uvs[i * 2 + 0], objPack.uvs[i * 2 + 1]);
                    uv[i] = v2;
                }


                Mesh mesh = new Mesh();
                mesh.Clear();
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = uv;
                mesh.triangles = objPack.indices;

                GameObject objMain = new GameObject();
                objMain.name = "OHA";
                MeshRenderer meshRenderer = objMain.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Standard"));


                objMain.AddComponent<MeshFilter>();
                objMain.GetComponent<MeshFilter>().mesh = mesh;
                
            }
            sceneTransferDirty = false;

        }

	}


    private void dataReceiver()
    {
        NetMQContext ctx = NetMQContext.Create();

        NetMQ.Sockets.RequestSocket dataReceiver = ctx.CreateRequestSocket();

        dataReceiver.Connect("tcp://127.0.0.1:5565");

        print("Send request message: requestData");
        dataReceiver.Send("objects");
        byte[] byteStream = dataReceiver.Receive() as byte[];
        print("byteStreamNodes size: " + byteStream.Length);
    
        
        int dataIdx = 0;
        while (dataIdx < byteStream.Length - 1)
        {
            ObjectPackage objPack = new ObjectPackage();

            // get vertices
            int numValues = BitConverter.ToInt32(byteStream, dataIdx);
            dataIdx += SceneDataHandler.size_int;
            objPack.vSize = numValues;
            objPack.vertices = new float[numValues*3];
            for (int i = 0; i < numValues*3; i++)
            {
                objPack.vertices[i] = BitConverter.ToSingle(byteStream, dataIdx);
                dataIdx += SceneDataHandler.size_float;
            }


            // get indices
            numValues = BitConverter.ToInt32(byteStream, dataIdx);
            dataIdx += SceneDataHandler.size_int;
            objPack.iSize = numValues;
            objPack.indices = new int[numValues];
            for (int i = 0; i < numValues; i++)
            {
                objPack.indices[i] = BitConverter.ToInt32(byteStream, dataIdx);
                dataIdx += SceneDataHandler.size_int;
            }

            // get normals
            numValues = BitConverter.ToInt32(byteStream, dataIdx);
            dataIdx += SceneDataHandler.size_int;
            objPack.nSize = numValues;
            objPack.normals = new float[numValues*3];
            for (int i = 0; i < numValues*3; i++)
            {
                objPack.normals[i] = BitConverter.ToSingle(byteStream, dataIdx);
                dataIdx += SceneDataHandler.size_float;
            }

            // get uvs
            numValues = BitConverter.ToInt32(byteStream, dataIdx);
            dataIdx += SceneDataHandler.size_int;
            objPack.uvSize = numValues;
            objPack.uvs = new float[numValues*2];
            for (int i = 0; i < numValues*2; i++)
            {
                objPack.uvs[i] = BitConverter.ToSingle(byteStream, dataIdx);
                dataIdx += SceneDataHandler.size_float;
            }

            objectList.Add(objPack);

        }



        dataReceiver.Send("textures");
        byteStream = dataReceiver.Receive() as byte[];
        print("byteStream size: " + byteStream.Length);

        dataIdx = 0;
        while (dataIdx < byteStream.Length - 1)
        {
            TexturePackage texPack = new TexturePackage();

            // get texture raw data
            int numValues = BitConverter.ToInt32(byteStream, dataIdx);
            dataIdx += SceneDataHandler.size_int;
            texPack.colorMapDataSize = numValues;
            texPack.colorMapData = new byte[numValues];
            Buffer.BlockCopy(byteStream, dataIdx, texPack.colorMapData, 0, numValues);
            dataIdx += numValues;

            print("texPack.colorMapDataSize: " + texPack.colorMapDataSize);

            textureList.Add(texPack);
        }
        /*
        int dataIdx = 0;
        while (dataIdx < byteStreamNodes.Length - 1)
        {
            byte[] sliceInt = new byte[size_int];
            Array.Copy(byteStreamNodes, dataIdx, sliceInt, 0, size_int);
            //checkEndian(ref sliceInt);
            NodeType nodeType = (NodeType)BitConverter.ToInt32(sliceInt, 0);
            dataIdx += size_int;

            switch (nodeType)
            {
                case NodeType.GROUP:
                    SceneNode myData1 = Receiver.ByteArrayToStructure<SceneNode>(byteStreamNodes, ref dataIdx);
                    print("Group");
                    break;
                case NodeType.GEO:
                    SceneNodeGeo myData = Receiver.ByteArrayToStructure<SceneNodeGeo>(byteStreamNodes, ref dataIdx);
                    print("Geo");
                    break;
                case NodeType.LIGHT:
                    SceneNodeLight myDataLight = Receiver.ByteArrayToStructure<SceneNodeLight>(byteStreamNodes, ref dataIdx);
                    print("LIGHT " + myDataLight.angle);
                    break;
                case NodeType.CAMERA:
                    SceneNodeCam myDataCam = Receiver.ByteArrayToStructure<SceneNodeCam>(byteStreamNodes, ref dataIdx);
                    print("CAMERA");
                    break;
            }
        }
        /


        dataReceiver.Disconnect("tcp://127.0.0.1:5565");
        dataReceiver.Close();

        sceneTransferDirty = true;

    }


    private void OnApplicationQuit()
    {
        receiverThread.Abort();
    }

}
*/
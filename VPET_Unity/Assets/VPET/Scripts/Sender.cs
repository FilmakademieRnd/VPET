/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using NetMQ;
using System.Runtime.InteropServices;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using vpet;

public class Sender : MonoBehaviour {

    Thread serverThread;
    bool isRunning = true;
    byte[] byteData;
    SceneNode myData;

	// Use this for initialization
	void Start ()
    {
        myData = new SceneNode();
        //myData.A = 23;
        //myData.B = "Gestern";
        //myData.C = gameObject.AddComponent<Light>();

        byteData = StructureToByteArray2(myData);

        print("Byte len: " + byteData.Length);

        serverThread = new Thread(new ThreadStart(dataServer));
        serverThread.Start();
	}
	


    private void dataServer()
    {
        NetMQContext ctx = NetMQContext.Create();

        NetMQ.Sockets.ResponseSocket dataSender = ctx.CreateResponseSocket();
        dataSender.Bind("tcp://127.0.0.1:5565");

        while (isRunning)
        {
            string message = dataSender.ReceiveString();
            print("Got request message: " + message);


            print("Send Data");
            dataSender.Send( byteData);
            print("Data sent");
        }

        dataSender.Unbind("tcp://127.0.0.1:5565");
        dataSender.Close();

    }

    private void OnApplicationQuit()
    {
        serverThread.Abort();
    }



    private byte[] StructureToByteArray(object obj)
    {
        int len = Marshal.SizeOf(obj);
        print(len);
        byte[] arr = new byte[len];

        IntPtr ptr = Marshal.AllocHGlobal(len);
        print("Allocated");

        Marshal.StructureToPtr(obj, ptr, false);
        print("StructToPtr");

        Marshal.Copy(ptr, arr, 0, len);
        print("Copied");

        Marshal.FreeHGlobal(ptr);
        print("Free Willy");

        return arr;
    }


    private byte[] StructureToByteArray2( object src)
    {
        var formatter = new BinaryFormatter();
        using (var stream = new MemoryStream())
        {
            formatter.Serialize(stream, src);
            return stream.ToArray();
        }

    }

}


*/
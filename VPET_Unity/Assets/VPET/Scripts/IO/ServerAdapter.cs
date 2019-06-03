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
using UnityEngine.Events;
using System.Collections;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using NetMQ;
using NetMQ.Sockets;

using System.Collections.Generic;

//!
//! adapter script handling all communication to network partners
//! receives and sends messages to the synchronization server
//! sends messages to katana server
//!

namespace vpet
{

    public class ServerAdapterProgressEvent : UnityEvent<float, string> { }

    public class ServerAdapter : MonoBehaviour
    {

#if SCENE_HOST
        public string hostIP = "192.168.161.100";
#endif

        //!
        //! unique id of client instance
        //!
        byte m_id;

        //!
        //! cached reference to Katana massage templates
        //!
        KatanaTemplates katanaTemplates;


        [HideInInspector]
        //!
        //! enable/disable receiving of ncam data
        //!
        public bool receiveNcam = false;

        //!
        //! timeout for receiving messages (on seconds)
        //!
        public static float receiveTimeout = 5.0f;

        //!
        //! currently locked object
        //!
        Transform currentlyLockedObject = null;

        //!
        //! is Application running or should threads stop working
        //!
        bool isRunning = false;

        //!
        //! deactivate receiving messages
        //!
        public bool deactivateReceive = false;
        //!
        //! deactivate sending messages to tablet syncronizing server
        //!
        public bool deactivatePublish = false;
        //!
        //! deactivate sending messages to katana server
        //!
        public bool deactivatePublishKatana = false;

        //!
        //! flag for writing scene to scene cache
        //!
        public bool doWriteScene = false;
        //!
        //! file path of scene cache
        //!
        public string sceneFileName = "tmp";
        //!
        //! none
        //!
        public bool doAutostartListener = false;

#if SCENE_HOST
        //!
        //! file path to file for storing recorded updates
        //!
        public string recordFileName = "";

        //!
        //! record updates
        //!
        public bool recordUpdates = false;

        //!
        //! Array of updates to be recorder to file
        //!
        public List<GameObject> recordObjects;
#endif

        //!
        //! none
        //!
        private string persistentDataPath;
        public string PersistentDataPath
        {
            set { persistentDataPath = value; }
        }

        //!
        //! do we record
        //!
        public bool isRecording = false;


        //!
        //! regEx expression to check if it is a valid IP adress
        //!
        Regex ipAdressFormat = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

        //!
        //! message queue for incoming messages
        //!
        ArrayList receiveMessageQueue;

        //!
        //! record queue for incoming updates
        //!
        ArrayList recordMessageQueue;

        //!
        //! reference to thread receiving all object updates from tablet syncronizing server
        //!
        Thread receiverThread;

#if !SCENE_HOST
        //!
        //! reference to thread receiving the scene from Katana
        //!
        Thread sceneReceiverThread;
#else
        //!
        //! reference to thread receiving all object updates from tablet syncronizing server
        //!
        Thread recorderThread;
#endif

        //!
        //! system time that the last ncam message was received
        //! used to hide the camera object when no ncam data was received after some time
        //!
        float lastNcamReceivedTime = 0;

        //!
        //! cached reference to the apps scene root object
        //!
        Transform scene;
        //!
        //! cached reference to katanas scene root object
        //!
        Transform dreamspaceRoot;

//#if !SCENE_HOST
        //!
        //! cached reference to main controller
        //!
        MainController mainController = null;
        //#endif
        
        //!
        //! list containing sceneObjects to sceneObject ID references
        //!
        [HideInInspector]
        public SceneObject[] sceneObjectRefList;



        //!
        //! none
        //!
        private SceneLoader sceneLoader;
        public SceneLoader SceneLoader
        {
            set { sceneLoader = value; }
        }


        public ServerAdapterProgressEvent OnProgressEvent = new ServerAdapterProgressEvent();

        // used to pass time to threads
        private float currentTimeTime = 0;

        private float lastReceiveTime = 0;

        private int listenerRestartCount = 0;

        private bool m_sceneTransferDirty = false;

        //!
        //!
        //! 
        private List<Thread> senderThreadList = new List<Thread>();

        //!
        //!
        //! 
        public static List<ObjectSender> objectSenderList = new List<ObjectSender>();

        //!
        //! Register sender objects
        //!
        public static void RegisterSender(ObjectSender sender)
        {
            print("Register " + sender.GetType());

            if (!objectSenderList.Contains(sender))
                objectSenderList.Add(sender);
        }




        void Awake()
        {
#if SCENE_HOST
            VPETSettings.Instance.serverIP = hostIP;

            if (!deactivateReceive && receiverThread == null)
            {
                receiverThread = new Thread(new ThreadStart(listener));
                receiverThread.Start();
                isRunning = true;
            }

            if(recordUpdates)
            {
                recordMessageQueue = new ArrayList();
                recorderThread = new Thread(new ThreadStart(recorder));
                recorderThread.Start();
            }
#else
            if (doAutostartListener)
            {
                VPETSettings.Instance.serverIP = "127.0.0.1";
            }
#endif
        }


        //!
        //! Use this for initialization
        //!
        void Start()
        {
#if SCENE_HOST
            m_id = byte.Parse(hostIP.ToString().Split('.')[3]);
#else

            //reads the network name of the device
            var hostName = Dns.GetHostName();

            var host = Dns.GetHostEntry(hostName);
            m_id = 0;

            //Take last ip adress of local network (which is local wlan ip address)
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    m_id = byte.Parse(ip.ToString().Split('.')[3]);
                }
            }

            //register cam sending function
            InvokeRepeating("sendPing", 0.0f, 2f);

            if (GameObject.Find("MainController") != null)
                mainController = GameObject.Find("MainController").GetComponent<MainController>();
#endif

            persistentDataPath = Application.persistentDataPath;

            print("persistentDataPath: " + persistentDataPath);

            scene = GameObject.Find("Scene").transform;

            sceneObjectRefList = new SceneObject[0];

            receiveMessageQueue = new ArrayList();

            dreamspaceRoot = scene; // GameObject.Find("Scene").transform;
            if (dreamspaceRoot == null) Debug.LogError(string.Format("{0}: Cant Find: Scene.", this.GetType()));
        }


        //!
        //! Init the sever adpater for receiving a scene
        //! This will check the IP, (re)start receiver thread, request progress state
        //!
        public void initServerAdapterTransfer()
        {

            if (!ipAdressFormat.IsMatch(VPETSettings.Instance.serverIP))
            {
                deactivatePublish = true;
                deactivateReceive = true;
                deactivatePublishKatana = true;
            }
#if !SCENE_HOST
            // clear previous scene
            sceneLoader.ResetScene();
#endif

            // if (!ipAdressFormat.IsMatch(VPETSettings.Instance.katanaIP)) deactivatePublishKatana = true;

            //bind Threads to methods & start them
            if (!deactivateReceive && receiverThread == null)
            {
                receiverThread = new Thread(new ThreadStart(listener));
                receiverThread.Start();
                isRunning = true;
            }
            if (!deactivatePublish)
            {
                // create thread for all registered sender
                foreach (ObjectSender sender in objectSenderList)
                {
#if SCENE_HOST
                    sender.SetTarget(hostIP, "5557");
#else
                    sender.SetTarget(VPETSettings.Instance.serverIP, "5557");
#endif
                    Thread _thread = new Thread(new ThreadStart(sender.Publisher));
                    _thread.Start();
                    if (!senderThreadList.Contains(_thread))
                        senderThreadList.Add(_thread);
                    sender.IsRunning = true;
                }
            }

#if !SCENE_HOST
            if (VPETSettings.Instance.doLoadFromResource)
            {
                sceneReceiverThread = null;
                sceneReceiverResource();
            }
            else
            {
                if (sceneReceiverThread != null)
                {
                    sceneReceiverThread.Abort();
                    sceneReceiverThread = null;
                }

                sceneReceiverThread = new Thread(new ThreadStart(sceneReceiver));
                sceneReceiverThread.Start();
            }
#endif
        }



        //!
        //! Update is called once per frame
        //!
        void Update()
        {
#if !SCENE_HOST
            // if we received new objects build them
            if (m_sceneTransferDirty)
            {
                m_sceneTransferDirty = false;
                print("sceneLoader.createSceneGraph");

                Vector3 scenePos = scene.position;
                Quaternion scenRot = scene.rotation;
                scene.rotation = Quaternion.identity;
                scene.position = Vector3.zero;

                sceneLoader.createSceneGraph();
                SendObjectUpdate(null, ParameterType.RESENDUPDATE);

                int refListCount = SceneLoader.SceneEditableObjects.Count + 
                                   SceneLoader.SelectableLights.Count + 
                                   SceneLoader.SceneCameraList.Count;

                sceneObjectRefList = new SceneObject[refListCount];

                foreach (GameObject gameObject in SceneLoader.SceneEditableObjects)
                {
                    Debug.Log(gameObject.name);
                    SceneObject sceneObject = gameObject.GetComponent<SceneObject>();
                    Debug.Log(sceneObject.id + " " + sceneObject.name);
                    if(sceneObjectRefList.Length > sceneObject.id)
                        sceneObjectRefList[sceneObject.id] = sceneObject;
                }

                foreach(GameObject gameObject in SceneLoader.SelectableLights)
                {
                    SceneObject sceneObject = gameObject.GetComponent<SceneObjectLight>();
                    if (sceneObjectRefList.Length > sceneObject.id)
                        sceneObjectRefList[sceneObject.id] = sceneObject;
                }

                foreach (GameObject gameObject in SceneLoader.SceneCameraList)
                {
                    SceneObject sceneObject = gameObject.GetComponent<SceneObjectCamera>();
                    GameObject camGeometry = sceneObject.transform.GetChild(0).gameObject;
                    camGeometry.SetActive(mainController.showCam);
                    if (sceneObjectRefList.Length > sceneObject.id)
                        sceneObjectRefList[sceneObject.id] = sceneObject;
                }

                mainController.SetSceneScale(VPETSettings.Instance.sceneScale);
                // Camera.main.GetComponent<MoveCamera>().calibrate();

                scene.rotation = scenRot;
                scene.position = scenePos;

                mainController.repositionCamera();
            }
#endif

            if (!deactivateReceive)
            {
                //process all available transforms send by server & delete them from Queue
                int count = 0;
                for (int i = 0; i < receiveMessageQueue.Count; i++)
                {
                    // Debug.Log(receiveMessageQueue[i] as string);
                    try
                    {
                        parseTransformation(receiveMessageQueue[i] as byte[]);
                    }
                    catch
                    {
                        VPETSettings.Instance.msg = "Error: parseTransformation";
                    }
                    count++;
                }
                receiveMessageQueue.RemoveRange(0, count);
            }

            currentTimeTime = Time.time;

        }

        //!
        //! sends current ping signal for sync server
        //!
        void sendPing()
        {
            SendObjectUpdate(null, ParameterType.PING);
        }

        //!
        //!
        //!
        public void SendObjectUpdate(SceneObject sobj, ParameterType paramType)
        {
            if (deactivatePublish)
                return;

            foreach (ObjectSender sender in objectSenderList)
            {
#if !SCENE_HOST
                sender.SendObject(m_id, sobj, paramType, (mainController.ActiveMode == MainController.Mode.objectLinkCamera), mainController.oldParent);
#else
                sender.SendObject(m_id, sobj, paramType, false , null);
#endif
            }

        }

        ////! function to be called to send a lock signal to server
        ////! @param  obj             Transform of GameObject to be locked
        ////! @param  locked          should it be locked or unlocked
        //public void sendLock(Transform obj, bool locked)
        //{
        //    if (locked) // lock it
        //    {
        //        if (currentlyLockedObject != null && currentlyLockedObject != obj && !deactivatePublish) // is another object already locked, release it first
        //        {
        //            string msg = "client " + id + "|" + "l" + "|" + this.getPathString(currentlyLockedObject, scene) + "|" + false;
        //            SendObjectUpdate<ObjectSenderBasic>(msg);
        //            // print("Unlock object " + currentlyLockedObject.gameObject.name );
        //        }
        //        if (currentlyLockedObject != obj && !deactivatePublish) // lock the object if it is not locked yet
        //        {
        //            string msg = "client " + id + "|" + "l" + "|" + this.getPathString(obj, scene) + "|" + true;
        //            SendObjectUpdate<ObjectSenderBasic>(msg);
        //            // print("Lock object " + obj.gameObject.name);
        //        }
        //        currentlyLockedObject = obj;
        //    }
        //    else // unlock it
        //    {
        //        if (currentlyLockedObject != null && !deactivatePublish) // unlock if locked
        //        {
        //            string msg = "client " + id + "|" + "l" + "|" + this.getPathString(obj, scene) + "|" + false;
        //            SendObjectUpdate<ObjectSenderBasic>(msg);
        //            // print("Unlock object " + obj.gameObject.name);
        //        }
        //        currentlyLockedObject = null;
        //    }
        //}  // commented at sync update rewrite 


        //! function parsing received message and executing change
        //! @param  message         message string received by server
        public void parseTransformation(byte[] msg)
        {
            if (msg[0] != m_id)
            {
                ParameterType paramType = (ParameterType) msg[1];
                int objectID = BitConverter.ToInt32(msg, 2);
                SceneObject sceneObject = sceneObjectRefList[objectID];

                switch (paramType)
                {
                    case ParameterType.POS:
                        {
                            sceneObject.transform.localPosition = new Vector3(BitConverter.ToSingle(msg, 6), 
                                                                              BitConverter.ToSingle(msg, 10), 
                                                                              BitConverter.ToSingle(msg, 14));
                        }
                        break;

                    case ParameterType.ROT:
                        {
                            sceneObject.transform.localRotation = new Quaternion(BitConverter.ToSingle(msg, 6),
                                                                                 BitConverter.ToSingle(msg, 10),
                                                                                 BitConverter.ToSingle(msg, 14),
                                                                                 BitConverter.ToSingle(msg, 18));
                        }
                        break;
                    case ParameterType.SCALE:
                        {
                            sceneObject.transform.localScale = new Vector3(BitConverter.ToSingle(msg, 6),
                                                                           BitConverter.ToSingle(msg, 10),
                                                                           BitConverter.ToSingle(msg, 14));
                        }
                        break;
                    case ParameterType.LOCK:
                        {
                            bool locked = BitConverter.ToBoolean(msg, 6);
                            sceneObject.enableRigidbody(!locked); 
                            sceneObject.locked = locked;
#if !SCENE_HOST
                            sceneObject.updateLockView();
#endif
                        }
                        break;
                    case ParameterType.HIDDENLOCK:
                        {
                            bool locked = BitConverter.ToBoolean(msg, 6);
                            sceneObject.enableRigidbody(!locked);
                            sceneObject.locked = locked;
                        }
                        break;
                    case ParameterType.KINEMATIC:
                        {
                            sceneObject.globalKinematic = BitConverter.ToBoolean(msg, 6);
                        }
                        break;
                    case ParameterType.FOV:
                        {
                            SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                            if (soc)
                                soc.fov = BitConverter.ToSingle(msg, 6);
                        }
                        break;
                    case ParameterType.ASPECT:
                        {
                            SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                            if (soc)
                                soc.aspect = BitConverter.ToSingle(msg, 6);
                        }
                        break;
                    case ParameterType.FOCUSDIST:
                        {
                            SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                            if (soc)
                                soc.focDist = BitConverter.ToSingle(msg, 6);
                        }
                        break;
                    case ParameterType.FOCUSSIZE:
                        {
                            SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                            if (soc)
                                soc.focSize = BitConverter.ToSingle(msg, 6);
                        }
                        break;
                    case ParameterType.APERTURE:
                        {
                            SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                            if (soc)
                                soc.aperture = BitConverter.ToSingle(msg, 6);
                        }
                        break;
                    case ParameterType.COLOR:
                        {
                            SceneObjectLight sol = (SceneObjectLight)sceneObject;
                            if (sol)
                            {
                                sol.transform.GetChild(0).GetComponent<Light>().color = new Color(BitConverter.ToSingle(msg, 6),
                                                                                        BitConverter.ToSingle(msg, 10),
                                                                                        BitConverter.ToSingle(msg, 14));
                            }
                        }
                        break;
                    case ParameterType.INTENSITY:
                        {
                            SceneObjectLight sol = (SceneObjectLight)sceneObject;
                            if (sol)
                            {
                                sol.transform.GetChild(0).GetComponent<Light>().intensity = BitConverter.ToSingle(msg, 6);
                            }
                        }
                        break;
                    case ParameterType.EXPOSURE:
                        {
                            // no exposure for unity lights
                            // used for katana
                        }
                        break;
                    case ParameterType.RANGE:
                        {
                            SceneObjectLight sol = (SceneObjectLight)sceneObject;
                            if (sol)
                            {
                                sol.transform.GetChild(0).GetComponent<Light>().range = BitConverter.ToSingle(msg, 6);
                            }
                        }
                        break;
                    case ParameterType.ANGLE:
                        {
                            SceneObjectLight sol = (SceneObjectLight)sceneObject;
                            if (sol)
                            {
                                sol.transform.GetChild(0).GetComponent<Light>().spotAngle = BitConverter.ToSingle(msg, 6);
                            }
                        }
                        break;
                    case ParameterType.BONEANIM:
                        {
                            sceneObject.transform.localPosition = new Vector3(BitConverter.ToSingle(msg, 6),
                                                                              BitConverter.ToSingle(msg, 10),
                                                                              BitConverter.ToSingle(msg, 14));
                            int offset = 12;
                            Quaternion[] animationState = new Quaternion[25];
                            for (int i = 0; i < 25; i++)
                            {
                                animationState[i] =  new Quaternion(  BitConverter.ToSingle(msg, offset + 6),
                                                                            BitConverter.ToSingle(msg, offset + 10),
                                                                            BitConverter.ToSingle(msg, offset + 14),
                                                                            BitConverter.ToSingle(msg, offset + 18));
                                offset += 16;
                            }
                            sceneObject.gameObject.GetComponent<CharacterAnimationController>().animationState = animationState;
                        }
                        break;
                    case ParameterType.PING:
                        {
                            // only for sync server
                        }
                        break;
                    case ParameterType.RESENDUPDATE:
                        {
                            // only for sync server
                        }
                        break;
                    default:
                        Debug.Log("Unknown paramType in ServerAdapter:ParseTransformation");
                        return;
                        break;
                }
                if (recordUpdates)
                    buildRecordMessage(sceneObject, paramType);
            }
        }

        private void buildRecordMessage(SceneObject sceneObject, ParameterType paramType)
        {
            if (recordObjects.Contains(sceneObject.gameObject))
            {
                Debug.Log("buildRecordMessage contains");

                Transform objTransform = sceneObject.transform;
                String messageHead = System.DateTime.Now.ToString("HH:mm:ss:ffff") + " " + getPathString(objTransform, scene) + " " + paramType.ToString("G") + " ";
                switch (paramType)
                {
                    case ParameterType.POS:
                        {
                            recordMessageQueue.Add(messageHead + objTransform.position.ToString("F6"));
                        }
                        break;

                    case ParameterType.ROT:
                        {
                            recordMessageQueue.Add(messageHead + objTransform.rotation.ToString("F6"));
                        }
                        break;
                    case ParameterType.SCALE:
                        {
                            recordMessageQueue.Add(messageHead + objTransform.lossyScale.ToString("F6"));
                        }
                        break;
                    case ParameterType.FOV:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectCamera)sceneObject).fov.ToString("F6"));
                        }
                        break;
                    case ParameterType.ASPECT:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectCamera)sceneObject).aspect.ToString("F6"));
                        }
                        break;
                    case ParameterType.FOCUSDIST:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectCamera)sceneObject).focDist.ToString("F6"));
                        }
                        break;
                    case ParameterType.FOCUSSIZE:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectCamera)sceneObject).focSize.ToString("F6"));
                        }
                        break;
                    case ParameterType.APERTURE:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectCamera)sceneObject).aperture.ToString("F6"));
                        }
                        break;
                    case ParameterType.COLOR:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectLight)sceneObject).getLightColor().ToString("F6"));
                        }
                        break;
                    case ParameterType.INTENSITY:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectLight)sceneObject).getLightIntensity().ToString("F6"));
                        }
                        break;
                    case ParameterType.RANGE:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectLight)sceneObject).getLightRange().ToString("F6"));
                        }
                        break;
                    case ParameterType.ANGLE:
                        {
                            recordMessageQueue.Add(messageHead + ((SceneObjectLight)sceneObject).getLightAngle().ToString("F6"));
                        }
                        break;
                }
            }
        }

        //! recursive function traversing GameObject hierarchy from Object up to main scene to find object path
        //! @param  obj         Transform of GameObject to find the path for
        //! @return     path to gameObject started at main scene, separated by "/"
        private string getPathString(Transform obj, Transform root, string separator = "/")
        {
            if (obj.parent)
            {
                if (obj.parent == Camera.main.transform)
                {
                    return getPathString(mainController.oldParent, root, separator) + separator + obj.name;
                }
                if (obj.transform.parent == root)
                    return obj.name;
                else
                {
                    return getPathString(obj.parent, root, separator) + separator + obj.name;
                }
            }
            return obj.name;
        }

        //! function searching for gameObject by path
        //! @param  path        path to gameObject started at main scene, separated by "/"
        //! @return             Transform of GameObject
        private Transform getOjectFromString(string path)
        {
            return scene.Find(path);
        }

        //!
        //! client function, listening for messages in receiveMessageQueue from server (executed in separate thread)
        //!
        public void listener()
        {
            AsyncIO.ForceDotNet.Force();
            using (var receiver = new SubscriberSocket())
            {
                receiver.SubscribeToAnyTopic();

                //receiver.Subscribe("client");
                //receiver.Subscribe("ncam");
                //receiver.Subscribe("record");

                receiver.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5556");

                lastReceiveTime = currentTimeTime;

                Debug.Log("Listener connected: " + "tcp://" + VPETSettings.Instance.serverIP + ":5556");
                byte[] input;
                while (isRunning)
                {
                    if (receiver.TryReceiveFrameBytes(out input))
                    {
                        this.receiveMessageQueue.Add(input);
                        lastReceiveTime = currentTimeTime;
                    }
                    else
                    {
                        listenerRestartCount = Mathf.Min(int.MaxValue, listenerRestartCount + 1);
                        // VPETSettings.Instance.msg = string.Format("Exception in Listener: {0}", listenerRestartCount);
                        if (currentTimeTime - lastReceiveTime > receiveTimeout)
                        {
                            // break;
                        }
                    }
                }
                receiver.Disconnect("tcp://" + VPETSettings.Instance.serverIP + ":5556");
                receiver.Close();
                receiver.Dispose();
            }
            //NetMQConfig.Cleanup();
        }

#if !SCENE_HOST
        //!
        //! receiver function, receiving the initial scene from the katana server (executed in separate thread)
        //!
        public void sceneReceiver()
        {

            AsyncIO.ForceDotNet.Force();
            using (var sceneReceiver = new RequestSocket())
            {
                print("Trying to receive scene.");

                OnProgress(0.1f, "Init Scene Receiver..");

                sceneReceiver.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5565");

                print("Server set up.");


                byte[] byteStream;


                // HEader
                print("header");
                sceneReceiver.SendFrame("header");
                byteStream = sceneReceiver.ReceiveFrameBytes();
                print("byteStreamHeader size: " + byteStream.Length);
                if (doWriteScene)
                {
                    writeBinary(byteStream, "header");
                }
                sceneLoader.SceneDataHandler.HeaderByteData = byteStream;
                OnProgress(0.15f, "..Received Header..");


#if TRUNK
                // Materials
                print("materials");
                sceneReceiver.SendFrame("materials");
                byteStream = sceneReceiver.ReceiveFrameBytes();
                print("byteStreamMatrilas size: " + byteStream.Length);
                if (doWriteScene)
                {
                    writeBinary(byteStream, "materials");
                }
                sceneLoader.SceneDataHandler.MaterialsByteData = byteStream;

                OnProgress(0.20f, "..Received Materials..");
#endif

                // Textures
                if (VPETSettings.Instance.doLoadTextures) {
                    print ("textures");
                    sceneReceiver.SendFrame("textures");
                    byteStream = sceneReceiver.ReceiveFrameBytes();
                    print ("byteStreamTextures size: " + byteStream.Length);
                    if (doWriteScene) {
                        writeBinary (byteStream, "textures");
                    }
                    sceneLoader.SceneDataHandler.TexturesByteData = byteStream;

                    OnProgress (0.33f, "..Received Texture..");
                }

                
                // Objects
                print ("objects");
                sceneReceiver.SendFrame("objects");
                byteStream = sceneReceiver.ReceiveFrameBytes();
                print ("byteStreamObjects size: " + byteStream.Length);
                if (doWriteScene) {
                    writeBinary (byteStream, "objects");
                }
                sceneLoader.SceneDataHandler.ObjectsByteData = byteStream;

                OnProgress (0.80f, "..Received Objects..");
                

                // Nodes
                print ("nodes");
                sceneReceiver.SendFrame("nodes");
                byteStream = sceneReceiver.ReceiveFrameBytes();
                print ("byteStreamNodess size: " + byteStream.Length);
                if (doWriteScene) {
                    writeBinary (byteStream, "nodes");
                }
                sceneLoader.SceneDataHandler.NodesByteData = byteStream;

                OnProgress (0.9f, "..Received Nodes..");


        
                sceneReceiver.Disconnect ("tcp://" + VPETSettings.Instance.serverIP + ":5565");
                sceneReceiver.Close();
                sceneReceiver.Dispose();
            }
            //NetMQConfig.Cleanup();
    
            print( "done receive scene" );

            m_sceneTransferDirty = true;

            OnProgress(1.0f, "..Building Scene..");

        }

        //!
        //! none
        //!
        public void sceneReceiverResource()
	    {
	        print( "Trying to load scene." );
	
			OnProgress( 0.1f, "Init Scene Receiver..");

            
            // Header
            byte[] byteStreamHeader = loadBinary("header");
            print("byteStreamHeader size: " + byteStreamHeader.Length);
            sceneLoader.SceneDataHandler.HeaderByteData = byteStreamHeader;
            OnProgress(0.15f, "..Received Header..");

#if TRUNK
            // Materials
            byte[] byteStreamMaterial = loadBinary("materials");
            print("byteStreamMaterial size: " + byteStreamMaterial.Length);
            sceneLoader.SceneDataHandler.MaterialsByteData = byteStreamMaterial;
            OnProgress(0.20f, "..Received Materials..");
#endif


            // Textures
            if (VPETSettings.Instance.doLoadTextures )
	        {
	            byte[] byteStreamTextures = loadBinary( "textures" );
	            print( "byteStreamTextures size: " + byteStreamTextures.Length );
                sceneLoader.SceneDataHandler.TexturesByteData = byteStreamTextures;
                OnProgress(0.33f, "..Received Texture..");
            }


            // Objects
            print("objects");
	        byte[] byteStreamObjects = loadBinary( "objects" );
	        print( "byteStreamObjects size: " + byteStreamObjects.Length );
            sceneLoader.SceneDataHandler.ObjectsByteData = byteStreamObjects;
            OnProgress( 0.80f, "..Received Objects..");

            // Nodes
            print( "nodes" );
	        byte[] byteStreamNodes = loadBinary( "nodes" );
	        print( "byteStreamNodess size: " + byteStreamNodes.Length );
            sceneLoader.SceneDataHandler.NodesByteData = byteStreamNodes;
            OnProgress(0.9f, "..Received Nodes..");


	        print( "done load scene" );
            m_sceneTransferDirty = true;
            OnProgress(1.0f, "..Building Scene..");

        }


        //!
        //! none
        //!
        private void writeBinary( byte[] data, string dataname)
		{
			string filesrc = persistentDataPath + "/" + sceneFileName + "_" + dataname + ".bytes";
			BinaryWriter writer = new BinaryWriter( File.Open( filesrc, FileMode.Create ) );
			print( "Write binary data: " + filesrc );
			writer.Write( data );
			writer.Close();
		}

        //!
        //! none
        //!
        private byte[] loadBinary( string dataname )
	    {
	        string filesrc = "VPET/SceneDumps/" + VPETSettings.Instance.sceneFileName + "_" + dataname;
	        print( "Load binary data: " + filesrc );
	        TextAsset asset = Resources.Load( filesrc ) as TextAsset;
	        return asset.bytes;
	        /*
	        Stream s = new MemoryStream( asset.bytes );
	        BinaryReader br = new BinaryReader( s );
	        */
	    }

        private void OnProgress( float progress, string msg="")
		{
			OnProgressEvent.Invoke(progress, msg);
		}
#else
        //!
        //! client function, listening for messages in receiveMessageQueue from server (executed in separate thread)
        //!
        public void recorder()
        {
            StreamWriter writer = new StreamWriter(recordFileName,true);
            while (isRunning && recordUpdates)
            {
                while (recordMessageQueue.Count != 0)
                {
                    Debug.Log("Got Message");
                    writer.WriteLine(recordMessageQueue[0]);
                    recordMessageQueue.RemoveAt(0);
                }
            }
            writer.Close();
        }
#endif

        //!
        //! Unity build in function beeing called just before Application is closed
        //! closes network Connections & terminates threads
        //!
        void OnApplicationQuit() 
	    {
	        Debug.Log("receiveMessageQueue.Count :" + receiveMessageQueue.Count);

	        isRunning = false;

            // finish sender objects
            foreach (ObjectSender sender in objectSenderList)
            {
                sender.Finish();
            }

            // final clean up after disposing ALL sockets
            NetMQConfig.Cleanup();

			// halt sender threads
			foreach (Thread _thread in senderThreadList)
			{
				if (_thread != null && _thread.IsAlive )
					_thread.Abort();
			}

#if !SCENE_HOST
            // halt scene receiver thread
            if ( sceneReceiverThread != null  && sceneReceiverThread.IsAlive )
	            sceneReceiverThread.Abort();
#endif

            // halt receiver thread
            if (receiverThread != null && receiverThread.IsAlive)
            {
                receiverThread.Join();
                receiverThread.Abort();
            }

            // halt recorder thread
            if (recorderThread != null && recorderThread.IsAlive)
            {
                recorderThread.Join();
                recorderThread.Abort();
            }

        }

    }
}
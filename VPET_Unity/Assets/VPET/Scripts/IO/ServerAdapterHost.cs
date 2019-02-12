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
﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using NetMQ;
using NetMQ.Sockets;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;

//!
//! adapter script handling all communication to network partners
//! receives and sends messages to the synchronization server
//! sends messages to katana server
//!
namespace vpet
{
    public class ServerAdapterProgressEventHost : UnityEvent<float, string> { }

    public class ServerAdapterHost : MonoBehaviour
	{
        //---------------------------------------
        // VIVE Integration
        //---------------------------------------

        /* TODO
		Bugs/Problems:
		Client which locks object doesn't see the updates

		Optimization:
		Update scale only when changed (reduces update calls by 1/3)
		Seperate recording and update rates (not that many updates needed)
		Buffer multiple lines when recording before write (less IO file writes)
		*/

        public enum VIVESelection
        {
            UpdateSingle,
            UpdateAll
        };

        [HideInInspector]
        public bool enableVIVE = false;

        [HideInInspector]
        public VIVESelection selectionMode = VIVESelection.UpdateAll;

        [HideInInspector]
        public float updateRate = 30f;

        [HideInInspector]
        public bool fileOutput = true;

        [HideInInspector]
        public string seperator = "|";

        [HideInInspector]
        public int precision = 2;

        [HideInInspector]
        public string point = ".";

        private float timer = 0f;
        private VIVEInput[] inputs = null;
        private Transform updateObj = null;

        //---------------------------------------

        public string IP = "192.168.161.100";

        //!
        //! unique id of client instance
        //!
        String id;
	
	    //!
	    //! cached reference to Katana massage templates
	    //!
	    KatanaTemplates katanaTemplates;
	
	    [HideInInspector]
	    //!
	    //! enable/disable receiving of ncam data
	    //!
		public bool receiveNcam = true;
	
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
	    //! none
	    //!
	    public bool doWriteScene = false;
	    //!
	    //! none
	    //!
	    public string sceneFileName = "tmp";
        //!
        //! none
        //!
        public bool doAutostartListener = false;

        //!
        //! none
        //!
        private string persistentDataPath;
	    public string PersistentDataPath
	    {
	        set { persistentDataPath = value;  }
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
	    //! reference to thread receiving all object updates from tablet syncronizing server
	    //!
	    Thread receiverThread;

	    //!
	    //! reference to thread receiving the scene from Katana
	    //!
	    Thread sceneReceiverThread;
	
	    //!
	    //! cached reference to the apps scene root object
	    //!
	    Transform scene;
		//!
	    //! cached reference to katanas scene root object
	    //!
	    Transform dreamspaceRoot;
	
	    //!
	    //! cached reference to main controller
	    //!
	    MainController mainController = null;

	    //!
	    //! none
	    //!
	    private SceneLoader sceneLoader;
	    public SceneLoader SceneLoader
	    {
	        set { sceneLoader = value;  }
	    }

        public ServerAdapterProgressEventHost OnProgressEvent = new ServerAdapterProgressEventHost();

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

        void Awake()
	    {
            VPETSettings.Instance.serverIP = IP;
            if (!deactivateReceive && receiverThread == null)
            {
                receiverThread = new Thread(new ThreadStart(listener));
                receiverThread.Start();
                isRunning = true;
            }
        }

	    //!
	    //! Use this for initialization
	    //!
	    void Start ()
	    {
            id = "XXX";

            if (GameObject.Find("MainController") != null )
    	        mainController = GameObject.Find("MainController").GetComponent<MainController>();
	
	        persistentDataPath = Application.persistentDataPath;
	
	        print( "persistentDataPath: " + persistentDataPath );
	
  	        scene = GameObject.Find( "Scene" ).transform;

			receiveMessageQueue = new ArrayList();			  
	
            dreamspaceRoot = scene; // GameObject.Find("Scene").transform;
	        if (dreamspaceRoot == null) Debug.LogError(string.Format("{0}: Cant Find: Scene.", this.GetType()));

            //Gets all VIVEInputs in the scene for later updating and registers a sender for the host to update the clients
            if (enableVIVE)
            {
                Utilities.CustomLog("Registered Host Sender");
                inputs = FindObjectsOfType<VIVEInput>();
                RegisterSender(ObjectSenderBasic.Instance);
            }

            initServerAdapterTransfer();
        }

        //!
        //! Register sender objects
        //!
        public static void RegisterSender(ObjectSender sender)
        {
            print("Register " + sender.GetType());

            if (!objectSenderList.Contains(sender))
                objectSenderList.Add(sender);
        }

        //!
        //! Init the sever adpater for receiving a scene
        //! This will check the IP, (re)start receiver thread, request progress state
        //!
        public void initServerAdapterTransfer()
	    {
            //bind Threads to methods & start them
            if (!deactivateReceive && receiverThread == null )
	        {
	            receiverThread = new Thread(new ThreadStart(listener));
	            receiverThread.Start();
				isRunning = true;
	        }

	        if (!deactivatePublish)
            {
                // create thread for all registered sender
                foreach ( ObjectSender sender in  objectSenderList)
				{
					sender.SetTarget(VPETSettings.Instance.serverIP, "5557");
					Thread _thread = new Thread(new ThreadStart(sender.Publisher));
					_thread.Start();
					if (!senderThreadList.Contains(_thread))
						senderThreadList.Add(_thread);
					sender.IsRunning = true;
				}
	        }
        }
	    	
	    //!
		//! Update is called once per frame
		//!
	    void Update () 
	    {
            // if we received new objects build them
            if ( m_sceneTransferDirty )
	        {
                m_sceneTransferDirty = false;
                print( "sceneLoader.createSceneGraph" );
	            sceneLoader.createSceneGraph( );
                sendUpdateObjects();
	            // HACK
	            mainController.repositionCamera();
	            // Camera.main.GetComponent<MoveCamera>().calibrate();
	
	        }

	        if (!deactivateReceive)
	        {
	            //process all available transforms send by server & delete them from Queue
	            int count = 0;
	            for (int i = 0; i < receiveMessageQueue.Count; i++)
	            {
                    // Debug.Log(receiveMessageQueue[i] as string);
                    try
                    {
                        parseTransformation(receiveMessageQueue[i] as string);
                    }
                    catch
                    {
                        VPETSettings.Instance.msg = "Error: parseTransformation";
                    }
	                count++;
	            }
	            receiveMessageQueue.RemoveRange(0, count);
	        }

            if (enableVIVE)
            {
                timer += Time.deltaTime;

                //Handles the updating to stay constant at a specific rate (reduces network load as well - compared to updates every frame)
                if (timer >= 1f / updateRate)
                {
                    timer -= 1f / updateRate;

                    //Updates a single (selected on client) object or all VIVEInputs in scene
                    if (selectionMode == VIVESelection.UpdateSingle)
                    {
                        if (updateObj)
                            UpdateSingleVIVEInput(updateObj.GetComponent<VIVEInput>());
                    }
                    else
                        UpdateAllVIVEInputs();
                }
            }

            currentTimeTime = Time.time;
		}

		//!
		//!
		//!
		public void SendObjectUpdate(Transform trn, bool onlyToClientsWithoutPhysics )
		{
			SendObjectUpdate(trn, NodeType.GROUP, onlyToClientsWithoutPhysics);
		}

		//!
		//!
		//!
		public void SendObjectUpdate(Transform trn, NodeType nodeType = NodeType.GROUP, params object[] args)
		{
			// bool onlyToClientsWithoutPhysics = false,
			
			if (trn.GetComponent<SceneObject>() != null)
				SendObjectUpdate(trn.GetComponent<SceneObject>(), nodeType, args);
		}

		//!
		//!
		//!
		public void SendObjectUpdate(SceneObject sobj, NodeType nodeType = NodeType.GROUP, params object[] args)
		{
			if (deactivatePublish)
				return;

			string dagPath = getPathString(sobj.transform, scene);

			foreach(ObjectSender sender in objectSenderList)
			{
				sender.SendObject(id, sobj, dagPath, nodeType, args);
			}
		}

		//!
		//!
		//!
		public void SendObjectUpdate<T>(string msg)
		{
			if (deactivatePublish)
				return;

			foreach(ObjectSender sender in objectSenderList)
			{
				if (sender.GetType() == typeof(T))
					sender.SendObject(msg);
			}
		}

	    //! function to be called to send a scale change to server
	    //! @param  obj             Transform of GameObject
	    //! @param  newPosition     new relative scale of GameObject in object space
	    public void sendFov(float fov, float left, float right, float bottom, float top)
	    {
	        if (!deactivatePublishKatana)
	        {
	            // katanaSendMessageQueue.Add(String.Format(katanaTemplates.camTemplate, fov, left, right, bottom, top));
	        }
	    }

	    //! function to be called to send a kinematic on/off signal to server
	    //! @param  obj             Transform of GameObject to be locked
	    //! @param  on              should it be set to on or off
	    public void sendKinematic(Transform obj, bool on)
	    {
 			string msg = "client " + id + "|" + "k" + "|" + this.getPathString(obj,scene) + "|" + on;
			SendObjectUpdate<ObjectSenderBasic>(msg);
	    }

	    //! function to be called to send a lock signal to server
	    //! @param  obj             Transform of GameObject to be locked
	    //! @param  locked          should it be locked or unlocked
	    public void sendLock(Transform obj, bool locked)
	    {
	        if (locked) // lock it
	        {
                if (currentlyLockedObject != null && currentlyLockedObject != obj && !deactivatePublish) // is another object already locked, release it first
                {
					string msg = "client " + id + "|" + "l" + "|" + this.getPathString(currentlyLockedObject, scene) + "|" + false;
 					SendObjectUpdate<ObjectSenderBasic>(msg);
                    // print("Unlock object " + currentlyLockedObject.gameObject.name );
                }
                if (currentlyLockedObject != obj && !deactivatePublish) // lock the object if it is not locked yet
                {
					string msg = "client " + id + "|" + "l" + "|" + this.getPathString(obj, scene) + "|" + true;
 					SendObjectUpdate<ObjectSenderBasic>(msg);
                    // print("Lock object " + obj.gameObject.name);
                }
                currentlyLockedObject = obj;
            }
            else // unlock it
	        {
                if ( currentlyLockedObject != null && !deactivatePublish ) // unlock if locked
                {
					string msg = "client " + id + "|" + "l" + "|" + this.getPathString(obj, scene) + "|" + false;
 					SendObjectUpdate<ObjectSenderBasic>(msg);
                    // print("Unlock object " + obj.gameObject.name);
                }
                currentlyLockedObject = null;
	        }
	    }

        public void sendAnimatorCommand(Transform obj, int cmd)
        {
			string msg = "client " + id + "|" + "m" + "|" + this.getPathString(obj, scene) + "|" + cmd;
			SendObjectUpdate<ObjectSenderBasic>(msg);			
        }

        public void sendColliderOffset(Transform obj, Vector3 offset )
        {
			string msg = "client " + id + "|" + "b" + "|" + this.getPathString(obj, scene) + "|" + offset.x + "|" + offset.y + "|" + offset.z;
			SendObjectUpdate<ObjectSenderBasic>(msg);			
        }

        //! function to be called to resend stored scene object attributes
        public void sendUpdateObjects()
        {
			SendObjectUpdate<ObjectSenderBasic>("client " + id + "|" + "udOb");
        }

        //! function parsing received message and executing change
        //! @param  message         message string received by server
        public void parseTransformation(string message)
	    {
            string[] splitMessage = message.Split('|');
            if (splitMessage.Length > 1)
            {
                if (splitMessage[2] == "cam")
                {
                    switch (splitMessage[1])
                    {
                        case "t":
                            if (splitMessage.Length == 6)
                                Camera.main.transform.position = new Vector3(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture));
                            break;
                        case "r":
                            if (splitMessage.Length == 7)
                            {
                                if (splitMessage[0].StartsWith("client", StringComparison.CurrentCulture))
                                    Camera.main.transform.rotation = new Quaternion(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture), float.Parse(splitMessage[6], CultureInfo.InvariantCulture));
                                else
                                {
                                    Quaternion quat = new Quaternion(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture), float.Parse(splitMessage[6], CultureInfo.InvariantCulture));
                                    Vector3 rot = quat.eulerAngles;
                                    Camera.main.transform.rotation = Quaternion.Euler(new Vector3(rot.x, -rot.y, -rot.z + 180));
                                }
                            }
                            break;
                        case "f":
                            if (splitMessage.Length == 4)
                                if (receiveNcam) Camera.main.fieldOfView = float.Parse(splitMessage[3], CultureInfo.InvariantCulture);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Transform obj = getOjectFromString(splitMessage[2]);

                    if ( obj && obj != currentlyLockedObject && splitMessage[0] != id )
	                {
	                    switch ( splitMessage[1] )
	                    {
	                        case "t":
	                            if (splitMessage.Length == 6)
								{
	                                if ( obj.GetComponent<SceneObject>() ) obj.GetComponent<SceneObject>().tempLock = true;
									try
									{
                                        // obj.position = new Vector3(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));
                                        obj.localPosition = new Vector3(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture));
                                    }
									catch {}
								}
	                            break;
	                        case "r":
	                            if (splitMessage.Length == 7)
								{
									if(obj.GetComponent<SceneObject>()) obj.GetComponent<SceneObject>().tempLock = true;
	                                try
									{
                                        // obj.rotation = new Quaternion(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]), float.Parse(splitMessage[6]));
                                        obj.localRotation = new Quaternion(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture), float.Parse(splitMessage[6], CultureInfo.InvariantCulture));
                                    }
                                    catch {}
								}
								break;
	                        case "s":
	                            if (splitMessage.Length == 6)
									try
									{
		                                obj.localScale = new Vector3(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture));
									}
									catch {}
	                            break;
	                        case "c":
	                            if (splitMessage.Length == 6)
								{
									try
									{
	                                	obj.GetChild(0).GetComponent<Light>().color = new Color(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture));
                                        //obj.GetComponent<Light>().color = new Color(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));                 
                                        //obj.GetChild(0).GetComponent<Renderer>().material.color = new Color(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));
                                    }
                                    catch {}
								}
								break;
	                        case "i":
	                            if (splitMessage.Length == 4)
									try
									{
		                                obj.GetChild(0).GetComponent<Light>().intensity = float.Parse(splitMessage[3], CultureInfo.InvariantCulture);
									}
									catch {}
	                            break;
	                        case "a":
	                            if (splitMessage.Length == 4)
									try
									{
	                                	obj.GetChild(0).GetComponent<Light>().spotAngle = float.Parse(splitMessage[3], CultureInfo.InvariantCulture);
									}
									catch {}	
									break;
	                        case "d":
	                            if (splitMessage.Length == 4)
									try
									{
	                                	obj.GetChild(0).GetComponent<Light>().range = float.Parse(splitMessage[3], CultureInfo.InvariantCulture);
									}
									catch {}
	                            break;
	                        case "k":
	                            if (splitMessage.Length == 4)
									try
									{
		                                obj.GetComponent<SceneObject>().setKinematic(bool.Parse(splitMessage[3]),false);
									}
									catch {}
	                            break;
	                        case "l":
                                if (splitMessage.Length == 4)
                                {
                                    //Handles the selection (locking) of an object to update it
                                    if (bool.Parse(splitMessage[3]))
                                    {
                                        updateObj = obj;
                                        mainController.unselectIfSelected(obj);
                                    }
                                    else
                                        updateObj = null;

                                    try
                                    {
                                        obj.GetComponent<SceneObject>().locked = bool.Parse(splitMessage[3]);
                                    }
                                    catch { }
                                }
	                            break;
                            case "b": // move bbox/collider
                                if (splitMessage.Length == 6)
                                    try
                                    {
                                        obj.GetComponent<SceneObject>().colliderOffset( new Vector3(float.Parse(splitMessage[3], CultureInfo.InvariantCulture), float.Parse(splitMessage[4], CultureInfo.InvariantCulture), float.Parse(splitMessage[5], CultureInfo.InvariantCulture)) );
                                    }
                                    catch { }
                                break;
                            case "m": // trigger mocap data
                                if (splitMessage.Length == 4 && obj.GetComponent<SceneObjectServer>() != null)
                                {
                                    try
                                    {
                                        obj.GetComponent<SceneObjectServer>().AnimatorCommand(int.Parse(splitMessage[3]));
                                    }
                                    catch { }
                                }
                                break;
                            default:
	                            break;
	                    }
	                }
	            }
	        }
	    }

        /// <summary>
        /// Loops through all VIVEInputs gathered before and updates them.
        /// </summary>
        private void UpdateAllVIVEInputs()
        {
            foreach (VIVEInput input in inputs)
                UpdateSingleVIVEInput(input);
        }

        /// <summary>
        /// Handles the actual tracker data, calculates the correct values and sends appropriate updates depending on selections.
        /// Only operating when the VIVEInput is capturing. (Origin is an exception)
        /// </summary>
        private void UpdateSingleVIVEInput(VIVEInput input)
        {
            updateObj = input.transform;

            if (!input.capture)
            {
                if (input.isOrigin)
                    SendVIVEObjectUpdate(new Vector3(), updateObj.localRotation);
  
                return;
            }

            Vector3 pos = updateObj.localPosition + input.positionOffset - (input.origin ? input.origin.localPosition : Vector3.zero);
            Vector3 rotDiff = TransformUtils.GetInspectorRotation(updateObj) + input.rotationOffset - updateObj.localEulerAngles;       //Workaround for the different rotation values from the inspector to the engine (Would be missleading if using engine values)
            Quaternion rot = Quaternion.Euler(updateObj.localEulerAngles + rotDiff);

            //Changes the specific update calls and file writes, depending on the selection where offset should be applied
            switch (input.offsetSelection)
            {
                case VIVEInput.OffsetSelection.Both:
                    SendVIVEObjectUpdate(pos, rot);
                    WriteFileLine(pos, updateObj.localEulerAngles + rotDiff, input);
                break;
                case VIVEInput.OffsetSelection.Updating:
                    SendVIVEObjectUpdate(pos, rot);
                    WriteFileLine(updateObj.localPosition - (input.origin ? input.origin.localPosition : Vector3.zero), TransformUtils.GetInspectorRotation(updateObj), input);
                break;
                case VIVEInput.OffsetSelection.Recording:
                    SendVIVEObjectUpdate(updateObj.localPosition - (input.origin ? input.origin.localPosition : Vector3.zero), updateObj.localRotation);
                    WriteFileLine(pos, updateObj.localEulerAngles + rotDiff, input);
                break;
            }
        }

        /// <summary>
        /// Limits the rotation from 0 to max. (Default: 360f)
        /// </summary>
        private Vector3 RangedRotation(Vector3 rot, float max=360f)
        {
            return new Vector3(rot.x % max, rot.y % max, rot.z % max);
        }

        /// <summary>
        /// Sends an object update to the synchronisation server for distribution to all clients. (VIVE Integration)
        /// </summary>
        private void SendVIVEObjectUpdate(Vector3 pos, Quaternion rot)
        {
            SendObjectUpdate<ObjectSenderBasic>("client " + id + "|" + "t" + "|" + getPathString(updateObj, scene) + "|" + pos.x.ToString(CultureInfo.InvariantCulture) + "|" + pos.y.ToString(CultureInfo.InvariantCulture) + "|" + pos.z.ToString(CultureInfo.InvariantCulture));
            SendObjectUpdate<ObjectSenderBasic>("client " + id + "|" + "r" + "|" + getPathString(updateObj, scene) + "|" + rot.x.ToString(CultureInfo.InvariantCulture) + "|" + rot.y.ToString(CultureInfo.InvariantCulture) + "|" + rot.z.ToString(CultureInfo.InvariantCulture) + "|" + rot.w.ToString(CultureInfo.InvariantCulture));
            SendObjectUpdate<ObjectSenderBasic>("client " + id + "|" + "s" + "|" + getPathString(updateObj, scene) + "|" + updateObj.localScale.x.ToString(CultureInfo.InvariantCulture) + "|" + updateObj.localScale.y.ToString(CultureInfo.InvariantCulture) + "|" + updateObj.localScale.z.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the returned line from BuildFileLine(Vector3 pos, Vector3 rot) to the actual file.
        /// Only operating when "File Output" is checked and the VIVEInput is recording.
        /// Also handles when the tracker signal is lost and marks it in the file.
        /// </summary>
        private void WriteFileLine(Vector3 pos, Vector3 rot, VIVEInput input)
        {
            if (fileOutput && input.recording)
            {
                string path = "data\\" + ((input.fileName == "") ? input.name : input.fileName);

                if (input.CheckTrackerSignal())
                {
                    File.AppendAllText(path, BuildFileLine(pos, RangedRotation(rot)) + Environment.NewLine);
                    input.wroteError = false;
                }
                else
                {
                    if (!input.wroteError)
                    {
                        File.AppendAllText(path, "\u2191 TRACKER SIGNAL LOST ABOVE \u2191" + Environment.NewLine);
                        input.wroteError = true;
                    }
                }
            }
        }

        /// <summary>
        /// Converts the position and rotation to a proper string using the selected precision, seperator and point values.
        /// </summary>
        private string BuildFileLine(Vector3 pos, Vector3 rot)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(pos.x.ToString("F" + precision));
            builder.Append(seperator);
            builder.Append(pos.y.ToString("F" + precision));
            builder.Append(seperator);
            builder.Append(pos.z.ToString("F" + precision));
            builder.Append(seperator);
            builder.Append(rot.x.ToString("F" + precision));
            builder.Append(seperator);
            builder.Append(rot.y.ToString("F" + precision));
            builder.Append(seperator);
            builder.Append(rot.z.ToString("F" + precision));
            builder.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, point);

            return builder.ToString();
        }

        //! recursive function traversing GameObject hierarchy from Object up to main scene to find object path
        //! @param  obj         Transform of GameObject to find the path for
        //! @return     path to gameObject started at main scene, separated by "/"
        private string getPathString(Transform obj, Transform root, string separator = "/") {
	        if (obj.parent) 
	        {
	            if (obj.parent == Camera.main.transform)
	            {
	                return getPathString(mainController.oldParent, root, separator) + separator + obj.name;
	            }
	            if(obj.transform.parent == root)
	                return obj.name;
	            else {
	                return getPathString(obj.parent,root,separator) + separator + obj.name;
	            }
	        }
	        return obj.name;
	    }
	
	    //! function searching for gameObject by path
	    //! @param  path        path to gameObject started at main scene, separated by "/"
	    //! @return             Transform of GameObject
	    private Transform getOjectFromString(string path) {
	        return scene.Find( path );
	    }
	
	    //!
	    //! client function, listening for messages in from server (executed in separate thread)
	    //!
	    public void listener() 
	    {
            AsyncIO.ForceDotNet.Force();
            using (var receiver = new SubscriberSocket())
            {
                receiver.Subscribe("client");
                receiver.Subscribe("ncam");
                receiver.Subscribe("record");

                receiver.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5556");

                lastReceiveTime = currentTimeTime;

                Debug.Log("Listener connected: " + "tcp://" + VPETSettings.Instance.serverIP + ":5556");
                string input;
                while (isRunning)
                {
                    if (receiver.TryReceiveFrameString(out input)) 
					{
                        this.receiveMessageQueue.Add (input.Substring (7));
                        lastReceiveTime = currentTimeTime;
                    } else {
                        listenerRestartCount = Mathf.Min(int.MaxValue, listenerRestartCount+1);
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
		
	    //!
	    //! receiver function, receiving the initial scene from the katana server (executed in separate thread)
	    //!
	    public void sceneReceiver()
	    {
	        
            AsyncIO.ForceDotNet.Force();
            using (var sceneReceiver = new RequestSocket())
            {
                print ("Trying to receive scene.");

                OnProgress (0.1f, "Init Scene Receiver..");

                sceneReceiver.Connect ("tcp://" + VPETSettings.Instance.serverIP + ":5565");
        
                print ("Server set up.");
        

                byte[] byteStream;

                
                // HEader
                print ("header");
                sceneReceiver.SendFrame("header");
                byteStream = sceneReceiver.ReceiveFrameBytes();
                print ("byteStreamHeader size: " + byteStream.Length);
                if (doWriteScene) {
                    writeBinary (byteStream, "header");
                }

                int dataIdx = 0;
                VPETSettings.Instance.lightIntensityFactor = BitConverter.ToSingle (byteStream, dataIdx);
                print ("VPETSettings.Instance.lightIntensityFactor " + VPETSettings.Instance.lightIntensityFactor);
                dataIdx += sizeof(float);
                VPETSettings.Instance.textureBinaryType = BitConverter.ToInt32 (byteStream, dataIdx);

                OnProgress (0.15f, "..Received Header..");


                //VpetHeader vpetHeader = SceneDataHandler.ByteArrayToStructure<VpetHeader>(byteStream, ref dataIdx);
                //VPETSettings.Instance.lightIntensityFactor = vpetHeader.lightIntensityFactor;
                //VPETSettings.Instance.textureBinaryType = vpetHeader.textureBinaryType;

                // Textures
                if (VPETSettings.Instance.doLoadTextures) {
                    print ("textures");
                    sceneReceiver.SendFrame("textures");
                    byteStream = sceneReceiver.ReceiveFrameBytes();
                    print ("byteStreamTextures size: " + byteStream.Length);
                    if (doWriteScene) {
                        writeBinary (byteStream, "textu");
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
                    writeBinary (byteStream, "objec");
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

            
            // HEader
            byte[] byteStreamHeader = loadBinary("header");
            print("byteStreamHeader size: " + byteStreamHeader.Length);
            int dataIdx = 0;
            VPETSettings.Instance.lightIntensityFactor = BitConverter.ToSingle(byteStreamHeader, dataIdx);
            print("VPETSettings.Instance.lightIntensityFactor " + VPETSettings.Instance.lightIntensityFactor);
            dataIdx += sizeof(float);
            VPETSettings.Instance.textureBinaryType = BitConverter.ToInt32(byteStreamHeader, dataIdx);

            OnProgress(0.15f, "..Received Header..");



            // Textures
            if (VPETSettings.Instance.doLoadTextures )
	        {
	            byte[] byteStreamTextures = loadBinary( "textu" );
	            print( "byteStreamTextures size: " + byteStreamTextures.Length );
                sceneLoader.SceneDataHandler.TexturesByteData = byteStreamTextures;
                OnProgress(0.33f, "..Received Texture..");
            }


            // Objects
            print("objects");
	        byte[] byteStreamObjects = loadBinary( "objec" );
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

            // halt scene receiver thread
	        if ( sceneReceiverThread != null  && sceneReceiverThread.IsAlive )
	            sceneReceiverThread.Abort();

            // halt receiver thread
            if (receiverThread != null && receiverThread.IsAlive)
            {
                receiverThread.Join();
                receiverThread.Abort();
            }
        }
    }
}
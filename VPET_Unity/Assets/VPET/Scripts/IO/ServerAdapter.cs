/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

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
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using NetMQ;

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
	    //! camera representing object
	    //!
	    Transform camObject;
	
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
	    //! message queue for outgoing messages for tablet syncronizing server
	    //!
	    ArrayList sendMessageQueue;
	    //!
	    //! message queue for outgoing messages for katana server
	    //!
	    ArrayList katanaSendMessageQueue;
	
	    //!
	    //! reference to thread receiving all object updates from tablet syncronizing server
	    //!
	    Thread receiverThread;
	    //!
	    //! reference to thread sending all object updates to katana server
	    //!
	    Thread katanaSenderThread;
	    //!
	    //! reference to thread sending all object updates to tablet syncronizing server
	    //!
	    Thread senderThread;
	    //!
	    //! reference to thread receiving the scene from Katana
	    //!
	    Thread sceneReceiverThread;
	
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
	
	    //!
	    //! cached reference to main controller
	    //!
	    MainController mainController;
	
	    //!
	    //! none
	    //!
	    private SceneLoader sceneLoader;
	    public SceneLoader SceneLoader
	    {
	        set { sceneLoader = value;  }
	    }
	    
	    List<SceneObjectKatana> receiveObjectQueue;
	
	
		public ServerAdapterProgressEvent OnProgressEvent = new ServerAdapterProgressEvent();

        // used to pass time to threads
        private float currentTimeTime = 0;

        private float lastReceiveTime = 0;

        private int listenerRestartCount = 0;

        private bool m_sceneTransferDirty = false;

        void Awake()
	    {
	        receiveObjectQueue = new List<SceneObjectKatana>();
	    }
	
	
	    //!
	    //! Use this for initialization
	    //!
	    void Start ()
	    {
	        id = Network.player.ipAddress.Split('.')[3];
	
	        mainController = GameObject.Find("MainController").GetComponent<MainController>();
	
	        camObject = GameObject.Find("camera").transform;
	
	        persistentDataPath = Application.persistentDataPath;
	
	        print( "persistentDataPath: " + persistentDataPath );
	
	        scene = GameObject.Find( "Scene" ).transform;
	
	
	        receiveMessageQueue = new ArrayList();
	        sendMessageQueue = new ArrayList();
	        katanaSendMessageQueue = new ArrayList();
	
	
	        dreamspaceRoot = GameObject.Find("Scene").transform;
	        if (dreamspaceRoot == null) Debug.LogError(string.Format("{0}: Cant Find: Scene.", this.GetType()));
		
	    }
	
	
	    //!
	    //! Init the sever adpater for receiving a scene
	    //! This will check the IP, (re)start receiver thread, request progress state
	    //!
	    public void initServerAdapterTransfer()
	    {
	
	        if (!ipAdressFormat.IsMatch( VPETSettings.Instance.serverIP))
	        {
	            deactivatePublish = true;
	            deactivateReceive = true;
	            deactivatePublishKatana = true;
	        }

            // clear previous scene
            sceneLoader.ResetScene();
	
	        // if (!ipAdressFormat.IsMatch(VPETSettings.Instance.katanaIP)) deactivatePublishKatana = true;
	
	        katanaTemplates = GameObject.Find("KatanaTemplates").GetComponent<KatanaTemplates>();
	
	
	        isRunning = true;
	
	
	        //bind Threads to methods & start them
	        if (!deactivateReceive && receiverThread == null )
	        {
	            receiverThread = new Thread(new ThreadStart(listener));
	            receiverThread.Start();
	        }
	        if (!deactivatePublish && senderThread == null)
            {
	            senderThread = new Thread(new ThreadStart(publisher));
	            senderThread.Start();
	        }
	        if (!deactivatePublishKatana && katanaSenderThread == null)
            {
	            katanaSenderThread = new Thread(new ThreadStart(sendKatana));
	            katanaSenderThread.Start();
	        }
	
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
        }
	
	    //!
	    //! Init the sever adpater for receiving a scene
	    //! This will check the IP, (re)start listener thread and publisher thread, request progress state
	    //!
	    public void initServerAdapterRuntime()
	    {
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
	
	        if (camObject!=null && camObject.GetComponent<Renderer>().enabled && (Time.time-lastNcamReceivedTime) > 10)
	        {
	            camObject.GetComponent<Renderer>().enabled = false;
	        }

            currentTimeTime = Time.time;

		}
	
	    //! function to be called to send a translation change to server
	    //! @param  obj             Transform of GameObject
	    //! @param  newPosition     new absolute position of GameObject in world space
	    public void sendTransform( Transform obj, bool onlyToClientsWithoutPhysics = false )
	    {
	        //Vector3 pos = obj.transform.localPosition;
	        //Vector3 rot = obj.localRotation.eulerAngles;
	        //Vector3 scl = obj.transform.localScale;
	
	        //if ( !deactivatePublish )
	        //{
	
	        //    string sendMessage = id + "|" + this.getPathString( obj, scene ) +
	        //                        "|" + pos.x + "|" + pos.y + "|" + pos.z + 
	        //                        "|" + rot.x + "|" + rot.y + "|" + rot.z +
	        //                        "|" + scl.x + "|" + scl.y + "|" + scl.z;
	
	        //    if (onlyToClientsWithoutPhysics)
	        //    {
	        //        sendMessage += "|physics";
	        //    }
	
	        //    sendMessageQueue.Add( sendMessage );
	        //}
	    }
	
	
		private void sendTransformKatana(string loc, Vector3 pos, Quaternion rot, Vector3 scl )
		{
			float angle = 0;
			Vector3 axis = Vector3.zero;
			rot.ToAngleAxis( out angle, out axis );

			katanaSendMessageQueue.Add(String.Format(katanaTemplates.objTemplateQuat,
				loc,
				(-pos.x + " " + pos.y + " " + pos.z),
				(angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
				(scl.x + " " + scl.y + " " + scl.z) ) );
		}

		private void sendTransformLightKatana(string loc, Vector3 pos, Quaternion rot, Vector3 scl )
		{
			float angle = 0;
			Vector3 axis = Vector3.zero;
			rot.ToAngleAxis( out angle, out axis );

			katanaSendMessageQueue.Add(String.Format(katanaTemplates.lightTransRotTemplate,
				loc,
				(-pos.x + " " + pos.y + " " + pos.z),
                0,
                0,
                0,
//				(angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
				(scl.x + " " + scl.y + " " + scl.z) ));
		}

	    //! function to be called to send a translation change to server
	    //! @param  obj             Transform of GameObject
	    //! @param  newPosition     new absolute position of GameObject in world space
	    public void sendTranslation(Transform obj, bool onlyToClientsWithoutPhysics = false)
	    {
	        if (!deactivatePublish)
	        {


                // HACK:
                onlyToClientsWithoutPhysics = false;



                if (!onlyToClientsWithoutPhysics) sendMessageQueue.Add(id + "|" + "t" + "|" + this.getPathString(obj, scene) + "|" + obj.localPosition.x + "|" + obj.localPosition.y + "|" + obj.localPosition.z);
				else sendMessageQueue.Add(id + "|" + "t" + "|" + this.getPathString(obj, scene) + "|" + obj.localPosition.x + "|" + obj.localPosition.y + "|" + obj.localPosition.z + "|physics");
	        }
	        if (!deactivatePublishKatana)
	        {
	            if ( obj.GetComponent<SceneObject>().IsLight )
	            {
					sendTransformLightKatana("/" + this.getPathString(obj, dreamspaceRoot),
											obj.localPosition,
											obj.localRotation,
											obj.localScale);

	            }
	            else if (obj != Camera.main.transform)
	            {
					sendTransformKatana("/" + this.getPathString(obj, dreamspaceRoot),
						obj.localPosition,
						obj.localRotation,
						obj.localScale);
	            }
	        }
	    }

		//! function to be called to send a rotation change to server
	    //! @param  obj             Transform of GameObject
	    //! @param  newPosition     new absolute rotation of GameObject in world space
	    public void sendRotation(Transform obj, bool onlyToClientsWithoutPhysics = false)
	    {
	        if (!deactivatePublish)
	        {


                // HACK:
                onlyToClientsWithoutPhysics = false;



                if (!onlyToClientsWithoutPhysics) sendMessageQueue.Add(id + "|" + "r" + "|" + this.getPathString(obj, scene) + "|" + obj.localRotation.x + "|" + obj.localRotation.y + "|" + obj.localRotation.z + "|" + obj.localRotation.w);
				else sendMessageQueue.Add(id + "|" + "r" + "|" + this.getPathString(obj, scene) + "|" + obj.localRotation.x + "|" + obj.localRotation.y + "|" + obj.localRotation.z + "|" + obj.localRotation.w + "|physics");
	        }
			if (!deactivatePublishKatana)
			{
				if ( obj.GetComponent<SceneObject>().IsLight )
				{
					sendTransformLightKatana("/" + this.getPathString(obj, dreamspaceRoot),
						obj.localPosition,
						obj.localRotation,
						obj.localScale);

				}
				else if (obj != Camera.main.transform)
				{
					sendTransformKatana("/" + this.getPathString(obj, dreamspaceRoot),
						obj.localPosition,
						obj.localRotation,
						obj.localScale);
				}
			}
	    }

	    //! function to be called to send a scale change to server
	    //! @param  obj             Transform of GameObject
	    //! @param  newPosition     new relative scale of GameObject in object space
	    public void sendScale(Transform obj )
	    {
	        if (!deactivatePublish)
	        {
				sendMessageQueue.Add(id + "|" + "s" + "|" + this.getPathString(obj, scene) + "|" + obj.localScale.x + "|" + obj.localScale.y + "|" + obj.localScale.z);
	        }
			if (!deactivatePublishKatana)
			{
				if ( obj.GetComponent<SceneObject>().IsLight )
				{
					sendTransformLightKatana("/" + this.getPathString(obj, dreamspaceRoot),
						obj.localPosition,
						obj.localRotation,
						obj.localScale);

				}
				else if (obj != Camera.main.transform)
				{
					sendTransformKatana("/" + this.getPathString(obj, dreamspaceRoot),
						obj.localPosition,
						obj.localRotation,
						obj.localScale);
				}
			}
	    }


		private void sendLightParamsKatana( string loc, string lightType, Color lightColor, float lightIntensity, float coneAngle, float exposure )
		{
			katanaSendMessageQueue.Add(String.Format(katanaTemplates.lightIntensityColorTemplate,
				loc,
				lightType,
				lightIntensity / VPETSettings.Instance.lightIntensityFactor,
				lightColor.r + " " + lightColor.g + " " + lightColor.b,
				exposure,
				coneAngle	));
		}

	    //! function to be called to send a light color change to server
	    //! @param  obj             Transform of GameObject (Light)
	    //! @param  newColor        new color of light
		public void sendLightColor(Transform obj, Light light, float exposure=3f )
	    {
	        if (!deactivatePublish)
	        {
				sendMessageQueue.Add(id + "|" + "c" + "|" + this.getPathString(obj, scene) + "|" + light.color.r + "|" + light.color.g + "|" + light.color.b);
	        }

	        if (!deactivatePublishKatana)
	        {
				sendLightParamsKatana( "/" + this.getPathString(obj, 
								dreamspaceRoot), ((LightTypeKatana)(light.type)).ToString(),
								light.color,
								light.intensity,
								light.spotAngle,
								exposure);
	        }
	    }
	
	    //! function to be called to send a light intensity change to server
	    //! @param  obj             Transform of GameObject (Light)
	    //! @param  newIntensity    new intensity of light
		public void sendLightIntensity(Transform obj, Light light, float exposure=3f )
	    {
	        if (!deactivatePublish)
	        {
				sendMessageQueue.Add(id + "|" + "i" + "|" + this.getPathString(obj, scene) + "|" + light.intensity );
	        }
	        if (!deactivatePublishKatana)
	        {
				sendLightParamsKatana( "/" + this.getPathString(obj, 
					dreamspaceRoot), ((LightTypeKatana)(light.type)).ToString(),
					light.color,
					light.intensity,
					light.spotAngle,
					exposure);
	        }
	    }
	
	    //! function to be called to send a light cone angle change of a spotlight to server
	    //! @param  obj             Transform of GameObject (Light)
	    //! @param  newAngle        new cone angle of light
		public void sendLightConeAngle(Transform obj, Light light, float exposure=3f )
	    {
	        if (!deactivatePublish)
	        {
				sendMessageQueue.Add(id + "|" + "a" + "|" + this.getPathString(obj, scene) + "|" + light.spotAngle);
	        }
	        if (!deactivatePublishKatana)
	        {
				sendLightParamsKatana( "/" + this.getPathString(obj, 
					dreamspaceRoot), ((LightTypeKatana)(light.type)).ToString(),
					light.color,
					light.intensity,
					light.spotAngle,
					exposure);
	        }
	    }
	
		/*
	    //! function to be called to send a light range change of a spotlight or pointlight to server
	    //! @param  obj             Transform of GameObject (Light)
	    //! @param  newRange        new range of light
	    public void sendLightRange(Transform obj, float newRange)
	    {
	        if (!deactivatePublish)
	        {
	            sendMessageQueue.Add(id + "|" + "d" + "|" + this.getPathString(obj, scene) + "|" + newRange);
	        }
	        if (!deactivatePublishKatana)
	        {
	            string lightType = "sphere";
	            katanaSendMessageQueue.Add(String.Format(katanaTemplates.lightRangeTemplate,
	                                                     "_" + this.getPathString(obj, dreamspaceRoot, "_"),
	                                                     "/" + this.getPathString(obj, dreamspaceRoot),
	                                                     Mathf.Pow(2, 1.0f / newRange),
	                                                     lightType));
	        }
	    }
		*/
	
	    //! function to be called to send a scale change to server
	    //! @param  obj             Transform of GameObject
	    //! @param  newPosition     new relative scale of GameObject in object space
	    public void sendFov(float fov, float left, float right, float bottom, float top)
	    {
	
	        if (!deactivatePublishKatana)
	        {
	            katanaSendMessageQueue.Add(String.Format(katanaTemplates.camTemplate, fov, left, right, bottom, top));
	
	        }
	
	    }
	
	    //! function to be called to send a kinematic on/off signal to server
	    //! @param  obj             Transform of GameObject to be locked
	    //! @param  on              should it be set to on or off
	    public void sendKinematic(Transform obj, bool on)
	    {
	        if (!deactivatePublish)
	        {
	            sendMessageQueue.Add(id + "|" + "k" + "|" + this.getPathString(obj,scene) + "|" + on);
	        }
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
                    sendMessageQueue.Add(id + "|" + "l" + "|" + this.getPathString(currentlyLockedObject, scene) + "|" + false);
                    // print("Unlock object " + currentlyLockedObject.gameObject.name );
                }
                if (currentlyLockedObject != obj && !deactivatePublish) // lock the object if it is not locked yet
                {
                    sendMessageQueue.Add(id + "|" + "l" + "|" + this.getPathString(obj, scene) + "|" + true);
                    // print("Lock object " + obj.gameObject.name);
                }
                currentlyLockedObject = obj;
            }
            else // unlock it
	        {
                if ( currentlyLockedObject != null && !deactivatePublish ) // unlock if locked
                {
                    sendMessageQueue.Add(id + "|" + "l" + "|" + this.getPathString(obj, scene) + "|" + false);
                    // print("Unlock object " + obj.gameObject.name);
                }
                currentlyLockedObject = null;
	        }
	    }

        //! function to be called to resend stored scene object attributes
        public void sendUpdateObjects()
        {
            sendMessageQueue.Add(id + "|" + "udOb");
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
                    if (!camObject.GetComponent<Renderer>().enabled) camObject.GetComponent<Renderer>().enabled = true;
                    lastNcamReceivedTime = Time.time;
                    switch (splitMessage[1])
                    {
                        case "t":
                            if (splitMessage.Length == 6)
                                if (receiveNcam) Camera.main.transform.position = new Vector3(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));
                                else camObject.position = new Vector3(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));
                            break;
                        case "r":
                            if (splitMessage.Length == 7)
                            {
                                Quaternion quat = new Quaternion(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]), float.Parse(splitMessage[6]));
                                Vector3 rot = quat.eulerAngles;

                                if (receiveNcam) Camera.main.transform.rotation = Quaternion.Euler(new Vector3(rot.x, -rot.y, -rot.z + 180));
                                else camObject.rotation = Quaternion.Euler(new Vector3(rot.x, -rot.y, -rot.z + 180));
                            }
                            break;
                        case "f":
                            if (splitMessage.Length == 4)
                                if (receiveNcam) Camera.main.fieldOfView = float.Parse(splitMessage[3]);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Transform obj = getOjectFromString(splitMessage[2]);
                    //print( message + " " + currentlyLockedObject  + " " + obj + "endl" );

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
										obj.localPosition = new Vector3(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));
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
										obj.localRotation = new Quaternion(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]), float.Parse(splitMessage[6]));
									}
									catch {}
								}
								break;
	                        case "s":
	                            if (splitMessage.Length == 6)
									try
									{
		                                obj.localScale = new Vector3(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));
									}
									catch {}
	                            break;
	                        case "c":
	                            if (splitMessage.Length == 6)
								{
									try
									{
	                                	obj.GetChild(0).GetComponent<Light>().color = new Color(float.Parse(splitMessage[3]), float.Parse(splitMessage[4]), float.Parse(splitMessage[5]));
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
		                                obj.GetChild(0).GetComponent<Light>().intensity = float.Parse(splitMessage[3]);
									}
									catch {}
	                            break;
	                        case "a":
	                            if (splitMessage.Length == 4)
									try
									{
	                                	obj.GetChild(0).GetComponent<Light>().spotAngle = float.Parse(splitMessage[3]);
									}
									catch {}	
									break;
	                        case "d":
	                            if (splitMessage.Length == 4)
									try
									{
	                                	obj.GetChild(0).GetComponent<Light>().range = float.Parse(splitMessage[3]);
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
	                                if (bool.Parse(splitMessage[3])) mainController.unselectIfSelected(obj);
										try
										{
	                            			obj.GetComponent<SceneObject>().locked = bool.Parse(splitMessage[3]);
										}
										catch {}
	                            break;
	                        default:
	                            break;
	                    }
	                }
	            }
	        }
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
	    //! client function, listening for messages in receiveMessageQueue from server (executed in separate thread)
	    //!
	    public void listener() 
	    {

            //create NetMQ context
            NetMQContext ctx = NetMQContext.Create();

            NetMQ.Sockets.SubscriberSocket receiver = ctx.CreateSubscriberSocket();
            receiver.Subscribe("client");
            receiver.Subscribe("ncam");
            receiver.Subscribe("record");

            receiver.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5556");

            lastReceiveTime = currentTimeTime;
            
            VPETSettings.Instance.msg = string.Format("Listener started");

            while (isRunning)
            {
                try
                {
                    this.receiveMessageQueue.Add(receiver.ReceiveString(NetMQ.zmq.SendReceiveOptions.DontWait).Substring(7));
                    lastReceiveTime = currentTimeTime;

                }
                catch (AgainException ex)
                {
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

	        
	    }
	
	    //!
	    //! sender function, sending messages in sendMessageQueue to server (executed in separate thread)
	    //!
	    public void publisher()
	    {
	        
	        //create NetMQ context
	        NetMQContext ctx = NetMQContext.Create();
	
	        NetMQ.Sockets.PublisherSocket sender = ctx.CreatePublisherSocket();
	        sender.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5557");
	
	        while (isRunning) 
	        {
	            if ( sendMessageQueue.Count > 0 )
	            {
	                Debug.Log("Publisher: " + sendMessageQueue[0] as string);
	                sender.Send("client " + sendMessageQueue[0] as string);
	                sendMessageQueue.RemoveAt(0);
	            }
	        }
	
	        sender.Disconnect("tcp://" + VPETSettings.Instance.serverIP + ":5557");
	        sender.Close();
	        
	    }
	
	    //!
	    //! sender function, sending messages in sendMessageQueue to katana server (executed in separate thread)
	    //!
	    public void sendKatana()
	    {
	        //create NetMQ context
	        NetMQContext ctx = NetMQContext.Create();
	
	        NetMQ.Sockets.PushSocket katanaSender = ctx.CreatePushSocket();
            katanaSender.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5555");

            //using (NetMQ.Poller poller = new NetMQ.Poller(katanaSender))
            //{
                while (isRunning)
                {


                    if (katanaSendMessageQueue.Count > 0)
                    {
                        // Debug.Log(katanaSendMessageQueue[0] as string);
                        try
                        {
                            katanaSender.Send(katanaSendMessageQueue[0] as string, true); // TODO: note added true argument to not wait
                        }
                        catch
                        {
                            Debug.Log("Failed katanaSenMessage");
                        }
                        katanaSendMessageQueue.RemoveAt(0);
                    }
                }
            //}
	        katanaSender.Disconnect( "tcp://" + VPETSettings.Instance.serverIP + ":5555" );
	        katanaSender.Close();
	        
	    }
	
	    //!
	    //! receiver function, receiving the initial scene from the katana server (executed in separate thread)
	    //!
	    public void sceneReceiver()
	    {
	        
	        //create NetMQ context
	        NetMQContext ctx = NetMQContext.Create();
	
	        print("Trying to receive scene.");

			OnProgress( 0.1f, "Init Scene Receiver..");


	        NetMQ.Sockets.RequestSocket sceneReceiver = ctx.CreateRequestSocket();
	        sceneReceiver.Connect("tcp://" + VPETSettings.Instance.serverIP + ":5565");
	
	        print("Server set up.");
	

            byte[] byteStream;

            
            // HEader
            print("header");
            sceneReceiver.Send("header");
            byteStream = sceneReceiver.Receive() as byte[];
            print("byteStreamHeader size: " + byteStream.Length);
            if (doWriteScene)
            {
                writeBinary(byteStream, "header");
            }

            int dataIdx = 0;
            VPETSettings.Instance.lightIntensityFactor = BitConverter.ToSingle(byteStream, dataIdx);
            dataIdx += sizeof(float);
            VPETSettings.Instance.textureBinaryType = BitConverter.ToInt32(byteStream, dataIdx);
            

            //VpetHeader vpetHeader = SceneDataHandler.ByteArrayToStructure<VpetHeader>(byteStream, ref dataIdx);
            //VPETSettings.Instance.lightIntensityFactor = vpetHeader.lightIntensityFactor;
            //VPETSettings.Instance.textureBinaryType = vpetHeader.textureBinaryType;

            // Textures
            if ( VPETSettings.Instance.doLoadTextures)
	        {
	            print("textures");
	            sceneReceiver.Send("textures");
	            byteStream = sceneReceiver.Receive() as byte[];
	            print("byteStreamTextures size: " + byteStream.Length);
                if (doWriteScene)
                {
                    writeBinary(byteStream, "textu");
                }
                sceneLoader.SceneDataHandler.TexturesByteData = byteStream;

                OnProgress(0.33f, "..Received Texture..");
            }

            
            // Objects
            print( "objects" );
	        sceneReceiver.Send( "objects" );
            byteStream = sceneReceiver.Receive() as byte[];
	        print( "byteStreamObjects size: " + byteStream.Length );
            if (doWriteScene)
            {
                writeBinary(byteStream, "objec");
            }
            sceneLoader.SceneDataHandler.ObjectsByteData = byteStream;

			OnProgress( 0.80f, "..Received Objects..");
            

            // Nodes
	        print( "nodes" );
	        sceneReceiver.Send( "nodes" );
	        byteStream = sceneReceiver.Receive() as byte[];
	        print( "byteStreamNodess size: " + byteStream.Length );
            if (doWriteScene)
            {
                writeBinary(byteStream, "nodes");
            }
            sceneLoader.SceneDataHandler.NodesByteData = byteStream;

            OnProgress(0.9f, "..Received Nodes..");


	
	        sceneReceiver.Disconnect("tcp://" + VPETSettings.Instance.serverIP + ":5565");
	        sceneReceiver.Close();
	
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

            /*
            // HEader
            byte[] byteStreamHeader = loadBinary("header");
            print("byteStreamHeader size: " + byteStreamHeader.Length);
            int dataIdx = 0;
            VpetHeader vpetHeader = SceneDataHandler.ByteArrayToStructure<VpetHeader>(byteStreamHeader, ref dataIdx);
            VPETSettings.Instance.lightIntensityFactor = vpetHeader.lightIntensityFactor;
            VPETSettings.Instance.textureBinaryType = vpetHeader.textureBinaryType;
            */

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
	        Debug.Log(receiveMessageQueue.Count);
	        Debug.Log(katanaSendMessageQueue.Count);
	        Debug.Log(sendMessageQueue.Count);
	        isRunning = false;
	        if ( receiverThread != null && receiverThread.IsAlive )
	            receiverThread.Abort();
	
	        if (senderThread != null && senderThread.IsAlive )
	            senderThread.Abort();
	
	        if ( katanaSenderThread != null && katanaSenderThread.IsAlive )
	            katanaSenderThread.Abort();
	
	        if ( sceneReceiverThread != null  && sceneReceiverThread.IsAlive )
	            sceneReceiverThread.Abort();
	    }
	
	
}}
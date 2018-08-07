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
using System.IO;
using System;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;


namespace vpet
{
	public class ObjectSenderKatana: ObjectSender
	{

		protected static readonly new ObjectSenderKatana instance = new ObjectSenderKatana();
	    public static new ObjectSender Instance
	    {
	        get
	        {
	            return (ObjectSender)instance;
	        }
	    }


		private string objTemplateQuat = "";
		private string lightTransRotTemplate = "";
		private string camTransRotTemplate = "";
		private string lightIntensityColorTemplate = "";

        protected PushSocket sender = null;

        public override void Publisher()
        {
            AsyncIO.ForceDotNet.Force();

            sender = new PushSocket();
            sender.Connect("tcp://" + IP + ":" + Port);
            Debug.Log("Connect ObjectSender to: " + "tcp://" + IP + ":" + Port);
            while (IsRunning)
            {
                Thread.Sleep(5);
                //if (sendMessageQueue.Count > 3)
                //    sendMessageQueue.RemoveRange(0, sendMessageQueue.Count - 3);
                if (sendMessageQueue.Count > 0)
                {
                    // Debug.Log("Send: " + sendMessageQueue[0]);
                    sender.SendFrame(sendMessageQueue[0], false); // true not wait
                    sendMessageQueue.RemoveAt(0);
                }
            }

            // disconnectClose();	
        }

        protected override void disconnectClose()
        {
            if (sender != null)
            {
                // TODO: check first if closed
                sender.Disconnect("tcp://" + IP + ":" + Port);
                sender.Close();
                sender.Dispose();
                sender = null;
            }
            //NetMQConfig.Cleanup();
        }


        ObjectSenderKatana()
		{	
			// override port
			Port = "5555";

			// load templates
			// TODO: could be hard coded in this class
			TextAsset binaryData = Resources.Load("VPET/TextTemplates/objTemplateQuat") as TextAsset;
			objTemplateQuat = binaryData.text;

	        binaryData = Resources.Load("VPET/TextTemplates/lightTransRotTemplate") as TextAsset;
	        lightTransRotTemplate = binaryData.text;

            binaryData = Resources.Load("VPET/TextTemplates/camTransRotTemplate") as TextAsset;
            camTransRotTemplate = binaryData.text;

	        binaryData = Resources.Load("VPET/TextTemplates/lightIntensityColorTemplate") as TextAsset;
	        lightIntensityColorTemplate = binaryData.text;


		}


		public override void SendObject(string id, SceneObject sceneObject, string dagPath, NodeType nodeType, params object[] args)
		{
            // HACK check missing '/' upstream
            dagPath = "/" + dagPath;

	        if ( sceneObject.GetType() == typeof(SceneObject) )
			{
				if (nodeType == NodeType.LIGHT)
				{
					if (sceneObject.IsLight)
					{
						Light light = sceneObject.SourceLight;

						sendMessageQueue.Add(String.Format(lightIntensityColorTemplate,
							dagPath,
							((LightTypeKatana)(light.type)).ToString(),
							light.intensity / VPETSettings.Instance.lightIntensityFactor,
							light.color.r + " " + light.color.g + " " + light.color.b,
							sceneObject.exposure,
							light.spotAngle	));
					}
				}
				else if (nodeType == NodeType.CAMERA)
				{

				}
				else // send transform
				{
					
					if (sceneObject.IsLight) // do transform for lights to katana differently
					{
						Transform obj = sceneObject.transform;

						Vector3 pos = obj.localPosition;
						Quaternion rot = obj.localRotation;
						Vector3 scl = obj.localScale;

						Quaternion rotY180 = Quaternion.AngleAxis(180, Vector3.up);
						rot = rot * rotY180;
						float angle = 0;
						Vector3 axis = Vector3.zero;
						rot.ToAngleAxis( out angle, out axis );

						sendMessageQueue.Add(String.Format(lightTransRotTemplate,
							dagPath,
							(-pos.x + " " + pos.y + " " + pos.z),
							(angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
							(scl.x + " " + scl.y + " " + scl.z) ));

					}
					else if (sceneObject.transform.GetComponent<CameraObject>() != null) // do camera different too --> in fact is the same as for lights??
					{
						Transform obj = sceneObject.transform;

						Vector3 pos = obj.localPosition;
						Quaternion rot = obj.localRotation;
						Vector3 scl = obj.localScale;


						Quaternion rotY180 = Quaternion.AngleAxis(180, Vector3.up);
						rot = rot * rotY180;
						float angle = 0;
						Vector3 axis = Vector3.zero;
						rot.ToAngleAxis(out angle, out axis);

						sendMessageQueue.Add(String.Format(camTransRotTemplate,
							dagPath,
							(-pos.x + " " + pos.y + " " + pos.z),
							(angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
							(scl.x + " " + scl.y + " " + scl.z)));

					}
					else
					{

						Transform obj = sceneObject.transform;

						Vector3 pos = obj.localPosition;
						Quaternion rot = obj.localRotation;
						Vector3 scl = obj.localScale;

						float angle = 0;
						Vector3 axis = Vector3.zero;
						rot.ToAngleAxis( out angle, out axis );

						sendMessageQueue.Add(String.Format(objTemplateQuat,
							dagPath,
							(-pos.x + " " + pos.y + " " + pos.z),
							(angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
							(scl.x + " " + scl.y + " " + scl.z) ) );
					}
				}
			}
			else //light, camera if ( sobj.GetType() == typeof(SceneObject) )
			{

			}
			
		}

	}

	
}

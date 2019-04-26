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
using System.Text;
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
        private Transform root;

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

                    try
                    {
                        sender.SendFrame(sendMessageQueue[0], false); // true not wait
                        sendMessageQueue.RemoveAt(0);
                    }
                    catch { }
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

            root = GameObject.Find("Scene").transform;
        }


		public override void SendObject(byte cID, SceneObject sceneObject, ParameterType paramType)
		{
            if (!sceneObject)
                return;

            string dagPath = getPathString(sceneObject.transform, root);
            // HACK check missing '/' upstream
            dagPath = "/" + dagPath;

            //Debug.Log(dagPath);

            NodeType nodeType = NodeType.GROUP;
            if (sceneObject is SceneObjectLight)
                nodeType = NodeType.LIGHT;
            else if (sceneObject is SceneObjectCamera)
                nodeType = NodeType.CAMERA;
     
            if (paramType == ParameterType.POS ||
               paramType == ParameterType.ROT ||
               paramType == ParameterType.SCALE)

            {
                if (nodeType == NodeType.LIGHT) // do transform for lights to katana differently
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

                    sendMessageQueue.Add(Encoding.UTF8.GetBytes(String.Format(lightTransRotTemplate,
                        dagPath,
                        (-pos.x + " " + pos.y + " " + pos.z),
                        (angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
                        (scl.x + " " + scl.y + " " + scl.z))));
                }
                else if (nodeType == NodeType.CAMERA) // do camera different too --> in fact is the same as for lights??
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

                    sendMessageQueue.Add(Encoding.UTF8.GetBytes(String.Format(camTransRotTemplate,
                        dagPath,
                        (-pos.x + " " + pos.y + " " + pos.z),
                        (angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
                        (scl.x + " " + scl.y + " " + scl.z))));

                    //Debug.Log(String.Format(camTransRotTemplate,
                    //    dagPath,
                    //    (-pos.x + " " + pos.y + " " + pos.z),
                    //    (-angle + " " + -axis.x + " " + axis.y + " " + axis.z),
                    //    (scl.x + " " + scl.y + " " + scl.z)));
                }
                else
                {
                    Transform obj = sceneObject.transform;

                    Vector3 pos = obj.localPosition;
                    Quaternion rot = obj.localRotation;
                    Vector3 scl = obj.localScale;

                    float angle = 0;
                    Vector3 axis = Vector3.zero;
                    rot.ToAngleAxis(out angle, out axis);

                    sendMessageQueue.Add(Encoding.UTF8.GetBytes(String.Format(objTemplateQuat,
                        dagPath,
                        (-pos.x + " " + pos.y + " " + pos.z),
                        (angle + " " + axis.x + " " + -axis.y + " " + -axis.z),
                        (scl.x + " " + scl.y + " " + scl.z))));
                }
            }
            else if(paramType == ParameterType.INTENSITY ||
                    paramType == ParameterType.COLOR ||
                    paramType == ParameterType.EXPOSURE ||
                    paramType == ParameterType.ANGLE)
            {
                if (nodeType == NodeType.LIGHT)
                {
                    SceneObjectLight sol = (SceneObjectLight)sceneObject;
                    Light light = sol.SourceLight;
                    LightTypeKatana lightType = (LightTypeKatana)(light.type);

                    if (sol.isAreaLight)
                        lightType = LightTypeKatana.rect;

                    //sendMessageQueue.Add(Encoding.UTF8.GetBytes(String.Format(lightIntensityColorTemplate,
                    //    dagPath,
                    //    ((LightTypeKatana)(light.type)).ToString(),
                    //    light.color.r + " " + light.color.g + " " + light.color.b,
                    //    light.intensity / VPETSettings.Instance.lightIntensityFactor,
                    //    sol.exposure,
                    //    light.spotAngle)));

                    sendMessageQueue.Add(Encoding.UTF8.GetBytes(String.Format(lightIntensityColorTemplate,
                        dagPath,
                        (lightType).ToString(),
                        light.color.r + " " + light.color.g + " " + light.color.b,
                        light.intensity / VPETSettings.Instance.lightIntensityFactor,
                        sol.exposure,
                        light.spotAngle)));
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
                //if (obj.parent == Camera.main.transform)
                //{
                //    return getPathString(mainController.oldParent, root, separator) + separator + obj.name;
                //}
                if (obj.transform.parent == root)
                    return obj.name;
                else
                {
                    return getPathString(obj.parent, root, separator) + separator + obj.name;
                }
            }
            return obj.name;
        }

    }

	
}

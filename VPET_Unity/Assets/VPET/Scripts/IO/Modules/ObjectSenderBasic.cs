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
using NetMQ;
using NetMQ.Sockets;
using System.Threading;


namespace vpet
{
	public class ObjectSenderBasic: ObjectSender
	{
		protected static readonly new ObjectSenderBasic instance = new ObjectSenderBasic();
	    public static new ObjectSender Instance
	    {
	        get
	        {
	            return (ObjectSender)instance;
	        }
	    }

        protected PublisherSocket sender = null;

        public override void Publisher()
        {
            AsyncIO.ForceDotNet.Force();

            sender = new PublisherSocket();
            sender.Connect("tcp://" + IP + ":" + Port);
            Debug.Log("Connect ObjectSender to: " + "tcp://" + IP + ":" + Port);
            while (IsRunning)
            {
                Thread.Sleep(1);
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


        public override void SendObject(string id, SceneObject sceneObject, string dagPath, NodeType nodeType, params object[] args)
		{
			bool onlyToClientsWithoutPhysics = false;

			if (args.Length > 0)
			{
				onlyToClientsWithoutPhysics = (bool)args[0];
			}


	        if ( sceneObject.GetType() == typeof(SceneObject) )
			{

				if (nodeType == NodeType.LIGHT)
				{
					if (sceneObject.IsLight)
					{
						Light light = sceneObject.SourceLight;

						// color
						sendMessageQueue.Add("client " + id + "|" + "c" + "|" + dagPath + "|" + light.color.r + "|" + light.color.g + "|" + light.color.b);
						//intensity
						sendMessageQueue.Add("client " + id + "|" + "i" + "|" + dagPath + "|" + light.intensity );
						// cone
						sendMessageQueue.Add("client " + id + "|" + "a" + "|" + dagPath + "|" + light.spotAngle);
						// range
						sendMessageQueue.Add("client " + id + "|" + "d" + "|" + dagPath + "|" + light.range);
					}
					
				}	
				else if (nodeType == NodeType.CAMERA)
				{

				}
				else if (nodeType == NodeType.GEO) // send mesh i.e.
				{

				}			
				else // send transform
				{
	
					Transform obj = sceneObject.transform;

					string physicString = "";
					if (onlyToClientsWithoutPhysics) 
						physicString =  "|physics";

					// translate
					sendMessageQueue.Add("client " + id + "|" + "t" + "|" + dagPath + "|" + obj.localPosition.x + "|" + obj.localPosition.y + "|" + obj.localPosition.z + physicString);
					// rotate
					sendMessageQueue.Add("client " + id + "|" + "r" + "|" + dagPath + "|" + obj.localRotation.x + "|" + obj.localRotation.y + "|" + obj.localRotation.z + "|" + obj.localRotation.w + physicString);
					// scale
					sendMessageQueue.Add("client " + id + "|" + "s" + "|" + dagPath + "|" + obj.localScale.x + "|" + obj.localScale.y + "|" + obj.localScale.z);
				}
			}
			else //light, camera if ( sobj.GetType() == typeof(SceneObjectLight) )
			{

			}
			
		}

	}

	
}

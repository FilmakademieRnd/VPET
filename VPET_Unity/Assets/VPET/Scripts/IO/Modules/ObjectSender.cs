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

namespace vpet
{
	public class ObjectSender
	{
		//!
		//! Singleton pattern
		//!
		protected static readonly ObjectSender instance = new ObjectSender();
	    public static ObjectSender Instance
	    {
	        get
	        {
	            return instance;
	        }
	    }


		public bool IsRunning = false;
		protected string IP = null;
		protected string Port = null;

		protected PublisherSocket sender = null;

		protected List<string> sendMessageQueue = new List<string>(); 


		public virtual void SendObject(string id, SceneObject sceneObject, string dagPath, NodeType nodeType, params object[] args) {}

		public void SendObject(string msg)
		{
			sendMessageQueue.Add(msg);	
		}

		public void SetTarget(string ip, string port)
		{
			if (IP == null)	IP = ip;
			if (Port == null) Port = port;			
		}

		public void Finish()
		{
			IsRunning = false;
			disconnectClose();
		}

		public void Publisher()
		{
			AsyncIO.ForceDotNet.Force();
	
	        sender = new PublisherSocket();
	        sender.Connect("tcp://" + IP + ":" + Port);
			Debug.Log("Connect ObjectSender to: " + "tcp://" + IP + ":" + Port);
	        while (IsRunning) 
	        {
	            if ( sendMessageQueue.Count > 0 )
	            {
					// Debug.Log("Send: " + sendMessageQueue[0]);
	                sender.SendFrame(sendMessageQueue[0], false); // true not wait
	                sendMessageQueue.RemoveAt(0);
	            }
	        }

			// disconnectClose();	
		}

		private void disconnectClose()
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

	}

	
}

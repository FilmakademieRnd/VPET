using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;

namespace vpet
{
	public class ObjectSender
	{
		//!
		//! Singleton pattern
		//!
		private static readonly ObjectSender instance = new ObjectSender();
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

		protected List<string> sendMessageQueue = new List<string>(); 

		public virtual void SendObject(string id, SceneObject sceneObject, string dagPath) {}

		public void Publisher()
		{
				        
	        //create NetMQ context
	        NetMQContext ctx = NetMQContext.Create();
	
	        NetMQ.Sockets.PublisherSocket sender = ctx.CreatePublisherSocket();
	        sender.Connect("tcp://" + IP + ":" + Port);
	
	        while (IsRunning) 
	        {
	            if ( sendMessageQueue.Count > 0 )
	            {
	                sender.Send("client " + sendMessageQueue[0] as string);
	                sendMessageQueue.RemoveAt(0);
	            }
	        }
	
	        sender.Disconnect("tcp://" + IP + ":" + Port);
	        sender.Close();
		}

	}

	
}

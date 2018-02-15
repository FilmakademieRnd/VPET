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

		protected NetMQ.Sockets.PublisherSocket sender = null;

		protected List<string> sendMessageQueue = new List<string>(); 

		public virtual void SendObject(string id, SceneObject sceneObject, string dagPath, NodeType nodeType, params object[] args) {}

		public void SetTarget(string ip, string port)
		{
			if (IP == null)
				IP = ip;

			if (Port == null)
				Port = port;			
		}

		public void Finish()
		{
			IsRunning = false;
			disconnectClose();
		}

		public void Publisher()
		{

	        //create NetMQ context
	        NetMQContext ctx = NetMQContext.Create();
	
	        sender = ctx.CreatePublisherSocket();
	        sender.Connect("tcp://" + IP + ":" + Port);
	
	        while (IsRunning) 
	        {
	            if ( sendMessageQueue.Count > 0 )
	            {
					//Debug.Log("Send: " + sendMessageQueue[0]);
	                sender.Send(sendMessageQueue[0], true); // true not wait
	                sendMessageQueue.RemoveAt(0);
	            }
	        }

			disconnectClose();	
		}

		private void disconnectClose()
		{
			if (sender != null)
			{
				// TODO: check first if closed
				sender.Disconnect("tcp://" + IP + ":" + Port);
				sender.Close();
				sender = null;
			}
		}

	}

	
}

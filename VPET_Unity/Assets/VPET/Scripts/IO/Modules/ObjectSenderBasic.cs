using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
	public class ObjectSenderBasic: ObjectSender
	{
		protected static readonly ObjectSenderBasic instance = new ObjectSenderBasic();
	    public static ObjectSender Instance
	    {
	        get
	        {
	            return (ObjectSender)instance;
	        }
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

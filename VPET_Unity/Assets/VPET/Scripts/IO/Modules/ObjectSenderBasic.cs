using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
	public class ObjectSenderBasic: ObjectSender
	{

		public override void SendObject(string id, SceneObject sceneObject, string dagPath)
		{
	        if ( sceneObject.GetType() == typeof(SceneObject) )
			{
				Transform obj = sceneObject.transform;

				// translate
				sendMessageQueue.Add("client " + id + "|" + "t" + "|" + dagPath + "|" + obj.localPosition.x + "|" + obj.localPosition.y + "|" + obj.localPosition.z + "|physics");
				// rotate
				sendMessageQueue.Add("client " + id + "|" + "r" + "|" + dagPath + "|" + obj.localRotation.x + "|" + obj.localRotation.y + "|" + obj.localRotation.z + "|" + obj.localRotation.w + "|physics");
				// scale
				sendMessageQueue.Add("client " + id + "|" + "s" + "|" + dagPath + "|" + obj.localScale.x + "|" + obj.localScale.y + "|" + obj.localScale.z);
			}
			else //light, camera if ( sobj.GetType() == typeof(SceneObject) )
			{

			}
			
		}

	}

	
}

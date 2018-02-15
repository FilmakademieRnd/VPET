using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;


namespace vpet
{
	public class ObjectSenderKatana: ObjectSender
	{

		private string objTemplateQuat = "";

		ObjectSenderKatana()
		{	
			// override port
			IP = "5555";

			TextAsset binaryData = Resources.Load("VPET/TextTemplates/objTemplateQuat") as TextAsset;
			objTemplateQuat = binaryData.text;
		}


		public override void SendObject(string id, SceneObject sceneObject, string dagPath)
		{
	        if ( sceneObject.GetType() == typeof(SceneObject) )
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
			else //light, camera if ( sobj.GetType() == typeof(SceneObject) )
			{

			}
			
		}

	}

	
}

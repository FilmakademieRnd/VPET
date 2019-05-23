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
using System;
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
                    //Debug.Log("Send: " + sendMessageQueue[0]);
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


		public override void SendObject(byte cID, SceneObject sceneObject, ParameterType paramType, bool sendParent, Transform parent) 
        {
            byte[] msg = null;
            switch (paramType)
            {
                case ParameterType.POS:
                    {
                        Vector3 locPos;
                        if (sendParent)
                            locPos = sceneObject.transform.position-parent.position;
                        else
                            locPos = sceneObject.transform.localPosition;
                        msg = new byte[18];

                        msg[0] = cID;
                        msg[1] = (byte)paramType;
                        Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locPos.x), 0, msg, 6, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locPos.y), 0, msg, 10, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locPos.z), 0, msg, 14, 4);
                    }
                    break;

                case ParameterType.ROT:
                    {
                        Quaternion locRot;
                        if (sendParent)
                            locRot = Quaternion.Inverse(parent.rotation) * sceneObject.transform.rotation;
                        else
                            locRot = sceneObject.transform.localRotation;
                       msg = new byte[22];

                        msg[0] = cID;
                        msg[1] = (byte)paramType;
                        Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locRot.x), 0, msg, 6, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locRot.y), 0, msg, 10, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locRot.z), 0, msg, 14, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locRot.w), 0, msg, 18, 4);
                    }
                    break;
                case ParameterType.SCALE:
                    {
                        Vector3 locScale = sceneObject.transform.localScale;
                        msg = new byte[18];

                        msg[0] = cID;
                        msg[1] = (byte)paramType;
                        Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locScale.x), 0, msg, 6, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locScale.y), 0, msg, 10, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(locScale.z), 0, msg, 14, 4);
                    }
                    break;
                case ParameterType.LOCK:
                    {
                        msg = new byte[7];

                        msg[0] = cID;
                        msg[1] = (byte) paramType;
                        Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                        msg[6] = Convert.ToByte(sceneObject.selected);
                    }
                    break;
                case ParameterType.HIDDENLOCK:
                    {
                        msg = new byte[7];

                        msg[0] = cID;
                        msg[1] = (byte)paramType;
                        Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                        msg[6] = Convert.ToByte(sceneObject.isPhysicsActive || sceneObject.isPlayingAnimation);
                    }
                    break;
                case ParameterType.KINEMATIC:
                    {
                        msg = new byte[7];

                        msg[0] = cID;
                        msg[1] = (byte)paramType;
                        Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                        msg[6] = Convert.ToByte(sceneObject.globalKinematic);
                    }
                    break;
                case ParameterType.FOV:
                    {
                        SceneObjectCamera soc = (SceneObjectCamera) sceneObject;
                        if (soc)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(soc.fov), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.ASPECT:
                    {
                        SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                        if (soc)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(soc.aspect), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.FOCUSDIST:
                    {
                        SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                        if (soc)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(soc.focDist), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.FOCUSSIZE:
                    {
                        SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                        if (soc)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(soc.focSize), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.APERTURE:
                    {
                        SceneObjectCamera soc = (SceneObjectCamera)sceneObject;
                        if (soc)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(soc.aperture), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.COLOR:
                    {
                        SceneObjectLight sol = (SceneObjectLight)sceneObject;
                        if (sol)
                        {
                            Color color = sol.getLightColor();
                            msg = new byte[18];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(color.r), 0, msg, 6, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(color.g), 0, msg, 10, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(color.b), 0, msg, 14, 4);
                        }
                    }
                    break;
                case ParameterType.INTENSITY:
                    {
                        SceneObjectLight sol = (SceneObjectLight)sceneObject;
                        if (sol)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(sol.getLightIntensity()), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.EXPOSURE:
                    {
                        SceneObjectLight sol = (SceneObjectLight)sceneObject;
                        if (sol)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(sol.exposure), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.RANGE:
                    {
                        SceneObjectLight sol = (SceneObjectLight)sceneObject;
                        if (sol)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(sol.getLightRange()), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.ANGLE:
                    {
                        SceneObjectLight sol = (SceneObjectLight)sceneObject;
                        if (sol)
                        {
                            msg = new byte[10];

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(sol.getLightAngle()), 0, msg, 6, 4);
                        }
                    }
                    break;
                case ParameterType.BONEANIM:
                    {
                        Animator animator = sceneObject.gameObject.GetComponent<Animator>();

                        if (animator)
                        {
                            msg = new byte[418];

                            Vector3 locPos = sceneObject.transform.localPosition;

                            msg[0] = cID;
                            msg[1] = (byte)paramType;
                            Buffer.BlockCopy(BitConverter.GetBytes((Int32)sceneObject.id), 0, msg, 2, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(locPos.x), 0, msg, 6, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(locPos.y), 0, msg, 10, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(locPos.z), 0, msg, 14, 4);
                            int offset = 12;
                            for(int i = 0; i < 25; i++)
                            {
                                Transform t = animator.GetBoneTransform((HumanBodyBones)i);
                                if((HumanBodyBones)i == HumanBodyBones.LeftUpperLeg)
                                    Debug.Log(t.localRotation);
                                if (t)
                                {
                                    Buffer.BlockCopy(BitConverter.GetBytes(t.localRotation.x), 0, msg, offset + 6, 4);
                                    Buffer.BlockCopy(BitConverter.GetBytes(t.localRotation.y), 0, msg, offset + 10, 4);
                                    Buffer.BlockCopy(BitConverter.GetBytes(t.localRotation.z), 0, msg, offset + 14, 4);
                                    Buffer.BlockCopy(BitConverter.GetBytes(t.localRotation.w), 0, msg, offset + 18, 4);
                                    offset += 16;
                                }                              
                            }
                        }
                    }
                    break;
                case ParameterType.PING:
                    {
                        msg = new byte[2];

                        msg[0] = cID;
                        msg[1] = (byte)paramType;
                    }
                    break;
                case ParameterType.RESENDUPDATE:
                    {
                        msg = new byte[2];

                        msg[0] = cID;
                        msg[1] = (byte)paramType;
                    }
                    break;
                default:
                    {
                        Debug.Log("Unknown paramType in ObjectSenderBasic:SendObject");
                    }
                    break;
            }
            if (msg != null)
            {
                sendMessageQueue.Add(msg);
            }
		}
	}
}

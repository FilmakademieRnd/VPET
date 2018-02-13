/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

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
﻿using UnityEngine;
using System.Collections;

namespace vpet
{
	public class GyroAdapter
	{
	
	    private bool isGyroSupported = false;
	
	    private Gyroscope gyro = null;
	
	    private Quaternion quatMult = Quaternion.identity;
	
	    private Quaternion quatMap  = Quaternion.identity;
	
	    public Quaternion Rotation
	    {
	        get {
	            GetGyroRotation();
	            return quatMap; // * quatMult;
	        }
	    }
	
	    public GyroAdapter()
	    {
	
	        if (SystemInfo.supportsGyroscope)
	        {
	            isGyroSupported = true;
	            gyro = Input.gyro;
	            gyro.enabled = true;
	            
	            #if UNITY_IPHONE
	            if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
	                quatMult = new Quaternion(0f,0f,0.7071f,0.7071f);
	            } else if (Screen.orientation == ScreenOrientation.LandscapeRight) {
	                quatMult = new Quaternion(0f,0f,-0.7071f,0.7071f);
	            } else if (Screen.orientation == ScreenOrientation.Portrait) {
	                quatMult = new Quaternion(0f,0f,1f,0f);
	            } else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
	                quatMult = new Quaternion(0f,0f,0f,1f);
	            }
	            #endif
	
	            #if UNITY_ANDROID
	            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
	            {
	                quatMult = new Quaternion(0, 0, 0.7071f, -0.7071f);
	            }
	            else if (Screen.orientation == ScreenOrientation.LandscapeRight)
	            {
	                quatMult = new Quaternion(0, 0, -0.7071f, -0.7071f);
	            }
	            else if (Screen.orientation == ScreenOrientation.Portrait)
	            {
	                quatMult = new  Quaternion(0, 0, 0, 1);
	            }
	            else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
	            {
	                quatMult = new Quaternion(0, 0, 1, 0);
	            }
	            #endif
	
	            quatMult = new Quaternion(0, 0, 1, 0);
	
	            // Screen.sleepTimeout = SleepTimeout.NeverSleep;
	        }
	
	
	    }
	
		private void GetGyroRotation()
	    {
	        if (isGyroSupported)
	        {
	            // transform.rotation = Input.gyro.attitude;
	            quatMap = new Quaternion(gyro.attitude.x, gyro.attitude.y, -gyro.attitude.z, -gyro.attitude.w);
	
	#if UNITY_ANDROID
	            //transform.Rotate(0f, 0f, 180f, Space.Self); //Swap "handedness" ofquaternionfromgyro.
	            //transform.Rotate(270f, 180f, 180f, Space.World); //Rotatetomakesenseasacamerapointingoutthebackofyourdevice.
	#else
	        //transform.Rotate(0f, 0f, 180f, Space.Self); //Swap "handedness" ofquaternionfromgyro.
	        //transform.Rotate(90f, 180f, 0f, Space.World); //Rotatetomakesenseasacamerapointingoutthebackofyourdevice.
	#endif
	            // appliedGyroYAngle = transform.eulerAngles.y; // Save the angle around y axis for use in calibration.
	
	        }
	
	    }
	
	
	}

}
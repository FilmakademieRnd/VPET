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
﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace vpet
{
	public sealed class VPETSettings
	{
	    private static readonly VPETSettings instance = new VPETSettings();
	
	    //!
	    //! IP Adress of server
	    //!
        [Config]
	    public string serverIP = "111.111.111.111";
        //!
        //! Do we load scene from dump file
        //!
        [Config]
        public bool doLoadFromResource = false;
        //!
        //! Dump scene file name
        //!
        public string sceneFileName = "VpetDemo";
        //!
        //! Shall we load Textures ?
        //!
        [Config]
        public bool doLoadTextures = true;
		//!
		//! Value of scene ambient light
		//!
		[Config]
		public float ambientLight = 0.1f;
		//!
		//! Value of AR tracking scale
		//!
		[Config]
		public float trackingScale = 1f;
		//!
		//! Property to store camera offset in orthografic view
		//!
		[Config]
		public Vector3 cameraOffsetTop = Vector3.zero;
		//!
		//! Property to store camera offset in orthografic view
		//!
		[Config]
		public Vector3 cameraOffsetFront = Vector3.zero;
		//!
		//! Property to store camera offset in orthografic view
		//!
		[Config]
		public Vector3 cameraOffsetLeft = Vector3.zero;
		//!
		//! Property to store camera offset in orthografic view
		//!
		[Config]
		public Vector3 cameraOffsetRight = Vector3.zero;
		//!
		//! Property to store camera offset in orthografic view
		//!
		[Config]
		public Vector3 cameraOffsetBottom = Vector3.zero;

		public string msg = "";

        public bool sceneDumpFolderEmpty = true;
        public bool debugMsg = false;
        
        public int canvasHalfWidth = 400;
	    public int canvasHalfHeight = 300;
	    public float canvasScaleFactor = 1f;
	    public float canvasAspectScaleFactor = 1f;
		public float canvasScale = 1f;
        public float lightIntensityFactor = 1; // liveview 50;
        public int textureBinaryType =1;
        public float sceneScale = 1f;
	
	    // Explicit static constructor to tell C# compiler
	    // not to mark type as beforefieldinit
	    static VPETSettings() { 
		}
	
	    private VPETSettings()
		{
    		GameObject canvas = GameObject.Find("GUI/Canvas");
            if (canvas == null) Debug.LogError(string.Format("{0}: Cant find Canvas.", this.GetType()));
            else
            {
                CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
                canvasHalfWidth = (int)(canvasScaler.referenceResolution.x / 2);
                canvasHalfHeight = (int)(canvasScaler.referenceResolution.y / 2);
                float w = canvasScaler.referenceResolution.x;
                float h = canvasScaler.referenceResolution.y;
                float w2 = Screen.width;
                float h2 = Screen.height;
                canvasScaleFactor = w2 / w;
                canvasAspectScaleFactor = ((w / h) / (w2 / h2));
            }
			}
	
	    public static VPETSettings Instance
	    {
	        get
	        {
	            return instance;
	        }
	    }
	
	
	
	    public static void mapValuesFromConfigFile( string configFilePath)
	    {
	        // copy property values from config file (txt) to dictionary 
	        Dictionary<string, string> config = new Dictionary<string, string>();
	        if (File.Exists(configFilePath))
	        {
	            string line;
	            try
	            {
	                using (StreamReader sr = new StreamReader(configFilePath))
	                {
	                    do
	                    {
	                        line = sr.ReadLine();
	                        if (line != null)
	                        {
	                            line = line.Trim();
	                            if (line != "" && !line.StartsWith("#"))
	                            {
	                                string[] parts = line.Split(':');
	                                if (parts.Length > 1)
	                                {
	                                    config.Add(parts[0], parts[1]);
	                                }
	                            }
	                        }
	                    }
	                    while (line != null);
	                }
					VPETSettings.Instance.msg = string.Format( "Load config file: {0}", configFilePath );
	            }
	            catch (Exception e)
	            {
	                Debug.Log(String.Format("Cant read file {0} ({1})", configFilePath, e.Message));
	            }
	        }
	
	
	        // now copy from dictionary to instant properties
	        foreach (KeyValuePair<string, string> pair in config)
	        {
	            FieldInfo fi_globals = Instance.GetType().GetField(pair.Key, BindingFlags.Public | BindingFlags.Instance);
	            if (fi_globals != null)
	            {
                    //Console.WriteLine(String.Format("Set Property value: {0} to: {1}", pair.Key, pair.Value));
                    if ( fi_globals.GetValue( Instance ).GetType() == typeof( Vector3 ))
					{
					}
					else if	( fi_globals.GetValue( Instance ).GetType() == typeof( Vector2 ))
					{
					}
	                else
	                {
	                    fi_globals.SetValue(Instance, Convert.ChangeType(pair.Value, fi_globals.GetValue(Instance).GetType()));
	                }
	            }
	        }
	    }


        public static void mapValuesToConfigFile(string configFilePath)
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(configFilePath))
                    {
                        FieldInfo[] fis = Instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                        foreach (FieldInfo info in fis)
                        {
                            if (info.GetCustomAttributes(typeof(ConfigAttribute), false).Length > 0)
                            {
                                string dataline = String.Format("{0}:{1}", info.Name.ToString(), info.GetValue(Instance).ToString());
                                sw.WriteLine(dataline);
                            }
                        }
                    }
					VPETSettings.Instance.msg = string.Format( "Wrote config file: {0}", configFilePath );
				}
                catch (Exception e)
                {
                    Debug.Log(String.Format("Cant write to file {0} ({1})", configFilePath, e.Message));
                }

            }
        }

        public static void mapValuesToObject( object target )
	    {
	        // copy property values from target class to instance 
	        FieldInfo[] fis = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
	        foreach (FieldInfo info in fis)
	        {
	            FieldInfo fi_globals = Instance.GetType().GetField(info.Name, BindingFlags.Public | BindingFlags.Instance);
	            if (fi_globals != null)
	            {
	                info.SetValue( target, fi_globals.GetValue(Instance));
	            }
	        }
	    }
	
	
	    public static void mapValuesFromObject(object source)
	    {
	        // copy property values from source class to instance 
	        FieldInfo[] fis = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
	        foreach (FieldInfo info in fis)
	        {
	            FieldInfo fi_globals = Instance.GetType().GetField(info.Name, BindingFlags.Public | BindingFlags.Instance);
	            if (fi_globals != null)
	            {
	                fi_globals.SetValue(Instance, info.GetValue(source));
	            }
	        }
	    }
	
		public static void mapValuesFromPreferences()
		{
			VPETSettings.Instance.msg = "Load User Preferences.";
			FieldInfo[] fis = Instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (FieldInfo info in fis)
			{
				if (info.GetCustomAttributes(typeof(ConfigAttribute), false).Length > 0 && PlayerPrefs.HasKey( info.Name ) )
				{
					if ( info.GetValue(Instance).GetType() == typeof(float) )
					{
						info.SetValue( Instance, PlayerPrefs.GetFloat( info.Name ) );
					}
					else if ( info.GetValue(Instance).GetType() == typeof(int) )
					{
						info.SetValue( Instance, PlayerPrefs.GetInt(info.Name) );
					}
					else if ( info.GetValue(Instance).GetType() == typeof(bool))
					{
						info.SetValue( Instance, Convert.ToBoolean( PlayerPrefs.GetInt(info.Name) ) );
					}
					else if ( info.GetValue(Instance).GetType() == typeof(Vector3))
					{
						info.SetValue( Instance, DeserializeVector3( PlayerPrefs.GetString(info.Name) ) );
					}
					else 
					{
						info.SetValue( Instance, PlayerPrefs.GetString( info.Name ) );
					}
				}
			}
		}

		public static void mapValuesToPreferences()
		{
			FieldInfo[] fis = Instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (FieldInfo info in fis)
			{
				if (info.GetCustomAttributes(typeof(ConfigAttribute), false).Length > 0)
				{
					if ( info.GetValue(Instance).GetType() == typeof(float) )
					{
						PlayerPrefs.SetFloat( info.Name, (float)info.GetValue(Instance) ) ;
					}
					else if ( info.GetValue(Instance).GetType() == typeof(int) )
					{
						PlayerPrefs.SetInt( info.Name, (int)info.GetValue(Instance) );
					}
					else if (  info.GetValue(Instance).GetType() == typeof(bool) )
					{
						PlayerPrefs.SetInt( info.Name, Convert.ToInt32( (bool)info.GetValue(Instance) ) );
					}
					else if ( info.GetValue(Instance).GetType() == typeof(Vector3))
					{
						PlayerPrefs.SetString( info.Name, SerializeVector3( (Vector3)info.GetValue(Instance) ) );
					}
					else 
					{
						PlayerPrefs.SetString( info.Name, (string)info.GetValue(Instance) );
					}
				}
			}
			VPETSettings.Instance.msg = "Save User Preferences.";
			PlayerPrefs.Save();
		}
	
	    public static void printValues()
	    {
	        // print instance public properties
	        FieldInfo[] fis = Instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
	        foreach (FieldInfo info in fis)
	        {
	            Debug.Log(string.Format("Globals property: {0} Value: {1}", info.Name, info.GetValue(Instance)));
	        }
	    }
	

		public static string SerializeVector3( Vector3 vec )
		{
			return String.Format("{0} {1} {2}", vec.x, vec.y, vec.z );
		}

		public static Vector3 DeserializeVector3( string vecString )
		{
			string[] parts = vecString.Split(' ');
			try {
				return new Vector3( float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]) );
			} catch {
				return Vector3.zero;
			}
		}


}

    internal class ConfigAttribute : Attribute
    {
    }
}
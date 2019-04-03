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
        //! The maximum extend of the scene
        //!
        public Vector3 sceneBoundsMax;
        public Vector3 sceneBoundsMin;
        public float maxExtend = 1f;

	    //!
	    //! IP Adress of server
	    //!
        [Config]
	    public string serverIP = "111.111.111.111";
        //!
        //! Do we load scene from dump file
        //!
        [Config]
        public bool doLoadFromResource = true;
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
	    public float canvasAspectScaleFactor = 1f;
        public float lightIntensityFactor = 1; // liveview 50;
        public int textureBinaryType =1;

        public float sceneScale = 1f;

        [Config]
        public float controllerSpeed = 1f;


        // Dictionary< Name of the property in the material (target), KeyValuePair< name of the property at the node(source), type of target value > >
        public static Dictionary<string, KeyValuePair<string, Type>> ShaderPropertyMap = new Dictionary<string, KeyValuePair<string, Type> > {
            {"_Color", new KeyValuePair<string, Type> ("color", typeof(Color))},
            {"_Glossiness", new KeyValuePair<string, Type> ("roughness", typeof(float))},
            {"_MainTex", new KeyValuePair<string, Type> ("textureId", typeof(Texture))}
        };

   	    // Explicit static constructor to tell C# compiler
	    // not to mark type as beforefieldinit
	    static VPETSettings() { 
		}
	
	    private VPETSettings()
		{
#if !SCENE_HOST
            GameObject canvas = GameObject.Find("GUI/Canvas");
            if (canvas == null) Debug.Log(string.Format("{0}: Cant find Canvas.", this.GetType()));
            else
            {
                CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
                canvasHalfWidth = (int)(canvasScaler.referenceResolution.x / 2);
                canvasHalfHeight = (int)(canvasScaler.referenceResolution.y / 2);
                float w = canvasScaler.referenceResolution.x;
                float h = canvasScaler.referenceResolution.y;
                float w2 = Screen.width;
                float h2 = Screen.height;
                canvasAspectScaleFactor = ((w / h) / (w2 / h2));
            }
#endif
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
            mapValuesFromPreferences(Instance);
        }

		public static void mapValuesFromPreferences(object target)
		{
            string prefix = string.Format("{0}.", target.GetType().Name);
            FieldInfo[] fis = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (FieldInfo info in fis)
			{
                if (info.GetCustomAttributes(typeof(ConfigAttribute), false).Length > 0 && PlayerPrefs.HasKey( prefix + info.Name ) )
				{
                    if ( info.GetValue(target).GetType() == typeof(float) )
					{
                        info.SetValue( target, PlayerPrefs.GetFloat( prefix + info.Name ) );
					}
                    else if ( info.GetValue(target).GetType() == typeof(int) )
					{
                        info.SetValue( target, PlayerPrefs.GetInt(prefix + info.Name) );
					}
                    else if ( info.GetValue(target).GetType() == typeof(bool))
					{
                        info.SetValue( target, Convert.ToBoolean( PlayerPrefs.GetInt(prefix + info.Name) ) );
					}
                    else if ( info.GetValue(target).GetType() == typeof(Vector3))
					{
                        info.SetValue( target, DeserializeVector3( PlayerPrefs.GetString(prefix + info.Name) ) );
					}
                    else if (info.GetValue(target).GetType() == typeof(Vector4))
                    {
                        info.SetValue(target, DeserializeVector4(PlayerPrefs.GetString(prefix + info.Name)));
                    }
                    else if (info.GetValue(target).GetType() == typeof(Color))
                    {
                        info.SetValue(target, (Color)DeserializeVector4(PlayerPrefs.GetString(prefix + info.Name)));
                    }
                    else 
					{
                        info.SetValue( target, PlayerPrefs.GetString( prefix +  info.Name ) );
					}
				}
			}
		}

        public static void mapValuesToPreferences()
        {
            VPETSettings.Instance.msg = "Save User Preferences.";
            mapValuesToPreferences(Instance);
        }

		public static void mapValuesToPreferences(object source)
		{
            string prefix = string.Format("{0}.", source.GetType().Name);
            FieldInfo[] fis = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (FieldInfo info in fis)
			{
				if (info.GetCustomAttributes(typeof(ConfigAttribute), false).Length > 0)
				{
                    if ( info.GetValue(source).GetType() == typeof(float) )
					{
                        PlayerPrefs.SetFloat( prefix + info.Name, (float)info.GetValue(source) ) ;
					}
                    else if ( info.GetValue(source).GetType() == typeof(int) )
					{
                        PlayerPrefs.SetInt( prefix + info.Name, (int)info.GetValue(source) );
					}
                    else if (  info.GetValue(source).GetType() == typeof(bool) )
					{
                        PlayerPrefs.SetInt( prefix + info.Name, Convert.ToInt32( (bool)info.GetValue(source) ) );
					}
                    else if ( info.GetValue(source).GetType() == typeof(Vector3))
					{
                        PlayerPrefs.SetString( prefix + info.Name, SerializeVector3( (Vector3)info.GetValue(source) ) );
					}
                    else if (info.GetValue(source).GetType() == typeof(Vector4))
                    {
                        PlayerPrefs.SetString(prefix + info.Name, SerializeVector4((Vector4)info.GetValue(source)));
                    }
                    else if (info.GetValue(source).GetType() == typeof(Color))
                    {
                        PlayerPrefs.SetString(prefix + info.Name, SerializeVector4((Color)info.GetValue(source)));
                    }
					else 
					{
                        PlayerPrefs.SetString( prefix + info.Name, (string)info.GetValue(source) );
					}
				}
			}
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

        public static string SerializeVector4(Vector4 vec)
        {
            return String.Format("{0} {1} {2} {3}", vec.x, vec.y, vec.z, vec.w);
        }

        public static Vector4 DeserializeVector4(string vecString)
        {
            string[] parts = vecString.Split(' ');
            try
            {
                return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            catch
            {
                return Vector4.zero;
            }
        }

}

    public class ConfigAttribute : Attribute
    {
    }

}
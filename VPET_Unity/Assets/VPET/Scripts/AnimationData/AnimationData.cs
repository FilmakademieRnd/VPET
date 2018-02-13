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
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

/*
 * Known Property Names:
 * m_LocalPosition.x
 * m_LocalPosition.y
 * m_LocalPosition.z
 * m_LocalRotation.x
 * m_LocalRotation.y
 * m_LocalRotation.z
 * m_LocalRotation.w
 * m_LocalScale.x
 * m_LocalScale.y
 * m_LocalScale.z
 * m_IsActive (enable / disable)
 * 
 */

//!
//! Singleton Class for the whole project holding a reference to
//! the Unity side Animation Data Structures during runtime
//!
namespace vpet
{
	public class AnimationData 
	{
	    //!
	    //! Dictionary to reference all AnimationClips used from one GameObject (through Animator Component)
	    //!
	    private Dictionary<GameObject, List<AnimationClip>> _animationClips;
	
	    //!
	    //! Dictionary to reference all AnimationCurves used by one specific AnimationClip
	    //!
	    private Dictionary<AnimationClip, List<AnimationCurve>> _animationCurves;
	
	    //!
	    //! Dictionary to build a relation between Unity AnimationCurves and XML Curves
	    //! This is useful since XML AnimationCurves contain more informations then the Unity ones
	    //!
	    private Dictionary<AnimationCurve, XML_AnimationCurve> _xmlCurves;
	
	    //!
	    //! getter for the data object
	    //!
	    public static AnimationData Data
	    {
	        get
	        {
	            if (_instance == null)
	            {
	                _instance = new AnimationData();
	            }
	            return _instance;
	        }
	    }
	
	    //!
		//! private static instance
	    //!
	    private static AnimationData _instance = null;
	
	    //!
	    //! private constructor
	    //!
	    private AnimationData()
	    {
	        _animationClips = new Dictionary<GameObject, List<AnimationClip>>();
	        _animationCurves = new Dictionary<AnimationClip, List<AnimationCurve>>();
	        _xmlCurves = new Dictionary<AnimationCurve, XML_AnimationCurve>();
	    }
	
	    //!
	    //! Adds a relation between a Unity AnimationCurve and a XML AnimationCurve
	    //!
	    public void addXMLCurve(AnimationCurve curve, XML_AnimationCurve xmlCurve)
	    {
	        _xmlCurves.Add(curve, xmlCurve);
	    }
	
	    //!
	    //! Returns the related XML AnimationCurve to a Unity AnimationCurve
	    //!
	    public XML_AnimationCurve getXMLCurve(AnimationCurve curve)
	    {
	        return _xmlCurves[curve];
	    }
	
	    //!
	    //! Returns all AnimationClips related to a gameobject (through animator component)
	    //!
	    public List<AnimationClip> getAnimationClips(GameObject obj)
	    {
	        if(_animationClips.ContainsKey(obj))
	            return _animationClips[obj];
	        return null;
	    }
	
	    //!
	    //! Returns all AnimationCurves related to a AnimationClip
	    //!
	    public List<AnimationCurve> getAnimationCurves(AnimationClip clip)
	    {
	        if (_animationCurves.ContainsKey(clip))
	            return _animationCurves[clip];
	        return null;
	    }
	
	    //!
	    //! Returns the System Type a AnimationCurve is working on (saved via XML) 
	    //!
	    public System.Type getCurveType(AnimationCurve curve)
	    {
	        XML_AnimationCurve xml = getXMLCurve(curve);
	        if (xml != null)
	        {
	            return GetTheType(xml.type);
	        }
	        return null;
	    }
	
	    //!
	    //! Returns the animation property a AnimationCurve is working on (saved via XML) 
	    //!
	    public string getCurveProperty(AnimationCurve curve)
	    {
	        XML_AnimationCurve xml = getXMLCurve(curve);
	        if (xml != null)
	        {
	            return xml.property;
	        }
	        return null;
	    }
	
	    //!
	    //! Returns a AnimationCurve by its property name and its related AnimationClip
	    //!
	    public AnimationCurve getAnimationCurve(AnimationClip clip, string propertyName)
	    {
	        foreach (AnimationCurve curve in _animationCurves[clip])
	        {
	            if(getXMLCurve(curve).property == propertyName)
	                return curve;
	        }
	        return null;
	    }
	
	    //!
	    //! Change a existing AnimationCurve from a Clip
	    //!
	    public void changeAnimationCurve(AnimationClip clip, System.Type type, string propertyName, AnimationCurve curve)
	    {
	        clip.SetCurve(null, type, propertyName, curve);
	    }
	
	    //!
	    //! Delete a existing AnimationCurve from a Clip
	    //!
	    public void deleteAnimationCurves(AnimationClip clip, System.Type type, string propertyName)
	    {
	        List<AnimationCurve> curves = _animationCurves[clip];
	        if (curves != null)
	        {
	            curves.RemoveRange(0, curves.Count);
	        }
	//        clip.SetCurve(null, type, propertyName, null);
	    }
	
	    //!
	    //! Add an AnimationClip to a GameObject
	    //!
	    public void addAnimationClip(GameObject obj, AnimationClip clip)
	    {
	        if(!_animationClips.ContainsKey(obj))
	            _animationClips.Add(obj, new List<AnimationClip>());

            _animationClips[obj].Add(clip);

            _animationCurves.Add(clip, new List<AnimationCurve>());
        }

        //!
        //! Remove all AnimationClips of a GameObject and delete the associated XML representation
        //!
        public void removeAnimationClips(GameObject obj)
	    {
	        if (_animationClips.ContainsKey(obj))
	        {
	            _animationClips[obj].RemoveRange(0, _animationClips[obj].Count);
	            _animationClips.Remove(obj);
	        }
	        string path = filePath() + ("/" + obj.name + ".xml");
	        System.IO.File.Delete(path);
	    }
	
	    //!
	    //! Add a AnimationCurve to a Clip
	    //!
	    public void addAnimationCurve(AnimationClip clip, System.Type type, string propertyName, AnimationCurve curve)
	    {
	        if (!_animationCurves.ContainsKey(clip))
	            _animationCurves.Add(clip, new List<AnimationCurve>());
	
	        _animationCurves[clip].Add(curve);
	        clip.SetCurve(null, type, propertyName, curve);
	        XML_AnimationCurve xmlCurve = new XML_AnimationCurve(type.ToString(), propertyName);
	        addXMLCurve(curve, xmlCurve);
	    }
	
	    //!
	    //! STATIC METHODS
	    //! 
	
	    //!
	    //! Save a XML File from disk into the given XML Data Structure
	    //!
	    public static void Save(string name, XML_AnimationData data)
	    {
	
	        // name might contain maya namespaces
	        string _name = name;
	        _name = _name.Replace( ":", "_" );
	
	        string path = filePath();
	
	        if (!System.IO.Directory.Exists(path))
	            System.IO.Directory.CreateDirectory(path);
	
			Debug.Log( "Write: " + path + "/" +  _name + ".xml" );

	        XmlSerializer serializer = new XmlSerializer(typeof(XML_AnimationData));
	        using ( FileStream stream = new FileStream( (path + "/" +  _name + ".xml"), FileMode.Create ) )
	        {
	            serializer.Serialize(stream, data);
	        }
	    }
	
	    //!
	    //! Load a XML Data Structure from a XML File on disk
	    //!
	    public static XML_AnimationData Load(string name)
	    {
	        // name might contain maya namespaces
	        string _name = name;
	        _name = _name.Replace( ":", "_" );
	
	        string path = filePath() + ("/" + _name + ".xml");       
	
	        if (System.IO.File.Exists(path))
	        {
	            Debug.Log( "Load: " + path );
	            XmlSerializer serializer = new XmlSerializer(typeof(XML_AnimationData));
	            using (FileStream stream = new FileStream(path, FileMode.Open))
	            {
	                return serializer.Deserialize(stream) as XML_AnimationData;
	            }
	        }
	        else
	            return null;
	    }
	
	    //!
	    //! Returns the correct file path depending if Unity
	    //! is running in Editor Mode or as a deployed game
	    //!
	    public static string filePath()
	    {
	        if (Application.isEditor)
	        {
	            return Application.persistentDataPath;
	        }
	        else
	        {
	            return Application.persistentDataPath;
	        }
	    }
	
	    //!
	    //! Translator from WrapMode Enum to String
	    //! (could maybe replaced through saving int values in XML)
	    //!
	    public static string WrapModeToString(WrapMode mode)
	    {
	        switch (mode)
	        {
	            case WrapMode.Clamp:
	                return "Clamp";
	
	            case WrapMode.ClampForever:
	                return "ClampForever";
	
	            case WrapMode.Loop:
	                return "Loop";
	
	            case WrapMode.PingPong:
	                return "PingPong";
	
	            default:
	                return "Default";
	        }
	    }
	
	    //!
	    //! Translator from String to WrapMode Enum
	    //! (could maybe replaced through saving int values in XML)
	    //!
	    public static WrapMode StringToWrapMode(string mode)
	    {
	        switch (mode)
	        {
	            case "Clamp":
	                return WrapMode.Clamp;
	
	            case "ClampForever":
	                return WrapMode.ClampForever;
	
	            case "Loop":
	                return WrapMode.Loop;
	
	            case "PingPong":
	                return WrapMode.PingPong;
	
	            default:
	                return WrapMode.Default;
	        }
	    }
	
	    //!
	    //! Enhanced method to return not only System Types
	    //! Found by .net/mono and also Classes from Unity / this project
	    //!
	    public static System.Type GetTheType(string TypeName)
	    {
	
	        // Try Type.GetType() first. This will work with types defined
	        // by the Mono runtime, in the same assembly as the caller, etc.
	        var type = System.Type.GetType(TypeName);
	
	        // If it worked, then we're done here
	        if (type != null)
	            return type;
	
	        // If the TypeName is a full name, then we can try loading the defining assembly directly
	        if (TypeName.Contains("."))
	        {
	
	            // Get the name of the assembly (Assumption is that we are using 
	            // fully-qualified type names)
	            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
	
	            // Attempt to load the indicated Assembly
	            var assembly = Assembly.Load(assemblyName);
	            if (assembly == null)
	                return null;
	
	            // Ask that assembly to return the proper Type
	            type = assembly.GetType(TypeName);
	            if (type != null)
	                return type;
	
	        }
	
	        // If we still haven't found the proper type, we can enumerate all of the 
	        // loaded assemblies and see if any of them define the type
	        var currentAssembly = Assembly.GetExecutingAssembly();
	        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
	        foreach (var assemblyName in referencedAssemblies)
	        {
	
	            // Load the referenced assembly
	            var assembly = Assembly.Load(assemblyName);
	            if (assembly != null)
	            {
	                // See if that assembly defines the named type
	                type = assembly.GetType(TypeName);
	                if (type != null)
	                    return type;
	            }
	        }
	        // The type just couldn't be found...
	        return null;
	    }
}
}
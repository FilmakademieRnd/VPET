/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/v-p-e-t

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
-----------------------------------------------------------------------------
*/
﻿using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;

//!
//! Script to serialize animation data to a XML File
//!
namespace vpet
{
	public class AnimationSerializer : MonoBehaviour
	{
	    //!
	    //! shall animation data be loaded on app startup
	    //!
	    public bool Load_Data_from_XML = true;
	
	    //!
	    //! observed properties of animation curves
	    //!
	    private string[] observedProperties = new string[3]  { "m_LocalPosition.x", "m_LocalPosition.y", "m_LocalPosition.z" };
	
	    //!
	    //! shall animation data be saved on app close
	    //!
	    public bool Save_Data_to_XML = true;
	    
	    //!
	    //! reference to Animation data loaded from XML
	    //!
	    private XML_AnimationData _animationData = null;
	
	
	    public float timeStart = 0f;
	
	
	    public float timeEnd = 0f;
	
	    //!
	    //! Getter and Setter for the XML Data if animations have been loaded
	    //!
	    private XML_AnimationData XML_Data
	    {
	        get
	        {
	            return _animationData;
	        }
	
	        set
	        {
	            _animationData = value;
	        }
	    }
	
	    //!
	    //! Loads Animation Data from File if they have been saved before
	    //!
	    public void loadData()
	    {
	        if (Load_Data_from_XML)
	        {
	            XML_Data = AnimationData.Load(name);
	
	            if (XML_Data == null)
	                return;
	
	            foreach (XML_AnimationClip XMLclip in XML_Data.XML_AnimationClips)
	            {
	                XMLclip.data = new AnimationClip();
	                AnimationData.Data.addAnimationClip(gameObject, XMLclip.data);
	                foreach (XML_AnimationCurve XMLcurve in XMLclip.XML_AnimationCurves)
	                {
	                    // print( "XMLcurve " + XMLcurve.property );
	
	                    XMLcurve.data = new AnimationCurve();
	                    foreach (XML_KeyFrame XMLkey in XMLcurve.XML_KeyFrames)
	                    {
	                        XMLkey.data = new Keyframe(XMLkey.t, XMLkey.v, XMLkey.inT, XMLkey.outT);
	                        XMLcurve.data.AddKey(XMLkey.data);
	
	
	                        timeStart = Mathf.Min( timeStart, XMLkey.t );
	                        timeEnd = Mathf.Max( XMLkey.t, timeEnd );
	
	
	                    }
	                    XMLcurve.data.postWrapMode = AnimationData.StringToWrapMode(XMLcurve.postWM);
	                    XMLcurve.data.preWrapMode = AnimationData.StringToWrapMode(XMLcurve.preWM);
	                    System.Type type = AnimationData.GetTheType(XMLcurve.type);
	                    AnimationData.Data.addAnimationCurve(XMLclip.data, type, XMLcurve.property, XMLcurve.data);
	                }
	            }
	
	            //register the object in the animation Controller
	            GameObject.Find("AnimationController").GetComponent<AnimationController>().registerAnimatedObject(gameObject.GetComponent<SceneObject>());
	
	            gameObject.GetComponent<SceneObject>().setKinematic(true, false);
	        }
	    }
	
	    //!
	    //! Saves Animation Data to a XML structure into a file
	    //!
	    public void OnApplicationQuit()
	    {
	        if (Save_Data_to_XML == false || AnimationData.Data.getAnimationClips(gameObject) == null)
	        {
	            return;
	        }
	
	        List<AnimationClip> clips = AnimationData.Data.getAnimationClips(gameObject);

	        XML_AnimationData runtimeXMLData = new XML_AnimationData();
	
	        List<XML_AnimationClip> XMLclipList = new List<XML_AnimationClip>();
	        foreach (AnimationClip clip in clips)
	        {
	            XML_AnimationClip XMLclip = new XML_AnimationClip(gameObject.name+"_Amimation");
	            foreach (string property in observedProperties)
	            {
	                List<XML_KeyFrame> XMLkeys = new List<XML_KeyFrame>();
	                foreach (Keyframe key in AnimationData.Data.getAnimationCurve(clip, property).keys)
	                {
	                    XMLkeys.Add(new XML_KeyFrame(key.inTangent, key.outTangent, key.time, key.value, key.tangentMode));
	                }
	                XML_AnimationCurve XMLcurve = new XML_AnimationCurve(XMLkeys, AnimationData.WrapModeToString(AnimationData.Data.getAnimationCurve(clip, property).postWrapMode), AnimationData.WrapModeToString(AnimationData.Data.getAnimationCurve(clip, property).preWrapMode), "UnityEngine.Transform", property);
	                XMLclip.XML_AnimationCurves.Add(XMLcurve);
	            }
	            XMLclipList.Add(XMLclip);
	        }
	        runtimeXMLData.XML_AnimationClips = XMLclipList;
	        AnimationData.Save(gameObject.name, runtimeXMLData);
	    }
}}
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

//!
//! Controller providing global undo & redo functionality
//! After each performed action, the entire scene state (parameters of all movable objects) is stored.
//! This is neccesary to be able to undo changes caused by physics, started by a user interaction.
//!
namespace vpet
{
	public class UndoRedoController : MonoBehaviour {
	
	    //!
	    //! stack of performed actions (actually scene state after a performed action)
	    //!
	    ArrayList undoActionStack;
	    
	    //!
	    //! stack of actions (scene states) already undone
	    //!
	    ArrayList redoActionStack;
	    
	    //!
	    //! reference to all moveable and editable objects
	    //! Needed to capture scene state fastly.
	    //! All changed parameters of each object in this array will be stored at each user driven modification of the scene.
	    //!
		ArrayList editableObjects;
	
	    //!
	    //! cached reference to the main controller
	    //!
	    MainController mainController;
	    //!
	    //! cached reference to the server adapter
	    //!
		ServerAdapter serverAdapter;
	
	    //!
		//! Use this for initialization
		//!
	    void Start () {
	        undoActionStack = new ArrayList();
	        redoActionStack = new ArrayList();
			editableObjects = new ArrayList();
	        mainController = GameObject.Find("MainController").GetComponent<MainController>();
			serverAdapter = GameObject.Find ("ServerAdapter").GetComponent<ServerAdapter> ();
	
	        //find all moveable and editable objects
			recursiveRegisterObjects (GameObject.Find("Scene").transform , editableObjects);
			editableObjects.Add (Camera.main.transform);
	
			addAction ();
	
		}
	
	    //!
	    //! recursively find all moveable and editable objects in the scene
	    //! @param      obj     parent object to start search from
	    //! @param      list    arraylist to add found objects to
	    //!
		private void recursiveRegisterObjects (Transform obj, ArrayList list)
		{
			if (obj.GetComponent<SceneObject>() != null && obj.gameObject.activeSelf)
			{
				if(obj.GetComponent<SceneObject>().isPointLight ||
				   obj.GetComponent<SceneObject>().isSpotLight ||
				   obj.GetComponent<SceneObject>().isDirectionalLight)
				{
					list.Add(obj.parent);
				}
				else
				{
					list.Add(obj);
				}
			}
			foreach (Transform child in obj)
			{
				recursiveRegisterObjects(child,list);
			}
		}
	
	    //!
	    //! add an action to the action stack (capture scene state)
	    //!
	    public void addAction()
	    {
            return;


			ArrayList actionList = new ArrayList();
			foreach(Transform obj in editableObjects) 
			{
				actionList.Add(new Action(obj,Action.Type.translation,obj.position, serverAdapter));
				actionList.Add(new Action(obj,obj.rotation, serverAdapter));
	            if (!obj.GetComponent<Light>())
	            {
	                actionList.Add(new Action(obj, Action.Type.scale, obj.localScale, serverAdapter));
	            }
				else
				{
					if(obj.GetComponent<Light>().type == LightType.Directional)
					{
						actionList.Add(new Action(obj,obj.GetComponent<Light>().color, serverAdapter));
						actionList.Add(new Action(obj,Action.Type.lightIntensity,obj.GetComponent<Light>().intensity, serverAdapter));
					}
					else if(obj.GetComponent<Light>().type == LightType.Point)
					{
						actionList.Add(new Action(obj,obj.GetComponent<Light>().color, serverAdapter));
						actionList.Add(new Action(obj,Action.Type.lightIntensity,obj.GetComponent<Light>().intensity, serverAdapter));
						actionList.Add(new Action(obj,Action.Type.lightRange,obj.GetComponent<Light>().range, serverAdapter));
					}
					else if(obj.GetComponent<Light>().type == LightType.Spot)
					{
						actionList.Add(new Action(obj,obj.GetComponent<Light>().color, serverAdapter));
						actionList.Add(new Action(obj,Action.Type.lightIntensity,obj.GetComponent<Light>().intensity, serverAdapter));
						actionList.Add(new Action(obj,Action.Type.lightRange,obj.GetComponent<Light>().range, serverAdapter));
						actionList.Add(new Action(obj,Action.Type.lightAngle,obj.GetComponent<Light>().spotAngle, serverAdapter));
					}
				}
			}
	        //add action to Stack
	        undoActionStack.Add(actionList);
	        //dump redo Stack
	        redoActionStack = new ArrayList();
	        if (undoActionStack.Count > 1) 
			{
				mainController.activateUndoButton ();
				mainController.deactivateRedoButton ();
			}
	    }
	
	    //!
	    //! undo the last performed action
	    //!
	    public void undoAction()
	    {
	        if (undoActionStack.Count > 1)
	        {
				for(int i = 0; i < (undoActionStack[undoActionStack.Count - 2] as ArrayList).Count; i++)
				{
					((undoActionStack[undoActionStack.Count - 2] as ArrayList)[i] as Action).execute();
				}
				redoActionStack.Add(undoActionStack[undoActionStack.Count - 1]);
				mainController.activateRedoButton();
	            undoActionStack.RemoveAt(undoActionStack.Count - 1);
	        }
	        if (undoActionStack.Count == 1)
	        {
	            mainController.deactivateUndoButton();
	        }
	    }
	
	    //!
	    //! redo the latest undone action
	    //!
	    public void redoAction()
	    {
	        if (redoActionStack.Count > 0)
	        {
				for(int i = 0; i < (redoActionStack[redoActionStack.Count - 1] as ArrayList).Count; i++)
				{
					((redoActionStack[redoActionStack.Count - 1] as ArrayList)[i] as Action).execute();
				}
				undoActionStack.Add(redoActionStack[redoActionStack.Count - 1]);
				mainController.activateUndoButton();
				redoActionStack.RemoveAt(redoActionStack.Count - 1);
	
	        }
	        if (redoActionStack.Count == 0)
	        {
	            mainController.deactivateRedoButton();
	        }
	    }
}
}
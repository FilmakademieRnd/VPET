/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "SceneCharacterObject.cs"
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Alexandru-Sebastian Tufis-Schwartz
//! @version 0
//! @date 02.08.2023

using System;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET SceneCharacterObject 
    //!
    public class SceneCharacterObject : SceneObject
    {
        //!
        //!dictionary to store bone transforms by their IDs
        //!
        private Dictionary<int, Transform> boneMap;
        //!
        //!the array of bone transforms from the SkinnedMeshRenderer
        //!
        private Transform[] bones;


        public override void Awake()
        {
            base.Awake();

            //!
            // Initialize the dictionary to store bone transforms by their IDs.
            //!
            boneMap = new Dictionary<int, Transform>();
            
            //!
            //!if server setBones is called on awake if not setBones is called from SceneCreatorModule line 137
            //!
            if (core.isServer)
            {
                setBones();
            }
            
        }
        
        //!
        //!Setting up all the bone rotation parameters
        //!
       public void setBones()
        {
            //!
            //! Get the array of bone transforms from the SkinnedMeshRenderer component.
            //!
            bones = GetComponentInChildren<SkinnedMeshRenderer>().bones;
            //!
            //! Loop through each bone transform obtained from the SkinnedMeshRenderer.
            //!
            for (int i = 0; i < bones.Length; i++)
            {
                Transform boneTransform = bones[i];
                if (boneTransform != null)
                {
                    //!
                    //! Create a new Quaternion parameter for each bone transform's local rotation.
                    //!
                    Parameter<Quaternion> localBoneRotationParameter =
                        new Parameter<Quaternion>(boneTransform.localRotation, boneTransform.name, this);
                    
                    //!
                    //! Attach a callback to the parameter's "hasChanged" event, which is triggered when the bone transform is updated.
                    //!
                    localBoneRotationParameter.hasChanged += updateRotation;
                    
                    //!
                    //! Use the parameter's ID as the key to store the bone transform in the dictionary.
                    //!
                    var id = localBoneRotationParameter.id;
                    boneMap.Add(id, boneTransform);
                }
            }
        }
       
       //!
       //! Callback method triggered when the bone transform's local rotation is updated.
       //!
       private void updateRotation(object sender, Quaternion a)
        {
            //!
            //! Retrieve the ID of the parameter whose value has changed.
            //!
            int id = ((Parameter<Quaternion>)sender).id;
            
            //!
            //! Update the bone transform's local rotation based on the new value.
            //!
            boneMap[id].localRotation = a;
            
            //!
            //! Emit a signal to notify that the parameter has changed (if necessary).
            //!
            emitHasChanged((AbstractParameter)sender);
        }
        
        public virtual void Update()
        {
            UpdateBoneTransform();
        }

        private void UpdateBoneTransform()
        {
            //!
            //! Loop through each bone transform stored in the dictionary.
            //!
            foreach (var VARIABLE in boneMap)
            {
                //!
                //! Check if the bone transform's local rotation has changed compared to the parameter's value.
                //!
                if (VARIABLE.Value.localRotation != ((Parameter<Quaternion>)parameterList[VARIABLE.Key]).value)
                {
                    //!
                    //! If the local rotation has changed, update the parameter's value to match the bone transform.
                    //!
                    ((Parameter<Quaternion>)parameterList[VARIABLE.Key]).value = VARIABLE.Value.localRotation;
                }
            }

        }
    }
}

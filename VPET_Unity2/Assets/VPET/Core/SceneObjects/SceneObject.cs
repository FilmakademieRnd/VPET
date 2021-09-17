/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneObject.cs"
//! @brief implementation of the VPET SceneObject, connecting Unity and VPET functionalty.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.06.2021

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET SceneObject, connecting Unity and VPET functionalty 
    //! around 3D scene specific objects.
    //!
    [DisallowMultipleComponent]
    public class SceneObject : MonoBehaviour
    {
        //!
        //! unique id of this sceneObject
        //!
        public int id;
        //!
        //! is the sceneObject reacting to physics
        //!
        public bool physicsActive;
        //!
        //! Position of the SceneObject
        //!
        private Parameter<Vector3> position;
        //!
        //! Rotation of the SceneObject
        //!
        private Parameter<Quaternion> rotation;
        //!
        //! Scale of the SceneObject
        //!
        private Parameter<Vector3> scale;
        //!
        //! List storing all parameters of this SceneObject.
        //!
        protected List<AbstractParameter> _parameterList;
        
        //!
        //! Getter for parameter list
        //!
        public ref List<AbstractParameter> parameterList
        {
            get => ref _parameterList;
        }

        //!
        //! Start is called before the first frame update
        //!
        public virtual void Start()
        {
            _parameterList = new List<AbstractParameter>();

            id = Helpers.getUniqueID();
            physicsActive = false;

            position = new Parameter<Vector3>(transform.localPosition, "position");
            position.hasChanged += updatePosition;
            _parameterList.Add(position);
            rotation = new Parameter<Quaternion>(transform.localRotation, "rotation");
            rotation.hasChanged += updateRotation;
            _parameterList.Add(rotation);
            scale = new Parameter<Vector3>(transform.localScale, "scale");
            scale.hasChanged += updateScale;
            _parameterList.Add(scale);
        }

        //!
        //! Update GameObject local position.
        //! @param   sender     Object calling the update function
        //! @param   a          new position value
        //!
        private void updatePosition(object sender, Parameter<Vector3>.TEventArgs a)
        {
            transform.localPosition = a.value;
        }

        //!
        //! Update GameObject local rotation.
        //! @param   sender     Object calling the update function
        //! @param   a          new rotation value
        //!
        private void updateRotation(object sender, Parameter<Quaternion>.TEventArgs a)
        {
            transform.localRotation = a.value;
        }

        //!
        //! Update GameObject local scale.
        //! @param   sender     Object calling the update function
        //! @param   a          new scale value
        //!
        private void updateScale(object sender, Parameter<Vector3>.TEventArgs a)
        {
            transform.localScale = a.value;
        }

        //!
        //! Update is called once per frame
        //!
        public virtual void Update()
        {
            // ToDo: implement a clever way to figure out when the transforms need to be updated 
            if(physicsActive)
                updateSceneObjectTransform();
        }

        //!
        //! updates the scene objects transforms and informs all connected parameters about the change
        //!
        private void updateSceneObjectTransform()
        {
            if (transform.localPosition != position.value)
                position.value = transform.localPosition;
            if (transform.localRotation != rotation.value)
                rotation.value = transform.localRotation;
            if (transform.localScale != scale.value)
                scale.value = transform.localScale;
        }
    }
}

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
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneObject.cs"
//! @brief Implementation of the VPET SceneObject, connecting Unity and VPET functionalty.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 18.02.2022

using System;
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
        //! Is the sceneObject locked?
        //!
        public bool _lock = false;
        //!
        //! Previous lock state.
        //!
        private bool m_oldLock;
        //!
        //! Is the sceneObject reacting to physics
        //!
        private bool _physicsActive = false;
        //!
        //! Is the sceneObject reacting to physics
        //!
        public bool physicsActive
        {
            get => _physicsActive;
        }
        //!
        //! unique id of this sceneObject
        //!
        private short _id;
        //!
        //! Getter for unique id of this sceneObject
        //!
        public short id
        {
            get => _id;
        }
        //!
        //! A reference to the VPET UI manager.
        //!
        private UIManager m_uiManager;
        //!
        //! Event emitted when parameter changed.
        //!
        public event EventHandler<AbstractParameter> hasChanged;
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
        public virtual void Awake()
        {
            Core core = GameObject.FindObjectOfType<Core>();

            if (core)
                m_uiManager = core.getManager<UIManager>();

            _parameterList = new List<AbstractParameter>();

            _id = Helpers.getSoID();
            _physicsActive = false;

            position = new Parameter<Vector3>(transform.localPosition, "position", this, (short)parameterList.Count);
            position.hasChanged += updatePosition;
            _parameterList.Add(position);
            rotation = new Parameter<Quaternion>(transform.localRotation, "rotation", this, (short)parameterList.Count);
            rotation.hasChanged += updateRotation;
            _parameterList.Add(rotation);
            scale = new Parameter<Vector3>(transform.localScale, "scale", this, (short)parameterList.Count);
            scale.hasChanged += updateScale;
            _parameterList.Add(scale);
        }

        //!
        //! Function called, when Unity emit it's OnDestroy event.
        //!
        public virtual void OnDestroy()
        {
            position.hasChanged -= updatePosition;
            rotation.hasChanged -= updateRotation;
            scale.hasChanged -= updateScale;
        }

        //!
        //! Function that emits the scene objects hasChanged event. (Used for parameter updates)
        //!
        //! @param parameter The parameter that has changed. 
        //!
        protected void emitHasChanged (AbstractParameter parameter)
        {
            if (!_lock)
                hasChanged?.Invoke(this, parameter);
        }

        //!
        //! Update GameObject local position.
        //! @param   sender     Object calling the update function
        //! @param   a          new position value
        //!
        private void updatePosition(object sender, Vector3 a)
        {
            transform.localPosition = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update GameObject local rotation.
        //! @param   sender     Object calling the update function
        //! @param   a          new rotation value
        //!
        private void updateRotation(object sender, Quaternion a)
        {
            transform.localRotation = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update GameObject local scale.
        //! @param   sender     Object calling the update function
        //! @param   a          new scale value
        //!
        private void updateScale(object sender, Vector3 a)
        {
            transform.localScale = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update is called once per frame
        //!
        public virtual void Update()
        {
            if (_lock != m_oldLock)
            {
                if (_lock)
                    m_uiManager.highlightSceneObject(this);
                else
                    m_uiManager.unhighlightSceneObject(this);
                m_oldLock = _lock;
            }

#if UNITY_EDITOR
            updateSceneObjectTransform();
#endif
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

        public Parameter<T> getParameter<T>(string name)
        {
           return (Parameter<T>) _parameterList.Find(parameter => parameter.name == name);
        }
    }
}

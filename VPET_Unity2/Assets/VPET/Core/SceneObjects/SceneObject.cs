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

//! @file "SceneObject.cs"
//! @brief Implementation of the VPET SceneObject, connecting Unity and VPET functionalty.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 01.03.2022

using System.Runtime.CompilerServices;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET SceneObject, connecting Unity and VPET functionalty 
    //! around 3D scene specific objects.
    //!
    [DisallowMultipleComponent]
    public class SceneObject : ParameterObject
    {
        //!
        //! Is the sceneObject locked?
        //!
        public bool _lock = false;
        //!
        //! Previous lock state for highlighting the sceneObject.
        //!
        private bool m_highlightLock = false;
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
        //! A reference to the scene objects gizmo.
        //!
        public GameObject _gizmo = null;
        //!
        //! A reference to the scene objects icon.
        //!
        public GameObject _icon = null;
        //!
        //! A reference to the VPET UI manager.
        //!
        private UIManager m_uiManager;
        //!
        //! Position of the SceneObject
        //!
        public Parameter<Vector3> position;
        //!
        //! Rotation of the SceneObject
        //!
        public Parameter<Quaternion> rotation;
        //!
        //! Scale of the SceneObject
        //!
        public Parameter<Vector3> scale;
        //!
        //! Factory to create a new SceneObject and do it's initialisation.
        //! Use this function instead GameObject.AddComponen<>!
        //!
        //! @param gameObject The gameObject the new SceneObject will be attached to.
        //! @sceneID The scene ID for the new SceneObject.
        //!
        public static new SceneObject Attach(GameObject gameObject, byte sceneID = 254)
        {
            SceneObject obj = gameObject.AddComponent<SceneObject>();
            obj.Init(sceneID);

            return obj;
        }
        //!
        //! Initialisation
        //!
        public override void Awake()
        {
            base.Awake();

            m_uiManager = _core.getManager<UIManager>();

            _physicsActive = false;

            position = new Parameter<Vector3>(transform.localPosition, "position", this);
            position.hasChanged += updatePosition;
            rotation = new Parameter<Quaternion>(transform.localRotation, "rotation", this);
            rotation.hasChanged += updateRotation;
            scale = new Parameter<Vector3>(transform.localScale, "scale", this);
            scale.hasChanged += updateScale;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void emitHasChanged (AbstractParameter parameter)
        {
            if (!_lock)
                base.emitHasChanged(parameter);
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
            if (_lock != m_highlightLock)
            {
                if (_lock)
                    m_uiManager.highlightSceneObject(this);
                else
                    m_uiManager.unhighlightSceneObject(this);
                m_highlightLock = _lock;
            }

//#if UNITY_EDITOR
            updateSceneObjectTransform();
//#endif
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

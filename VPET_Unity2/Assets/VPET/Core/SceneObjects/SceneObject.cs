/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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
        public GameObject _gizmo = null;
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
    }
}

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

//! @file "ARModule.cs"
//! @brief implementation of VPET camera navigation features
//! @author Paulo Scatena
//! @version 0
//! @date 22.02.2022

using System;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! implementation of VPET camera navigation
    //!
    public class CameraNavigationModule : InputManagerModule
    {
        private Core vpetCore;


        //!
        //! A reference to the VPET input manager.
        //!
        private InputManager m_inputManager;

        //!
        //! A reference to the main camera transform.
        //!
        private Transform camXform;

        // Orbiting center
        private Vector3 pivotPoint;

        // Camera movement multiplies - todo: something more elegant than just magic numbers?
        readonly float panFactor = .005f;
        readonly float orbitFactor = .15f;
        readonly float dollyFactor = .007f;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public CameraNavigationModule(string name, Core core) : base(name, core)
        {

        }

        //!
        //! Init callback for the UICreator2D module.
        //! Called after constructor. 
        //!
        protected override void Init(object sender, EventArgs e)
        {
            camXform = Camera.main.transform;

            // Grabbing from the input manager directly
            m_inputManager = m_core.getManager<InputManager>();

            // Subscription to input events
            m_inputManager.pinchEvent += CameraDolly;
            m_inputManager.twoDragEvent += CameraOrbit;
            m_inputManager.threeDragEvent += CameraPan;
        }

        private void CameraDolly(object sender, InputManager.PinchEventArgs e)
        {
            // dolly cam
            camXform.Translate(0f, 0f, e.distance * dollyFactor);
        }

        private void CameraOrbit(object sender, InputManager.DragEventArgs e)
        {
            // todo - do not execute in every call? one suffices
            // pivot point at 6m into camera
            pivotPoint = camXform.TransformPoint(Vector3.forward * 6f);

            // arc 
            camXform.RotateAround(pivotPoint, Vector3.up, orbitFactor * e.delta.x);
            // tilt
            camXform.RotateAround(pivotPoint, camXform.right, -orbitFactor * e.delta.y);
        }

        private void CameraPan(object sender, InputManager.DragEventArgs e)
        {
            Vector2 offset = -panFactor * e.delta;

            // Pan around
            camXform.Translate(offset.x, offset.y, 0);
        }
    }
}
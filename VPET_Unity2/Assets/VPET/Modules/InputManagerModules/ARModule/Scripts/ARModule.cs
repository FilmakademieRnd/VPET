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

//! @file "UICreator2DModule.cs"
//! @brief implementation of VPET 2D UI scene creator module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 09.11.2021

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

namespace vpet
{
    //!
    //! implementation of VPET augmented reality (AR) tracking module
    //!
    public class ARModule : InputManagerModule
    {
        private Core vpetCore;
        private GameObject arOrigin;
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public ARModule(string name, Core core) : base(name, core)
        {
            GameObject arSessionPrefab = Resources.Load<GameObject>("Prefabs/ARSession");
            SceneObject.Instantiate(arSessionPrefab, Vector3.zero, Quaternion.identity);

            GameObject arSessionOriginPrefab = Resources.Load<GameObject>("Prefabs/ARSessionOrigin");
            arOrigin = SceneObject.Instantiate(arSessionOriginPrefab, Vector3.zero, Quaternion.identity);

            arOrigin.GetComponent<PlaceScene>().scene = core.getManager<SceneManager>().scnRoot;

            arOrigin.transform.parent = Camera.main.transform.parent;
            Camera.main.transform.parent = arOrigin.transform;
            arOrigin.GetComponent<ARSessionOrigin>().camera = Camera.main;

            Camera.main.gameObject.AddComponent<ARPoseDriver>();
            Camera.main.gameObject.AddComponent<ARCameraManager>();
            Camera.main.gameObject.AddComponent<AROcclusionManager>();
            Camera.main.gameObject.AddComponent<ARCameraBackground>();

            vpetCore = core;
        }

        //!
        //! Init callback for the UICreator2D module.
        //! Called after constructor. 
        //!
        protected override void Init(object sender, EventArgs e)
        {
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.started += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.performed += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
            //vpetCore.getManager<InputManager>().touchInputs.ARTouchScreen.PlaceScene.canceled += ctx => arOrigin.GetComponent<PlaceScene>().placeScene(ctx);
        }
    }
}
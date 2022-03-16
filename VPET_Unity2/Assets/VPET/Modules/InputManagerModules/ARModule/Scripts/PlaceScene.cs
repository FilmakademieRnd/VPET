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
//! @brief implementation of VPET AR features
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 09.11.2021

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace vpet
{
    public class PlaceScene : MonoBehaviour
    {
        ARRaycastManager m_RaycastManager;

        static List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

        private GameObject m_scene;
        public GameObject scene
        {
            set { m_scene = value; }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
        }

        // [REVIEW]
        //public void placeScene(InputAction.CallbackContext context)
        //{
        //    var touchPosition = context.ReadValue<Vector2>();
        //    if (m_RaycastManager.Raycast(touchPosition, m_Hits, TrackableType.PlaneWithinPolygon))
        //    {
        //        // Raycast hits are sorted by distance, so the first one
        //        // will be the closest hit.
        //        var hitPose = m_Hits[0].pose;
        //        if (m_scene)
        //        {
        //            m_scene.transform.position = hitPose.position;
        //            m_scene.transform.rotation = hitPose.rotation;
        //        }
        //    }
        //}
    }
}

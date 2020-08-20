/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

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

#if USE_AR
using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System;

namespace vpet
{
	public class ARFoundationController : MonoBehaviour
	{
        private ARAnchorManager m_anchorManager;
        private ARRaycastManager m_RaycastManager;
        private ARAnchor m_anchor;
        private Quaternion deltaRotation;
        private Quaternion lastHitPoseRotation;
        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        // Use this for initialization
        void Awake()
        {
            m_anchorManager = this.transform.GetComponent<ARAnchorManager>();
            m_RaycastManager = this.transform.GetComponent<ARRaycastManager>();
            deltaRotation = Quaternion.identity;
            lastHitPoseRotation = Quaternion.identity;
        }

        public void AddAnchor(Vector2 touchPosition)
        {
            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.FeaturePoint))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                Pose hitPose = s_Hits[0].pose;
                if (m_anchor != null)
                {
                    //update position of previous anchor
                    m_anchor.transform.position = hitPose.position;
                    lastHitPoseRotation = hitPose.rotation;
                    m_anchor.transform.rotation = lastHitPoseRotation * deltaRotation;
                }
                else
                {
                    m_anchor = m_anchorManager.AddAnchor(hitPose);
                }
            }
        }

        public void rotateAnchor(Quaternion rotation)
        {
            deltaRotation = rotation;
            m_anchor.transform.rotation = lastHitPoseRotation * deltaRotation;
        }


        //remove previous anchor
        //this makes the camera movable again
        public void removeAnchor()
        {
            m_anchorManager.RemoveAnchor(m_anchor);
            m_anchor = null;
        }
    }
}
#endif

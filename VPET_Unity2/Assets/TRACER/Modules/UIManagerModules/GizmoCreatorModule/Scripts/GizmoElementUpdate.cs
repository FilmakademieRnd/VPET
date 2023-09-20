/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
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

//! @file "GizmoElementUpdate.cs"
//! @brief Implementation of the TRACER GizmoElementUpdate component, updating line based gizmo objects.
//! @author Simon Spielmann
//! @version 0
//! @date 18.02.2022

using UnityEngine;

namespace tracer
{
    //!
    //! Implementation of the TRACER GizmoElementUpdate component, updating line based gizmo objects. 
    //!
    public class GizmoElementUpdate : MonoBehaviour
    {
        //!
        //! The default width parameter for the line renderer.
        //!
        private float m_lineWidth = 1.0f;
        //!
        //! The calculated Depth between main camera and gizmo from last frame call.
        //!
        private float m_oldDepth = 0.0f;
        //!
        //! The gizmos line renderer. 
        //!
        private LineRenderer m_lineRenderer;

        //!
        //! Start is called before the first frame update
        //!
        void Start()
        {
            m_lineRenderer = transform.gameObject.GetComponent<LineRenderer>();
            m_lineWidth = m_lineRenderer.startWidth;
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            Transform camera = Camera.main.transform;
            float depth = Vector3.Dot(camera.position - transform.position, camera.forward);

            if (m_oldDepth != depth)
            {
                m_lineRenderer.startWidth = m_lineWidth * depth;
                m_lineRenderer.endWidth = m_lineWidth * depth;
                m_oldDepth = depth;
            }
        }
    }
}

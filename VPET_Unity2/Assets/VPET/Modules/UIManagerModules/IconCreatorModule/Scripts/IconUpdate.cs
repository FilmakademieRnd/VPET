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

//! @file "IconUpdate.cs"
//! @brief Implementation of the VPET IconUpdate component, updating a icons properties.
//! @author Simon Spielmann
//! @version 0
//! @date 03.03.2022

using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET GizmoElementUpdate component, updating line based gizmo objects. 
    //!
    public class IconUpdate : MonoBehaviour
    {
        //!
        //! The calculated Depth between main camera and gizmo from last frame call.
        //!
        private Vector3 m_iconScale;

        //!
        //! A reference to the parent Scene Object.
        //!
        public SceneObject m_parentObject;
        
        //!
        //! Start is called before the first frame update
        //!
        void Start()
        {
            Core core = GameObject.Find("VPET").GetComponent<Core>();
            m_iconScale = Vector3.one * core.getManager<UIManager>().settings.uiScale.value;
            transform.right = Camera.main.transform.right;
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            Transform camera = Camera.main.transform;
            float depth = Vector3.Dot(camera.position - transform.position, camera.forward);

            transform.position = m_parentObject.transform.position;
            transform.rotation = camera.rotation;
            transform.localScale = m_iconScale * Mathf.Abs(depth * 0.1f);
        }
    }
}

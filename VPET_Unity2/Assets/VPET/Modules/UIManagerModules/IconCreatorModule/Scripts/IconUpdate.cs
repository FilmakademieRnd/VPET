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
        //! Start is called before the first frame update
        //!
        void Start()
        {
            m_iconScale = transform.localScale;
            transform.right = Camera.main.transform.right;
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            Transform camera = Camera.main.transform;
            float depth = Vector3.Dot(camera.position - transform.position, camera.forward);

            transform.rotation = camera.rotation;
            transform.localScale = m_iconScale * Mathf.Abs(depth * 0.1f);
        }
    }
}

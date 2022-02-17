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

//! @file "GizmoCreatorModule.cs"
//! @brief Implementation of the VPET GizmoCreatorModule, creating line based gizmo objects.
//! @author Simon Spielmann
//! @version 0
//! @date 10.02.2022

using System;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class GizmoCreatorModule : UIManagerModule
    {
        private List<VPETGizmo> m_GizmoElements;

        private static Vector3[] m_linePos;

        private static Vector3[] m_rectPos; 

        private static Vector3[] m_circlePos;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public GizmoCreatorModule(string name, Core core) : base(name, core)
        {

            m_linePos = new Vector3[]
            {
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, 0.0f)
            };

            m_rectPos = new Vector3[]
            {
                new Vector3( -1.0f,-1.0f, 0.0f ),
                new Vector3(  1.0f,-1.0f, 0.0f ),
                new Vector3(  1.0f, 1.0f, 0.0f ),
                new Vector3( -1.0f, 1.0f, 0.0f )
            };

            m_circlePos = new Vector3[32];
            
            for (int i=0; i<m_circlePos.Length; i++)
            {
                float step = (Mathf.PI * 2.0f * i) / m_circlePos.Length;
                m_circlePos[i] = new Vector3(Mathf.Sin(step), Mathf.Cos(step), 0f);
            }

        }

        protected override void Init(object sender, EventArgs e)
        {
            SceneManager sceneManager = m_core.getManager<SceneManager>();
            sceneManager.sceneReady += createGizmos;

           // VPETGizmo m_areaLight = new VPETGizmo("testGizmo");

           // m_areaLight.addElement(ref m_linePos);
           // m_areaLight.addElement(ref m_circlePos, true);

        }

        private void createGizmos(object sender, EventArgs e)
        {
            SceneManager sceneManager = (SceneManager)sender;

            foreach (SceneObject sceneObject in sceneManager.sceneObjects)
            {
                switch (sceneObject)
                {
                    case SceneObjectLight:
                        {
                            VPETGizmo gizmo = new VPETGizmo(sceneObject.name + "_Gizmo", sceneObject.transform);
                            Color lightColor = sceneObject.GetComponent<Light>().color;
                            sceneObject.getParameter<Color>("color").hasChanged += gizmo.setColor;
                            switch (sceneObject)
                            {
                                case SceneObjectPointLight:
                                    {
                                        gizmo.addElement(ref m_circlePos, lightColor, true);
                                        gizmo.addElement(ref m_linePos, lightColor);
                                        break;
                                    }
                                case SceneObjectDirectionalLight:
                                    {
                                        gizmo.addElement(ref m_circlePos, lightColor, true);
                                        gizmo.addElement(ref m_linePos, lightColor);
                                        break;
                                    }
                                case SceneObjectSpotLight:
                                    {
                                        gizmo.addElement(ref m_circlePos, lightColor, true);
                                        gizmo.addElement(ref m_linePos, lightColor);
                                        break;
                                    }
                            }
                            break;
                        }
                     case SceneObjectCamera:
                        {
                            VPETGizmo gizmo = new VPETGizmo(sceneObject.name + "_Gizmo", sceneObject.transform);
                            //((Parameter<Color>)sceneObject.getParameter("color")).hasChanged += gizmo.setColor;
                            gizmo.addElement(ref m_rectPos, Color.yellow, true);
                            gizmo.addElement(ref m_linePos, Color.yellow);
                            break;
                        }
                }

            }
        }

    }
}

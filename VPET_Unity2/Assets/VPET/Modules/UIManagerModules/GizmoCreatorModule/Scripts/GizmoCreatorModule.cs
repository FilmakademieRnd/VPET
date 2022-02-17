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
        private List<VPETGizmo> m_gizmos;

        private static Vector3[] m_linePos;

        private static Vector3[] m_rectPos; 

        private static Vector3[] m_conePos; 

        private static Vector3[] m_circlePos;

        private List<Tuple<SceneObject, EventHandler<AbstractParameter>>> m_ParameterEventHandlers;
        private List<Tuple<Parameter<float>, EventHandler<float>>> m_eventHandlersC;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public GizmoCreatorModule(string name, Core core) : base(name, core)
        {
            m_ParameterEventHandlers = new List<Tuple<SceneObject, EventHandler<AbstractParameter>>>();
            m_gizmos = new List<VPETGizmo>();

            m_linePos = new Vector3[]
            {
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 5.0f)
            };

            m_rectPos = new Vector3[]
            {
                new Vector3( -0.5f,-0.5f, 0.0f ),
                new Vector3(  0.5f,-0.5f, 0.0f ),
                new Vector3(  0.5f, 0.5f, 0.0f ),
                new Vector3( -0.5f, 0.5f, 0.0f )
            };

            m_conePos = new Vector3[]
            {
                new Vector3(  0.0f, 0.0f, 0.0f ),
                new Vector3( -0.5f,-0.5f, 1.0f ),
                                          
                new Vector3(  0.0f, 0.0f, 0.0f ),
                new Vector3(  0.5f,-0.5f, 1.0f ),
                                          
                new Vector3(  0.0f, 0.0f, 0.0f ),
                new Vector3(  0.5f, 0.5f, 1.0f ),
                                          
                new Vector3(  0.0f, 0.0f, 0.0f ),
                new Vector3( -0.5f, 0.5f, 1.0f )
            };

            m_circlePos = new Vector3[32];
            
            for (int i=0; i<m_circlePos.Length; i++)
            {
                float step = (Mathf.PI * 2.0f * i) / m_circlePos.Length;
                m_circlePos[i] = new Vector3(Mathf.Sin(step)/2f, Mathf.Cos(step)/2f, 0f);
            }

        }

        protected override void Init(object sender, EventArgs e)
        {
            manager.selectionChanged += createGizmos;
        }

        private void createGizmos(object sender, List<SceneObject> sceneObjects)
        {
            diosposeGizmos();

            foreach (SceneObject sceneObject in sceneObjects)
            {
                VPETGizmo gizmo = null;
                switch (sceneObject)
                {
                    case SceneObjectLight:
                        {
                            gizmo = new VPETGizmo(sceneObject.name + "_Gizmo", sceneObject.transform);
                            Color lightColor = sceneObject.GetComponent<Light>().color;
                            Parameter<Color> colorParameter = sceneObject.getParameter<Color>("color");
                            colorParameter.hasChanged += gizmo.setColor;
                            //m_eventHandlers.Add(new Tuple<Parameter<float>, EventHandler<float>>(colorParameter, updateScalePoint));
                            switch (sceneObject)
                            {
                                case SceneObjectPointLight:
                                    {
                                        gizmo.addElement(ref m_circlePos, lightColor, true).localScale = new Vector3(2,2,2);
                                        Transform sphere = gizmo.addElement(ref m_circlePos, lightColor, true);
                                        sphere.localScale = new Vector3(2, 2, 2);
                                        sphere.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
                                        updateScalePoint(sceneObject, null);
                                        sceneObject.hasChanged += updateScalePoint;
                                        m_ParameterEventHandlers.Add(new Tuple<SceneObject, EventHandler<AbstractParameter>>(sceneObject, updateScalePoint));
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
                                        gizmo.addElement(ref m_conePos, lightColor).localScale = new Vector3(0.7071f, 0.7071f, 1f);
                                        gizmo.addElement(ref m_circlePos, lightColor, true).localPosition = new Vector3(0,0,1);
                                        
                                        updateScaleSpot(sceneObject, null);
                                        sceneObject.hasChanged += updateScaleSpot;
                                        m_ParameterEventHandlers.Add(new Tuple<SceneObject, EventHandler<AbstractParameter>>(sceneObject, updateScaleSpot));
                                        break;
                                    }
                            }
                            break;
                        }
                     case SceneObjectCamera:
                        {
                            gizmo = new VPETGizmo(sceneObject.name + "_Gizmo", sceneObject.transform);
                            //GizmoElementUpdate nearPlane = gizmo.addElement(ref m_rectPos, Color.yellow, true);
                            gizmo.addElement(ref m_conePos, Color.yellow, false);
                            gizmo.addElement(ref m_rectPos, Color.yellow, true).localPosition = new Vector3(0,0,1);

                            updateScaleCamera(sceneObject, null);
                            sceneObject.hasChanged += updateScaleCamera;
                            m_ParameterEventHandlers.Add(new Tuple<SceneObject, EventHandler<AbstractParameter>>(sceneObject, updateScaleCamera));
                            break;
                        }
                }
                if (gizmo != null)
                    m_gizmos.Add(gizmo);
            }
        }

        private void updateScalePoint(object sender, AbstractParameter parameter)
        {
            SceneObject sceneObject = (SceneObject) sender;

            float range = sceneObject.getParameter<float>("range").value;
            sceneObject.transform.GetChild(0).localScale = new Vector3(range, range, range);
        }

        private void updateScaleSpot(object sender, AbstractParameter parameter)
        {
            SceneObject sceneObject = (SceneObject) sender;
            float range = sceneObject.getParameter<float>("range").value;
            float angle = sceneObject.getParameter<float>("spotAngle").value;

            // diameter = 2 * distance * tan( angle * 0.5 )
            float dia = 2f * range * MathF.Tan(angle / 180f * Mathf.PI * 0.5f);

            sceneObject.transform.GetChild(0).localScale = new Vector3(dia, dia, range);
        }

        private void updateScaleCamera(object sender, AbstractParameter parameter)
        {
            SceneObject sceneObject = (SceneObject)sender;
            float far = sceneObject.getParameter<float>("farClipPlane").value;
            float fov = sceneObject.getParameter<float>("fov").value;
            float aspect = sceneObject.getParameter<float>("aspectRatio").value;

            // diameter = 2 * distance * tan( angle * 0.5 )
            float dia = 2f * far * MathF.Tan(fov / 180f * Mathf.PI * 0.5f);

            sceneObject.transform.GetChild(0).localScale = new Vector3(dia * aspect, dia, far);
        }

        private void diosposeGizmos()
        {
            foreach (Tuple<SceneObject, EventHandler<AbstractParameter>> t in m_ParameterEventHandlers)
                t.Item1.hasChanged -= t.Item2;
           
            m_ParameterEventHandlers.Clear();

            foreach (VPETGizmo gizmo in m_gizmos)
                gizmo.dispose();
           
            m_gizmos.Clear();
        }

    }
}

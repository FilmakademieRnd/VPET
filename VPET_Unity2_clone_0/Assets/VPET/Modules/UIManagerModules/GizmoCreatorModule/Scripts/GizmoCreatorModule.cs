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

//! @file "GizmoCreatorModule.cs"
//! @brief Implementation of the VPET GizmoCreatorModule, creating line based gizmo objects.
//! @author Simon Spielmann
//! @version 0
//! @date 18.02.2022

using System;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET GizmoCreatorModule, creating line based gizmo objects.
    //!
    public class GizmoCreatorModule : UIManagerModule
    {
        //!
        //! The list of created gizmos.
        //!
        private List<VPETGizmo> m_gizmos;
        //!
        //! Stored positions for a line.
        //!
        private static Vector3[] m_linePos = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 5.0f)
        };
        //!
        //! Stored positions for a rectangle.
        //!
        private static Vector3[] m_rectPos = new Vector3[]
        {
            new Vector3( -0.5f,-0.5f, 0.0f ),
            new Vector3(  0.5f,-0.5f, 0.0f ),
            new Vector3(  0.5f, 0.5f, 0.0f ),
            new Vector3( -0.5f, 0.5f, 0.0f )
        };
        //!
        //! Stored positions for a cone.
        //!
        private static Vector3[] m_conePos = new Vector3[]
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
        //!
        //! Stored positions for a circle.
        //!
        private static Vector3[] m_circlePos;
        //!
        //! List storing event connections for releasing them before gizmos will be deleted.
        //!
        private List<Tuple<SceneObject, EventHandler<AbstractParameter>>> m_ParameterEventHandlers;
        //!
        //! 
        //!
        private List<Tuple<Parameter<Color>, EventHandler<Color>>> m_eventHandlersColor;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public GizmoCreatorModule(string name, Manager manager) : base(name, manager)
        {
            m_ParameterEventHandlers = new List<Tuple<SceneObject, EventHandler<AbstractParameter>>>();
            m_eventHandlersColor = new List<Tuple<Parameter<Color>, EventHandler<Color>>>();
            m_gizmos = new List<VPETGizmo>();
            m_circlePos = new Vector3[32];

            // creating points for a circle
            for (int i=0; i<m_circlePos.Length; i++)
            {
                float step = (Mathf.PI * 2.0f * i) / m_circlePos.Length;
                m_circlePos[i] = new Vector3(Mathf.Sin(step)/2f, Mathf.Cos(step)/2f, 0f);
            }

        }

        //!
        //! Init Function, connecting module with celsction changed event.
        //!
        protected override void Init(object sender, EventArgs e)
        {
            base.Init(sender, e);
            manager.selectionChanged += createGizmos;
        }

        //! 
        //! Virtual function called before Unity destroys the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);

            manager.selectionChanged -= createGizmos;
            diosposeGizmos();
        }

        //!
        //! Function that parses the given list of scene objects to create and
        //! add gizmo objects depending on it's type as child objects.
        //!
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
                            m_eventHandlersColor.Add(new Tuple<Parameter<Color>, EventHandler<Color>>(colorParameter, gizmo.setColor));
                            switch (sceneObject)
                            {
                                case SceneObjectPointLight:
                                    {
                                        gizmo.addElement(ref m_circlePos, lightColor, true).localScale = new Vector3(2,2,2);
                                        Transform sphere = gizmo.addElement(ref m_circlePos, lightColor, true);
                                        sphere.localScale = new Vector3(2, 2, 2);
                                        sphere.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

                                        sceneObject._gizmo = gizmo.root;
                                        updateScalePoint(sceneObject, null);
                                        sceneObject.hasChanged += updateScalePoint;
                                        m_ParameterEventHandlers.Add(new Tuple<SceneObject, EventHandler<AbstractParameter>>(sceneObject, updateScalePoint));
                                        break;
                                    }
                                case SceneObjectDirectionalLight:
                                    {
                                        gizmo.addElement(ref m_circlePos, lightColor, true);
                                        gizmo.addElement(ref m_linePos, lightColor);
                                        sceneObject._gizmo = gizmo.root;
                                        break;
                                    }
                                case SceneObjectSpotLight:
                                    {
                                        gizmo.addElement(ref m_conePos, lightColor).localScale = new Vector3(0.7071f, 0.7071f, 1f);
                                        gizmo.addElement(ref m_circlePos, lightColor, true).localPosition = new Vector3(0,0,1);

                                        sceneObject._gizmo = gizmo.root;
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

                            sceneObject._gizmo = gizmo.root;
                            updateScaleCamera(sceneObject, null);
                            sceneObject.hasChanged += updateScaleCamera;
                            m_ParameterEventHandlers.Add(new Tuple<SceneObject, EventHandler<AbstractParameter>>(sceneObject, updateScaleCamera));
                            break;
                        }
                }
                if (gizmo != null)
                {
                    m_gizmos.Add(gizmo);
                }
            }
        }

        //!
        //! Function for calculating and setting of scale updates for a point light gizmo.
        //!
        private void updateScalePoint(object sender, AbstractParameter parameter)
        {
            SceneObjectPointLight sceneObject = (SceneObjectPointLight) sender;

            float range = sceneObject.range.value;
            sceneObject._gizmo.transform.localScale = new Vector3(range, range, range);
        }

        //!
        //! Function for calculating and setting of scale updates for a spot light gizmo.
        //!
        private void updateScaleSpot(object sender, AbstractParameter parameter)
        {
            SceneObjectSpotLight sceneObject = (SceneObjectSpotLight) sender;
            float range = sceneObject.range.value;
            float angle = sceneObject.spotAngle.value;

            // diameter = 2 * distance * tan( angle * 0.5 )
            float dia = 2f * range * MathF.Tan(angle / 180f * Mathf.PI * 0.5f);

            sceneObject._gizmo.transform.localScale = new Vector3(dia, dia, range);
        }

        //!
        //! Function for calculating and setting of scale updates for a camera gizmo.
        //!
        private void updateScaleCamera(object sender, AbstractParameter parameter)
        {
            SceneObjectCamera sceneObject = (SceneObjectCamera)sender;
            float far = sceneObject.far.value;
            float fov = sceneObject.fov.value;
            float aspect = sceneObject.aspect.value;

            // diameter = 2 * distance * tan( angle * 0.5 )
            float dia = 2f * far * MathF.Tan(fov / 180f * Mathf.PI * 0.5f);

            sceneObject._gizmo.transform.localScale = new Vector3(dia * aspect, dia, far);
        }

        //!
        //! Function for disposing and cleanup of all created gizmos.
        //!
        private void diosposeGizmos()
        {
            foreach (Tuple<Parameter<Color>, EventHandler<Color>> t in m_eventHandlersColor)
                t.Item1.hasChanged -= t.Item2;

            m_eventHandlersColor.Clear();
            
            foreach (Tuple<SceneObject, EventHandler<AbstractParameter>> t in m_ParameterEventHandlers)
                t.Item1.hasChanged -= t.Item2;
           
            m_ParameterEventHandlers.Clear();

            foreach (VPETGizmo gizmo in m_gizmos)
                gizmo.dispose();
           
            m_gizmos.Clear();
        }

    }
}

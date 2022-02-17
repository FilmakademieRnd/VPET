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

//! @file "Spinner.cs"
//! @brief base class of spinner manipulators
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Justus Henne
//! @version 0
//! @date 02.02.2022

using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;


namespace vpet
{
    public class Spinner : Manipulator
    {
        private int currentAxis;
        private SnapSelect snapSelect;

        //!
        //! function to initalize the spinner
        //!
        public void Init(AbstractParameter p)
        {
            abstractParam = p;
            snapSelect = this.GetComponent<SnapSelect>();

            AbstractParameter.ParameterType type = abstractParam.vpetType;

            List<Tuple<float, string>> list = new List<Tuple<float, string>>();

            switch (type)
            {
                case AbstractParameter.ParameterType.FLOAT:
                    Parameter<float> paramFloat = (Parameter<float>)abstractParam;
                    paramFloat.hasChanged += snapSelect.setParam;
                    float f = paramFloat.value;
                    list.Add(new Tuple<float, string>(f, ""));
                    snapSelect.Init(list, 10f);
                    break;
                case AbstractParameter.ParameterType.VECTOR2:
                    Parameter<Vector2> paramVec2 = (Parameter<Vector2>)abstractParam;
                    paramVec2.hasChanged += snapSelect.setParam;
                    Vector2 vec2 = paramVec2.value;
                    list.Add(new Tuple<float, string>(vec2.x, "X"));
                    list.Add(new Tuple<float, string>(vec2.y, "Y"));
                    snapSelect.Init(list, 10f);
                    break;
                case AbstractParameter.ParameterType.VECTOR3:
                    Parameter<Vector3> paramVec3 = (Parameter<Vector3>)abstractParam;
                    paramVec3.hasChanged += snapSelect.setParam;
                    Vector3 vec3 = paramVec3.value;
                    list.Add(new Tuple<float, string>(vec3.x, "X"));
                    list.Add(new Tuple<float, string>(vec3.y, "Y"));
                    list.Add(new Tuple<float, string>(vec3.z, "Z"));
                    list.Add(new Tuple<float, string>((vec3.x+ vec3.y + vec3.z) / 3f, "XYZ"));
                    snapSelect.Init(list, 10f);
                    break;
                case AbstractParameter.ParameterType.QUATERNION:
                    Parameter<Quaternion> paramQuat = (Parameter<Quaternion>)abstractParam;
                    paramQuat.hasChanged += snapSelect.setParam;
                    Vector3 rot = paramQuat.value.eulerAngles;
                    list.Add(new Tuple<float, string>(rot.x, "X"));
                    list.Add(new Tuple<float, string>(rot.y, "Y"));
                    list.Add(new Tuple<float, string>(rot.z, "Z"));
                    snapSelect.Init(list, 500f);
                    break;
                default:
                    Helpers.Log("Parameter Type cannot be edited with Spinner.");
                    break;
            }
        }

        private void Start()
        {
            snapSelect.parameterChanged += changeAxis;
            snapSelect.valueChanged += setValue;
        }

        //!
        //! function to perform axis drag
        //! @param axis axis to be used
        //! @param value drag value
        //!
        private void setValue(object sender, float val)
        {
            AbstractParameter.ParameterType type = abstractParam.vpetType;
            switch (type)
            {
                case AbstractParameter.ParameterType.FLOAT:
                    Parameter<float> paramFloat = (Parameter<float>)abstractParam;
                    paramFloat.setValue(paramFloat.value + val);
                    break;
                case AbstractParameter.ParameterType.VECTOR2:
                    Parameter<Vector2> paramVec2 = (Parameter<Vector2>)abstractParam;
                    Vector2 valVec2 = paramVec2.value;
                    if (currentAxis == 0)
                        paramVec2.setValue(new Vector2(valVec2.x + val, valVec2.y));
                    else
                        paramVec2.setValue(new Vector2(valVec2.x, valVec2.y + val));
                    break;
                case AbstractParameter.ParameterType.VECTOR3:
                    Parameter<Vector3> paramVec3 = (Parameter<Vector3>)abstractParam;
                    Vector3 valVec3 = paramVec3.value;
                    if (currentAxis == 0)
                        paramVec3.setValue(new Vector3(valVec3.x + val, valVec3.y, valVec3.z));
                    else if (currentAxis == 1)
                        paramVec3.setValue(new Vector3(valVec3.x, valVec3.y + val, valVec3.z));
                    else if (currentAxis == 2)
                        paramVec3.setValue(new Vector3(valVec3.x, valVec3.y, valVec3.z + val));
                    else
                        paramVec3.setValue(new Vector3(valVec3.x + val, valVec3.y + val, valVec3.z + val));
                    break;
                case AbstractParameter.ParameterType.QUATERNION:
                    Parameter<Quaternion> paramQuat = (Parameter<Quaternion>)abstractParam;
                    Vector3 rot = paramQuat.value.eulerAngles;
                    if (currentAxis == 0)
                        rot = new Vector3(rot.x + val, rot.y, rot.z);
                    else if (currentAxis == 1)
                        rot = new Vector3(rot.x, rot.y + val, rot.z);
                    else
                        rot = new Vector3(rot.x, rot.y, rot.z + val);
                    paramQuat.setValue(Quaternion.Euler(rot));
                    break;
                default:
                    Helpers.Log("Parameter Type cannot be edited with Spinner.");
                    break;
            }
        }

        private void changeAxis(object sender, int index)
        {
            currentAxis = index;
        }
    }
}
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
        //!
        //! currently edited axis of the parameter (e.g. x, y or z)
        //!
        private int _currentAxis;

        //!
        //! Reference to _snapSelect
        //!
        private SnapSelect _snapSelect;

        //!
        //! Reference to VPET uiSettings
        //!
        private VPETUISettings _uiSettings;
        public VPETUISettings uiSettings
        {
            get => _uiSettings;
            set => _uiSettings = value;
        }

        //!
        //! Event emitted when parameter has changed
        //!
        public event EventHandler<AbstractParameter> doneEditing;

        ~Spinner()
        {
            if (abstractParam != null)
                _snapSelect.editingEnded -= editingDone;
        }

        //!
        //! function to initalize the spinner
        //!
        public void Init(AbstractParameter p)
        {
            abstractParam = p;
            _snapSelect = this.GetComponent<SnapSelect>();
            _snapSelect.uiSettings = _uiSettings;

            AbstractParameter.ParameterType type = abstractParam.vpetType;

            switch (type)
            {
                case AbstractParameter.ParameterType.FLOAT:
                    Parameter<float> paramFloat = (Parameter<float>)abstractParam;
                    paramFloat.hasChanged += _snapSelect.setParam;
                    _snapSelect.setSensitivity(100f);
                    _snapSelect.addElement("", paramFloat.value);
                    break;
                case AbstractParameter.ParameterType.VECTOR2:
                    Parameter<Vector2> paramVec2 = (Parameter<Vector2>)abstractParam;
                    paramVec2.hasChanged += _snapSelect.setParam;
                    _snapSelect.setSensitivity(10f);
                    _snapSelect.addElement("X", paramVec2.value.x);
                    _snapSelect.addElement("Y", paramVec2.value.y);
                    break;
                case AbstractParameter.ParameterType.VECTOR3:
                    Parameter<Vector3> paramVec3 = (Parameter<Vector3>)abstractParam;
                    paramVec3.hasChanged += _snapSelect.setParam;
                    _snapSelect.setSensitivity(10f);
                    _snapSelect.addElement("X", paramVec3.value.x);
                    _snapSelect.addElement("Y", paramVec3.value.y);
                    _snapSelect.addElement("Z", paramVec3.value.z);
                    _snapSelect.addElement("XYZ", (paramVec3.value.x + paramVec3.value.y + paramVec3.value.z) / 3f);
                    break;
                case AbstractParameter.ParameterType.QUATERNION:
                    Parameter<Quaternion> paramQuat = (Parameter<Quaternion>)abstractParam;
                    paramQuat.hasChanged += _snapSelect.setParam;
                    Vector3 rot = paramQuat.value.eulerAngles;
                    _snapSelect.setSensitivity(500f);
                    _snapSelect.addElement("X", rot.x);
                    _snapSelect.addElement("Y", rot.y);
                    _snapSelect.addElement("Z", rot.z);
                    break;
                default:
                    Helpers.Log("Parameter Type cannot be edited with Spinner.");
                    break;
            }
        }

        //!
        //! function connecting the events
        //!
        private void Start()
        {
            _snapSelect.parameterChanged += changeAxis;
            _snapSelect.valueChanged += setValue;
            _snapSelect.editingEnded += editingDone;
        }

        //!
        //! event invoking the doneEditing event whenever the user stops editing a parameter (e.g. finger lifted)
        //! @param sender source of the event
        //! @param e payload
        //!
        private void editingDone(object sender, bool e)
        {
            doneEditing?.Invoke(this, abstractParam);
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
                    if (_currentAxis == 0)
                        paramVec2.setValue(new Vector2(valVec2.x + val, valVec2.y));
                    else
                        paramVec2.setValue(new Vector2(valVec2.x, valVec2.y + val));
                    break;
                case AbstractParameter.ParameterType.VECTOR3:
                    Parameter<Vector3> paramVec3 = (Parameter<Vector3>)abstractParam;
                    Vector3 valVec3 = paramVec3.value;
                    if (_currentAxis == 0)
                        paramVec3.setValue(new Vector3(valVec3.x + val, valVec3.y, valVec3.z));
                    else if (_currentAxis == 1)
                        paramVec3.setValue(new Vector3(valVec3.x, valVec3.y + val, valVec3.z));
                    else if (_currentAxis == 2)
                        paramVec3.setValue(new Vector3(valVec3.x, valVec3.y, valVec3.z + val));
                    else
                        paramVec3.setValue(new Vector3(valVec3.x + val, valVec3.y + val, valVec3.z + val));
                    break;
                case AbstractParameter.ParameterType.QUATERNION:
                    Parameter<Quaternion> paramQuat = (Parameter<Quaternion>)abstractParam;
                    Quaternion rot = Quaternion.identity;
                    if (_currentAxis == 0)
                        rot = Quaternion.Euler(val, 0, 0);
                    else if (_currentAxis == 1)
                        rot = Quaternion.Euler(0, val, 0);
                    else
                        rot = Quaternion.Euler(0, 0, val); ;
                    paramQuat.setValue(paramQuat.value * rot);
                    break;
                default:
                    Helpers.Log("Parameter Type cannot be edited with Spinner.");
                    break;
            }
        }

        //!
        //! function changing the axis
        //! @param sender source of the event
        //! @param new axis index
        //!
        private void changeAxis(object sender, int index)
        {
            _currentAxis = index;
        }
    }
}
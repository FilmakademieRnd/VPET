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

//! @file "UICreator3DModule.cs"
//! @brief implementation of VPET 3D UI scene creator module
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Paulo Scatena
//! @version 0
//! @date 16.02.2022

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vpet
{
    //!
    //! implementation of VPET 3D UI scene creator module
    //!
    public class UICreator3DModule : UIManagerModule
    {
        private GameObject manipT;
        private GameObject manipR;
        private GameObject manipS;

        // Collider layer mask
        private int layerMask = 1 << 5;

        // Temporary to manipulate
        private SceneObject selObj;
        private int tIndex;
        private int rIndex;
        private int sIndex;

        Vector3 planeVec = Vector3.zero;
        Plane helperPlane;
        GameObject manipulator;

        // Internal reference of manipulator
        GameObject manipTx;
        GameObject manipTy;
        GameObject manipTz;
        //GameObject manipTxy;
        //GameObject manipTxz;
        //GameObject manipTyz;
        //GameObject manipRx;
        //GameObject manipRy;
        //GameObject manipRz;
        GameObject manipSx;
        GameObject manipSy;
        GameObject manipSz;
        GameObject manipSxy;
        GameObject manipSxz;
        GameObject manipSyz;


        Vector3 hitPosOffset = Vector3.zero;
        bool firstPress = true;
        Vector3 initialSca = Vector3.one;
        //GameObject scaleReferenceObj;

        // for free rotation
        Collider freeRotationColl;

        // Mode of operation of TRS manipulator
        int modeTRS = -1;

        Vector3 vecXY = new Vector3(1, 1, 0);
        Vector3 vecXZ = new Vector3(1, 0, 1);
        Vector3 vecYZ = new Vector3(0, 1, 1);

        //!
        //! A reference to the VPET input manager.
        //!
        private InputManager m_inputManager;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UICreator3DModule(string name, Core core) : base(name, core)
        {

        }

        //!
        //! Init callback for the UICreator3D module.
        //! Called after constructor. 
        //!
        protected override void Init(object sender, EventArgs e)
        {
            Debug.Log("Init 3D module");

            // Subscribe to selection change
            manager.selectionChanged += SelectionUpdate;

            // Subscrive to manipulator change
            manager.manipulatorChange += SetManipulatorMode;

            // Grabbing from the input manager directly
            m_inputManager = m_core.getManager<InputManager>();

            // Hookup to input events
            m_inputManager.InputPressStart += PressStart;
            m_inputManager.InputPressEnd += PressEnd;
            

            // Instantiate TRS widgest but keep them hidden
            InstantiateAxes();
            HideAxes();

            // hack temp scale 
            //scaleReferenceObj = new("ScaleReference");
        }



        //!
        //! Function that does nothing.
        //! Being called when selection has changed.
        //!
        private void PressStart(object sender, InputManager.InputEventArgs e)
        {
            //Debug.Log("Press start: " + e.point.ToString());

            // grab the hit manip
            manipulator = CameraRaycast(e.point);

            if (manipulator)
            {
                //Debug.Log(manipulator);

                // make a plane based on it
                planeVec = manipulator.transform.forward;
                Vector3 center = manipulator.GetComponent<Collider>().bounds.center;
                helperPlane = new Plane(planeVec, center);

                // if root modifier - plane normal is camera axis
                if (manipulator.tag == "gizmoCenter")
                    helperPlane = new Plane(Camera.main.transform.forward, center);

                // HACK - if translate single axis - plane normal is camera axis projected on the axis plane
                if (manipulator == manipTx || manipulator == manipTy || manipulator == manipTz)
                    helperPlane = new Plane(Vector3.ProjectOnPlane(Camera.main.transform.forward, manipulator.transform.up), center);


                // semi hack - if manip = main rotator - free rotation
                if (manipulator == manipR)
                {
                    freeRotationColl = manipulator.GetComponent<Collider>();
                }

                // monitor move
                m_inputManager.InputMove += Move;
            }
            // hack - storing initial scale in case of ui operation
            if (selObj)
            {
                Parameter<Vector3> sca = (Parameter<Vector3>)selObj.parameterList[sIndex];
                initialSca = sca.value;
            }
        }

        // Raycaster helper
        // Potential bad behaviors if there are other objects besides the manipulators with physics collider inside UI layer
        private GameObject CameraRaycast(Vector3 pos, int layerMask = 1 << 5)
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out RaycastHit hit, Mathf.Infinity, layerMask))
                return hit.collider.gameObject;

            return null;

        }

        private void PressEnd(object sender, InputManager.InputEventArgs e)
        {
            //Debug.Log("Press end: " + e.point.ToString());

            // stop monitoring move
            m_inputManager.InputMove -= Move;
            firstPress = true;

            // Hack - restore scale
            // restore position instead
            if (modeTRS == 2)
            {
                manipSx.transform.localPosition = Vector3.zero;
                manipSy.transform.localPosition = Vector3.zero;
                manipSz.transform.localPosition = Vector3.zero;
                manipSxy.transform.localPosition = Vector3.zero;
                manipSxz.transform.localPosition = Vector3.zero;
                manipSyz.transform.localPosition = Vector3.zero;
            }
        }

        // This for mouse drag
        // Should only operate in case of existing selection
        // But what happens if touch input is moving the object and other function change the selection
        
        private void Move(object sender, InputManager.InputEventArgs e)
        {

            //Debug.Log("Moving: " + e.point.ToString());
            if (selObj == null)
                return;
 
            // drag object - translate
            if (modeTRS == 0)
            {
                //Create a ray from the Mouse click position
                Ray ray = Camera.main.ScreenPointToRay(e.point);
                if (helperPlane.Raycast(ray, out float enter))
                {
                    //Get the point that is clicked
                    Vector3 hitPoint = ray.GetPoint(enter);

                    Vector3 projectedVec = hitPoint;
                    // dirty temp hack - identify if single axis
                    if (manipulator == manipTx || manipulator == manipTy || manipulator == manipTz)
                    {
                        projectedVec = Vector3.Project(hitPoint - manipulator.transform.position, manipulator.transform.up) + manipulator.transform.position;
                    }

                    // store the offset between clicked point and center of obj
                    if (firstPress)
                    {
                        hitPosOffset = projectedVec - manipulator.transform.position;
                        firstPress = false;
                    }
                    // adjust
                    projectedVec -= hitPosOffset;

                    //actual move things - vpet assets
                    Vector3 localVec = selObj.transform.parent.transform.InverseTransformPoint(projectedVec);
                    Parameter<Vector3> pos = (Parameter<Vector3>)selObj.parameterList[tIndex];
                    pos.setValue(localVec);

                    // make gizmo follow
                    //TransformAxis(manipT, selObj.transform);
                }

            }
            

            // drag rotate - manip version
            if (modeTRS == 1)
            {
                bool hit = false;
                Vector3 hitPoint = Vector3.zero;

                //Create a ray from the Mouse click position
                Ray ray = Camera.main.ScreenPointToRay(e.point);

                // HACK if manip = main rotator - free rotation
                if (manipulator == manipR)
                {
                    if (freeRotationColl.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    {
                        hit = true;
                        //Get the point that is clicked - on the sphere collider
                        hitPoint = hitInfo.point;
                    }
                }
                else
                {
                    if (helperPlane.Raycast(ray, out float enter))
                    {
                        hit = true;
                        //Get the point that is clicked - on the place
                        hitPoint = ray.GetPoint(enter);
                    }
                }
                if (hit)
                {
                    // store the offset between clicked point and center of obj
                    if (firstPress)
                    {
                        hitPosOffset = hitPoint - manipulator.transform.position;
                        firstPress = false;
                    }

                    // get orientation quaternion
                    Quaternion rotQuat = new Quaternion();
                    rotQuat.SetFromToRotation(hitPosOffset, hitPoint - manipulator.transform.position);

                    //actual rotate things - vpet assets
                    Parameter<Quaternion> rot = (Parameter<Quaternion>)selObj.parameterList[rIndex];
                    rot.setValue(rotQuat * rot.value);
                    // make gizmo follow
                    //TransformAxis(manipR, selObj.transform);

                    // update offset
                    hitPosOffset = hitPoint - manipulator.transform.position;
                }
            }

            // drag object - scale
            if (modeTRS == 2)
            {
                //Create a ray from the Mouse click position
                Ray ray = Camera.main.ScreenPointToRay(e.point);
                if (helperPlane.Raycast(ray, out float enter))
                {
                    //Get the point that is clicked
                    Vector3 hitPoint = ray.GetPoint(enter);

                    Vector3 projectedVec = hitPoint;
                    // temp hack - identify if single axis
                    if (manipulator == manipSx || manipulator == manipSy || manipulator == manipSz)
                    {
                        projectedVec = Vector3.Project(hitPoint - manipS.transform.position, manipulator.transform.up) + manipS.transform.position;
                    }

                    Parameter<Vector3> sca;

                    // store the offset between clicked point and center of obj
                    if (firstPress)
                    {
                        hitPosOffset = projectedVec;
                        firstPress = false;
                        //initialSca = GameObject.Find("CubeUni").transform.localScale;
                        //sca = (Parameter<Vector3>)selObj.parameterList[sIndex];
                        //initialSca = sca.value;
                    }

                    //actual scale things - vpet assets
                    Vector3 deltaClick = projectedVec - hitPosOffset + manipS.transform.position;
                    Vector3 localDelta = manipS.transform.InverseTransformPoint(deltaClick);

                    // hack to see if it's main controller and so would use uniform scale - average values
                    if (manipulator == manipS)
                        localDelta = Vector3.one * (localDelta.x + localDelta.y + localDelta.z) / 3f;
                    
                    Vector3 scaleOffset = Vector3.one + localDelta;
                    sca = (Parameter<Vector3>)selObj.parameterList[sIndex];
                    sca.setValue(Vector3.Scale(initialSca, scaleOffset));

                    //// make gizmo follow - for UX feedback only
                    //// try moving the individual manipulator
                    //if (manipulator != manipS)
                    //{
                    //    manipulator.transform.localPosition = localDelta;
                    //    // if a double one
                    //    if (manipulator == manipSxy || manipulator == manipSxz || manipulator == manipSyz)
                    //    {
                    //        // also move the axes
                    //        manipSx.transform.localPosition = Vector3.Scale(localDelta, Vector3.right) / .61f;
                    //        manipSy.transform.localPosition = Vector3.Scale(localDelta, Vector3.up) / .61f;
                    //        manipSz.transform.localPosition = Vector3.Scale(localDelta, Vector3.forward) / .61f;
                    //    }
                    //}
                    //else // move them all to follow
                    //{
                    //    manipSx.transform.localPosition = Vector3.Scale(localDelta, Vector3.right) / .61f;
                    //    manipSy.transform.localPosition = Vector3.Scale(localDelta, Vector3.up) / .61f;
                    //    manipSz.transform.localPosition = Vector3.Scale(localDelta, Vector3.forward) / .61f;
                    //    manipSxy.transform.localPosition = Vector3.Scale(localDelta, new Vector3(1, 1, 0));
                    //    manipSxz.transform.localPosition = Vector3.Scale(localDelta, new Vector3(1, 0, 1));
                    //    manipSyz.transform.localPosition = Vector3.Scale(localDelta, new Vector3(0, 1, 1));
                    //}
                }

            }
        }

        //!
        //! Function that does nothing.
        //! Being called when selection has changed.
        //!
        private void SelectionUpdate(object sender, List<SceneObject> sceneObjects)
        {

            // Log
            //Debug.Log("Selection changed");

            if (sceneObjects.Count > 0)
            {
                // Grab object
                selObj = sceneObjects[0];
                //Debug.Log(selObj);
                GrabParameterIndex();

                // Start with translation
                if (modeTRS == -1)
                    //SetModeT();
                    SetManipulatorMode(null, 0);
            }
            else // empty selection
            {
                HideAxes();
                modeTRS = -1;
                SetManipulatorMode(null, -1);
            }

        }


        void GrabParameterIndex()
        {
            //Debug.Log("INDEXES");
            for (int i = 0; i < selObj.parameterList.Count; i++)
            {
                if (selObj.parameterList[i].name.Equals("rotation"))
                    rIndex = i;
                if (selObj.parameterList[i].name.Equals("position"))
                    tIndex = i;
                if (selObj.parameterList[i].name.Equals("scale"))
                    sIndex = i;
            }
            //Debug.Log(tIndex);
            //Debug.Log(rIndex);
            //Debug.Log(sIndex);

        }

        void UpdateParameter()
        {
            Debug.Log("PARAMETER");
            foreach (var paramater in selObj.parameterList)
            {
                //ParameterType vpetType = toVPETType(paramater._type);
                Debug.Log(paramater.cType);
                Debug.Log(paramater.name);
            }

            //selObj.parameterList;
            //AbstractParameter abstractParam
            //Parameter<Quaternion> p = (Parameter<Quaternion>)abstractParam;
        }

        private void InstantiateAxes()
        {
            // Tranlation
            GameObject resourcePrefab = Resources.Load<GameObject>("Prefabs/gizmoTranslate");
            manipT = GameObject.Instantiate(resourcePrefab);
            // Rotation
            resourcePrefab = Resources.Load<GameObject>("Prefabs/gizmoRotate");
            manipR = GameObject.Instantiate(resourcePrefab);
            // Scale
            resourcePrefab = Resources.Load<GameObject>("Prefabs/gizmoScale");
            manipS = GameObject.Instantiate(resourcePrefab);

            // Grab its parts
            manipTx = manipT.transform.GetChild(0).GetChild(0).gameObject;
            manipTy = manipT.transform.GetChild(0).GetChild(1).gameObject;
            manipTz = manipT.transform.GetChild(0).GetChild(2).gameObject;
            //manipTxy = manipT.transform.GetChild(0).GetChild(3).gameObject;
            //manipTxz = manipT.transform.GetChild(0).GetChild(4).gameObject;
            //manipTyz = manipT.transform.GetChild(0).GetChild(5).gameObject;

            //manipRx = manipR.transform.GetChild(0).GetChild(0).gameObject;
            //manipRy = manipR.transform.GetChild(0).GetChild(1).gameObject;
            //manipRz = manipR.transform.GetChild(0).GetChild(2).gameObject;

            manipSx = manipS.transform.GetChild(0).GetChild(0).gameObject;
            manipSy = manipS.transform.GetChild(0).GetChild(1).gameObject;
            manipSz = manipS.transform.GetChild(0).GetChild(2).gameObject;
            manipSxy = manipS.transform.GetChild(0).GetChild(3).gameObject;
            manipSxz = manipS.transform.GetChild(0).GetChild(4).gameObject;
            manipSyz = manipS.transform.GetChild(0).GetChild(5).gameObject;
        }

        private void HideAxis(GameObject manip)
        {
            manip.SetActive(false);
        }

        private void HideAxes()
        {
            HideAxis(manipT);
            HideAxis(manipR);
            HideAxis(manipS);
            modeTRS = -1;
        }


        private void ShowAxis(GameObject manip)
        {
            manip.SetActive(true);
        }


        private void MoveAxis(Vector3 pos)
        {
            manipT.transform.position = pos;
        }

        private void TransformAxis(GameObject manip, Transform xform)
        {
            manip.transform.position = xform.position;
            manip.transform.rotation = xform.rotation;

            // Adjust scale
            manip.transform.localScale = GetModifierScale();
        }

        private void SetManipulatorMode(object sender, int manipulatorMode)
        {
            if (selObj)
            {
                // Unsubscribe all
                Parameter<Vector3> pos = (Parameter<Vector3>)selObj.parameterList[tIndex];
                Parameter<Quaternion> rot = (Parameter<Quaternion>)selObj.parameterList[rIndex];
                Parameter<Vector3> sca = (Parameter<Vector3>)selObj.parameterList[sIndex];
                pos.hasChanged -= UpdateManipulatorPosition;
                rot.hasChanged -= UpdateManipulatorRotation;
                sca.hasChanged -= UpdateManipulatorScale;

                if (manipulatorMode == 0)
                {
                    SetModeT();
                    // Subscribe to change
                    pos.hasChanged += UpdateManipulatorPosition;
                }
                else if (manipulatorMode == 1)
                {
                    SetModeR();
                    // Subscribe to change
                    rot.hasChanged += UpdateManipulatorRotation;
                }
                else if (manipulatorMode == 2)
                {
                    SetModeS();
                    // Subscribe to change
                    sca.hasChanged += UpdateManipulatorScale;
                }
            }

        }

        public void UpdateManipulatorPosition(object sender, Vector3 position)
        {
            manipT.transform.position = position;
            manipT.transform.localScale = GetModifierScale();
        }

        public void UpdateManipulatorRotation(object sender, Quaternion rotation)
        {
            manipR.transform.rotation = rotation;
        }

        public void UpdateManipulatorScale(object sender, Vector3 scale)
        {
            Vector3 vecOfsset = Vector3.Scale(scale, VecInvert(initialSca));
            Vector3 localDelta = vecOfsset - Vector3.one;
            //Vector3 deltaNorm = localDelta.normalized;
            //Debug.Log("DELTA " + localDelta.ToString());
            
            // Grab "dimension" of delta
            float UniX = NonZero(localDelta.x);
            float UniY = NonZero(localDelta.y);
            float UniZ = NonZero(localDelta.z);

            // Main axes
            manipSx.transform.localPosition = Vector3.Scale(localDelta, Vector3.right) / .61f;
            manipSy.transform.localPosition = Vector3.Scale(localDelta, Vector3.up) / .61f;
            manipSz.transform.localPosition = Vector3.Scale(localDelta, Vector3.forward) / .61f;
            // Multi axes
            manipSxy.transform.localPosition = UniX * UniY * Vector3.Scale(localDelta, vecXY);
            manipSxz.transform.localPosition = UniX * UniZ * Vector3.Scale(localDelta, vecXZ);
            manipSyz.transform.localPosition = UniY * UniZ * Vector3.Scale(localDelta, vecYZ);
        }

        private float NonZero(float number)
        {
            // This is not working
            // return Mathf.Approximately(number, 0.0f) ? 0.0f : 1.0f;
            // Tolerance needs to be higher than Mathf.Epsilon 
            if (number >= -1E-06 && number <= 1E-06)
            {
                return 0f;
            }
            return 1f;
        }

        // Invert vector by component
        private Vector3 VecInvert(Vector3 vec)
        {
            return new Vector3(1 / vec.x, 1 / vec.y, 1 / vec.z);
        }

        // For scale adjustment
        private Vector3 GetModifierScale()
        {
            return Vector3.one
                       * (Vector3.Distance(Camera.main.transform.position, selObj.transform.position)
                       * (2.0f * Mathf.Tan(0.5f * (Mathf.Deg2Rad * Camera.main.fieldOfView)))
                       * Screen.dpi / 1000);
        }

        public void SetModeT()
        {
            Debug.Log("T mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipT);
                TransformAxis(manipT, selObj.transform);
                modeTRS = 0;
            }
        }

        public void SetModeR()
        {
            Debug.Log("R mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipR);
                TransformAxis(manipR, selObj.transform);
                modeTRS = 1;
            }
        }

        public void SetModeS()
        {
            Debug.Log("S mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipS);
                TransformAxis(manipS, selObj.transform);
                modeTRS = 2;
            }
        }

    }
}
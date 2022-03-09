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
//! @date 07.03.2022

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

        // Selected objects to manipulate
        private SceneObject selObj;
        List<SceneObject> selObjs;
        List<Vector3> objOffsets = new List<Vector3>();

        // Indexes of T R S parameters
        private int tIndex;
        private int rIndex;
        private int sIndex;

        // Auxiliary vector and plane for raycasting
        Vector3 planeVec = Vector3.zero;
        Plane helperPlane;

        // Active (clicked) manipulator object
        GameObject manipulator;

        // Internal reference of manipulator parts
        GameObject manipTx;
        GameObject manipTy;
        GameObject manipTz;
        GameObject manipSx;
        GameObject manipSy;
        GameObject manipSz;
        GameObject manipSxy;
        GameObject manipSxz;
        GameObject manipSyz;

        // Auxiliart vectors for correct manipulation
        Vector3 hitPosOffset = Vector3.zero;
        bool firstPress = true;
        Vector3 initialSca = Vector3.one;

        // Auxiliary collider for raycasting
        Collider freeRotationColl;

        // Buffer quaternion for visualizing multi object rotation
        Quaternion visualRot = Quaternion.identity;

        // Mode of operation of TRS manipulator
        int modeTRS = -1;

        // Auxiliary preconstructed vectors
        readonly Vector3 vecXY = new(1, 1, 0);
        readonly Vector3 vecXZ = new(1, 0, 1);
        readonly Vector3 vecYZ = new(0, 1, 1);

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
            //Debug.Log("Init 3D module");

            // Subscribe to selection change
            manager.selectionChanged += SelectionUpdate;

            // Subscribe to manipulator change
            //manager.manipulatorChange += SetManipulatorMode;
            UICreator2DModule UI2DModule = manager.getModule<UICreator2DModule>();
            UI2DModule.parameterChanged += SetManipulatorMode;

            // Grabbing from the input manager directly
            m_inputManager = m_core.getManager<InputManager>();

            // Hookup to input events
            m_inputManager.InputPressStart += PressStart;
            m_inputManager.InputPressEnd += PressEnd;
            

            // Instantiate TRS widgest but keep them hidden
            InstantiateAxes();
            HideAxes();
        }


        //!
        //! Function to select the manipulator and prepare for transformations.
        //! Called with the start of click from InputManager
        //! @param sender callback sender
        //! @param e event reference
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

        //!
        //! Helper function that raycasts from camera and returns hit game object
        //! It subscribes (at PressStart) to the event triggered at every position update from InputManager
        //! @param pos screen point through which to raycast
        //! @param layerMask layer mask containing the objects to be considered
        //!
        // Warning?
        // Potential bad behaviors if there are other objects besides the manipulators with physics collider inside UI layer
        private GameObject CameraRaycast(Vector3 pos, int layerMask = 1 << 5)
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out RaycastHit hit, Mathf.Infinity, layerMask))
                return hit.collider.gameObject;

            return null;

        }

        //!
        //! Function to finalize manipulator operation
        //! Called with the end (cancellation) of click from InputManager
        //! @param sender callback sender
        //! @param e event reference
        //!
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

            // for multi selection
            objOffsets.Clear();
            if(selObjs != null)
                if (selObjs.Count > 1)
                {
                    // restore rotation gizmo orientation
                    visualRot = Quaternion.identity;
                    TransformManipR(visualRot);
                }

        }

        //!
        //! Function to be performed on click/touch drag
        //! It subscribes (at PressStart) to the event triggered at every position update from InputManager
        //! @param sender callback sender
        //! @param e event reference
        //!
        // Warning?
        // Should only operate in case of existing selection
        // But what happens if touch input is moving the object and other function change the selection?
        private void Move(object sender, InputManager.InputEventArgs e)
        {

            //Debug.Log("Moving: " + e.point.ToString());
            if (selObj == null)
                return;

            // drag object - translate
            if (modeTRS == 0) // multi obj dev
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

                        // for multi move
                        foreach (SceneObject obj in selObjs)
                        {
                            objOffsets.Add(obj.transform.position - manipulator.transform.position);
                            Debug.Log("OBJECT: " + obj.ToString());
                            Debug.Log("OFFSET: " + (obj.transform.position - manipulator.transform.position).ToString());
                        }
                    }
                    // adjust
                    projectedVec -= hitPosOffset;

                    // Actual translation operation
                    // For a single object
                    if (selObjs.Count == 1)
                    {
                        Vector3 localVec = selObj.transform.parent.transform.InverseTransformPoint(projectedVec);
                        Parameter<Vector3> pos = (Parameter<Vector3>)selObj.parameterList[tIndex];
                        pos.setValue(localVec);
                    }
                    // For multiple objects
                    else
                    {
                        for (int i = 0; i < selObjs.Count; i++)
                        {
                            Vector3 localVec = selObjs[i].transform.parent.transform.InverseTransformPoint(projectedVec + objOffsets[i]);
                            Parameter<Vector3> pos = (Parameter<Vector3>)selObjs[i].parameterList[tIndex];
                            pos.setValue(localVec);
                        }
                    }
                }
            }

            

            // drag rotate - manip version
            if (modeTRS == 1)
            {
                bool hit = false;
                Vector3 hitPoint = Vector3.zero;

                // reate a ray from the Mouse click position
                Ray ray = Camera.main.ScreenPointToRay(e.point);

                // if manip = main rotator - free rotation
                if (manipulator == manipR)
                {
                    if (freeRotationColl.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    {
                        hit = true;
                        //Get the point that is clicked - on the sphere collider
                        hitPoint = hitInfo.point;
                    }
                }
                // else it's one of the axis spinner
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

                    // Actual rotation operation
                    // For a single object
                    if (selObjs.Count == 1)
                    {
                        Parameter<Quaternion> rot = (Parameter<Quaternion>)selObj.parameterList[rIndex];
                        rot.setValue(rotQuat * rot.value);
                    }
                    // For multiple objects
                    else
                    {
                        for (int i = 0; i < selObjs.Count; i++)
                        {
                            // Effect on position
                            Vector3 srcPos = selObjs[i].transform.position;
                            Vector3 pivotPoint = manipR.transform.position;
                            Vector3 dstPos = rotQuat * (srcPos - pivotPoint) + pivotPoint;
                            Vector3 localVec = selObjs[i].transform.parent.transform.InverseTransformPoint(dstPos);
                            Parameter<Vector3> pos = (Parameter<Vector3>)selObjs[i].parameterList[tIndex];
                            pos.setValue(localVec);

                            // Rotation
                            Parameter<Quaternion> rot = (Parameter<Quaternion>)selObjs[i].parameterList[rIndex];
                            rot.setValue(rotQuat * rot.value);
                        }

                        // Make gizmo follow
                        visualRot *= rotQuat;
                        TransformManipR(visualRot);
                    }

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
                // by reference
                selObjs = sceneObjects;
                // by clone
                selObjs = new List<SceneObject>(sceneObjects);

                //Debug.Log(selObj);
                GrabParameterIndex();

                // Start with translation
                // todo: confirm this design choice
                if (modeTRS == -1)
                    SetManipulatorMode(null, 0);

                // development for multi
                if (sceneObjects.Count > 1)
                    SetMultiManipulatorMode(null, 0);

            }
            else // empty selection
            {
                // Clean selection
                selObj = null;

                HideAxes();
                modeTRS = -1;
                //SetManipulatorMode(null, -1);
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

        private void TransformAxisMulti(GameObject manip)
        {
            // average position
            Vector3 averagePos = Vector3.zero;
            foreach (SceneObject obj in selObjs)
            {
                averagePos += obj.transform.position;
            }
            averagePos /= selObjs.Count;

            manip.transform.position = averagePos;
            manip.transform.rotation = selObj.transform.rotation;
            if (selObjs.Count > 1)
                manip.transform.rotation = Quaternion.identity;

            // Adjust scale
            manip.transform.localScale = GetModifierScale();
        }

        private void TransformManipR(Quaternion rot)
        {
            manipR.transform.rotation = rot;
        }


        private void TransformAxis(GameObject manip, Transform xform)
        {
            manip.transform.position = xform.position;
            manip.transform.rotation = xform.rotation;

            // Adjust scale
            manip.transform.localScale = GetModifierScale();
        }

        private void SetMultiManipulatorMode(object sender, int manipulatorMode)
        {
            //if (selObjs.Count > 1)
            HideAxes();
            ShowAxis(manipT);
            // transform axis for both
            Vector3 averagePos = Vector3.zero;
            foreach (SceneObject obj in selObjs)
            {
                averagePos += obj.transform.position;
            }
            averagePos /= selObjs.Count;
            manipT.transform.position = averagePos;
            // neutral rotation for global mode
            manipT.transform.rotation = Quaternion.identity;
            manipT.transform.localScale = GetModifierScale();
            modeTRS = 0;
            
        }

        // now for multi
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
            // for multi selection
            if (selObjs.Count > 1)
            {
                Vector3 averagePos = Vector3.zero;
                foreach (SceneObject obj in selObjs)
                {
                    averagePos += obj.transform.position;
                }
                averagePos /= selObjs.Count;
                manipT.transform.position = averagePos;
            }
        }

        public void UpdateManipulatorRotation(object sender, Quaternion rotation)
        {
            if (selObjs.Count <= 1) // only update here if single selection
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
            // Following short-version is not working
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
            //Debug.Log("T mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipT);
                //TransformAxis(manipT, selObj.transform);
                TransformAxisMulti(manipT);
                modeTRS = 0;
            }
        }

        public void SetModeR()
        {
            //Debug.Log("R mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipR);
                //TransformAxis(manipR, selObj.transform);
                TransformAxisMulti(manipR);
                modeTRS = 1;
            }
        }

        public void SetModeS()
        {
            //Debug.Log("S mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipS);
                //TransformAxis(manipS, selObj.transform);
                TransformAxisMulti(manipS);
                modeTRS = 2;
            }
        }

    }
}
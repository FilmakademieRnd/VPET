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
        //!
        //! Reference to the translation manipulator
        //!
        private GameObject manipT;

        //!
        //! Reference to the rotation manipulator
        //!
        private GameObject manipR;

        //!
        //! Reference to the scale manipulator
        //!
        private GameObject manipS;

        //!
        //! Active (clicked) manipulator object
        //! 
        GameObject manipulator;

        //!
        //! Reference to the last active manipulator (for hide/unhide cases)
        //!
        private GameObject lastActiveManip;

        //!
        //! Selected object to be manipulated
        //!
        private SceneObject selObj;

        //!
        //! List of selected objects to be manipulated (for multi-selection)
        //!
        List<SceneObject> selObjs = new();

        //!
        //! List of the positional offset of selected objects (for spatial preservation)
        //!
        List<Vector3> objOffsets = new();

        // Indexes of T R S parameters - or can it always be assumed to be 0, 1, 2?
        private int tIndex;
        private int rIndex;
        private int sIndex;

        //!
        //! UI scale multiplier
        //!
        float uiScale = 1;

        //!
        //! Auxiliary vector for raycasting
        //!
        Vector3 planeVec = Vector3.zero;

        //!
        //! Auxiliary plane for raycasting
        //!
        Plane helperPlane;

        //!
        //! Internal reference of manipulator parts - translate X
        //!
        GameObject manipTx;

        //!
        //! Internal reference of manipulator parts - translate Y
        //!
        GameObject manipTy;

        //!
        //! Internal reference of manipulator parts - translate Z
        //!
        GameObject manipTz;

        //!
        //! Internal reference of manipulator parts - scale X
        //!
        GameObject manipSx;

        //!
        //! Internal reference of manipulator parts - scale Y
        //!
        GameObject manipSy;

        //!
        //! Internal reference of manipulator parts - scale Z
        //!        
        GameObject manipSz;

        //!
        //! Internal reference of manipulator parts - scale XY
        //!
        GameObject manipSxy;

        //!
        //! Internal reference of manipulator parts - scale XZ
        //!        
        GameObject manipSxz;

        //!
        //! Internal reference of manipulator parts - scale YZ
        //!
        GameObject manipSyz;

        //!
        //! 
        //!

        //!
        //! Auxiliary vector for storing the click offset
        //! 
        Vector3 hitPosOffset = Vector3.zero;

        //!
        //! Control boolean for single procedure for the first touch contact
        //!
        bool firstPress = true;

        //!
        //! Reference to initial scale value for parameter change
        //!
        Vector3 initialSca = Vector3.one;

        //!
        //! Stores manipulator position in its object local space (to save multiple calls)
        //!
        Vector3 localManipPosition;

        //!
        //! Buffer quaternion for visualizing multi object rotation
        //!
        Quaternion visualRot = Quaternion.identity;

        //!
        //! Mode of operation of TRS manipulator
        //!
        int modeTRS = -1;

        //!
        //! Auxiliary preconstructed vector - XY plane
        //!
        readonly Vector3 vecXY = new(1, 1, 0);

        //!
        //! Auxiliary preconstructed vector - XZ plane
        //!
        readonly Vector3 vecXZ = new(1, 0, 1);

        //!
        //! Auxiliary preconstructed vector - YZ plane
        //!
        readonly Vector3 vecYZ = new(0, 1, 1);

        //!
        //! Reference of main camera
        //!
        Camera mainCamera;

        //!
        //! A reference to the VPET input manager.
        //!
        private InputManager m_inputManager;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public UICreator3DModule(string name, Manager manager) : base(name, manager)
        {

        }

        //!
        //! Destructor, cleaning up event registrations. 
        //!
        public override void Dispose()
        {
            base.Dispose();

            // Unsubscribe
            manager.selectionChanged -= SelectionUpdate;

            m_inputManager.inputPressPerformed -= PressStart;
            m_inputManager.inputPressEnd -= PressEnd;

            m_inputManager.fingerGestureEvent -= updateGizmoScale;
            m_inputManager.updateCameraUICommand -= updateGizmoScale;

            UICreator2DModule UI2DModule = manager.getModule<UICreator2DModule>();
            CameraSelectionModule CamModule = manager.getModule<CameraSelectionModule>();
            UIManager m_UIManager = core.getManager<UIManager>();
            if (UI2DModule != null && CamModule != null && m_UIManager != null)
            {
                UI2DModule.parameterChanged -= SetManipulatorMode;
                CamModule.uiCameraOperation -= SetCameraManipulator;
                m_UIManager.settings.uiScale.hasChanged -= updateUIScale;
            }
        }

        //!
        //! Init callback for the UICreator3D module.
        //! Called after constructor. 
        //!
        protected override void Init(object sender, EventArgs e)
        {
            //Debug.Log("Init 3D module");
            mainCamera = Camera.main;

            // Subscribe to selection change
            manager.selectionChanged += SelectionUpdate;

            // Subscribe to manipulator change
            //manager.manipulatorChange += SetManipulatorMode;
            UICreator2DModule UI2DModule = manager.getModule<UICreator2DModule>();
            // is a safety non-null check need?
            UI2DModule.parameterChanged += SetManipulatorMode;

            // Subscribe to camera change?
            CameraSelectionModule CamModule = manager.getModule<CameraSelectionModule>();
            if (CamModule != null)
                CamModule.uiCameraOperation += SetCameraManipulator;

            // Grabbing from the input manager directly
            m_inputManager = core.getManager<InputManager>();

            // Hookup to input events
            m_inputManager.inputPressPerformed += PressStart;
            m_inputManager.inputPressEnd += PressEnd;

            m_inputManager.fingerGestureEvent += updateGizmoScale;
            m_inputManager.updateCameraUICommand += updateGizmoScale;

            // Grabbing scene scale
            UIManager m_UIManager = core.getManager<UIManager>();
            if (m_UIManager != null)
            {
                uiScale = m_UIManager.settings.uiScale.value;
                m_UIManager.settings.uiScale.hasChanged += updateUIScale;
            }

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
                //Debug.DrawRay(center, planeVec * 10, Color.red, 1);

                // if root modifier - plane normal is camera axis
                if (manipulator.tag == "gizmoCenter")
                    helperPlane = new Plane(mainCamera.transform.forward, center);

                // HACK - if translate single axis - plane normal is camera axis projected on the axis plane
                if (manipulator == manipTx || manipulator == manipTy || manipulator == manipTz)
                    helperPlane = new Plane(Vector3.ProjectOnPlane(mainCamera.transform.forward, manipulator.transform.up), center);

                // semi hack - if manip = main rotator - free rotation
                if (manipulator == manipR)
                    //{
                    //freeRotationColl = manipulator.GetComponent<Collider>();
                    // make the collision plane a bit in front of the object
                    helperPlane = new Plane(mainCamera.transform.forward, center - .2f * Vector3.Distance(mainCamera.transform.position, selObj.transform.position) * mainCamera.transform.forward);
                //}

                // store manipulator position in its object local space (to save multiple calls)
                localManipPosition = selObj.transform.parent.transform.InverseTransformPoint(manipulator.transform.position);

                // monitor move
                m_inputManager.inputMove += Move;
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

            if (Physics.Raycast(mainCamera.ScreenPointToRay(pos), out RaycastHit hit, Mathf.Infinity, layerMask))
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
            m_inputManager.inputMove -= Move;
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
                Ray ray = mainCamera.ScreenPointToRay(e.point);
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
                            //Debug.Log("OBJECT: " + obj.ToString());
                            //Debug.Log("OFFSET: " + (obj.transform.position - manipulator.transform.position).ToString());
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
                Ray ray = mainCamera.ScreenPointToRay(e.point);
                //Debug.Log(e.point);
                //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

                // if manip = main rotator - free rotation
                if (manipulator == manipR)
                {
                    //// If click within sphere collider
                    //if (freeRotationColl.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    //    //if (helperSphere.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    //{
                    //    hit = true;
                    //    //Get the point that is clicked - on the sphere collider
                    //    hitPoint = hitInfo.point;
                    //    if (!insideFreeRotColl)
                    //    {
                    //        insideFreeRotColl = true;
                    //        firstPress = true;
                    //    }
                    //}
                    // If click beyond sphere collider
                    //else if (helperPlane.Raycast(ray, out float enter))
                    if (helperPlane.Raycast(ray, out float enter))
                    {
                        //if (insideFreeRotColl)
                        //{
                        //    insideFreeRotColl = false;
                        //    firstPress = true;
                        //}
                        hit = true;
                        //Get the point that is clicked - on the normal to camera plane
                        hitPoint = ray.GetPoint(enter);
                        // Convert to camera space
                        //hitPoint = mainCamera.transform.InverseTransformPoint(hitPoint);
                        // Convert to object space
                        hitPoint = selObj.transform.parent.transform.InverseTransformPoint(hitPoint);
                    }
                }
                // else it's one of the axis spinner
                else
                {
                    if (helperPlane.Raycast(ray, out float enter))
                    {
                        hit = true;
                        //Get the point that is clicked - on the plane
                        hitPoint = ray.GetPoint(enter);
                        // change to the object local space
                        hitPoint = selObj.transform.parent.transform.InverseTransformPoint(hitPoint);
                    }
                }
                if (hit)
                {
                    // store the offset between clicked point and center of obj
                    if (firstPress)
                    {
                        //if (manipulator == manipR && !insideFreeRotColl)
                        //    hitPosOffset = hitPoint - mainCamera.transform.InverseTransformPoint(manipulator.transform.position);
                        //else
                        //hitPosOffset = hitPoint - manipulator.transform.position;
                        hitPosOffset = hitPoint - localManipPosition;

                        firstPress = false;
                    }

                    // get orientation quaternion
                    Quaternion rotQuat = new Quaternion();

                    // Specific case of extension of rotation beyond sphere collider
                    //if (manipulator == manipR)// && !insideFreeRotColl)
                    //{
                    //    // Use the difference between hit points in camera space
                    //    Vector3 hitPos = hitPoint - mainCamera.transform.InverseTransformPoint(manipulator.transform.position);
                    //    Vector3 click1 = hitPosOffset;
                    //    Vector3 deltaPos = hitPos - hitPosOffset;
                    //    float deltaAngle = Vector3.SignedAngle(hitPosOffset, hitPos, mainCamera.transform.forward);
                    //    // Break in components
                    //    float lateralComponent = MathF.Sin(deltaAngle * Mathf.Deg2Rad) * hitPos.magnitude;
                    //    float frontalComponent = MathF.Cos(deltaAngle * Mathf.Deg2Rad) * hitPos.magnitude - click1.magnitude;
                    //    // Spin
                    //    rotQuat = Quaternion.AngleAxis(lateralComponent * 50, mainCamera.transform.forward);
                    //    // Tumble
                    //    Vector3 rotAxis = Quaternion.Euler(90, 90, 0) * new Vector3(deltaPos.x, deltaPos.y, 0);
                    //    rotQuat *= Quaternion.AngleAxis(Mathf.Abs(frontalComponent) * 200, rotAxis);
                    //}
                    //// Default case
                    //rotQuat.SetFromToRotation(Vector3.zero, (hitPoint - localManipPosition)-hitPosOffset);
                    //else
                    //rotQuat.SetFromToRotation(hitPosOffset, hitPoint - manipulator.transform.position);
                    rotQuat.SetFromToRotation(hitPosOffset, hitPoint - localManipPosition);

                    // Strengthen free rotation
                    if (manipulator == manipR)
                        rotQuat *= rotQuat;

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
                    //if (manipulator == manipR && !insideFreeRotColl)
                    //    hitPosOffset = hitPoint - mainCamera.transform.InverseTransformPoint(manipulator.transform.position);
                    //else
                    //hitPosOffset = hitPoint - manipulator.transform.position;
                    hitPosOffset = hitPoint - localManipPosition;
                }
            }

            // drag object - scale
            if (modeTRS == 2)
            {
                //Create a ray from the Mouse click position
                Ray ray = mainCamera.ScreenPointToRay(e.point);
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
                selObjs.Clear();

                //HideAxes();
                //modeTRS = -1;
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
        }

        //!
        //! Initial call for gizmo prefabs instantiation
        //!
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

            manipSx = manipS.transform.GetChild(0).GetChild(0).gameObject;
            manipSy = manipS.transform.GetChild(0).GetChild(1).gameObject;
            manipSz = manipS.transform.GetChild(0).GetChild(2).gameObject;
            manipSxy = manipS.transform.GetChild(0).GetChild(3).gameObject;
            manipSxz = manipS.transform.GetChild(0).GetChild(4).gameObject;
            manipSyz = manipS.transform.GetChild(0).GetChild(5).gameObject;
        }

        //!
        //! Hide specific manipulator
        //!
        private void HideAxis(GameObject manip)
        {
            manip.SetActive(false);
        }

        //!
        //! Hide all manipulators
        //!
        private void HideAxes()
        {
            if (manipT) HideAxis(manipT);
            if (manipR) HideAxis(manipR);
            if (manipS) HideAxis(manipS);
            modeTRS = -1;
        }

        //!
        //! Unhide last active manipulator
        //!
        private void UnhideAxis()
        {
            lastActiveManip.SetActive(true);
        }

        //!
        //! Show specific manipulator
        //!
        private void ShowAxis(GameObject manip)
        {
            manip.SetActive(true);
            lastActiveManip = manip;
        }

        //!
        //! Update the transform gizmo based on one or more selected objects
        //!
        private void TransformAxisMulti(GameObject manip)
        {
            // average position
            Vector3 averagePos = Vector3.zero;
            foreach (SceneObject obj in selObjs)
            {
                averagePos += obj.transform.position;
            }
            averagePos /= selObjs.Count;

            manip.transform.SetPositionAndRotation(averagePos, selObj.transform.rotation);
            if (selObjs.Count > 1)
                manip.transform.rotation = Quaternion.identity;

            // Adjust scale
            manip.transform.localScale = GetModifierScale();
        }

        //!
        //! Update the transform gizmo scale
        //!
        private void UpdateManipScale()
        {
            if (lastActiveManip)
                lastActiveManip.transform.localScale = GetModifierScale();
        }

        //!
        //! Update the rotate transform gizmo 
        //!
        private void TransformManipR(Quaternion rot)
        {
            manipR.transform.rotation = rot;
        }

        //!
        //! Update the rotate transform gizmo 
        //!
        private void SetMultiManipulatorMode(object sender, int manipulatorMode)
        {
            // Start in translate mode
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
            // Incomplete function - lacking manipulator mode
        }

        //!
        //! Set the mode of operation of the manipulator and its respective event subscriptions
        //!
        private void SetManipulatorMode(object sender, int manipulatorMode)
        {
            // Disable manipulator
            if (manipulatorMode < 0 || manipulatorMode > 2)
            {
                HideAxes();
                modeTRS = -1;
                // Place manipulator out of range to avoid unwanted click recognition when it's activated
                // [REVIEW]
                // float max is not the best coise for hiding an object
                if (manipT)
                    manipT.transform.position = float.MaxValue * Vector3.one;
                return;
            }

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
                    //pos.hasChanged += UpdateManipulatorPosition;
                }
                else if (manipulatorMode == 1)
                {
                    SetModeR();
                    // Subscribe to change
                    //rot.hasChanged += UpdateManipulatorRotation;
                }
                else if (manipulatorMode == 2)
                {
                    SetModeS();
                    // Subscribe to change
                    //sca.hasChanged += UpdateManipulatorScale;
                }
            }

        }

        //!
        //! Update the manipulator position according to position parameter changes
        //!
        public void UpdateManipulatorPosition(object sender, Vector3 position)
        {
            manipT.transform.localScale = GetModifierScale();
            // for one or more selected objects 
            Vector3 averagePos = Vector3.zero;
            foreach (SceneObject obj in selObjs)
            {
                averagePos += obj.transform.position;
            }
            averagePos /= selObjs.Count;
            manipT.transform.position = averagePos;
        }

        //!
        //! Update the manipulator rotation according to rotation parameter changes
        //!
        public void UpdateManipulatorRotation(object sender, Quaternion rotation)
        {
            if (selObjs.Count <= 1) // only update here if single selection
                manipR.transform.localRotation = selObj.transform.rotation;
        }

        //!
        //! Update the manipulator scale according to scale parameter changes
        //!
        public void UpdateManipulatorScale(object sender, Vector3 scale)
        {
            Vector3 vecOfsset = Vector3.Scale(scale, VecInvert(initialSca));
            Vector3 localDelta = vecOfsset - Vector3.one;

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

        //!
        //! Function coupled with UI camera operation to hide/unhide the manipulator
        //!
        private void SetCameraManipulator(object sender, bool cameraMode)
        {
            if (cameraMode)
                HideAxes();
            else
                UnhideAxis();
        }

        //!
        //! Helper function for non-zero evaluation
        //!
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

        //!
        //! Helper function for vector inversion by component
        //!
        private Vector3 VecInvert(Vector3 vec)
        {
            return new Vector3(1 / vec.x, 1 / vec.y, 1 / vec.z);
        }

        //!
        //! Helper function for transform gizmo scale adjustment according to screen and UI scale parameter
        //!
        private Vector3 GetModifierScale()
        {
            if (!selObj)
                return Vector3.one;
         
            return Vector3.one * uiScale
                       * (Vector3.Distance(mainCamera.transform.position, selObj.transform.position)
                       * (4.0f * Mathf.Tan(0.5f * (Mathf.Deg2Rad * mainCamera.fieldOfView)))
                       * Screen.dpi / (Screen.width + Screen.height));
        }

        //!
        //! Set transform gizmo to translate mode
        //!
        public void SetModeT()
        {
            //Debug.Log("T mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipT);
                TransformAxisMulti(manipT);
                modeTRS = 0;
            }
        }

        //!
        //! Set transform gizmo to rotate mode
        //!
        public void SetModeR()
        {
            //Debug.Log("R mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipR);
                TransformAxisMulti(manipR);
                modeTRS = 1;
            }
        }

        //!
        //! Set transform gizmo to scale mode
        //!
        public void SetModeS()
        {
            //Debug.Log("S mode");
            if (selObj != null)
            {
                HideAxes();
                ShowAxis(manipS);
                TransformAxisMulti(manipS);
                modeTRS = 2;
            }
        }

        //!
        //! Function coupled to user UI scale changes to update the gizmo scale
        //!
        private void updateUIScale(object sender, float e)
        {
            uiScale = e;
            UpdateManipScale();
        }

        //!
        //! Function coupled to camera operations to update the gizmo scale
        //!
        private void updateGizmoScale(object sender, bool e)
        {
            UpdateManipScale();
        }
    }
}
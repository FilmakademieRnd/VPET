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

//! @file "SceneObjectCamera.cs"
//! @brief Implementation of the SceneObjectCamera as a specialisation of SceneObject.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 01.03.2022

using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET camera object as a specialisation of the SceneObject
    //!
    public class SceneObjectCamera : SceneObject
    {
        //!
        //! field of view (horizonal value from Katana)
        //!
        public Parameter<float> fov;

        //!
        //! aspect ratio
        //!
        public Parameter<float> aspect;

        //!
        //! near plane
        //!
        public Parameter<float> near;

        //!
        //! far plane
        //!
        public Parameter<float> far;

        //!
        //! focus distance (in world space, meter)
        //!
        private Parameter<float> focDist;

        //!
        //! aperture
        //!
        private Parameter<float> aperture;

        //!
        //! reference to camera component
        //!
        private Camera _camera;

        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            _camera = this.GetComponent<Camera>();

            if (_camera)
            {
                fov = new Parameter<float>(_camera.fieldOfView, "fov", this);
                fov.hasChanged += updateFov;
                aspect = new Parameter<float>(_camera.aspect, "aspectRatio", this);
                aspect.hasChanged += updateAspect;
                near = new Parameter<float>(_camera.nearClipPlane, "nearClipPlane", this);
                near.hasChanged += updateNearClipPlane;
                far = new Parameter<float>(_camera.farClipPlane, "farClipPlane", this);
                far.hasChanged += updateFarClipPlane;
                focDist = new Parameter<float>(1f, "focalDistance", this);
                focDist.hasChanged += updateFocalDistance;
                aperture = new Parameter<float>(2.8f, "aperture", this);
                aperture.hasChanged += updateAperture;
            }
            else
                Helpers.Log("no camera component found!");
        }

        //!
        //! Function called, when Unity emit it's OnDestroy event.
        //!
        public override void OnDestroy()
        {
            base.OnDestroy();
            fov.hasChanged -= updateFov;
            aspect.hasChanged -= updateAspect;
            near.hasChanged -= updateNearClipPlane;
            far.hasChanged -= updateFarClipPlane;
            focDist.hasChanged -= updateFocalDistance;
            aperture.hasChanged -= updateAperture;
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
#if UNITY_EDITOR
            updateCameraParameters();
#endif
        }

        //!
        //! Update the camera fov of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new fov value
        //!
        private void updateFov(object sender, float a)
        {
            _camera.fieldOfView = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update the camera aspect ratio of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new aspect ratio value
        //!
        private void updateAspect(object sender, float a)
        {
            _camera.aspect = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update the camera near clip plane of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new near clip plane value
        //!
        private void updateNearClipPlane(object sender, float a)
        {
            _camera.nearClipPlane = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update the camera far clip plane of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new far clip plane value
        //!
        private void updateFarClipPlane(object sender, float a)
        {
            _camera.farClipPlane = a;
            emitHasChanged((AbstractParameter)sender);
        }

        //!
        //! Update the camera focal distance of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new focal distance value
        //!
        private void updateFocalDistance(object sender, float a)
        {
            //ToDO: Use this in PostEffect.
        }

        //!
        //! Update the camera aperture of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new aperture value
        //!
        private void updateAperture(object sender, float a)
        {
            //ToDO: Use this in PostEffect.
        }


        //!
        //! updates the Unity camera component specific parameters and informs all connected VPET parameters about the change
        //!
        private void updateCameraParameters()
        {
            if (_camera.fieldOfView != fov.value)
                fov.value = _camera.fieldOfView;
            if (_camera.aspect != aspect.value)
                aspect.value = _camera.aspect;
            if (_camera.nearClipPlane != near.value)
                near.value = _camera.nearClipPlane;
            if (_camera.farClipPlane != far.value)
                far.value = _camera.farClipPlane;
        }
    }
}
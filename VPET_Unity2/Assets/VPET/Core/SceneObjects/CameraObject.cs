using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET camera object as a specialisation of the SceneObject
    //!
    public class CameraObject : SceneObject
    {
        //!
        //! field of view (horizonal value from Katana)
        //!
        private Parameter<float> fov;

        //!
        //! aspect ratio
        //!
        private Parameter<float> aspect;

        //!
        //! near plane
        //!
        private Parameter<float> near;

        //!
        //! far plane
        //!
        private Parameter<float> far;

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
        public override void Start()
        {
            base.Start();
            _camera = this.GetComponent<Camera>();

            if (_camera)
            {
                fov = new Parameter<float>(_camera.fieldOfView);
                aspect = new Parameter<float>(_camera.aspect);
                near = new Parameter<float>(_camera.nearClipPlane);
                far = new Parameter<float>(_camera.farClipPlane);
                focDist = new Parameter<float>(1f);
                aperture = new Parameter<float>(2.8f);
            }
            else
                Helpers.Log("no camera component found!");
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
            updateCameraParameters();
        }

        //!
        //! updates the Unity camera component specific parameters and informs all connected VPET parameters about the change
        //!
        private void updateCameraParameters()
        {
            if (_camera.fieldOfView != fov.value)
                fov.setValue(_camera.fieldOfView);
            if (_camera.aspect != aspect.value)
                aspect.setValue(_camera.aspect);
            if (_camera.nearClipPlane != near.value)
                near.setValue(_camera.nearClipPlane);
            if (_camera.farClipPlane != far.value)
                far.setValue(_camera.farClipPlane);
        }
    }
}
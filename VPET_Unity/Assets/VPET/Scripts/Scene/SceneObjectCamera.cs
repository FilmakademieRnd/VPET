﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class SceneObjectCamera : SceneObject
    {
        //! enum listing the usable camera parameters (used by UI, Controller)
        public enum CameraParameter { FOV, LENS, APERTURE, FOCDIST, FOCSIZE };
        //!
        //! field of view (horizonal value from Katana)
        //!
        public float fov = 70f;
        //!
        //! aspect ratio
        //!
        public float aspect = 1.7777f;
        //!
        //! near plane
        //!
        public float near = 0.1f;
        //!
        //! far plane
        //!
        public float far = 100000f;
        //!
        //! focus distance (in world space, meter)
        //!
        public float focDist = 1.7f;
        //!
        //! focus size
        //!
        public float focSize = 0.3f;
        //!
        //! aperture
        //!
        public float aperture = 0.5f;

        // Start is called before the first frame update
        void Start()
        {
            base.Start();

            boxCollider.isTrigger = true; // not interacting
            this.gameObject.AddComponent<Rigidbody>();
            this.gameObject.GetComponent<Rigidbody>().mass = 100.0f;
            this.gameObject.GetComponent<Rigidbody>().drag = 2.5f;

            // TODO: temporary
            this.gameObject.GetComponent<Rigidbody>().useGravity = false;

            this.transform.localScale = new Vector3(1, 1, 1) * VPETSettings.Instance.maxExtend / 2000.0f;
        }

        //!
        //! hide or show the visualization of the camera
        //! @param      set     hide-> true, show->false   
        //!
#if !SCENE_HOST
        public override void hideVisualization(bool set)
        {
            if (set)
            {
                this.transform.GetChild(0).gameObject.SetActive(false);
                mainController.cameraAdapter.registerNearObject(this.transform.GetChild(0).gameObject);
            }
        }
#endif

        // Update is called once per frame
        void Update()
        {
            base.Update();
#if !SCENE_HOST
            if (mainController.ActiveMode == MainController.Mode.lookThroughCamMode && this.selected && (this.translationStillFrameCount == 0 || this.rotationStillFrameCount == 0))
            {
                Camera.main.transform.position = this.transform.position;
                Camera.main.transform.rotation = this.transform.rotation;
            }
#endif
        }
    }
}

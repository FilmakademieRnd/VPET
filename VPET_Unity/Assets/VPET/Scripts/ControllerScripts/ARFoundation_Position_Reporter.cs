#if !SCENE_HOST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace vpet
{
    public class ARFoundation_Position_Reporter : MonoBehaviour
    {
        private GameObject scene;
        private GameObject modifier;
        private MainController mainController;

        private void Awake()
        {
            scene = GameObject.Find("Scene");
            modifier = this.transform.GetChild(0).gameObject;
            mainController = GameObject.Find("MainController").GetComponent<MainController>();
        }

        // Update is called once per frame
        void Update()
        {
            scene.transform.position = this.transform.position;
            scene.transform.rotation = this.transform.rotation;

            if (mainController.arSetupMode)
            {
                modifier.SetActive(true);
                modifier.transform.localScale = Vector3.one * (Vector3.Distance(Camera.main.transform.position, modifier.transform.position) / 1.7f) * (Camera.main.fieldOfView / 8) * (Screen.dpi / 300) / Camera.main.transform.lossyScale.x;
            }
            else
            {
                modifier.SetActive(false);
            }
        }
    }
}
#endif
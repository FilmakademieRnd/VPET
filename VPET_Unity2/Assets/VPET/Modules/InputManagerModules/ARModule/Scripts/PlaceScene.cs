using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

namespace vpet
{
    public class PlaceScene : MonoBehaviour
    {
        ARRaycastManager m_RaycastManager;

        static List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

        private GameObject m_scene;
        public GameObject scene
        {
            set { m_scene = value; }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
        }

        public void placeScene(InputAction.CallbackContext context)
        {
            var touchPosition = context.ReadValue<Vector2>();
            if (m_RaycastManager.Raycast(touchPosition, m_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = m_Hits[0].pose;
                if (m_scene)
                {
                    m_scene.transform.position = hitPose.position;
                    m_scene.transform.rotation = hitPose.rotation;
                }
            }
        }
    }
}

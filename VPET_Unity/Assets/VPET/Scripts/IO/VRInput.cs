using UnityEngine;
using Valve.VR;

namespace vpet
{
	public class VRInput : MonoBehaviour 
	{
        public Vector3 scale = new Vector3(1, 1, 1);
        public Vector3 offset = new Vector3(0, 1f, 0);
        public SteamVR_TrackedObject.EIndex index = SteamVR_TrackedObject.EIndex.Device1;
        public bool capture = true;

        [HideInInspector]
        public SteamVR_TrackedObject tracker; 

        void Awake()
        {
            tracker = gameObject.AddComponent<SteamVR_TrackedObject>();
            tracker.index = index;
        }
    }
}
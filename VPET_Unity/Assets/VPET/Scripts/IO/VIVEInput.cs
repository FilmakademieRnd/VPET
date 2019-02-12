using UnityEditor;
using UnityEngine;

namespace vpet
{
    public class VIVEInput : MonoBehaviour 
	{
        public enum OffsetSelection
        {
            Both,
            Updating,
            Recording
        };

        public bool capture = true;
        public bool isOrigin = false;
        public bool recording = false;
        public bool searching = false;
        public bool wroteError = false;
        public string fileName = "";

        public Vector3 positionOffset = new Vector3(0, 0, 0);
        public Vector3 rotationOffset = new Vector3(0, 0, 0);
        public OffsetSelection offsetSelection = OffsetSelection.Both;

        public Transform origin;
        public SteamVR_TrackedObject tracker;
        public SteamVR_TrackedObject.EIndex device = SteamVR_TrackedObject.EIndex.None;
        public SteamVR_TrackedObject.EIndex newDevice = SteamVR_TrackedObject.EIndex.None;
        public SteamVR_TrackedObject.EIndex assignedDevice = SteamVR_TrackedObject.EIndex.None;

        private void Awake()
        {
            tracker = gameObject.GetComponent<SteamVR_TrackedObject>();

            if (!tracker)
                tracker = gameObject.AddComponent<SteamVR_TrackedObject>();

            tracker.index = device;
        }

        //Replaced in CheckSelectedDevice
        //private void Update()
        //{
        //    if (tracker != null)
        //        tracker.index = device;
        //}

        /// <summary>
        /// Checks if tracker still sends signals. (ignored when no tracker assigned)
        /// </summary>
        public bool CheckTrackerSignal()
        {
            if(!tracker || device == SteamVR_TrackedObject.EIndex.None)
                return true;

            return tracker.isValid;
        }

        /// <summary>
        /// Gets the current offset from the input transform.
        /// </summary>
        public void GetCurrentOffset()
        {
            positionOffset = -transform.localPosition;  // + (origin ? origin.localPosition : Vector3.zero)     //Not used because it might be quite inconvenient
            rotationOffset = -TransformUtils.GetInspectorRotation(transform);   //-transform.localEulerAngles   //Not used because inspector rotation is different from engine rotation  
        }
    }
}
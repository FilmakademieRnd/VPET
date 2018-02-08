#if USE_ARKIT
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.iOS;

namespace vpet
{
	public class ARPlane : MonoBehaviour
	{
		private MeshCollider meshCollider; //declared to avoid code stripping of class
		private MeshFilter meshFilter; //declared to avoid code stripping of class
		private static GameObject planePrefab = null;
		private Dictionary<string, ARPlaneAnchorGameObject> planeAnchorMap;

		void Awake()
		{
			if (planePrefab == null) 
			{
				planePrefab = Resources.Load ("VPET/Prefabs/debugPlanePrefab", typeof(GameObject)) as GameObject;
			}

			planeAnchorMap = new Dictionary<string,ARPlaneAnchorGameObject> ();
			UnityARSessionNativeInterface.ARAnchorAddedEvent += AddAnchor;
			UnityARSessionNativeInterface.ARAnchorUpdatedEvent += UpdateAnchor;
			UnityARSessionNativeInterface.ARAnchorRemovedEvent += RemoveAnchor;
		}

		private GameObject CreatePlaneInScene(ARPlaneAnchor arPlaneAnchor)
		{
			GameObject plane;
			if (planePrefab != null) {
				plane = GameObject.Instantiate(planePrefab);
			} else {
				plane = new GameObject (); //put in a blank gameObject to get at least a transform to manipulate
			}

			plane.name = arPlaneAnchor.identifier;

			return UpdatePlaneWithAnchorTransform(plane, arPlaneAnchor);

		}

		private GameObject UpdatePlaneWithAnchorTransform(GameObject plane, ARPlaneAnchor arPlaneAnchor)
		{

			//do coordinate conversion from ARKit to Unity
			plane.transform.position = UnityARMatrixOps.GetPosition (arPlaneAnchor.transform) * VPETSettings.Instance.trackingScale;
			plane.transform.rotation = UnityARMatrixOps.GetRotation (arPlaneAnchor.transform);

			MeshFilter mf = plane.GetComponentInChildren<MeshFilter> ();

			if (mf != null) {
				//since our plane mesh is actually 10mx10m in the world, we scale it here by 0.1f
				mf.gameObject.transform.localScale = new Vector3(arPlaneAnchor.extent.x * 0.1f ,arPlaneAnchor.extent.y * 0.1f ,arPlaneAnchor.extent.z * 0.1f );

				//convert our center position to unity coords
				mf.gameObject.transform.localPosition = new Vector3(arPlaneAnchor.center.x,arPlaneAnchor.center.y, -arPlaneAnchor.center.z);
			}

			return plane;
		}

		private void AddAnchor(ARPlaneAnchor arPlaneAnchor)
		{
			GameObject go = CreatePlaneInScene (arPlaneAnchor);
			//go.AddComponent<DontDestroyOnLoad> ();  //this is so these GOs persist across scene loads
			ARPlaneAnchorGameObject arpag = new ARPlaneAnchorGameObject ();
			arpag.planeAnchor = arPlaneAnchor;
			arpag.gameObject = go;
			planeAnchorMap.Add (arPlaneAnchor.identifier, arpag);
		}

		private void RemoveAnchor(ARPlaneAnchor arPlaneAnchor)
		{
			if (planeAnchorMap.ContainsKey (arPlaneAnchor.identifier)) {
				ARPlaneAnchorGameObject arpag = planeAnchorMap [arPlaneAnchor.identifier];
				GameObject.Destroy (arpag.gameObject);
				planeAnchorMap.Remove (arPlaneAnchor.identifier);
			}
		}

		private void UpdateAnchor(ARPlaneAnchor arPlaneAnchor)
		{
			if (planeAnchorMap.ContainsKey (arPlaneAnchor.identifier)) {
				ARPlaneAnchorGameObject arpag = planeAnchorMap [arPlaneAnchor.identifier];
				UpdatePlaneWithAnchorTransform (arpag.gameObject, arPlaneAnchor);
				arpag.planeAnchor = arPlaneAnchor;
				planeAnchorMap [arPlaneAnchor.identifier] = arpag;
			}
		}

		private void UnsubscribeEvents()
		{
			UnityARSessionNativeInterface.ARAnchorAddedEvent -= AddAnchor;
			UnityARSessionNativeInterface.ARAnchorUpdatedEvent -= UpdateAnchor;
			UnityARSessionNativeInterface.ARAnchorRemovedEvent -= RemoveAnchor;
		}

		void OnDestroy()
		{
			foreach (ARPlaneAnchorGameObject arpag in planeAnchorMap.Values.ToList ()) {
				GameObject.Destroy (arpag.gameObject);
				Debug.Log ("Destroy Plane " + arpag.gameObject.name);
			}

			planeAnchorMap.Clear ();
			UnsubscribeEvents ();
		}
	}
}
#endif

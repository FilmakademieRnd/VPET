using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gizmoControl : MonoBehaviour
{
    public float scaleFactor = 1;
    public GameObject sceneObject;
    private GameObject sceneRoot = null;
    private bool activeGizmo;
    private Vector3 lastMousePos;

    private Vector3 screenPoint;
    private Vector3 clickPosition;
    private Vector3 offset;
    private GameObject gizmoSphere;
    private Vector3 gizmoCenter;
    private Vector3 offCenter;

    // Start is called before the first frame update
    void Awake()
    {
        sceneRoot = GameObject.Find("Scene");

        if (gameObject.CompareTag("gizmoCenter"))
        {

        }
    }

    private void OnMouseDown()
    {
        gizmoSphere = GameObject.FindGameObjectWithTag("gizmoCenter");
        //convert gizmo position to screenPosition
        clickPosition = Input.mousePosition;
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        gizmoCenter = gizmoSphere.transform.position;

        offCenter = gameObject.transform.position - gizmoCenter;

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(clickPosition.x, clickPosition.y, screenPoint.z));
    }

    private void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        
        Vector3 newPosition = curPosition - offCenter;
        
        if (gameObject.name == "gizmoTranslate_X")
        {
            gizmoSphere.transform.position = new Vector3(newPosition.x, gizmoSphere.transform.position.y, gizmoSphere.transform.position.z);
        }
        if (gameObject.name == "gizmoTranslate_XY")
        {
            gizmoSphere.transform.position = new Vector3(newPosition.x, newPosition.y, gizmoSphere.transform.position.z);
        }
        if (gameObject.name == "gizmoTranslate_XZ")
        {
            gizmoSphere.transform.position = new Vector3(newPosition.x, gizmoSphere.transform.position.y, newPosition.z);
        }
        if (gameObject.name == "gizmoTranslate_Y")
        {
            gizmoSphere.transform.position = new Vector3(gizmoSphere.transform.position.x, newPosition.y, gizmoSphere.transform.position.z);
        }
        if (gameObject.name == "gizmoTranslate_YZ")
        {
            gizmoSphere.transform.position = new Vector3(gizmoSphere.transform.position.x, newPosition.y, newPosition.z);
        }
        if (gameObject.name == "gizmoTranslate_Z")
        {
            gizmoSphere.transform.position = new Vector3(gizmoSphere.transform.position.x, gizmoSphere.transform.position.y, newPosition.z);
        }
        if (gameObject.name == "gizmoRotate_X")
        {
            Vector3 curMouseOffset = curScreenPoint - clickPosition;
            gizmoSphere.transform.Rotate(curMouseOffset.x*0.5f, 0.0f, 0.0f, Space.World);
        }
        if (gameObject.name == "gizmoRotate_Y")
        {
            Vector3 curMouseOffset = curScreenPoint - clickPosition;
            gizmoSphere.transform.Rotate(0.0f, curMouseOffset.x * -0.5f, 0.0f, Space.World);
        }
        if (gameObject.name == "gizmoRotate_Z")
        {
            Vector3 curMouseOffset = curScreenPoint - clickPosition;
            gizmoSphere.transform.Rotate(0.0f, 0.0f, curMouseOffset.y * 0.5f, Space.World);
        }
        if (gameObject == gizmoSphere)
        {
            if (gizmoSphere.name == "gizmoTranslate")
            {
                gizmoSphere.transform.position = newPosition;
            }
        }

        clickPosition = Input.mousePosition;

        sceneObject.transform.position = gizmoSphere.transform.position;
        sceneObject.transform.rotation = gizmoSphere.transform.rotation;
    }

    private void OnMouseUp()
    {
        gizmoSphere.transform.GetChild(0).transform.rotation = Quaternion.identity;
    }

    bool CheckTag(GameObject obj, string tag)
    {
        return obj.CompareTag(tag);
    }

    // Update is called once per frame
    void Update()
    {
        //scale gizmo with distance
        if (CheckTag(gameObject, "gizmoCenter")) { 
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            var size = distance * scaleFactor * Camera.main.fieldOfView;
            transform.localScale = Vector3.one * size;
        }
    }
}

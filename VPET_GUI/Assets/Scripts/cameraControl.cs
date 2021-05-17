using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControl : MonoBehaviour
{
    public float orbitSpeed = 2.0f;
    public GameObject cameraOrbit;
    public GameObject cameraPitch;
    public GameObject sceneCamera;

    private bool cameraLock = false;
    private Vector3 lastMousePos = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            cameraLock = gameObject.GetComponent<UIControl>().cameraLock;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0) && !cameraLock)
        {
            Vector3 currentMousePos = Input.mousePosition;
            float xOffset = currentMousePos.x - lastMousePos.x;
            float yOffset = currentMousePos.y - lastMousePos.y;
            cameraOrbit.transform.Rotate(0.0f, xOffset * orbitSpeed, 0.0f, Space.Self);
            cameraPitch.transform.Rotate(-yOffset * orbitSpeed * 0.5f, 0.0f, 0.0f, Space.Self);
            lastMousePos = Input.mousePosition;
        }
    }
}

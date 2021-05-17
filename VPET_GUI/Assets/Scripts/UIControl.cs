using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    public bool activeUI = false;
    public bool cameraLock = false;
    public GameObject canvas;
    public GameObject gizmo_translate;
    public GameObject gizmo_rotate;
    public GameObject gizmo_scale;
    private bool dragging = false;
    public bool activeGizmo = false;
    private Vector3 clickPosition = new Vector3(0, 0, 0);
    private GameObject activeSceneObject = null;
    private GameObject activeButton = null;
    private GameObject currentGizmo = null;
    private string currentMode = "none";
    private int currentAxis = 0;
    public Sprite[] sprites = null;

    List<RaycastResult> hitObjects = new List<RaycastResult>();

    
    // Start is called before the first frame update
    void Start()
    {
        TurnOffRightMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            activeUI = false;
            cameraLock = false;
            activeGizmo = false;
            clickPosition = Input.mousePosition;
            GameObject clicked = GetObjectUnderMouse();
            if (clicked != null)
            {
                if (clicked.layer == 5)
                {
                    cameraLock = true;
                    activeUI = true;
                }
                else if (clicked.tag.Contains("gizmo"))
                {
                    activeGizmo = cameraLock = true;
                    activeUI = false;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            float dragDistance = Vector3.Distance(Input.mousePosition, clickPosition);
            if (dragDistance <= 0.5)
            {
                dragging = false;
                if (activeUI) //clicking UI
                {
                    activeButton = GetObjectUnderMouse();
                }
                else //selecting object
                {
                    GameObject obj = GetObjectUnderMouse();
                    if (!activeGizmo) selectSceneObject(obj);
                }
            }
            else //dragging
            {
                
            } 
        }
    }

    private GameObject GetObjectUnderMouse()
    {
        var pointer = new PointerEventData(EventSystem.current);

        pointer.position = Input.mousePosition;

        EventSystem.current.RaycastAll(pointer, hitObjects);

        if (hitObjects.Count <= 0 && dragging == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 100))
            {
                //selectSceneObject(hit.transform.gameObject);
                return hit.transform.gameObject;
            }
            else
            {
                //selectSceneObject(null);
                return null;
            }
        }

        //ButtonPress(hitObjects[0].gameObject);
        return hitObjects[0].gameObject;
    }

    void selectSceneObject(GameObject obj)
    {
        Debug.Log("selecting");
        //deselect active object
        if (activeSceneObject != null)
        {
            if (activeSceneObject.GetComponent<Outline>() != null) { activeSceneObject.GetComponent<Outline>().enabled = false; }
            CreateGizmo(null, "none");
            SetAllCollidersStatus(true, activeSceneObject);
            activeSceneObject = null;
        }
        //select new object
        if (obj != null)
        {
            activeSceneObject = obj;

            if (activeSceneObject.GetComponent<Outline>() != null) { activeSceneObject.GetComponent<Outline>().enabled = true; }
            SetAllCollidersStatus(false, activeSceneObject);
            //CreateGizmo(obj, "translate");

            SetMode("sceneObject", "translate");
        }
    }

    public void CreateGizmo(GameObject obj, string mode)
    {
        if (currentGizmo != null) Destroy(currentGizmo);

        if (obj != null && mode != "none")
        {
            GameObject newGizmo = null;
            
            if (mode == "translate")
            {
                newGizmo = Instantiate(gizmo_translate, activeSceneObject.transform.position, Quaternion.identity) as GameObject;
            }
            if (mode == "rotate")
            {
                newGizmo = Instantiate(gizmo_rotate, activeSceneObject.transform.position, Quaternion.identity) as GameObject;
            }
            if (mode == "scale")
            {
                newGizmo = Instantiate(gizmo_scale, activeSceneObject.transform.position, Quaternion.identity) as GameObject;
            }

            currentGizmo = newGizmo;
            Component[] gizmoScripts = newGizmo.GetComponentsInChildren<gizmoControl>();
            foreach (gizmoControl giz in gizmoScripts)
            {
                giz.sceneObject = obj;
            }
        }
    }

    public void SetAllCollidersStatus(bool active, GameObject obj)
    {
        foreach (Collider c in obj.GetComponents<Collider>())
        {
            c.enabled = active;
        }
    }

    public void ButtonPress(GameObject button)
    {
        activeUI = true;

        if (!button.transform.parent.CompareTag("menuRight"))
        {
            selectSceneObject(null);
            cameraLock = true;
            activeUI = true;

            SetMode(button.name, "null");
        }

        //activate mode per button on right menu
        if (button.name == "btnTranslate") SetMode("sceneObject", "translate");
        if (button.name == "btnRotate") SetMode("sceneObject", "rotate");
        if (button.name == "btnScale") SetMode("sceneObject", "scale");

        if (button.name == "spnAxis") SetAxis(button);
    }

    private void SetAxis(GameObject button)
    {
        currentAxis += 1;
        if (currentAxis > 2) currentAxis = 0;

        Image temp = button.GetComponent<Image>();

        temp.sprite = sprites[currentAxis];
    }

    public void SetMode(string context, string mode)
    {
        TurnOffRightMenu();
        currentMode = mode;

        if (context == "sceneObject")
        {
            TurnOnPanel("panelObject");

            if (mode == "translate")
            {
                CreateGizmo(activeSceneObject, "translate");
                currentMode = "translateObject";
                Debug.Log("translate");
            }
            if (mode == "rotate")
            {
                CreateGizmo(activeSceneObject, "rotate");
                currentMode = "rotateObject";
                Debug.Log("rotate");
            }
            if (mode == "scale")
            {
                CreateGizmo(activeSceneObject, "scale");
                currentMode = "scaleObject";
                Debug.Log("scale");
            }

        }

        if (context == "btnCamera")
        {
            TurnOnPanel("panelCamera");
        }

        if (context == "btnAnimation")
        {
            TurnOnPanel("panelAnimation");
        }

        if (context == "btnSettings")
        {
            TurnOnPanel("panelSettings");
        }

        if (context == "btnScene")
        {
            TurnOnPanel("panelScene");
        }
    }

    public void TurnOffRightMenu()
    {
        GameObject[] leftMenues = GameObject.FindGameObjectsWithTag("menuRight");
        foreach (GameObject leftMenu in leftMenues)
        {
            leftMenu.SetActive(false);
        }
    }

    public void TurnOnPanel(string panelName)
    {
        GameObject transmenu = (canvas.transform.Find(panelName)).gameObject;
        if (transmenu != null)
        {
            transmenu.SetActive(true);
        }
    }

}

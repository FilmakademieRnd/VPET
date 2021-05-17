using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonFunctions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showMenu()
    {
        Debug.Log("showMenu");
        turnOffRightMenu();
    }

    public void turnOnRightMenu()
    {

    }

    public void turnOffRightMenu()
    {
        GameObject[] leftMenues = GameObject.FindGameObjectsWithTag("menuRight");

        foreach (GameObject leftMenu in leftMenues)
        {
            leftMenu.SetActive(false);
        }
    }
}

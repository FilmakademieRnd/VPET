using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class HelpScreen : MonoBehaviour
    {

        private Vector3 lastPos;
        private MainController mainController;
        private Transform helpScreens;
        private bool smoothDragActive;
        private float smoothDragTime;
        private Vector3 targetDrag;
        private float dragDamping;
        private bool directionRight;


        // Use this for initialization
        void Awake()
        {
            mainController = GameObject.Find("MainController").GetComponent<MainController>();
            lastPos = Vector3.negativeInfinity;
            helpScreens = GameObject.Find("GUI/Canvas/HelpScreen/Screens").GetComponent<Transform>();
            dragDamping = 5f;
            directionRight = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (smoothDragActive)
            {
                helpScreens.localPosition = Vector3.Lerp(helpScreens.localPosition, targetDrag, Time.deltaTime * dragDamping);
                if (Vector3.Distance(helpScreens.localPosition, targetDrag) < 0.2f)
                {
                    helpScreens.localPosition = targetDrag;
                    smoothDragActive = false;
                }
            }
        }

        public void moveMenu(Vector3 pos)
        {
            smoothDragActive = false;
            if (helpScreens && lastPos != Vector3.negativeInfinity)
            {
                float deltaX = pos.x - lastPos.x;
                directionRight = deltaX > 0;          
                helpScreens.localPosition += new Vector3(deltaX, 0, 0);
                lastPos = new Vector3(pos.x, 0, 0);
            }
        }

        public void initPos(Vector3 pos)
        {
            lastPos = new Vector3(pos.x, 0, 0);
        }

        public void startSnap()
        {
            if (helpScreens)
            {
                smoothDragTime = Time.time;
                if (directionRight)
                {
                    targetDrag = new Vector3(Mathf.Ceil(helpScreens.localPosition.x / 1750) * 1750, helpScreens.localPosition.y, 0);
                }
                else
                {
                    targetDrag = new Vector3(Mathf.Floor(helpScreens.localPosition.x / 1750) * 1750, helpScreens.localPosition.y, 0);
                }
                if (targetDrag.x > 0) targetDrag = new Vector3(0, helpScreens.localPosition.y, 0);
                if (targetDrag.x < -12250) targetDrag = new Vector3(-12250, helpScreens.localPosition.y, 0);
                smoothDragActive = true;
            }
        }

        public void openHelpPage(int pageNumber)
        {
            mainController.helpActive = true;
            helpScreens.localPosition = new Vector3(pageNumber * 1750, helpScreens.localPosition.y, helpScreens.localPosition.z);
        }

    }
}

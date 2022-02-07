/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "ScrollSnap.cs"
//! @brief implementation of scroll and snap functionality for Manipulators such as Spinner
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @author Justus Henne
//! @version 0
//! @date 02.02.2022

//! CAUTION: external
//! originates form here: https://github.com/taka-oyama/ScrollSnap
//! License: MIT

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

namespace vpet
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ScrollRect), typeof(CanvasGroup))]
    public class ScrollSnap : UIBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        [SerializeField] public int startingIndex = 0;
        [SerializeField] public bool wrapAround = false;
        [SerializeField] public float lerpTimeMilliSeconds = 200f;
        [SerializeField] public float triggerPercent = 5f;
        [Range(0f, 10f)] public float triggerAcceleration = 1f;

        public class OnLerpCompleteEvent : UnityEvent { }
        public OnLerpCompleteEvent onLerpComplete;
        public class OnReleaseEvent : UnityEvent<int> { }
        public OnReleaseEvent onRelease;

        public delegate void OnAxisDrag(Spinner.Axis axis, float value);
        public OnAxisDrag onAxisDrag;

        int actualIndex;
        int cellIndex;
        ScrollRect scrollRect;
        CanvasGroup canvasGroup;
        RectTransform content;
        Vector2 cellSize;
        bool indexChangeTriggered = false;
        bool isLerping = false;
        float lerpStartedAt;
        Vector2 releasedPosition;
        Vector2 targetPosition;

        //VPET Axis Dragging
        private Vector2 startedDragPos;
        bool axisLocked = false;
        public float screenPercentAxisDrag = 2.5f;

        protected override void Awake()
        {
            base.Awake();
            actualIndex = startingIndex;
            cellIndex = startingIndex;
            this.onLerpComplete = new OnLerpCompleteEvent();
            this.onRelease = new OnReleaseEvent();
            this.scrollRect = GetComponent<ScrollRect>();
            this.canvasGroup = GetComponent<CanvasGroup>();
            this.content = scrollRect.content;
            this.cellSize = content.GetComponent<GridLayoutGroup>().cellSize;
            content.anchoredPosition = new Vector2(-cellSize.x * cellIndex, content.anchoredPosition.y);
            int count = LayoutElementCount();
            SetContentSize(count);

            if(startingIndex < count) {
                MoveToIndex(startingIndex);
            }
        }

        void LateUpdate()
        {
            if(isLerping) {
                LerpToElement();
                if(ShouldStopLerping()) {
                    isLerping = false;
                    canvasGroup.blocksRaycasts = true;
                    onLerpComplete.Invoke();
                    onLerpComplete.RemoveListener(WrapElementAround);
                }
            }
        }

        public void PushLayoutElement(LayoutElement element)
        {
            element.transform.SetParent(content.transform, false);
            SetContentSize(LayoutElementCount());
        }

        public void PopLayoutElement()
        {
            LayoutElement[] elements = content.GetComponentsInChildren<LayoutElement>();
            Destroy(elements[elements.Length - 1].gameObject);
            SetContentSize(LayoutElementCount() - 1);
            if(cellIndex == CalculateMaxIndex()) {
                cellIndex -= 1;
            }
        }

        public void UnshiftLayoutElement(LayoutElement element)
        {
            cellIndex += 1;
            element.transform.SetParent(content.transform, false);
            element.transform.SetAsFirstSibling();
            SetContentSize(LayoutElementCount());
            content.anchoredPosition = new Vector2(content.anchoredPosition.x - cellSize.x, content.anchoredPosition.y);
        }

        public void ShiftLayoutElement()
        {
            Destroy(GetComponentInChildren<LayoutElement>().gameObject);
            SetContentSize(LayoutElementCount() - 1);
            cellIndex -= 1;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x + cellSize.x, content.anchoredPosition.y);
        }

        public int LayoutElementCount()
        {
            return content.GetComponentsInChildren<LayoutElement>(false)
                .Count(e => e.transform.parent == content);
        }

        public int CurrentIndex
        {
            get
            {
                int count = LayoutElementCount();
                int mod = actualIndex % count;
                return mod >= 0 ? mod : count + mod;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startedDragPos = NormalizeToScreenSpace(eventData.position);
        }

        public void OnDrag(PointerEventData data)
        {
            if(!axisLocked)
            {
                //Scroll Snapping Code
                float dx = data.delta.x;
                float dt = Time.deltaTime * 1000f;
                float acceleration = Mathf.Abs(dx / dt);
                if(acceleration > triggerAcceleration && !float.IsPositiveInfinity(acceleration)) {
                    indexChangeTriggered = true;
                }

                float dy = data.delta.y;
            } 
            else
            {
                //Axis Dragging Code
                float dy = NormalizeToScreenSpace(data.delta).y;
                float dt = Time.deltaTime;
                
                float accelerationY = dy / dt;
                //Debug.Log($"Axis Y Axelleration {accelerationY}");

                //Transforming value back to decimal percent 5% -> 0.05
                onAxisDrag?.Invoke((Spinner.Axis)CurrentIndex, accelerationY);// / 100f);
            }

            if(!axisLocked)
            {
                Vector2 currentDragPos = NormalizeToScreenSpace(data.position);
                if(Mathf.Abs(currentDragPos.y - startedDragPos.y) * 100f > screenPercentAxisDrag)
                {
                    AxisLock();
                }
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            if(IndexShouldChangeFromDrag(data)) {
                int direction = (data.pressPosition.x - data.position.x) > 0f ? 1 : -1;
                SnapToIndex(cellIndex + direction * CalculateScrollingAmount(data));
            }
            else {
                StartLerping();
            }

            AxisLock(true);
        }

        public int CalculateScrollingAmount(PointerEventData data)
        {
            var offset = scrollRect.content.anchoredPosition.x + cellIndex * cellSize.x;
            var normalizedOffset = Mathf.Abs(offset / cellSize.x);
            var skipping = (int)Mathf.Floor(normalizedOffset);
            if(skipping == 0)
                return 1;
            if((normalizedOffset - skipping) * 100f > triggerPercent) {
                return skipping + 1;
            }
            else {
                return skipping;
            }
        }

        public void SnapToNext()
        {
            SnapToIndex(cellIndex + 1);
        }

        public void SnapToPrev()
        {
            SnapToIndex(cellIndex - 1);
        }

        public void SnapToIndex(int newCellIndex)
        {
            int maxIndex = CalculateMaxIndex();
            if(wrapAround && maxIndex > 0) {
                actualIndex += newCellIndex - cellIndex;
                cellIndex = newCellIndex;
                onLerpComplete.AddListener(WrapElementAround);
            }
            else {
                newCellIndex = Mathf.Clamp(newCellIndex, 0, maxIndex);
                actualIndex += newCellIndex - cellIndex;
                cellIndex = newCellIndex;
            }
            onRelease.Invoke(cellIndex);
            StartLerping();
        }

        public void MoveToIndex(int newCellIndex)
        {
            int maxIndex = CalculateMaxIndex();
            if(newCellIndex >= 0 && newCellIndex <= maxIndex) {
                actualIndex += newCellIndex - cellIndex;
                cellIndex = newCellIndex;
            }
            onRelease.Invoke(cellIndex);
            content.anchoredPosition = CalculateTargetPoisition(cellIndex);
        }

        void StartLerping()
        {
            releasedPosition = content.anchoredPosition;
            targetPosition = CalculateTargetPoisition(cellIndex);
            lerpStartedAt = Time.time;
            canvasGroup.blocksRaycasts = false;
            isLerping = true;
        }

        int CalculateMaxIndex()
        {
            int cellPerFrame = Mathf.FloorToInt(scrollRect.GetComponent<RectTransform>().rect.size.x / cellSize.x);
            return LayoutElementCount() - cellPerFrame;
        }

        bool IndexShouldChangeFromDrag(PointerEventData data)
        {
            // acceleration was above threshold
            if(indexChangeTriggered) {
                indexChangeTriggered = false;
                return true;
            }
            // dragged beyond trigger threshold
            var offset = scrollRect.content.anchoredPosition.x + cellIndex * cellSize.x;
            var normalizedOffset = Mathf.Abs(offset / cellSize.x);
            return normalizedOffset * 100f > triggerPercent;
        }

        void LerpToElement()
        {
            float t = (Time.time - lerpStartedAt) * 1000f / lerpTimeMilliSeconds;
            float newX = Mathf.Lerp(releasedPosition.x, targetPosition.x, t);
            content.anchoredPosition = new Vector2(newX, content.anchoredPosition.y);
        }

        void WrapElementAround()
        {
            if(cellIndex <= 0) {
                var elements = content.GetComponentsInChildren<LayoutElement>();
                elements[elements.Length - 1].transform.SetAsFirstSibling();
                cellIndex += 1;
                content.anchoredPosition = new Vector2(content.anchoredPosition.x - cellSize.x, content.anchoredPosition.y);
            }
            else if(cellIndex >= CalculateMaxIndex()) {
                var element = content.GetComponentInChildren<LayoutElement>();
                element.transform.SetAsLastSibling();
                cellIndex -= 1;
                content.anchoredPosition = new Vector2(content.anchoredPosition.x + cellSize.x, content.anchoredPosition.y);
            }
        }

        void SetContentSize(int elementCount)
        {
            content.sizeDelta = new Vector2(cellSize.x * elementCount, content.rect.height);
        }

        Vector2 CalculateTargetPoisition(int index)
        {
            return new Vector2(-cellSize.x * index, content.anchoredPosition.y);
        }

        bool ShouldStopLerping()
        {
            return Mathf.Abs(content.anchoredPosition.x - targetPosition.x) < 0.001f;
        }

        Vector2 NormalizeToScreenSpace(Vector2 pointerPos)
        {
            Vector2 pointerPosNormalized = new Vector2(pointerPos.x / Screen.width, pointerPos.y / Screen.height);
            return pointerPosNormalized;
        }

        void AxisLock(bool unlock = false)
        {
            if(unlock)
            {
                scrollRect.enabled = true;
                axisLocked = false;
                //Debug.Log("Axis unlocked");
            }
            else
            {
                scrollRect.StopMovement();
                scrollRect.enabled = false;
                StartLerping();
                axisLocked = true;
            }
        }
    }
}
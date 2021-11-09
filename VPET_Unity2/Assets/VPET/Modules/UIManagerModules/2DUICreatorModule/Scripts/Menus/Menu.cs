using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace vpet
{
    public enum UIMenuStart
    {
        activeOnStart = 0,
        inactiveOnStart = 1,
        leaveAsIs = 3
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Menu : MonoBehaviour
    {
        // =================================================================
        #region Public Fields
        public UIMenuStart startBehaviour;
        public bool turnOnAndOffCanvasObject = false; 
        public bool blocksRaycasts = true;
        #if UNITY_EDITOR
        [ReadOnly]
        #endif
        public CanvasGroup menuGroup;
 
        [HideInInspector]
        public bool isActive = true;
        #endregion
        // =================================================================
 
        // =================================================================
        #region Private Fields
        #endregion
        // =================================================================
 
        // =================================================================
        #region Unity Methods
        protected void Start()
        {
            if(menuGroup == null)
            {
                Debug.Log($"UIMenu {name} doesn't reference a CanvasGroup!\nTrying to get Component from {name} itself...");
                menuGroup = GetComponent<CanvasGroup>();
                if(menuGroup == null)
                {
                    Debug.LogError($"UIMenu {name} can't find any CanvasGroup!\nExecution will lead to NullRefs...");
                }
            }
 
            //De-/-Activate Menu into default state
            switch (startBehaviour)
            {
                case UIMenuStart.activeOnStart:
                    SetMenu(true, true);
                    break;
                case UIMenuStart.inactiveOnStart:
                    SetMenu(false, true);
                    break;
                case UIMenuStart.leaveAsIs:
                    break;
            }
        }
        #endregion
        // =================================================================
 
        // =================================================================
        #region Private Methods   
        #endregion
        // =================================================================
 
        // =================================================================
        #region Public Methods

        //Helper Methods for Registration within UnityEvents, as the only accept one or less parameter functions
        public void ActivateMenu()
        {
            ShowMenu(true);
        }
 
        public void DeActivateMenu()
        {
            HideMenu(true);
        }  
        //Helper functions end      
 
        public void SetInteractable(bool value)
        {
            menuGroup.interactable = value;
            menuGroup.blocksRaycasts = value? blocksRaycasts : false;
        }
 
        public virtual void ShowMenu(bool instant = false, bool setInteractable = true, System.Action onCmplete = null)
        {
            Debug.Log("Showing " + name + " UI Menu: " + (instant? "instant" : "fading"));
 
            //Before Fade
            BeforeShow();
            if(turnOnAndOffCanvasObject)
            {
                menuGroup.gameObject.SetActive(true);
            }
 
            //Fade
            if(!instant)
            {
                menuGroup.DOFade(1f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => OnComplete());
            } 
            else 
            {
                menuGroup.alpha = 1f;
                OnComplete();
            }
 
            //After Fade
            void OnComplete()
            {
                isActive = true;
 
                if(setInteractable)
                {
                    SetInteractable(true);
                }
 
                AfterShow();
 
                if(onCmplete != null)
                {
                    onCmplete.Invoke();
                }
            }
        }
 
        public void HideMenu(bool instant = false, bool setInteractable = true, System.Action onCmplete = null)
        {
            Debug.Log("Hiding " + name + " UI Menu: " + (instant? "instant" : "fading"));
 
            //Before Fade
            BeforeHide();
            isActive = false;
 
            if(setInteractable)
            {
                SetInteractable(false);
            }
            
            //Fade            
            if(!instant)
            {
                menuGroup.DOFade(0f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => OnComplete());
            }
            else
            {
                menuGroup.alpha = 0f;
                OnComplete();
            }
 
            //After fade
            void OnComplete()
            {
                if(turnOnAndOffCanvasObject)
                {
                    menuGroup.gameObject.SetActive(false);
                }
 
                AfterHide();
 
                if(onCmplete != null)
                {
                    onCmplete.Invoke();
                }                
            }
        }
 
        public void SetMenu(bool On, bool instant = false)
        {
            if(On)
            {
                ShowMenu(instant);
            }
            else 
            {
                HideMenu(instant);
            }
        }
        
        protected virtual void BeforeShow(){}
        protected virtual void AfterShow(){}
        protected virtual void BeforeHide(){}
        protected virtual void AfterHide(){}
 
        #endregion
        // =================================================================    
 
        // =================================================================
        #region Utils
        #endregion
        // =================================================================
    }
}

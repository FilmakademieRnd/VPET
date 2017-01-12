/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Reflection;

//!
//! Central UI Adapter that creates and manages all the different widgets and is the only way to access menu events & functionality from other parts of the app.
//!
namespace vpet
{
	public partial class UI : MonoBehaviour
	{
        //!
        //! Event to propagate UI-changes. Register functions here which need to be called to update elements (button visibility e.g.). This gets called
        //! e.g. menus open
        //!
        [HideInInspector]
        public static UnityEvent OnUIChanged = new UnityEvent();

	    //! 
	    //! Global icon scale
	    //!
	    public static float IconScale = 1f;
	
	    //!
	    //! Linear Menu displayed in the top right corner.
	    //!
	    private MainMenu mainMenu;

	    //!
	    //! Second Menu displayed at bottom.
	    //!
	    private SecondaryMenu secondaryMenu;
	
	    //!
	    //! Center Menu displayed at bottom.
	    //!
	    private CenterMenu centerMenu;

        //!
        //! Parameter Menu displayed on left side.
        //!
        private SubMenu parameterMenu;

        //!
        //! Cached reference to the main controller.
        //!
        private MainController mainController;

        //!
        //! Cached reference to the Tango controller.
        //!
#if USE_TANGO﻿
        private TangoController tangoController;
#endif

        //!
        //! Cached reference to the animation controller.
        //!
        private AnimationController animationController;
	    
	    //!
	    //! Reference to main menu button.
	    //!
	    private GameObject mainMenuButton;
	
	    //!
	    //! Is the main menu button active?
	    //!
	    private bool mainMenuActive = false;
	
	    //!
	    //! Reference to undo button.
	    //!
	    GameObject undoButton;
	
	    //!
	    //! Is the undo button active? (Is there something to undo?)
	    //!
	    // private bool undoActive = false;
	
	    //!
	    //! Reference to redo button.
	    //!
	    GameObject redoButton;

        //!
        //! Is the redo button active? (Is there something to redo?)
        //!
        // private bool redoActive = false;      

        private layouts layoutUI = layouts.EDIT;
	
	
	    public layouts LayoutUI
	    {
	        get { return layoutUI;  }
	    }
	
	
	    // public static Vector2 SpriteSize = new Vector2(Screen.height / 8*IconSize, Screen.height / 8 *IconSize);
	    public static Vector2 SpriteSize = new Vector2(100 * IconScale, 100 * IconScale);
	
	    // public static int ButtonOffset = (int)(UI.SpriteSize.x + UI.SpriteSize.x / 5);
	    public static int ButtonOffset = (int)(UI.SpriteSize.x+ UI.SpriteSize.x/4f);
        public static int ButtonBorderOffset = ButtonOffset;
	
	    private LightSettingsWidget lightSettingsWidget;
	
	    private Transform canvas;

        private SplashWidget splashWidget;


        private ConfigWidget configWidget = null;
        public ConfigWidget ConfigWidget
        {
            get { return configWidget; }
        }

        private RoundProgressBar progressWidget = null;


        private RangeSlider rangeSlider;

	    void Awake()
	    {
	        // read icons from resouces and assign to class properties
	        FieldInfo[] fi = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance );
	        foreach (FieldInfo info in fi)
	        {
	            Sprite sprite = Resources.Load<Sprite>("VPET/Icons/" + info.Name );
	            if ( sprite != null )
	            {
	                // print("load " + "Icons/" + info.Name);
	                info.SetValue(this, sprite);
	            }
	            else
	            {
	                // print("Not Found " + "Icons/" + info.Name);
	            }
	        }

            // set canvas
            canvas = transform.parent;

	
			print("Screen.width " + Screen.width);
			print("Screen.height " + Screen.height);
			print("VPETSettings.Instance.canvasHalfWidth " + VPETSettings.Instance.canvasHalfWidth);
			print("VPETSettings.Instance.canvasHalfHeight " + VPETSettings.Instance.canvasHalfHeight);
	        print("VPETSettings.Instance.canvasScaleFactor " + VPETSettings.Instance.canvasScaleFactor);
	        print("VPETSettings.Instance.canvasAspectScaleFactor " + VPETSettings.Instance.canvasAspectScaleFactor);
			print("VPETSettings.Instance.canvasScale " + VPETSettings.Instance.canvasScale);
	
	    }
	
	    //!
	    //! Use this for initialization
	    //!
	    void Start ()
	    {
	
			try
			{
		        //cache reference to main Controller
		        mainController = GameObject.Find("MainController").GetComponent<MainController>();
	
	
		        //cache reference to animation Controller
		        animationController = GameObject.Find("AnimationController").GetComponent<AnimationController>();

#if USE_TANGO﻿
                //cache reference to tango Controller
                tangoController = GameObject.Find("Tango").GetComponent<TangoController>();
#endif
            }
            catch
			{
				print("Fix Me");
			}


            // get range slider
            rangeSlider = canvas.GetComponentInChildren<RangeSlider>(true);
            if (rangeSlider == null) Debug.LogError(string.Format("{0}: Cant Find Component in Canvas: RangeSlider.", this.GetType()));
            rangeSlider.OnValueChanged.AddListener(onRangeSliderChanged);

            // get light settings widget
            lightSettingsWidget = canvas.GetComponentInChildren<LightSettingsWidget>(true);
            if (lightSettingsWidget == null) Debug.LogError(string.Format("{0}: Cant Find Component in Canvas: LightSettingsWidget.", this.GetType()));


            //initalize main Menu
            GameObject mainMenuObject = new GameObject("mainMenuObject");
	        mainMenuObject.transform.SetParent(this.transform.parent, false);
	        mainMenu = mainMenuObject.AddComponent<MainMenu>();
	
	        //initalize secondary Menu
	        GameObject secondaryMenuObj = new GameObject("secondaryMenuObject");
	        secondaryMenuObj.transform.SetParent(this.transform, false);
	        secondaryMenu = secondaryMenuObj.AddComponent<SecondaryMenu>();
	
	        //initalize center Menu
	        GameObject centerMenuObj = new GameObject("centerMenuObject");
	        centerMenuObj.transform.SetParent(this.transform, false);
	        centerMenu = centerMenuObj.AddComponent<CenterMenu>();

            //initalize paramter Menu
            GameObject paramterMenuObj = new GameObject("paramterMenuObj");
            paramterMenuObj.transform.SetParent(this.transform, false);
            parameterMenu = paramterMenuObj.AddComponent<SubMenu>();

            // initalize ConfigWidget
            GameObject refObject = GameObject.Find("GUI/Canvas/ConfigWidget");
			if (refObject == null)
			{
				Debug.LogWarning(string.Format("{0}: No GUI/Canvas/ConfigWidget Object found. Load From Resource.", this.GetType()));
				GameObject refObjectPrefab = Resources.Load<GameObject>("VPET/Prefabs/ConfigWidget");
				refObject = Instantiate(refObjectPrefab);
				refObject.name = refObjectPrefab.name;
				GameObject refParent = GameObject.Find("GUI/Canvas");
				refObject.transform.SetParent( refParent.transform, false);
			}
			configWidget = refObject.GetComponent<ConfigWidget>();
			if (configWidget == null) 
			{
				Debug.LogWarning(string.Format("{0}: No ConfigWidget Component found. Create", this.GetType()));
				configWidget = refObject.AddComponent<ConfigWidget>();
			}
			// Submit listener
			configWidget.SubmitEvent.AddListener(mainController.configWidgetSubmit);
			// Ambient light listener
			configWidget.AmbientChangedEvent.AddListener( mainController.setAmbientIntensity );

#if USE_TANGO﻿
                // Show Tango Scale UI objects
                GameObject tangoScaleSliderUI = GameObject.Find("GUI/Canvas/ConfigWidget/TangoScale_slider");                
                // tangoScaleSliderUI.transform.localPosition = new Vector3(31.0f, -560.0f, 0.0f);
                GameObject tangoScaleLabelUI = GameObject.Find("GUI/Canvas/ConfigWidget/TangoScale_label");
                //tangoScaleLabelUI.transform.localPosition = new Vector3(-105.0f, 530.0f, 0.0f);
                GameObject startButton = GameObject.Find("GUI/Canvas/ConfigWidget/Start_button");
                GameObject sliderValueText = GameObject.Find("GUI/Canvas/ConfigWidget/TangoScale_Value");
                sliderValueText.gameObject.SetActive(true);
                tangoScaleLabelUI.gameObject.SetActive(true);
                tangoScaleSliderUI.gameObject.SetActive(true);
                // Tango Scale Listener
                configWidget.TangoScaleChangedEvent.AddListener( tangoController.setTangoScaleIntensity );
#endif


			/*
	        //initalize undo buttons
	        undoButton = Instantiate(GameObject.Find("ButtonTemplate"));
	        undoButton.transform.SetParent(this.transform,false);
	        //undoButton.GetComponent<RectTransform>().sizeDelta = SpriteSize;
	        undoButton.GetComponent<RectTransform>().position = new Vector2(Screen.height / 16 + 10, ((Screen.height / 3) * 2) - (Screen.height / 8 + 10));
	        undoButton.GetComponent<Image>().sprite =   GeneralMenu_Undo_nrm;
	        SpriteState undoSprites = new SpriteState();
	        undoSprites.disabledSprite = GeneralMenu_Undo_nrm;
	        undoSprites.highlightedSprite = GeneralMenu_Undo_nrm;
	        undoSprites.pressedSprite = GeneralMenu_Undo_sel;
	        undoButton.GetComponent<Button>().spriteState = undoSprites;
	        undoButton.GetComponent<Button>().interactable = false;
	        undoButton.GetComponent<Button>().onClick.AddListener(() => undoButtonClicked());
	        // temp hide it
	        undoButton.GetComponent<Image>().enabled = false;
	
	        //initalize redo buttons
	        redoButton = Instantiate(GameObject.Find("ButtonTemplate"));
	        redoButton.transform.SetParent(this.transform, false);
	        //redoButton.GetComponent<RectTransform>().sizeDelta = SpriteSize;
	        redoButton.GetComponent<RectTransform>().position = new Vector2(Screen.width - (Screen.height / 16 + 10), ((Screen.height / 3) * 2) - (Screen.height / 8 + 10));
	        redoButton.GetComponent<Image>().sprite = GeneralMenu_Redo_nrm;
	        SpriteState redoSprites = new SpriteState();
	        redoSprites.disabledSprite = GeneralMenu_Redo_nrm;
	        redoSprites.highlightedSprite = GeneralMenu_Redo_nrm;
	        redoSprites.pressedSprite = GeneralMenu_Redo_sel;
	        redoButton.GetComponent<Button>().spriteState = redoSprites;
	        redoButton.GetComponent<Button>().interactable = false;
	        redoButton.GetComponent<Button>().onClick.AddListener(() => redoButtonClicked());
	        // temp hide it
	        redoButton.GetComponent<Image>().enabled = false;
			*/
	
	        //initalise mainMenu button
			IMenuButton iMainMenuButton = Elements.MenuButtonToggle();
			mainMenuButton = ((Button)iMainMenuButton).gameObject;
			mainMenuButton.name = "MainMenuButton";
			mainMenuButton.transform.SetParent( this.transform.parent, false);
			mainMenuButton.GetComponent<RectTransform>().localPosition = new Vector2( VPETSettings.Instance.canvasHalfWidth - UI.ButtonOffset, ( VPETSettings.Instance.canvasHalfHeight - UI.ButtonOffset) * VPETSettings.Instance.canvasAspectScaleFactor );
			iMainMenuButton.AddAction(GeneralMenu_Main_sel, GeneralMenu_Main_nrm, () => mainMenuToggle());
			mainMenuButton.SetActive(false);
			iMainMenuButton.Menu = mainMenu;
	
	        //call setup function for all available menues
	        setupMainMenu();
	        setupSecondaryMenu();
	        setupCenterMenu();
            setupParameterMenu();
        }


        public void resetRangeSlider()
        {
            rangeSlider.Value = 0;
            rangeSlider.MinValue = float.MinValue;
            rangeSlider.MaxValue = float.MaxValue;
        }

        public void drawRangeSlider( UnityAction<float> callback, float initValue = 0f, float sensitivity=0.1f )
        {
            
            if (centerMenu.ActiveButton != null && centerMenu.ActiveButton.GetComponent<Button>() != null)
            {
                Sprite buttonSprite = centerMenu.ActiveButton.GetComponent<Button>().spriteState.disabledSprite;
                rangeSlider.CenterSprite = buttonSprite;
            }
            else if (secondaryMenu.ActiveButton != null && secondaryMenu.ActiveButton.GetComponent<Button>() != null)
            {
                Sprite buttonSprite = secondaryMenu.ActiveButton.GetComponent<Button>().spriteState.disabledSprite;
                rangeSlider.CenterSprite = buttonSprite;

            }

            // 
            rangeSlider.Callback = callback;
            rangeSlider.Value = initValue;
            rangeSlider.Sensitivity = sensitivity;
            rangeSlider.Show();
        }

        public void updateRangeSlider( float v )
        {
            rangeSlider.Value = v;
        }

        public void hideRangeSlider()
        {
            rangeSlider.Hide();
        }

        private void onRangeSliderChanged(float v)
        {
            // pass it to controller to trigger set key
            mainController.SliderValueChanged(v);
        }

        public void drawMainMenuButton( bool doOpen = false)
		{
			mainMenuButton.SetActive(true);
			if ( doOpen && !mainMenuButton.GetComponent<IMenuButton>().Toggled )
            {
                // mainMenuButton.GetComponent<MenuButton>().onClick.Invoke();
                // HACK because above dont work
                mainMenu.show();
                mainMenuButton.GetComponent<MenuButtonToggle>().Toggled = true;
            }
        }
	
		public void hideMainMenuButton()
		{
			mainMenuButton.SetActive(false);
		}

        public void resetMainMenu()
        {
            mainMenu.reset();
        }
	
	    //!
	    //! Receiving function of a button press event on the main menu activation button.
	    //!
	    private void mainMenuToggle()
	    {
			if ( mainMenuButton.GetComponent<IMenuButton>().Toggled )
			{
				mainMenu.show();
			}
			else
			{
				mainMenu.hide();
			}
	    }

        public void switchLayoutMainMenu( layouts lay )
        {
            mainMenu.switchLayout(layouts.DEFAULT);

            // HACK
            mainMenuButton.GetComponent<MenuButtonToggle>().Toggled = false;
        }




    public SplashWidget drawSplashWidget()
        {
            if (splashWidget == null)
            {
                // get SplashWidget
                GameObject refObject = GameObject.Find("GUI/Canvas/SplashWidget");
                if (refObject == null)
                {
                    Debug.LogWarning(string.Format("{0}: No GUI/Canvas/SplashWidget Object found. Load From Resource.", this.GetType()));
                    GameObject refObjectPrefab = Resources.Load<GameObject>("VPET/Prefabs/SplashWidget");
                    refObject = Instantiate(refObjectPrefab);
                    refObject.name = refObjectPrefab.name;
                    GameObject refParent = GameObject.Find("GUI/Canvas");
                    refObject.transform.SetParent(refParent.transform, false);
                }
                splashWidget = refObject.GetComponent<SplashWidget>();
                if (splashWidget == null)
                {
                    Debug.LogWarning(string.Format("{0}: No SplashWidget Component found. Create", this.GetType()));
                    splashWidget = refObject.AddComponent<SplashWidget>();
                }
            }

            splashWidget.Show();
            return splashWidget;
        }

        public void hideSplashWidget()
        {
            // splashWidget.Hide();
        }


        public ConfigWidget drawConfigWidget()
	    {
            // get radial menu
            // get ProgressWidget
            GameObject radialrefObject = GameObject.Find("GUI/Canvas/ProgressWidget");
            if (radialrefObject != null)
            {
                radialrefObject.gameObject.SetActive(false);
            }

           // copy values from global settings to config widget
            VPETSettings.mapValuesToObject( configWidget );

			configWidget.Show();           
            configWidget.initConfigWidget();
            UI.OnUIChanged.Invoke();
            return configWidget;
	    }
	
	    public void hideConfigWidget()
	    {
			// must be first
			configWidget.Hide();

			// copy values from config widget to global settings
			VPETSettings.mapValuesFromObject( configWidget );

			// save global settings in preferences
			VPETSettings.mapValuesToPreferences();

            UI.OnUIChanged.Invoke();
        }


        // TODO: create generic widget class
        /*
        public void hideConfigWidget( ConfigWidget widget)
        {
            widget.Hide();
        }
        */


        public RoundProgressBar drawProgressWidget()
		{
			// draw and return progress widget
			if ( progressWidget == null )
			{
                // get ProgressWidget
                GameObject refObject = GameObject.Find("GUI/Canvas/ProgressWidget");
				if (refObject == null)
				{
					Debug.LogWarning(string.Format("{0}: No GUI/Canvas/ProgressWidget Object found. Load From Resource.", this.GetType()));
					GameObject refObjectPrefab = Resources.Load<GameObject>("VPET/Prefabs/ProgressWidget");
					refObject = Instantiate(refObjectPrefab);
					refObject.name = refObjectPrefab.name;
					GameObject refParent = GameObject.Find("GUI/Canvas");
					refObject.transform.SetParent( refParent.transform, false);
				}
				progressWidget = refObject.GetComponent<RoundProgressBar>();
				if (progressWidget == null) 
				{
					Debug.LogWarning(string.Format("{0}: No ProgressWidget Component found. Create", this.GetType()));
					progressWidget = refObject.AddComponent<RoundProgressBar>();
				}
			}

			progressWidget.Show();
			return progressWidget;

		}
	
		public void hideProgressWidget()
		{
			progressWidget.Hide();
		}
	
	    public void drawCenterMenu( layouts layout )
	    {
            centerMenu.reset();
	        centerMenu.switchLayout(layout);
            UI.OnUIChanged.Invoke();
	        centerMenu.show();
	    }
	
		public void hideCenterMenu()
		{
			centerMenu.ActiveButton = null;
			centerMenu.hide();           
		}
	
		public void drawSecondaryMenu(layouts layout)
	    {
	        secondaryMenu.switchLayout(layout);
            secondaryMenu.show();
	    }
	
		public void hideSecondaryMenu()
		{
			secondaryMenu.hide();
		}

        public void drawParameterMenu(layouts layout)
        {
            // reset parmater menu and set first button toggled
            parameterMenu.ActiveButton = null;
            parameterMenu.reset();
            IMenuButton button = parameterMenu.GetButton(0);
            if (button != null) button.Toggled = true;


            parameterMenu.switchLayout(layout);
            float _adjustOddCount = (parameterMenu.ActiveButtonCount % 2) * UI.ButtonOffset / 2f;
            parameterMenu.offset = new Vector2(-VPETSettings.Instance.canvasHalfWidth + UI.ButtonOffset, parameterMenu.ActiveButtonCount * UI.ButtonOffset / 2f + _adjustOddCount);
            parameterMenu.show();
        }

        public  void hideParameterMenu()
        {
            parameterMenu.hide();
        }

        /*
	    //!
	    //! Display the object modification menu with an intial animation.
	    //! @param      isKinematic     is the objects kinematic state true or false (needed to visualize the kinematic button acordingly)
	    //! @param      hasAnimation    is the currently selected object animated
	    //!
	    public void drawObjectModificationMenu(bool hasAnimation, bool isKinematic)
	    {
	        activeMenu = sceneObjectCenterMenu;
	        activeMenu.reset();
	        if (!hasAnimation) activeMenu.deactivateButton(8);
	        if (isKinematic) activeMenu.swapSprites(4);
	        if (AnimationData.Data.getAnimationClips(mainController.getCurrentSelection().gameObject) != null) activeMenu.deactivateButton(4);
	        activeMenu.animatedDraw();
	    }
	
	
	    //!
	    //! Display the keyframe menu with an intial animation.
	    //!
	    public void drawKeyframeMenu()
	    {
	        activeMenu = keyframeCenterMenu;
	        activeMenu.reset();
	        keyframeCenterMenu.deactivateButton(3);
	        keyframeCenterMenu.deactivateButton(4);
	        activeMenu.animatedDraw();
	    }
	
	    //!
	    //! Display the directional light modification menu with an intial animation.
	    //!
	    public void drawDirectionLightModificationMenu()
	    {
	        activeMenu = directionalLightCenterMenu;
	        activeMenu.animatedDraw();
	    }
	
	    //!
	    //! Display the point light modification menu with an intial animation.
	    //!
	    public void drawPointLightModificationMenu()
	    {
	        activeMenu = pointLightCenterMenu;
	        activeMenu.animatedDraw();
	    }
	
	    //!
	    //! Display the spot light modification menu with an intial animation.
	    //!
	    public void drawSpotLightModificationMenu()
	    {
	        activeMenu = spotLightCenterMenu;
	        activeMenu.animatedDraw();
	    }
	    */


        //!
        //! Display the light parameters modification menu.
        //!
        public void drawLightSettingsWidget()
	    {
	        lightSettingsWidget.gameObject.SetActive(true);
            if ( lightSettingsWidget.GetSliderType() == LightSettingsWidget.SliderType.COLOR )
            {
                hideParameterMenu();
                hideRangeSlider();
            }
            lightSettingsWidget.show( mainController.getCurrentSelection().GetComponent<SceneObject>() );
	    }

	    //!
	    //! Hide the light parameters modification menu.
	    //!
	    public void hideLightSettingsWidget()
	    {
	        lightSettingsWidget.hide();
	        lightSettingsWidget.gameObject.SetActive(false);
	    }

	
	    /*
	    //!
	    //! Hide the currently active center or light parameter menu and move the activated menu button to the active position.
	    //!
	    public void hideActiveMenu()
	    {
	        if(activeMenu) activeMenu.hide(activeMenuButton);
	        if (ciPickerMenu.active) ciPickerMenu.active = false;
	    }
	
	    //!
	    //! Hide the currently active center or light parameter menu entirely.
	    //!
	    public void forceHideActiveMenu()
	    {
	        if (activeMenu) activeMenu.hide(-1);
	        activeMenuButton = -1;
	        activeMenu = null;
	        if (ciPickerMenu.active) ciPickerMenu.active = false;
	    }
	    */
	
		/*
	    //!
	    //! Reset the currently active menu.
	    //!
	    public void reset()
	    {
	        if (activeMenu != null)
	        {
	            activeMenu.reset();
	        }
	    }
		*/
	
		/*
	    //!
	    //! Returns the id of a button at a screen position.
	    //! @param      pos     position on screen
	    //! @return     id of the Button at the screen position, if none returns -1
	    //!
	    public int getButtonId(Vector3 pos)
	    {
	        int val = -1;
	        if (activeMenu) val =  activeMenu.contains(pos);
	        if (mainMenuActive && val == -1) val = mainMenu.contains(pos);
	        return val;
	    }
		*/
	
		/*
	    //!
	    //! checks if the given screen position is on the light parameters menu.
	    //! @param      pos     position on screen
	    //! @return     true if on the menu
	    //!
	    public bool isOnLightSettingsPicker(Vector3 pos)
	    {
	        return ciPickerMenu.contains(new Vector2(pos.x, Screen.height - pos.y));
	    }
		*/
	
		/*
	    //!
	    //! Forwards the color request to the menu and returns the answer.
	    //! @param      pos     position on screen
	    //! @return     color returned by menu evaluation
	    //!
	    public Color getColorPickerValue(Vector3 pos)
	    {
	        return ciPickerMenu.getColor(pos);
	    }
		*/
	
		/*
	    //!
	    //! Forwards the intensity request to the menu and returns the answer.
	    //! @param      pos     position on screen
	    //! @return     intensity returned by menu evaluation
	    //!
	    public float getIntensityPickerValue(Vector3 pos)
	    {
	        return ciPickerMenu.getIntensity(pos);
	    }
		*/
		/*
	    //!
	    //! Forwards the range request to the menu and returns the answer.
	    //! @param      pos     position on screen
	    //! @return     range returned by menu evaluation
	    //!
	    public float getRangePickerDeltaValue(Vector3 pos)
	    {
	        return ciPickerMenu.getDeltaRange(pos);
	    }
		*/
	
		/*
	    //!
	    //! Forwards the cone angle request to the menu and returns the answer.
	    //! @param      pos     position on screen
	    //! @return     cone angle returned by menu evaluation
	    //!
	    public float getAnglePickerValue(Vector3 pos)
	    {
	        return ciPickerMenu.getAngle(pos);
	    }
		*/
	
		/*
	    //!
	    //! Receiving function for the undo button click event.
	    //!
	    private void undoButtonClicked()
	    {
	        if (undoActive) mainController.undoLastAction();
	    }
		*/
	
		/*
	    //!
	    //! Receiving function for the redo button click event.
	    //!
	    private void redoButtonClicked()
	    {
	        if (redoActive) mainController.redoLastAction();
	    }
		*/
	
	    /*
	    //!
	    //! Activate/Deactivate the undo button
	    //! @param  set     new state of button
	    //!
	    public void setUndoActive(bool set)
	    {
	        undoButton.GetComponent<Button>().interactable = set;
	        undoActive = set;
	    }
	    */
	
	        /*
	    //!
	    //! Activate/Deactivate the redo button
	    //! @param  set     new state of button
	    //!
	    public void setRedoActive(bool set)
	    {
	        redoButton.GetComponent<Button>().interactable = set;
	        redoActive = set;
	    }
	    */
}}

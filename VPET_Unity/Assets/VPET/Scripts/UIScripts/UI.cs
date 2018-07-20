/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

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
        public MainMenu MainMenu
        {
            get { return mainMenu; }
        }
        //!
        //! Second Menu displayed at bottom.
        //!
        private SecondaryMenu secondaryMenu;
	
	    //!
	    //! Center Menu displayed at bottom.
	    //!
	    private CenterMenu centerMenu;
        public CenterMenu CenterMenu
        {
            get { return centerMenu; }
        }
        //!
        //! Parameter Menu displayed on left side.
        //!
        private SubMenu parameterMenu;

        //!
        //!
        //!
        private GameObject helpContext;

        //!
        //! Cached reference to the main controller.
        //!
        private MainController mainController;

        //!
        //! Cached reference to the Tango controller.
        //!
#if USE_TANGO
        private TangoController trackingController;
#elif USE_ARKIT
        private ARKitController trackingController;
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
	    // private bool mainMenuActive = false;
	
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
	        print("VPETSettings.Instance.canvasAspectScaleFactor " + VPETSettings.Instance.canvasAspectScaleFactor);
	
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

#if USE_TANGO
                //cache reference to tracking Controller
                trackingController = GameObject.Find("Tango").GetComponent<TangoController>();
#elif USE_ARKIT
                trackingController = GameObject.Find("ARKit").GetComponent<ARKitController>();
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

            helpContext = GameObject.Find("GUI/Canvas/HelpScreen");

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

#if USE_TANGO || USE_ARKIT
				configWidget.ToggleAREvent.AddListener(mainController.ToggleArMode);
				// ar key connects
				configWidget.KeyDepthChangedEvent.AddListener(mainController.setARKeyDepth);
				configWidget.KeyColorChangedEvent.AddListener(mainController.setARKeyColor);	
				configWidget.KeyRadiusChangedEvent.AddListener(mainController.setARKeyRadius);	
				configWidget.KeyThresholdChangedEvent.AddListener(mainController.setARKeyThreshold);	
                configWidget.ToggleARMatteEvent.AddListener(mainController.ToggleARMatteMode);
				configWidget.ToggleARKeyEvent.AddListener(mainController.ToggleARKeyMode);
				// add other ar managers here or change a global variable like VPET.Settings.sceneScale!
#endif


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

        public void changeARLockRotationButtonImage(bool active)
        {
            if (active) GameObject.Find("GUI/Canvas/ARConfigWidget/Lock_Rotation").GetComponent<Image>().sprite = Misc_SceneRotate_nrm;
            else GameObject.Find("GUI/Canvas/ARConfigWidget/Lock_Rotation").GetComponent<Image>().sprite = Misc_SceneRotate_sel;
        }

        public void changeARLockScaleButtonImage(bool active)
        {
            if (active) GameObject.Find("GUI/Canvas/ARConfigWidget/Lock_Scale").GetComponent<Image>().sprite = Misc_SceneScale_nrm;
            else GameObject.Find("GUI/Canvas/ARConfigWidget/Lock_Scale").GetComponent<Image>().sprite = Misc_SceneScale_sel;
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

		public void acceptARConfig()
		{
			drawConfigWidget ();
		}

		public void requestKeyConfig()
		{
			hideConfigWidget();
			GameObject arKeyWidget = GameObject.Find("GUI/Canvas/ARKeyWidget");
			arKeyWidget.SetActive(true);			
		}

		public void acceptKeyConfig()
		{
			GameObject arKeyWidget = GameObject.Find("GUI/Canvas/ARKeyWidget");
			arKeyWidget.SetActive(false);
            // save values to preferences
            VPETSettings.mapValuesToPreferences(configWidget);
			drawConfigWidget();
		}

        public ConfigWidget drawConfigWidget()
	    {
			// TODO: next 5 lines should be in acceptARConfig !?!
			GameObject arConfigWidget = GameObject.Find("GUI/Canvas/ARConfigWidget");
			GameObject rootScene = SceneLoader.scnRoot;
			arConfigWidget.SetActive(false);
			rootScene.SetActive(true);
			mainController.hideARWidgets();

            // get radial menu
            // get ProgressWidget
            GameObject radialrefObject = GameObject.Find("GUI/Canvas/ProgressWidget");
            if (radialrefObject != null)
            {
                radialrefObject.gameObject.SetActive(false);
            }

            // copy values from global settings to config widget TODO: load it directly, see next line
            VPETSettings.mapValuesToObject( configWidget );

            // map values from preferences to config widget
            VPETSettings.mapValuesFromPreferences(configWidget);

			configWidget.Show();           
            configWidget.initConfigWidget();
            UI.OnUIChanged.Invoke();
            return configWidget;
	    }
	
	    public void hideConfigWidget()
	    {
			// must be first
			configWidget.Hide();

            // copy values from config widget to global settings TODO: obsolete!?
			VPETSettings.mapValuesFromObject( configWidget );

            // save global settings in preferences TODO: save it directly, see next line
			VPETSettings.mapValuesToPreferences();

            // save config widget settings to prferences
            VPETSettings.mapValuesToPreferences(configWidget);

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
            // IMenuButton button = parameterMenu.GetButton(0);

            parameterMenu.switchLayout(layout);
            float _adjustOddCount = (parameterMenu.ActiveButtonCount % 2) * UI.ButtonOffset / 2f;
            parameterMenu.offset = new Vector2(-VPETSettings.Instance.canvasHalfWidth + UI.ButtonOffset, parameterMenu.ActiveButtonCount * UI.ButtonOffset / 2f + _adjustOddCount);

			if ( parameterMenu.ActiveButtonCount > 1)
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
	
}}

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
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
-----------------------------------------------------------------------------
*/

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

//!
//! INFO: the mainController class is separeted into different files
//! find all parts in the files named like MainController.XXX
//!
namespace vpet
{
	public class GravityChangeEvent : UnityEvent<bool> { }
	public class VPETBoolEvent : UnityEvent<bool> { }

	public partial class MainController : MonoBehaviour {
	

		public GravityChangeEvent OnObjectGravityChange = new GravityChangeEvent();

		/*
	    //!
	    //! check if screen position is on an active GUI element & get id of the button at screen position
	    //! @param      pos     position on screen (z is always 0)
	    //! @return     id of button under screen position, -1 if none
	    //!
	    public int getGuiElementId(Vector3 pos)
	    {
	        return ui.getButtonId(new Vector2(pos.x, Screen.height - pos.y)); ;
	    }
		*/
	
	    //!
	    //! move the camera relative to last position
	    //! @param      translation     new position relative to old one
	    //!
	    public void moveCameraObject(Vector3 translation)
	    {
	        Camera.main.transform.Translate(translation / 10000.0f);
	    }
	
	    //!
	    //! move the camera's relative to last position
	    //! @param      translation     new position relative to old one
	    //!
	    public void moveCameraParent( Vector3 translation )
	    {
	        cameraRig.Translate( translation / 10000.0f );
	    }

        //!
        //! switch wether a standalone windows version will recive klicks or touch events
        //!
        public bool standaloneReceivesClicks = false;

        /*
	    //!
	    //! check if screen position is on color or intensity picker (in light mode)
	    //! @param      pos     position on screen (z is always 0)
	    //! @return     true if no GUI Element at screen position
	    //!
	    public bool isOnLightSettingsPicker(Vector3 pos)
	    {
	        return ui.isOnLightSettingsPicker(pos);
	    }
		*/

        /*
	    //!
	    //! update the light parameters when beeing in lightEditing mode based on the mouse position on GUI
	    //! @param      pos     position on screen (z is always 0)
	    //!
	    public void updateLight(Vector3 pos)
	    {
	        Color newColor = ui.getColorPickerValue(pos);
	        if (newColor != new Color(0, 0, 0, 0))
	        {
	            if (currentSelection) currentSelection.GetComponent<SceneObject>().setLightColor(newColor);
	            return;
	        }
	        float newIntensity = ui.getIntensityPickerValue(pos);
	        if (newIntensity != -1)
	        {
	            if (currentSelection) currentSelection.GetComponent<SceneObject>().setLightIntensity(newIntensity * 8.0f);
	            return;
	        }
	        float newDeltaRange = ui.getRangePickerDeltaValue(pos);
	        if (newDeltaRange != 0)
	        {
	            if (currentSelection) currentSelection.GetComponent<SceneObject>().setLightDeltaRange(newDeltaRange);
	            return;
	        }
	        float newAngle = ui.getAnglePickerValue(pos);
	        if (newAngle != -1)
	        {
	            if (currentSelection) currentSelection.GetComponent<SceneObject>().setLightAngle(newAngle);
	            return;
	        }
	    }
		*/

        //!
        //! execute a server update for the currently selected object
        //!
        public void sendUpdateToServer()
	    {
	        if (currentSelection) currentSelection.GetComponent<SceneObject>().sendUpdate();
	    }
	
	    //!
	    //! open light menu when currentSelection is a light otherwise open the objectMenu
	    //! is normally called when an editing mode is terminated but the object remains selected
	    //!
	    public void openMenu()
	    {
			if(currentSelection && activeMode != Mode.animationEditing ) // HACK: to handle animationEditing mode
	        {
	            if (currentSelection.GetComponent<SceneObject>().isDirectionalLight ||
	                currentSelection.GetComponent<SceneObject>().isPointLight ||
	                currentSelection.GetComponent<SceneObject>().isSpotLight)
	            {
	                activeMode = Mode.lightMenuMode;
	            }
                // TODO: NILS: this is a hack
                //else if (activeMode != Mode.animationEditing)
                else
				{
                    activeMode = Mode.objectMenuMode;
                }
	        }
	    }
	
	
	    //!
	    //! get the position of the current selection if something is selected
	    //! @return      absolut position of selection, if nothing selected (0,0,0)
	    //!
	    public Vector3 getSelectionPosition()
	    {
	        if(currentSelection) return currentSelection.position;
	        return Vector3.zero;
	    }
	
	    //!
	    //! unselect the given object if it is selected (is currentSelection)
	    //! @param      obj     obj to check against
	    //!
	    public void unselectIfSelected(Transform obj)
	    {
	        if (currentSelection == obj)
	            activeMode = Mode.idle;
	    }
	
	    //!
	    //! resets all elements to initial positions
	    //!
	    public void globalReset()
	    {
	        recursiveResetChilds(scene.transform);
			undoRedoController.addAction();
	    }
	
	    //!
	    //! recursively reset all childs
	    //! @param      obj     target transform
	    //!
	    private void recursiveResetChilds(Transform obj)
	    {
	        for(int i = 0; i < obj.childCount; i++)
	        {
	            if (obj.GetChild(i).GetComponent<SceneObject>() && obj.GetChild(i).gameObject.activeSelf)
	            {
	                obj.GetChild(i).GetComponent<SceneObject>().resetAll();
	            }
	            recursiveResetChilds(obj.GetChild(i));
	        }
	    }
	
		//!
		//! turn on/off ncam synchronisation
		//!
		public void toggleNcam()
		{
			bool isOn = serverAdapter.receiveNcam;
			serverAdapter.receiveNcam = !isOn;
			camObject.GetComponent<Renderer>().enabled = isOn;
			cameraAdapter.setMove( isOn );

			VPETSettings.Instance.msg = "Ncam set: " + serverAdapter.receiveNcam.ToString();

			/*
			if (serverAdapter.receiveNcam) 
			{
				serverAdapter.receiveNcam = false;
	            camObject.GetComponent<Renderer>().enabled = true;
				cameraAdapter.setMove(true);
			}
			else
			{
				serverAdapter.receiveNcam = true;
	            camObject.GetComponent<Renderer>().enabled = false;
				cameraAdapter.setMove(false);
			}
			*/
		}
	
	    //!
	    //! toggle ignore rotation
	    //!
	    public void toggleCameraRotation()
	    {
	        cameraAdapter.doApplyRotation =  !cameraAdapter.doApplyRotation; 
	    }
	
	
		//!
		//! set camera to perspective if is orthografic
		//!
		public void setPerspectiveCamera()
		{
			// switch off ncam if on
			if ( serverAdapter.receiveNcam ) toggleNcam();

			if (Camera.main.orthographic == true)
			{
				Camera.main.orthographic = false;
                foreach (Camera cam in Camera.main.transform.GetComponentsInChildren<Camera>())
                    cam.orthographic = false;
                cameraAdapter.setMove(true);
				currentCameraView = View.PERSP;
			}

			// restore previous view, e.g. when returning from orthographic view
			Camera.main.transform.position = camPreviousPosition;
			Camera.main.transform.rotation = camPreviousRotation;

		}

        public void resetCameraOffset()
        {
            cameraAdapter.resetCameraOffset();
        }

        //!
        //! reposition the camera to predefined positions
        //!
        public void repositionCamera()
	    {
			setPerspectiveCamera();

            if (sceneAdapter.SceneCameraList.Count > 0)
            {
                GameObject camObject = sceneAdapter.SceneCameraList[camPrefabPosition];
                Camera.main.transform.position = camObject.transform.position; // cameraPositions[camPrefabPosition];
                Camera.main.transform.rotation = camObject.transform.rotation; // cameraPositions[camPrefabPosition];

                // callibrate 
                // cameraAdapter.calibrate(Camera.main.transform.eulerAngles.y);

                // set camera properties
                CameraObject camScript = camObject.GetComponent<CameraObject>();
                if (camScript != null)
                {
                    Camera.main.fieldOfView = camScript.fov;
                    Camera.main.nearClipPlane = camScript.near;
                    Camera.main.farClipPlane = camScript.far;
                }
                camPrefabPosition = (camPrefabPosition + 1) % sceneAdapter.SceneCameraList.Count;
            }
	    }
	
	    //!
	    //! getter function for the current selection
	    //! @return     reference to the currently selected object
	    //!
	    public Transform getCurrentSelection()
	    {
	        return currentSelection;
	    }
	
	    //!
	    //! switches the camera to orthographic view
	    //!
	    public void setOrthographicCamera( View view )
	    {
			// save current view
			camPreviousPosition = Camera.main.transform.position;
			camPreviousRotation = Camera.main.transform.rotation;

			// switch off ncam if on
			if ( serverAdapter.receiveNcam ) toggleNcam();

	        Camera.main.orthographic = true;
            foreach (Camera cam in Camera.main.transform.GetComponentsInChildren<Camera>())
                cam.orthographic = true;
            cameraAdapter.setMove(false);
			currentCameraView = view;

	        switch ( view )
	        {
	            case View.TOP:
	                Camera.main.transform.rotation = Quaternion.Euler( new Vector3(90f, 180f, 0f) );
					Camera.main.transform.position = new Vector3(-15f, 10f, 5f) + VPETSettings.Instance.cameraOffsetTop;
	                break;
	            case View.FRONT:
	                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
					Camera.main.transform.position = new Vector3(-15f, 5f, 14f) + VPETSettings.Instance.cameraOffsetFront;
                    break;
	            case View.RIGHT:
                    Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
					Camera.main.transform.position = new Vector3(-22.9f, 5f, 4.7f) + VPETSettings.Instance.cameraOffsetRight;
                    break;
	            case View.LEFT:
                    Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
					Camera.main.transform.position = new Vector3(-7.4f, 5f, 4.7f) + VPETSettings.Instance.cameraOffsetLeft;                    
	                break;
	            case View.BOTTOM:
	                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(270f, 0f, 0f));
					Camera.main.transform.position = new Vector3(-15f, -1.88f, 2.24f) + VPETSettings.Instance.cameraOffsetBottom;
	                break;
	        }
	    }
		//!
		//! calculate the camera positional offset and store in user preferences
		//!
		public void saveCameraOffset()
		{
			switch ( currentCameraView )
			{
				case View.PERSP:
					// nothing
					break;
				case View.TOP:
					VPETSettings.Instance.cameraOffsetTop = Camera.main.transform.position - new Vector3(-15f, 10f, 5f);
					break;
				case View.FRONT:
					VPETSettings.Instance.cameraOffsetFront = Camera.main.transform.position - new Vector3(-15f, 5f, 14f);
					break;
				case View.RIGHT:
					VPETSettings.Instance.cameraOffsetRight = Camera.main.transform.position - new Vector3(-22.9f, 5f, 4.7f);
					break;
				case View.LEFT:
					VPETSettings.Instance.cameraOffsetLeft = Camera.main.transform.position - new Vector3(-7.4f, 5f, 4.7f);
					break;
				case View.BOTTOM:
					VPETSettings.Instance.cameraOffsetLeft = Camera.main.transform.position - new Vector3(-15f, -1.88f, 2.24f);
					break;
			}

			VPETSettings.mapValuesToPreferences();

		}
	

        private void splashFinished()
        {
            // draw main menu 
            ui.drawMainMenuButton(true);





			/*
            // HACK to invoke
            MenuButton configButton = GameObject.Find("GUI/Canvas/UI/mainMenuObject/3_GeneralMenu_Settings_sel").GetComponent<MenuButton>();
            configButton.onClick.Invoke();
            configButton.Toggled = true;
			*/




            // set values in config widget according to vpet setting
			ConfigWidget configWidget = ui.drawConfigWidget();
			VPETSettings.mapValuesToObject(configWidget);
            configWidget.initConfigWidget();

        }


        //!
        //! Handle submited config widget
        //! This triggers the server adapter to request a scene
        //! @param widget     source widget
        //!
        public void configWidgetSubmit( ConfigWidget widget)
	    {

			ui.hideConfigWidget();

            ui.switchLayoutMainMenu(layouts.DEFAULT);

            ui.resetMainMenu();

            // request progress bar from UI and connect listener to server adapter
            RoundProgressBar progressWidget =  ui.drawProgressWidget();
			// Connect set value to progress event
			serverAdapter.OnProgressEvent.AddListener(progressWidget.SetValue);
			// Connect scene load complete to progress finish event
			progressWidget.OnFinishEvent.AddListener(this.sceneLoadCompleted);

            // init server adapter
            serverAdapter.initServerAdapterTransfer();
		}
		
	    //!
	    //! Handle after the scene has been transfered and loaded into the engine
	    //! @param widget     source widget
	    //!
	    public void sceneLoadCompleted()
	    {
			ui.hideProgressWidget();
	    }

		//
		public void setAmbientIntensity( float v )
		{
			RenderSettings.ambientIntensity = v;
		}

        public void ToggleArMode( bool active )
        {
            if ( active)
            {
                sceneAdapter.HideGeometry();
                //Camera.main.renderingPath = RenderingPath.Forward;
                //tangoApplication.m_enableVideoOverlay = true;
                //tangoApplication.m_videoOverlayUseTextureMethod = true;
                Camera.main.gameObject.GetComponent<TangoARScreen>().enabled = true;
            }
            else
            {
                Camera.main.gameObject.GetComponent<TangoARScreen>().enabled = false;
                // tangoApplication.m_enableVideoOverlay = false;
                //Camera.main.renderingPath = RenderingPath.DeferredShading;
                sceneAdapter.ShowGeometry();
            }
        }

    }
}
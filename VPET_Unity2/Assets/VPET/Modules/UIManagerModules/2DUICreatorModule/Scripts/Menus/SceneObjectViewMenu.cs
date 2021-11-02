using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace vpet
{
    public class SceneObjectViewMenu : Menu
    {
        public RectTransform manipulatorParent;
        public RectTransform manipulatorSelection;
        public RectTransform objectSelection;
        public RectTransform viewSettings;

        private SelectionAnalyzer selectionAnalyzer = new SelectionAnalyzer();
        
        private List<GameObject> instancedManipulators = new List<GameObject>();

        public void Init(UICreator2DModule ui2DModule, List<SceneObject> selection)
        {
            //TODO Analyze Selection selectionAnalyzer.DetermineManipulators(selection);
            List<ManipulatorReference> manipulators = ui2DModule.settings.manipulators;
            SceneObject mainSelection = selection[0];

            foreach (var paramater in mainSelection.parameterList)
            {
                //Check which type of Manipulator edits this type of Parameter
                ManipulatorSetting manipSetting = ui2DModule.settings.parameterMapping[paramater.vpetType];

                if(manipSetting.uiSprites.Count < 1)
                {
                    //No Manipulator Setting was found for this type of parameter
                    continue;
                }

                //Get UI Prefab Reference via this Manipulator Type
                ManipulatorReference manipRef = ui2DModule.settings.manipulators.First(reference => reference.type == manipSetting.manipulator);

                if(manipRef.manipulatorPrefab != null)
                {
                    manipulators.Add(manipRef);
                    Debug.Log($"VPET Added determined to create {manipRef.type} Manipulator");
                }

                GameObject createdManip = Instantiate(manipRef.manipulatorPrefab, manipulatorParent);
                createdManip.SetActive(false);
                instancedManipulators.Add(createdManip);

                GameObject createdManipSelector = Instantiate(ui2DModule.settings.manipulatorSelector, manipulatorSelection);
            }


        }

        public void InstantiateManipulators(List<ManipulatorReference> selection)
        {
            //Add Manipulator Icons to the Selector part
            //Add Actual Manipulators as child of manipulator parent
        }

        public void InitManipulatorSelection()
        {

        }

        public void InitObjectSelection()
        {

        }    
        
        public void InitViewSettings()
        {
            
        }          
    }
}
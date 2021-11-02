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
        private List<GameObject> instancedManipulatorSelectors = new List<GameObject>();

        public void Init(UICreator2DModule ui2DModule, List<SceneObject> selection)
        {
            if(selection.Count < 1)
            {
                Debug.Log("Selection was empty, no UI will be displayed!");
                return;
            }

            ShowMenu();

            //TODO Analyze Selection selectionAnalyzer.DetermineManipulators(selection);
            List<ManipulatorReference> manipulators = ui2DModule.settings.manipulators;
            SceneObject mainSelection = selection[0];

            int manipIndex = 0;
            foreach (var paramater in mainSelection.parameterList)
            {
                //Check which type of Manipulator edits this type of Parameter
                try
                {
                    ManipulatorSetting manipSetting = ui2DModule.settings.parameterMapping[paramater.vpetType];

                    //Get UI Prefab Reference via this Manipulator Type
                    ManipulatorReference manipRef = ui2DModule.settings.manipulators.First(reference => reference.type == manipSetting.manipulator);

                    if(manipRef.manipulatorPrefab != null)
                    {
                        manipulators.Add(manipRef);
                        Debug.Log($"VPET Added determined to create {manipRef.type} Manipulator");
                        
                        GameObject createdManip = Instantiate(manipRef.manipulatorPrefab, manipulatorParent);
                        createdManip.SetActive(false);
                        instancedManipulators.Add(createdManip);

                        ManipulatorSelector createdManipSelector = Instantiate(ui2DModule.settings.manipulatorSelector, manipulatorSelection);
                        createdManipSelector.Init(this, manipSetting.uiSprites[0], manipIndex);
                        instancedManipulatorSelectors.Add(createdManipSelector.gameObject);

                        manipIndex++;
                    }
                }
                catch(KeyNotFoundException e)
                {
                    //No Manipulator Setting was found for this type of parameter
                    continue;
                }
            }

            if(instancedManipulators.Count > 0)
            {
                SelectManipulator(0);
            }
        }

        public void Clear()
        {
            foreach (var manip in instancedManipulators)
            {
                Destroy(manip);
            }
            instancedManipulators.Clear();

            foreach (var manipSelec in instancedManipulatorSelectors)
            {
                Destroy(manipSelec);
            }
            instancedManipulatorSelectors.Clear();            
        }

        public void InstantiateManipulators(List<ManipulatorReference> selection)
        {
            //Add Manipulator Icons to the Selector part
            //Add Actual Manipulators as child of manipulator parent
        }

        public void SelectManipulator(int index)
        {
            instancedManipulators.ForEach(manip => manip.SetActive(false));
            instancedManipulators[index].SetActive(true);
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
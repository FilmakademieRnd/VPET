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
        
        private List<Manipulator> instancedManipulators = new List<Manipulator>();
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
            SceneObject mainSelection = selection[0];

            int manipIndex = 0;
            int paramIndex = 0;
            foreach (var paramater in mainSelection.parameterList)
            {
                //Check which type of Manipulator edits this type of Parameter
                //Get UI Prefab Reference via this Manipulator Type
                try 
                {
                    ManipulatorReference manipRef = ui2DModule.settings.manipulators[paramIndex];
                    
                    if(manipRef.manipulatorPrefab != null)
                    {                        
                        Manipulator createdManip = Instantiate(manipRef.manipulatorPrefab, manipulatorParent);
                        createdManip.gameObject.SetActive(false);
                        createdManip.LinkToParameter(paramater);
                        instancedManipulators.Add(createdManip);

                        ManipulatorSelector createdManipSelector = Instantiate(ui2DModule.settings.manipulatorSelector, manipulatorSelection);
                        instancedManipulatorSelectors.Add(createdManipSelector.gameObject);

                        //Initialization of the different Manipulators
                        switch (manipRef.parameterType) 
                        {
                            case ParameterType.Position:
                                createdManipSelector.Init(this, manipRef.selectorIcon, manipIndex);
                                Spinner spinnerPos = (Spinner) createdManip;
                                spinnerPos.Init(mainSelection.gameObject.transform.position);
                                break;
                            case ParameterType.Rotation:
                                createdManipSelector.Init(this, manipRef.selectorIcon, manipIndex);
                                Spinner spinnerRot = (Spinner)createdManip;
                                spinnerRot.Init(mainSelection.gameObject.transform.rotation.eulerAngles);
                                break;
                            case ParameterType.Scale:
                                createdManipSelector.Init(this, manipRef.selectorIcon, manipIndex);
                                Spinner spinnerScale = (Spinner)createdManip;
                                spinnerScale.Init(mainSelection.gameObject.transform.localScale);
                                break;
                        }

                        manipIndex++;
                    }
                }
                catch(System.IndexOutOfRangeException e)
                {
                    
                }

                paramIndex++;
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
            instancedManipulators.ForEach(manip => manip.gameObject.SetActive(false));
            instancedManipulators[index].gameObject.SetActive(true);
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
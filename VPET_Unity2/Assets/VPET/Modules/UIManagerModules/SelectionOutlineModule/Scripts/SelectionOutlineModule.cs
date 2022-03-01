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

//! @file "SelectionOutlineModule.cs"
//! @brief Implementation of the VPET SelectionOutlineModule, adding a outline material to a selected scene object.
//! @author Simon Spielmann
//! @version 0
//! @date 23.11.2021

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace vpet
{
    public class SelectionOutlineModule : UIManagerModule
    {
        //!
        //! The outline material to be added to the selectet object.
        //!
        private Material _outlineMaterial;

        //!
        //! The outline material to be added to a locked object.
        //!
        private Material _outlineLockMaterial;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public SelectionOutlineModule(string name, Core core) : base(name, core)
        {
        }

        //! 
        //! Function called when Unity initializes the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            Shader outlineShader = Resources.Load<Shader>("Shader/SelectionOutlineShader");
            _outlineMaterial = new Material(outlineShader);
            _outlineLockMaterial = new Material(outlineShader);
            _outlineLockMaterial.SetColor("_OutlineColor", Color.red);

            manager.selectionAdded += HighlightSelection;
            manager.selectionRemoved += DisableHighlightSelection;

            manager.highlightLocked += HighlightLocked;
            manager.unhighlightLocked += DisableHighlightLocked;
        }

        //!
        //! Function that is called when the UIManager signals an selection.
        //! Will add the outline material to all renderes of the given scene object.
        //!
        //! @param sender A reference to the UIManager.
        //! @param eventArgs Event Arguments containing the Scene Object and the highlight color.
        //!
        private void HighlightLocked(object sender, SceneObject sceneObject)
        {
            Renderer[] renderers = sceneObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                List<Material> materials = renderer.sharedMaterials.ToList();
                materials.Add(_outlineLockMaterial);
                renderer.materials = materials.ToArray();
            }
        }

        //!
        //! Function that is called when the UIManager signals an selection.
        //! Will add the outline material to all renderes of the given scene object.
        //!
        //! @param sender A reference to the UIManager.
        //! @param sceneObject The selected sceneObject.
        //!
        private void HighlightSelection(object sender, SceneObject sceneObject)
        {
            Renderer[] renderers = sceneObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                List<Material> materials = renderer.sharedMaterials.ToList();
                materials.Add(_outlineMaterial);
                renderer.materials = materials.ToArray();
            }
        }

        //!
        //! Function that is called when the UIManager signals an selection removed.
        //! Will remove the outline material on all renderes of the given scene object.
        //!
        //! @param sender A reference to the UIManager.
        //! @param sceneObject The selected sceneObject.
        //!
        private void DisableHighlightSelection(object sender, SceneObject sceneObject)
        {
            if (sceneObject)
            {
                Renderer[] renderers = sceneObject.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    List<Material> materials = renderer.sharedMaterials.ToList();
                    materials.Remove(_outlineMaterial);

                    renderer.materials = materials.ToArray();
                }
            }
        }

        //!
        //! Function that is called when the UIManager signals an selection removed.
        //! Will remove the outline material on all renderes of the given scene object.
        //!
        //! @param sender A reference to the UIManager.
        //! @param sceneObject The selected sceneObject.
        //!
        private void DisableHighlightLocked(object sender, SceneObject sceneObject)
        {
            Renderer[] renderers = sceneObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                List<Material> materials = renderer.sharedMaterials.ToList();
                materials.Remove(_outlineLockMaterial);

                renderer.materials = materials.ToArray();
            }
        }
    }
}
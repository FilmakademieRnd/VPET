using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace vpet
{
    public class SelectionOutlineModule : UIManagerModule
    {
        private Material _outlineMaterial;
        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public SelectionOutlineModule(string name, Core core) : base(name, core)
        {
        }

        protected override void Init(object sender, EventArgs e)
        {
            Shader outlineShader = Resources.Load<Shader>("Shader/SelectionOutlineShader");
            _outlineMaterial = new Material(outlineShader);

            manager.selectionAdded += HighlightSelection;
            manager.selectionRemoved += DisableHighlightSelection;
        }

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

        private void DisableHighlightSelection(object sender, SceneObject sceneObject)
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
}
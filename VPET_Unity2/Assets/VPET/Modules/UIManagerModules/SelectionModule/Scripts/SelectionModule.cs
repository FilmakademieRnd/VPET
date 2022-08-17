/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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

//! @file "PexelSelectorModule.cs"
//! @brief implementation of the VPET SelectionModule, a pixel precise selection method.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.11.2021

using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace vpet
{
    //!
    //! Module to be used per camera that provide selection from main camera.
    //! There can be multiple instances of this class, providing local camera space selection.
    //!
    public class SelectionModule : UIManagerModule
    {
        //!
        //! Name of the shader tag for the selection shader.
        //!
        private const string SelectableType = "SelectableType";
        //!
        //! Value od the shader tag for the selection shader.
        //!
        public const string Selectable = "Selectable";
        //!
        //! Name of the shader property holding the selectable id.
        //!
        private const string m_SelectableIdPropertyName = "_SelectableId";
        //!
        //! The shater to be used for object ID rendering.
        //!
        private Shader objectIdShader;
        //!
        //! The render texture for the selection.
        //!
        private RenderTexture gpuTexture;
        //!
        //! The color/ID data to be stored in the CPU texture.
        //!
        private NativeArray<Color32> cpuData;
        //!
        //! Tracked materials with selectable tag.
        //!
        private readonly Dictionary<Material, Material> m_materials;
        //!
        //! A reference to the VPET scene manager.
        //!
        private SceneManager m_sceneManager;
        //!
        //! A reference to the VPET input manager.
        //!
        private InputManager m_inputManager;
        //!
        //! The async redner request.
        //!
        private AsyncGPUReadbackRequest request;
        //!
        //! Re-used property block used to set selectable id.
        //!
        private MaterialPropertyBlock m_properties;
        //!
        //! Cached shader property id of selectable id.
        //!
        private int m_selectableIdPropertyId;
        //!
        //! Scaled width and height of render texture.
        //!
        private int dataWidth, dataHeight;
        //!
        //! Divides the screen resolution for the selection.
        //!
        private int scaleDivisor = 4;
        //!
        //! Flag to determine a selection render request. 
        //!
        private bool m_hasAsyncRequest;
        //!
        //! Flag to determine if touch is active.
        //!
        private bool m_isRenderActive = false;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public SelectionModule(string name, Manager manager) : base(name, manager)
        {
            objectIdShader = Resources.Load<Shader>("Shader/SelectableId");
            m_materials = new Dictionary<Material, Material>();
            m_selectableIdPropertyId = Shader.PropertyToID(m_SelectableIdPropertyName);
        }

        //! 
        //! Function called when Unity initializes the VPET core.
        //! 
        //! @param sender A reference to the VPET core.
        //! @param e Arguments for these event. 
        //! 
        protected override void Init(object sender, EventArgs e)
        {
            core.updateEvent += renderUpdate;
           
            m_sceneManager = core.getManager<SceneManager>();
            m_inputManager = core.getManager<InputManager>();
            
            m_sceneManager.sceneReady += modifyMaterials;

            // hookup to input events
            m_inputManager.objectSelectionEvent += SelectFunction;
        }

        //!
        //! Callback from VPET core when Unity calls OnDestroy.
        //! Used to cleanup resources used by the PixelSelector module.
        //!
        protected override void Cleanup(object sender, EventArgs e)
        {
            base.Cleanup(sender, e);

            core.updateEvent -= renderUpdate;
            m_sceneManager.sceneReady -= modifyMaterials;
            m_inputManager.objectSelectionEvent -= SelectFunction;

            dataWidth = 0;
            dataHeight = 0;

            if (gpuTexture != null)
                gpuTexture.Release();

            if (cpuData.IsCreated)
                cpuData.Dispose();

            m_hasAsyncRequest = false;
            request = default(AsyncGPUReadbackRequest);
        }

        //!
        //! Function to connect input managers input event for estimating a scene object at
        //! a certain screen coortinate with UI managers scene object selection mechanism.
        //!
        //! @param sender The input manager.
        //! @param e The screen coorinates from the input event.
        //!
        private async void SelectFunction(object sender, InputManager.InputEventArgs e)
        {
            m_isRenderActive = true;

            // give the system some time to render the object id's
            await System.Threading.Tasks.Task.Delay(50);

            manager.clearSelectedObject();
            
            SceneObject obj = GetSelectableAtCollider(e.point);

            if (!obj)
                obj = GetSelectableAtPixel(e.point);

            if (obj)
            {
                switch (obj)
                {
                    case SceneObjectCamera:
                        if (manager.activeRole == UIManager.Roles.EXPERT ||
                            manager.activeRole == UIManager.Roles.DOP)
                            manager.addSelectedObject(obj);
                        break;
                    case SceneObjectLight:
                        if (manager.activeRole == UIManager.Roles.EXPERT ||
                            manager.activeRole == UIManager.Roles.LIGHTING ||
                            manager.activeRole == UIManager.Roles.SET)
                            manager.addSelectedObject(obj);
                        break;
                    default:
                        if (manager.activeRole == UIManager.Roles.EXPERT ||
                            manager.activeRole == UIManager.Roles.SET)
                            manager.addSelectedObject(obj);
                        break;
                }
            }

            m_isRenderActive = false;
        }

        //!
        //! Retrieve the selectable present at the current location in camera screenspace, if any.
        //! 
        //! @param screenPosition The position to get the selectable at.
        //! @return The selectable at the specified screen position or null if there is none.
        //!
        public SceneObject GetSelectableAtPixel(Vector2 screenPosition)
        {
            if (!cpuData.IsCreated)
                return null;

            int scaledX = (int)screenPosition.x / scaleDivisor;
            int scaledY = (int)screenPosition.y / scaleDivisor;
            int pos = scaledX + dataWidth * scaledY;
            
            if (cpuData.Length < pos || pos < 0)
                return null;
            
            Color32 packedId = cpuData[pos];
            int id = DecodeId(packedId);

            return m_sceneManager.getSceneObject(id);
        }

        //!
        //! Retrieve the selectable present at the current location by tracing a ray into the scene and looking for colliders.
        //! 
        //! @param screenPosition The position to get the selectable at.
        //! @param layerMask The object layers to be considered for the ray intersection.
        //! @return The selectable at the traced collider or null if there is none.
        //!
        public SceneObject GetSelectableAtCollider(Vector2 screenPosition)
        {
            RaycastHit hit;
            SceneObject sceneObject = null;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out hit, Mathf.Infinity))
            {
                GameObject gameObject = hit.collider.gameObject;
                IconUpdate iconUpdate = gameObject.GetComponent<IconUpdate>();
                // check if an icon has been hit
                if (iconUpdate)
                    sceneObject = iconUpdate.m_parentObject;

                if (!sceneObject)
                    sceneObject = gameObject.GetComponent<SceneObject>();

                if (!sceneObject)
                    sceneObject = gameObject.transform.parent.GetComponent<SceneObject>();

            }

            return sceneObject;
        }

        //! 
        //! Gets a cached adjusted material or creates a new one based on the specified material.
        //! Selectable materials are identical to the specified material except that they have the
        //! SelectableType tag set so they are rendered in the replacement pass used to render
        //! selectable ids.
        //! 
        //! Note that all adjusted materials are destroyed when the selection manager is destroyed!
        //! 
        //! @param material The material to be changed for selection rendering.
        //! @return An adjusted m_instance of the specified material with the selectable tag set.
        //!
        private Material GetSelectableMaterial(Material material)
        {
            Material selectableMaterial;
            if (!m_materials.TryGetValue(material, out selectableMaterial))
            {
                selectableMaterial = UnityEngine.Object.Instantiate(material);
                selectableMaterial.SetOverrideTag(SelectableType, Selectable);
                m_materials.Add(material, selectableMaterial);
            }

            return selectableMaterial;
        }

        //!
        //! Callback from VPET core when Unity calls it's render update.
        //! Used setup render texture, render the object ID pass and copy
        //! it asyncron into a Color32 array. 
        //!
        private void renderUpdate(object sender, EventArgs e)
        {
            if (m_isRenderActive)
            {
                Camera camera = Camera.main;

                // If camera size changed (e.g. window resize) adjust sizes of selection textures.
                int currentWidth = camera.pixelWidth / scaleDivisor;
                int currentHeight = camera.pixelHeight / scaleDivisor;
                if (gpuTexture == null || dataWidth != currentWidth || dataHeight != currentHeight)
                {
                    if (gpuTexture != null) gpuTexture.Release();
                    if (cpuData.IsCreated) cpuData.Dispose();

                    dataWidth = currentWidth;
                    dataHeight = currentHeight;

                    int depthBits = camera.depthTextureMode == DepthTextureMode.None ? 16 : 0;
                    gpuTexture = new RenderTexture(dataWidth, dataHeight, depthBits, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    gpuTexture.filterMode = FilterMode.Point;
                    cpuData = new NativeArray<Color32>(dataWidth * dataHeight, Allocator.Persistent);
                }

                RenderTexture oldRenderTexture = camera.targetTexture;
                CameraClearFlags oldClearFlags = camera.clearFlags;
                Color oldBackgroundColor = camera.backgroundColor;
                RenderingPath oldRenderingPath = camera.renderingPath;
                bool oldAllowMsaa = camera.allowMSAA;

                camera.targetTexture = gpuTexture; // Render into render texture.
                camera.clearFlags = CameraClearFlags.SolidColor; // Make sure non-rendered pixels have id zero.
                camera.backgroundColor = Color.clear;
                camera.renderingPath = RenderingPath.Forward; // No gbuffer required.
                camera.allowMSAA = false; // Avoid interpolated colors.

                camera.RenderWithShader(objectIdShader, SelectableType);

                camera.targetTexture = oldRenderTexture;
                camera.clearFlags = oldClearFlags;
                camera.backgroundColor = oldBackgroundColor;
                camera.renderingPath = oldRenderingPath;
                camera.allowMSAA = oldAllowMsaa;

                if (!m_hasAsyncRequest)
                {
                    m_hasAsyncRequest = true;
                    request = AsyncGPUReadback.Request(gpuTexture);
                }
                else if (request.done)
                {
                    if (!request.hasError)
                    {
                        request.GetData<Color32>().CopyTo(cpuData);
                    }

                    request = AsyncGPUReadback.Request(gpuTexture);
                }
            }
        }

        //!
        //! Function that creates a new property block for all renderable
        //! objects in the scene to set the object ID as a shader parameter.
        //! This function is called after the scene has been loaded.
        //!
        private void modifyMaterials(object sender, EventArgs e)
        {
            foreach (Renderer renderer in m_sceneManager.scnRoot.GetComponentsInChildren<Renderer>())
            {
                if (m_properties == null)
                    m_properties = new MaterialPropertyBlock();

                SceneObject sceneObject = renderer.gameObject.GetComponent<SceneObject>();

                if ((sceneObject is SceneObjectCamera) || (sceneObject is SceneObjectLight))
                    continue;

                int id = 0;
                if (sceneObject)
                    id = m_sceneManager.getSceneObjectId(ref sceneObject);
                else
                {
                    Transform t = renderer.transform;
                    Transform root = core.getManager<SceneManager>().scnRoot.transform;
                    
                    while (t.parent != root)
                    {
                        if (t.parent.tag == "editable")
                        {
                            SceneObject so = t.parent.GetComponent<SceneObject>();
                            if (so)
                                id = m_sceneManager.getSceneObjectId(ref so);
                            break;
                        }
                        else
                        {
                            t = t.parent;
                        }

                    }

                }

                Color32 packedId = EncodeId(id);

                m_properties.Clear();

                // Keep existing changed properties.
                if (renderer.HasPropertyBlock())
                    renderer.GetPropertyBlock(m_properties);

                m_properties.SetColor(m_selectableIdPropertyId, packedId);

                renderer.SetPropertyBlock(m_properties);

                renderer.sharedMaterial = GetSelectableMaterial(renderer.sharedMaterial);
            }
        }

        //! 
        //! Encodes a selectable id into a color.
        //! 
        //! @param id The selectable id to be encoded.
        //! @return The color representing the encoded id.
        //!
        private Color32 EncodeId(int id)
        {
            return new Color32(
                (byte)(id >> (3 * 8)),
                (byte)(id >> (2 * 8)),
                (byte)(id >> (1 * 8)),
                (byte)(id >> (0 * 8)));
        }

        //! 
        //! Decodes a color into a selectable id.
        //! 
        //! @param color The color to decode into a selectable id.
        //! @return The decoded selectable id.
        //!
        private int DecodeId(Color32 color)
        {
            return (color.r << (3 * 8)) |
                   (color.g << (2 * 8)) |
                   (color.b << (1 * 8)) |
                   (color.a << (0 * 8));
        }
    }

}

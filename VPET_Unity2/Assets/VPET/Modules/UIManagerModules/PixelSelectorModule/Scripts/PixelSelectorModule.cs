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
    public class PixelSelectorModule : UIManagerModule
    {
        private const string SelectableType = "SelectableType";
        public const string Selectable = "Selectable";
        //!
        //! Name of the shader property holding the selectable id.
        //!
        private const string m_SelectableIdPropertyName = "_SelectableId";
        //!
        //! Divides the screen resolution for the selection.
        //!
        private int scaleDivisor = 4;
        //!
        //! The shater to be used for object ID rendering.
        //!
        private Shader objectIdShader;
        //!
        //! A reference to the VPET scene manager.
        //!
        private SceneManager m_sceneManager;
        //!
        //! Scaled width and height of render texture.
        //!
        private int dataWidth, dataHeight;
        //!
        //! The render texture for the selection.
        //!
        private RenderTexture gpuTexture;
        //!
        //! The color/ID data to be stored in the CPU texture.
        //!
        private NativeArray<Color32> cpuData;
        //!
        //! Flag to determine a selection render request. 
        //!
        private bool hasAsyncRequest;
        //!
        //! The async redner request.
        //!
        private AsyncGPUReadbackRequest request;
        //!
        //! Tracked materials with selectable tag.
        //!
        private readonly Dictionary<Material, Material> m_materials;
        //!
        //! Re-used property block used to set selectable id.
        //!
        private MaterialPropertyBlock m_properties;

        //!
        //! Cached shader property id of selectable id.
        //!
        private int m_selectableIdPropertyId;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public PixelSelectorModule(string name, Core core) : base(name, core)
        {
            objectIdShader = Resources.Load<Shader>("Shader/SelectableId");

            m_sceneManager = core.getManager<SceneManager>();
            core.updateEvent += renderUpdate;
            core.destroyEvent += Cleanup;
            m_materials = new Dictionary<Material, Material>();

            m_selectableIdPropertyId = Shader.PropertyToID(m_SelectableIdPropertyName);

            m_sceneManager.sceneReady += modifyMaterials;
        }

        private void Cleanup(object sender, EventArgs e)
        {
            dataWidth = 0;
            dataHeight = 0;

            if (gpuTexture != null)
                gpuTexture.Release();

            if (cpuData.IsCreated) 
                cpuData.Dispose();

            hasAsyncRequest = false;
            request = default(AsyncGPUReadbackRequest);
        }

        //!
        //! Retrieve the selectable present at the current location in camera screenspace, if any.
        //! 
        //! @param screenPosition The position to get the selectable at.
        //! @return The selectable at the specified screen position or null if there is none.
        //!
        public SceneObject GetSelectableAt(Vector2 screenPosition)
        {
            if (!cpuData.IsCreated)
                return null;

            int scaledX = (int)screenPosition.x / scaleDivisor;
            int scaledY = (int)screenPosition.y / scaleDivisor;
            Color32 packedId = cpuData[scaledX + dataWidth * scaledY];
            int id = DecodeId(packedId);

            return m_sceneManager.getSceneObject(id);
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
        internal Material GetSelectableMaterial(Material material)
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

        private void renderUpdate(object sender, EventArgs e)
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

            // [REVIEW]
            //if (Input.GetMouseButtonDown(0))
            //{
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
            //}

            if (!hasAsyncRequest)
            {
                hasAsyncRequest = true;
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

            if (Input.GetMouseButtonDown(0))
            {
                SceneObject sceneObject = GetSelectableAt(Input.mousePosition);
                if (sceneObject)
                    Helpers.Log(sceneObject.name);
            }

        }

        private void modifyMaterials(object sender, EventArgs e)
        {
            foreach (Renderer renderer in m_sceneManager.scnRoot.GetComponentsInChildren<Renderer>())
            {
                if (m_properties == null)
                    m_properties = new MaterialPropertyBlock();

                SceneObject sceneObject = renderer.gameObject.GetComponent<SceneObject>();
                int id = 0;
                if (sceneObject)
                    id = m_sceneManager.getSceneObjectId(sceneObject);

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
        public Color32 EncodeId(int id)
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
        public int DecodeId(Color32 color)
        {
            return (color.r << (3 * 8)) |
                   (color.g << (2 * 8)) |
                   (color.b << (1 * 8)) |
                   (color.a << (0 * 8));
        }
    }

}

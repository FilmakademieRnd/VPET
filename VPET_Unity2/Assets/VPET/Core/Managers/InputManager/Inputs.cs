//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/VPET/Core/Managers/InputManager/Inputs.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @Inputs : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Inputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Inputs"",
    ""maps"": [
        {
            ""name"": ""VPETMap"",
            ""id"": ""8cc45c96-8744-4e65-a250-74d6bef146fe"",
            ""actions"": [
                {
                    ""name"": ""Navigate"",
                    ""type"": ""Value"",
                    ""id"": ""b48a59d7-3b11-4d2b-afe3-1044fd9f54c6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MiddleClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""2849e90e-27c2-40fc-a527-5d8dc8c17120"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""aaa7ebd1-bb7d-4e74-95f8-3ed939d14844"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Position"",
                    ""type"": ""PassThrough"",
                    ""id"": ""3c39c183-85d3-4bc6-a1b1-a648ca158568"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""PassThrough"",
                    ""id"": ""c60f0eb5-f4f4-48c0-a545-1b9951e44d13"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Tap"",
                    ""type"": ""PassThrough"",
                    ""id"": ""9523916c-5742-4565-84c9-3c2af598c1df"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""cbab1f1a-130f-4b7b-98f3-53112a3532fc"",
                    ""expectedControlType"": ""Quaternion"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""OrbitClick"",
                    ""type"": ""Button"",
                    ""id"": ""a950519f-0476-42db-83cd-857c420abf2e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""DragClick"",
                    ""type"": ""Button"",
                    ""id"": ""d0d60628-4b7a-45b7-9566-8f68e73cc8fb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ZoomWheel"",
                    ""type"": ""Value"",
                    ""id"": ""40f8c425-a372-4fd3-bb25-b215531abf9b"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4fc21893-c29a-424f-9d7f-f6490366ec48"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""34758f0d-7b63-4764-8066-022c355a4e07"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""MiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""e1d85f92-3258-4f58-83ee-1fb209f2c798"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Navigate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""eafd8b7a-d5bb-4f71-8d7d-5c6358ed6c96"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""up"",
                    ""id"": ""dabbab34-77d1-44c5-b4f5-2d48e204c2da"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d0cd3da8-78ba-43c2-b6fd-0aafde1f5ff9"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""af81361e-6a60-46b3-989f-e919ca47e86b"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""73600b5e-3eee-44df-a600-5ee768cf75bd"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e5310c2c-314c-4ff7-9267-3263ec8c01f5"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""c42ab6ff-4d3c-4f92-97cd-c7fd784b6847"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""7f5f0c47-1138-47d4-be20-b88bf2c1029d"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Navigate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6b00df6e-eddd-4d30-a6e9-a108e1d3bc7d"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ee91648a-bf6d-4efd-a98a-5f0edcc87166"",
                    ""path"": ""<Touchscreen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a7643f37-af8b-4aec-ada8-2b5386ba6819"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9d031a88-0d48-45c6-939d-1205fcf74ecf"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2618a06d-4a32-45c3-b190-66a20f46e2a6"",
                    ""path"": ""<AttitudeSensor>/attitude"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""78fdeaff-2fa2-4c5f-88fb-fe721b7fd3c1"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""715b8480-ec4b-444e-befa-a951b749e286"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""164fd2fe-3e48-4e7d-bdb9-e8727926afb1"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OrbitClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""310391ce-651a-4f0c-ba29-12dbadd74063"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DragClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d0570901-3b5b-4bc4-9b01-1e2878c1ad0b"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ZoomWheel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // VPETMap
        m_VPETMap = asset.FindActionMap("VPETMap", throwIfNotFound: true);
        m_VPETMap_Navigate = m_VPETMap.FindAction("Navigate", throwIfNotFound: true);
        m_VPETMap_MiddleClick = m_VPETMap.FindAction("MiddleClick", throwIfNotFound: true);
        m_VPETMap_RightClick = m_VPETMap.FindAction("RightClick", throwIfNotFound: true);
        m_VPETMap_Position = m_VPETMap.FindAction("Position", throwIfNotFound: true);
        m_VPETMap_Click = m_VPETMap.FindAction("Click", throwIfNotFound: true);
        m_VPETMap_Tap = m_VPETMap.FindAction("Tap", throwIfNotFound: true);
        m_VPETMap_Look = m_VPETMap.FindAction("Look", throwIfNotFound: true);
        m_VPETMap_OrbitClick = m_VPETMap.FindAction("OrbitClick", throwIfNotFound: true);
        m_VPETMap_DragClick = m_VPETMap.FindAction("DragClick", throwIfNotFound: true);
        m_VPETMap_ZoomWheel = m_VPETMap.FindAction("ZoomWheel", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // VPETMap
    private readonly InputActionMap m_VPETMap;
    private IVPETMapActions m_VPETMapActionsCallbackInterface;
    private readonly InputAction m_VPETMap_Navigate;
    private readonly InputAction m_VPETMap_MiddleClick;
    private readonly InputAction m_VPETMap_RightClick;
    private readonly InputAction m_VPETMap_Position;
    private readonly InputAction m_VPETMap_Click;
    private readonly InputAction m_VPETMap_Tap;
    private readonly InputAction m_VPETMap_Look;
    private readonly InputAction m_VPETMap_OrbitClick;
    private readonly InputAction m_VPETMap_DragClick;
    private readonly InputAction m_VPETMap_ZoomWheel;
    public struct VPETMapActions
    {
        private @Inputs m_Wrapper;
        public VPETMapActions(@Inputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Navigate => m_Wrapper.m_VPETMap_Navigate;
        public InputAction @MiddleClick => m_Wrapper.m_VPETMap_MiddleClick;
        public InputAction @RightClick => m_Wrapper.m_VPETMap_RightClick;
        public InputAction @Position => m_Wrapper.m_VPETMap_Position;
        public InputAction @Click => m_Wrapper.m_VPETMap_Click;
        public InputAction @Tap => m_Wrapper.m_VPETMap_Tap;
        public InputAction @Look => m_Wrapper.m_VPETMap_Look;
        public InputAction @OrbitClick => m_Wrapper.m_VPETMap_OrbitClick;
        public InputAction @DragClick => m_Wrapper.m_VPETMap_DragClick;
        public InputAction @ZoomWheel => m_Wrapper.m_VPETMap_ZoomWheel;
        public InputActionMap Get() { return m_Wrapper.m_VPETMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(VPETMapActions set) { return set.Get(); }
        public void SetCallbacks(IVPETMapActions instance)
        {
            if (m_Wrapper.m_VPETMapActionsCallbackInterface != null)
            {
                @Navigate.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnNavigate;
                @Navigate.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnNavigate;
                @Navigate.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnNavigate;
                @MiddleClick.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnMiddleClick;
                @RightClick.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnRightClick;
                @RightClick.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnRightClick;
                @RightClick.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnRightClick;
                @Position.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnPosition;
                @Position.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnPosition;
                @Position.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnPosition;
                @Click.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnClick;
                @Tap.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnTap;
                @Tap.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnTap;
                @Tap.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnTap;
                @Look.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnLook;
                @OrbitClick.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnOrbitClick;
                @OrbitClick.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnOrbitClick;
                @OrbitClick.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnOrbitClick;
                @DragClick.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnDragClick;
                @DragClick.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnDragClick;
                @DragClick.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnDragClick;
                @ZoomWheel.started -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnZoomWheel;
                @ZoomWheel.performed -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnZoomWheel;
                @ZoomWheel.canceled -= m_Wrapper.m_VPETMapActionsCallbackInterface.OnZoomWheel;
            }
            m_Wrapper.m_VPETMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Navigate.started += instance.OnNavigate;
                @Navigate.performed += instance.OnNavigate;
                @Navigate.canceled += instance.OnNavigate;
                @MiddleClick.started += instance.OnMiddleClick;
                @MiddleClick.performed += instance.OnMiddleClick;
                @MiddleClick.canceled += instance.OnMiddleClick;
                @RightClick.started += instance.OnRightClick;
                @RightClick.performed += instance.OnRightClick;
                @RightClick.canceled += instance.OnRightClick;
                @Position.started += instance.OnPosition;
                @Position.performed += instance.OnPosition;
                @Position.canceled += instance.OnPosition;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @Tap.started += instance.OnTap;
                @Tap.performed += instance.OnTap;
                @Tap.canceled += instance.OnTap;
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @OrbitClick.started += instance.OnOrbitClick;
                @OrbitClick.performed += instance.OnOrbitClick;
                @OrbitClick.canceled += instance.OnOrbitClick;
                @DragClick.started += instance.OnDragClick;
                @DragClick.performed += instance.OnDragClick;
                @DragClick.canceled += instance.OnDragClick;
                @ZoomWheel.started += instance.OnZoomWheel;
                @ZoomWheel.performed += instance.OnZoomWheel;
                @ZoomWheel.canceled += instance.OnZoomWheel;
            }
        }
    }
    public VPETMapActions @VPETMap => new VPETMapActions(this);
    public interface IVPETMapActions
    {
        void OnNavigate(InputAction.CallbackContext context);
        void OnMiddleClick(InputAction.CallbackContext context);
        void OnRightClick(InputAction.CallbackContext context);
        void OnPosition(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
        void OnTap(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnOrbitClick(InputAction.CallbackContext context);
        void OnDragClick(InputAction.CallbackContext context);
        void OnZoomWheel(InputAction.CallbackContext context);
    }
}

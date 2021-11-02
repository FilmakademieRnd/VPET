// GENERATED AUTOMATICALLY FROM 'Assets/VPET/Core/Managers/InputManager/Inputs.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Inputs : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Inputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Inputs"",
    ""maps"": [
        {
            ""name"": ""Map"",
            ""id"": ""798cb63f-9c9c-4fd7-a89f-1451c7084aa4"",
            ""actions"": [
                {
                    ""name"": ""TouchInput"",
                    ""type"": ""PassThrough"",
                    ""id"": ""92822953-cf9d-498c-b915-ee4322205c40"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TouchPress"",
                    ""type"": ""Button"",
                    ""id"": ""25011a82-3a0a-4c38-94a8-4fed0e6d9f28"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TouchPosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""bff22165-01c6-4e4c-af37-f3703b288aae"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MousePress"",
                    ""type"": ""Button"",
                    ""id"": ""ce2ea5ae-2ae2-4e56-81e3-e02ffac7fe5e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""f007b5c1-c042-4f3e-9988-2e9d240c34a6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""39fcc17a-55fb-4ba4-9d1e-54760c46ff76"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchInput"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e43ffaa6-36d2-433a-a589-2c13dc21b59c"",
                    ""path"": ""<Touchscreen>/primaryTouch/press"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchPress"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""afa5801b-dfeb-4de6-a229-ad6fde62a390"",
                    ""path"": ""<Touchscreen>/primaryTouch/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4d4b32e5-6458-4d15-97ca-f40e5e5a880d"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePress"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e07aad17-2b11-449c-bdfe-1366acea0938"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""tonioMap"",
            ""id"": ""8cc45c96-8744-4e65-a250-74d6bef146fe"",
            ""actions"": [
                {
                    ""name"": ""Navigate"",
                    ""type"": ""Value"",
                    ""id"": ""b48a59d7-3b11-4d2b-afe3-1044fd9f54c6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Submit"",
                    ""type"": ""Button"",
                    ""id"": ""72d4c606-0968-4616-bcc1-4ad463d0fde7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""df10f81d-a22c-4340-a842-99d5ef9ab4cb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Point"",
                    ""type"": ""PassThrough"",
                    ""id"": ""6ce499a9-b30f-4b6b-b826-af44eabb983f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""PassThrough"",
                    ""id"": ""39e9fa23-d5d0-4a24-880d-340c9cc20808"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ScrollWheel"",
                    ""type"": ""PassThrough"",
                    ""id"": ""98f0d282-f6bb-45fd-96d3-8bd85aa5aed1"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MiddleClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""2849e90e-27c2-40fc-a527-5d8dc8c17120"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""PassThrough"",
                    ""id"": ""aaa7ebd1-bb7d-4e74-95f8-3ed939d14844"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TrackedDevicePosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""e15f11d5-585c-4e90-a02b-fa16435a41b6"",
                    ""expectedControlType"": ""Vector3"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TrackedDeviceOrientation"",
                    ""type"": ""PassThrough"",
                    ""id"": ""bdb52728-6ac9-4d37-8f36-50d3e26f6f9c"",
                    ""expectedControlType"": ""Quaternion"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Drag"",
                    ""type"": ""Button"",
                    ""id"": ""200863e5-f743-4e9b-a495-b099c194b10a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""f19f6a66-b296-4479-aae7-dbc6a2d6e4f3"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drag"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
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
                    ""name"": """",
                    ""id"": ""04652379-ec84-4c83-bf9c-d8691466e2ad"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""ScrollWheel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9b312dd7-857e-45aa-ab64-acb5ac7aa135"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c89c4db1-d070-438f-9759-3024e93a2522"",
                    ""path"": ""<Pen>/tip"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""11fd20d5-1927-4d81-9769-d4a1c5dabfa4"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7004ff55-8516-4f66-999d-87243e7d1234"",
                    ""path"": ""<Pen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard&Mouse"",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9e421463-3fb5-439c-a8b4-d068f7b6fabd"",
                    ""path"": ""*/{Cancel}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9337eb41-3879-401e-abbe-7d4c6c6e85c9"",
                    ""path"": ""*/{Submit}"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Submit"",
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
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Map
        m_Map = asset.FindActionMap("Map", throwIfNotFound: true);
        m_Map_TouchInput = m_Map.FindAction("TouchInput", throwIfNotFound: true);
        m_Map_TouchPress = m_Map.FindAction("TouchPress", throwIfNotFound: true);
        m_Map_TouchPosition = m_Map.FindAction("TouchPosition", throwIfNotFound: true);
        m_Map_MousePress = m_Map.FindAction("MousePress", throwIfNotFound: true);
        m_Map_MousePosition = m_Map.FindAction("MousePosition", throwIfNotFound: true);
        // tonioMap
        m_tonioMap = asset.FindActionMap("tonioMap", throwIfNotFound: true);
        m_tonioMap_Navigate = m_tonioMap.FindAction("Navigate", throwIfNotFound: true);
        m_tonioMap_Submit = m_tonioMap.FindAction("Submit", throwIfNotFound: true);
        m_tonioMap_Cancel = m_tonioMap.FindAction("Cancel", throwIfNotFound: true);
        m_tonioMap_Point = m_tonioMap.FindAction("Point", throwIfNotFound: true);
        m_tonioMap_Click = m_tonioMap.FindAction("Click", throwIfNotFound: true);
        m_tonioMap_ScrollWheel = m_tonioMap.FindAction("ScrollWheel", throwIfNotFound: true);
        m_tonioMap_MiddleClick = m_tonioMap.FindAction("MiddleClick", throwIfNotFound: true);
        m_tonioMap_RightClick = m_tonioMap.FindAction("RightClick", throwIfNotFound: true);
        m_tonioMap_TrackedDevicePosition = m_tonioMap.FindAction("TrackedDevicePosition", throwIfNotFound: true);
        m_tonioMap_TrackedDeviceOrientation = m_tonioMap.FindAction("TrackedDeviceOrientation", throwIfNotFound: true);
        m_tonioMap_Drag = m_tonioMap.FindAction("Drag", throwIfNotFound: true);
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

    // Map
    private readonly InputActionMap m_Map;
    private IMapActions m_MapActionsCallbackInterface;
    private readonly InputAction m_Map_TouchInput;
    private readonly InputAction m_Map_TouchPress;
    private readonly InputAction m_Map_TouchPosition;
    private readonly InputAction m_Map_MousePress;
    private readonly InputAction m_Map_MousePosition;
    public struct MapActions
    {
        private @Inputs m_Wrapper;
        public MapActions(@Inputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @TouchInput => m_Wrapper.m_Map_TouchInput;
        public InputAction @TouchPress => m_Wrapper.m_Map_TouchPress;
        public InputAction @TouchPosition => m_Wrapper.m_Map_TouchPosition;
        public InputAction @MousePress => m_Wrapper.m_Map_MousePress;
        public InputAction @MousePosition => m_Wrapper.m_Map_MousePosition;
        public InputActionMap Get() { return m_Wrapper.m_Map; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MapActions set) { return set.Get(); }
        public void SetCallbacks(IMapActions instance)
        {
            if (m_Wrapper.m_MapActionsCallbackInterface != null)
            {
                @TouchInput.started -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchInput;
                @TouchInput.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchInput;
                @TouchInput.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchInput;
                @TouchPress.started -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchPress;
                @TouchPress.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchPress;
                @TouchPress.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchPress;
                @TouchPosition.started -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchPosition;
                @TouchPosition.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchPosition;
                @TouchPosition.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnTouchPosition;
                @MousePress.started -= m_Wrapper.m_MapActionsCallbackInterface.OnMousePress;
                @MousePress.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnMousePress;
                @MousePress.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnMousePress;
                @MousePosition.started -= m_Wrapper.m_MapActionsCallbackInterface.OnMousePosition;
                @MousePosition.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnMousePosition;
                @MousePosition.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnMousePosition;
            }
            m_Wrapper.m_MapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @TouchInput.started += instance.OnTouchInput;
                @TouchInput.performed += instance.OnTouchInput;
                @TouchInput.canceled += instance.OnTouchInput;
                @TouchPress.started += instance.OnTouchPress;
                @TouchPress.performed += instance.OnTouchPress;
                @TouchPress.canceled += instance.OnTouchPress;
                @TouchPosition.started += instance.OnTouchPosition;
                @TouchPosition.performed += instance.OnTouchPosition;
                @TouchPosition.canceled += instance.OnTouchPosition;
                @MousePress.started += instance.OnMousePress;
                @MousePress.performed += instance.OnMousePress;
                @MousePress.canceled += instance.OnMousePress;
                @MousePosition.started += instance.OnMousePosition;
                @MousePosition.performed += instance.OnMousePosition;
                @MousePosition.canceled += instance.OnMousePosition;
            }
        }
    }
    public MapActions @Map => new MapActions(this);

    // tonioMap
    private readonly InputActionMap m_tonioMap;
    private ITonioMapActions m_TonioMapActionsCallbackInterface;
    private readonly InputAction m_tonioMap_Navigate;
    private readonly InputAction m_tonioMap_Submit;
    private readonly InputAction m_tonioMap_Cancel;
    private readonly InputAction m_tonioMap_Point;
    private readonly InputAction m_tonioMap_Click;
    private readonly InputAction m_tonioMap_ScrollWheel;
    private readonly InputAction m_tonioMap_MiddleClick;
    private readonly InputAction m_tonioMap_RightClick;
    private readonly InputAction m_tonioMap_TrackedDevicePosition;
    private readonly InputAction m_tonioMap_TrackedDeviceOrientation;
    private readonly InputAction m_tonioMap_Drag;
    public struct TonioMapActions
    {
        private @Inputs m_Wrapper;
        public TonioMapActions(@Inputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Navigate => m_Wrapper.m_tonioMap_Navigate;
        public InputAction @Submit => m_Wrapper.m_tonioMap_Submit;
        public InputAction @Cancel => m_Wrapper.m_tonioMap_Cancel;
        public InputAction @Point => m_Wrapper.m_tonioMap_Point;
        public InputAction @Click => m_Wrapper.m_tonioMap_Click;
        public InputAction @ScrollWheel => m_Wrapper.m_tonioMap_ScrollWheel;
        public InputAction @MiddleClick => m_Wrapper.m_tonioMap_MiddleClick;
        public InputAction @RightClick => m_Wrapper.m_tonioMap_RightClick;
        public InputAction @TrackedDevicePosition => m_Wrapper.m_tonioMap_TrackedDevicePosition;
        public InputAction @TrackedDeviceOrientation => m_Wrapper.m_tonioMap_TrackedDeviceOrientation;
        public InputAction @Drag => m_Wrapper.m_tonioMap_Drag;
        public InputActionMap Get() { return m_Wrapper.m_tonioMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TonioMapActions set) { return set.Get(); }
        public void SetCallbacks(ITonioMapActions instance)
        {
            if (m_Wrapper.m_TonioMapActionsCallbackInterface != null)
            {
                @Navigate.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnNavigate;
                @Navigate.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnNavigate;
                @Navigate.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnNavigate;
                @Submit.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnSubmit;
                @Submit.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnSubmit;
                @Submit.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnSubmit;
                @Cancel.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnCancel;
                @Point.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnPoint;
                @Point.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnPoint;
                @Point.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnPoint;
                @Click.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnClick;
                @ScrollWheel.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnScrollWheel;
                @ScrollWheel.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnScrollWheel;
                @ScrollWheel.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnScrollWheel;
                @MiddleClick.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnMiddleClick;
                @MiddleClick.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnMiddleClick;
                @RightClick.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnRightClick;
                @RightClick.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnRightClick;
                @RightClick.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnRightClick;
                @TrackedDevicePosition.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDevicePosition.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDevicePosition.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnTrackedDevicePosition;
                @TrackedDeviceOrientation.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnTrackedDeviceOrientation;
                @Drag.started -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnDrag;
                @Drag.performed -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnDrag;
                @Drag.canceled -= m_Wrapper.m_TonioMapActionsCallbackInterface.OnDrag;
            }
            m_Wrapper.m_TonioMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Navigate.started += instance.OnNavigate;
                @Navigate.performed += instance.OnNavigate;
                @Navigate.canceled += instance.OnNavigate;
                @Submit.started += instance.OnSubmit;
                @Submit.performed += instance.OnSubmit;
                @Submit.canceled += instance.OnSubmit;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
                @Point.started += instance.OnPoint;
                @Point.performed += instance.OnPoint;
                @Point.canceled += instance.OnPoint;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @ScrollWheel.started += instance.OnScrollWheel;
                @ScrollWheel.performed += instance.OnScrollWheel;
                @ScrollWheel.canceled += instance.OnScrollWheel;
                @MiddleClick.started += instance.OnMiddleClick;
                @MiddleClick.performed += instance.OnMiddleClick;
                @MiddleClick.canceled += instance.OnMiddleClick;
                @RightClick.started += instance.OnRightClick;
                @RightClick.performed += instance.OnRightClick;
                @RightClick.canceled += instance.OnRightClick;
                @TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
                @TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
                @TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
                @TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
                @TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
                @Drag.started += instance.OnDrag;
                @Drag.performed += instance.OnDrag;
                @Drag.canceled += instance.OnDrag;
            }
        }
    }
    public TonioMapActions @tonioMap => new TonioMapActions(this);
    public interface IMapActions
    {
        void OnTouchInput(InputAction.CallbackContext context);
        void OnTouchPress(InputAction.CallbackContext context);
        void OnTouchPosition(InputAction.CallbackContext context);
        void OnMousePress(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
    }
    public interface ITonioMapActions
    {
        void OnNavigate(InputAction.CallbackContext context);
        void OnSubmit(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnPoint(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
        void OnScrollWheel(InputAction.CallbackContext context);
        void OnMiddleClick(InputAction.CallbackContext context);
        void OnRightClick(InputAction.CallbackContext context);
        void OnTrackedDevicePosition(InputAction.CallbackContext context);
        void OnTrackedDeviceOrientation(InputAction.CallbackContext context);
        void OnDrag(InputAction.CallbackContext context);
    }
}

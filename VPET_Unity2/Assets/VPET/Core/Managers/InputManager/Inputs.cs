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
                    ""expectedControlType"": """",
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
                    ""path"": ""<Touchscreen>/primaryTouch"",
                    ""interactions"": """",
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
    public interface IMapActions
    {
        void OnTouchInput(InputAction.CallbackContext context);
        void OnTouchPress(InputAction.CallbackContext context);
        void OnTouchPosition(InputAction.CallbackContext context);
        void OnMousePress(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
    }
}

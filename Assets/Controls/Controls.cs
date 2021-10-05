// GENERATED AUTOMATICALLY FROM 'Assets/Controls/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Draw"",
            ""id"": ""f9aa2cf3-06ec-4607-b929-45637825afca"",
            ""actions"": [
                {
                    ""name"": ""Draw"",
                    ""type"": ""Button"",
                    ""id"": ""377b5516-9a38-4583-ba61-4721447b5c82"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""81509a06-2665-472e-98c3-ab3b71539da3"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Draw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard and Mouse"",
            ""bindingGroup"": ""Keyboard and Mouse"",
            ""devices"": []
        },
        {
            ""name"": ""Tablet and Mouse"",
            ""bindingGroup"": ""Tablet and Mouse"",
            ""devices"": []
        }
    ]
}");
        // Draw
        m_Draw = asset.FindActionMap("Draw", throwIfNotFound: true);
        m_Draw_Draw = m_Draw.FindAction("Draw", throwIfNotFound: true);
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

    // Draw
    private readonly InputActionMap m_Draw;
    private IDrawActions m_DrawActionsCallbackInterface;
    private readonly InputAction m_Draw_Draw;
    public struct DrawActions
    {
        private @Controls m_Wrapper;
        public DrawActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Draw => m_Wrapper.m_Draw_Draw;
        public InputActionMap Get() { return m_Wrapper.m_Draw; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DrawActions set) { return set.Get(); }
        public void SetCallbacks(IDrawActions instance)
        {
            if (m_Wrapper.m_DrawActionsCallbackInterface != null)
            {
                @Draw.started -= m_Wrapper.m_DrawActionsCallbackInterface.OnDraw;
                @Draw.performed -= m_Wrapper.m_DrawActionsCallbackInterface.OnDraw;
                @Draw.canceled -= m_Wrapper.m_DrawActionsCallbackInterface.OnDraw;
            }
            m_Wrapper.m_DrawActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Draw.started += instance.OnDraw;
                @Draw.performed += instance.OnDraw;
                @Draw.canceled += instance.OnDraw;
            }
        }
    }
    public DrawActions @Draw => new DrawActions(this);
    private int m_KeyboardandMouseSchemeIndex = -1;
    public InputControlScheme KeyboardandMouseScheme
    {
        get
        {
            if (m_KeyboardandMouseSchemeIndex == -1) m_KeyboardandMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard and Mouse");
            return asset.controlSchemes[m_KeyboardandMouseSchemeIndex];
        }
    }
    private int m_TabletandMouseSchemeIndex = -1;
    public InputControlScheme TabletandMouseScheme
    {
        get
        {
            if (m_TabletandMouseSchemeIndex == -1) m_TabletandMouseSchemeIndex = asset.FindControlSchemeIndex("Tablet and Mouse");
            return asset.controlSchemes[m_TabletandMouseSchemeIndex];
        }
    }
    public interface IDrawActions
    {
        void OnDraw(InputAction.CallbackContext context);
    }
}

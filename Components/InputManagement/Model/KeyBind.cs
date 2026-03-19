using Damntry.Utils.ExtensionMethods;
using System;
using UnityEngine;

namespace Damntry.UtilsUnity.Components.InputManagement.Model {

    public class KeyBind : IEquatable<KeyBind> {

        private enum KeyCodeType {
            KeyStroke,
            MouseButton,
            MouseScroll
        }

        public KeyCode KeyCode { get; init; }
        public KeyCode[] Modifiers { get; init; }

        internal ModifierMask ModifiersMask { get; init; }

        private KeyCodeType keyCodeType;


        public bool IsSuppressedKeyCode => KeyCode == KeyCode.None;
        public bool IsMouseButtonKeyCode => KeyCode >= KeyCode.Mouse0 && KeyCode <= KeyCode.Mouse6;
        public bool IsMouseScrollKeyCode => KeyCode == KeyCode.WheelDown || KeyCode == KeyCode.WheelUp;

        
        public KeyBind(KeyCode keyCode, KeyCode[] modifiers){
            KeyCode = keyCode;
            SetKeyCodeType();

            Modifiers = modifiers ?? [];
            ModifiersMask = InputDetection.KeyCodeModifiersToMask(Modifiers);
        }

        public void SetKeyCodeType() {
            if (IsMouseButtonKeyCode) {
                keyCodeType = KeyCodeType.MouseButton;
            } else if (IsMouseScrollKeyCode) {
                keyCodeType = KeyCodeType.MouseScroll;
            } else {
                keyCodeType = KeyCodeType.KeyStroke;
            }
        }

        public virtual bool IsTriggered(InputState triggerState, ModifierMask pressedModifMask, bool ignoreModifiers) {
            if (IsSuppressedKeyCode) {
                return false;
            }

            bool isModifiersCheckPassed = ignoreModifiers || IsModifierEqual(pressedModifMask);

            if (keyCodeType == KeyCodeType.KeyStroke) {
                return isModifiersCheckPassed && IsKeyStrokeTriggered(triggerState);
            } else if (keyCodeType == KeyCodeType.MouseButton) {
                int mouseButtonIndex = (int)KeyCode - (int)KeyCode.Mouse0;
                return isModifiersCheckPassed && IsMouseButtonTriggered(triggerState, mouseButtonIndex);
            } else if (keyCodeType == KeyCodeType.MouseScroll) {
                float scrollValue = KeyCode == KeyCode.WheelDown ? -1f : 1f;
                return isModifiersCheckPassed && Input.GetAxisRaw("Mouse ScrollWheel") == scrollValue;
            }

            throw new InvalidOperationException($"Invalid type {keyCodeType}");
        }

        /// <summary>
        /// Compares our mask of accepted modifiers with the one of currently pressed modifiers.
        /// </summary>
        public bool IsModifierEqual(ModifierMask pressedModifMask) =>
            ModifiersMask == pressedModifMask;

        private bool IsKeyStrokeTriggered(InputState triggerState) =>
            triggerState switch {
                InputState.KeyDown => Input.anyKeyDown && Input.GetKeyDown(KeyCode),
                //KeyUp doesnt have any related method in Input, so we need to check it directly.
                InputState.KeyUp => Input.GetKeyUp(KeyCode),
                InputState.KeyHeld => Input.anyKey && Input.GetKey(KeyCode),
                _ => throw new NotImplementedException(triggerState.GetDescription())
            };

        protected bool IsMouseButtonTriggered(InputState triggerState, int mouseIndex) =>
            triggerState switch {
                InputState.KeyDown => Input.GetMouseButtonDown(mouseIndex),
                InputState.KeyUp => Input.GetMouseButtonUp(mouseIndex),
                InputState.KeyHeld => Input.GetMouseButton(mouseIndex),
                _ => throw new NotImplementedException(triggerState.GetDescription()),
            };

        public override bool Equals(object other) => 
            this == (KeyBind)other;

        public bool Equals(KeyBind other) => 
            this == other;
            
        public static bool operator !=(KeyBind key1, KeyBind key2) => 
            !(key1 == key2);

        public static bool operator ==(KeyBind key1, KeyBind key2) {
            if (key1 is null) {
                return false;
            }
            if (key2 is null) {
                return false;
            }
            return ReferenceEquals(key1, key2) || 
                key1.KeyCode == key2.KeyCode && key1.ModifiersMask == key2.ModifiersMask;
        }

        public override int GetHashCode() => HashCode.Combine(KeyCode, ModifiersMask);

        public override string ToString() => ToStringKeyCodes();

        public string ToStringKeyCodes() =>
            KeyCodeUtils.KeyCombinationToString(useLiterals: false, KeyCode, modifiers: Modifiers);

    }

}

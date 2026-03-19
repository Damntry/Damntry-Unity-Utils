using System;
using UnityEngine;
using static Damntry.UtilsUnity.Components.InputManagement.InputDetection;

namespace Damntry.UtilsUnity.Components.InputManagement.Model {

    public class KeyBindContext : KeyBindState, IEquatable<KeyBindContext> {

        public HotkeyContext HotkeyContext { get; private set; }

        public KeyBindContext(KeyCode keyCode, KeyCode[] modifiers,
                InputState inputState, HotkeyContext hotkeyCtx) : base(keyCode, modifiers, inputState) {

            HotkeyContext = hotkeyCtx;
        }


        public bool IsTriggeredWithContext(ModifierMask pressedModifMask, bool ignoreModifiers) {
            if (IsSuppressedKeyCode) {
                return false;
            }

            if (HotkeyContext.ActivationCondition != null && !HotkeyContext.ActivationCondition()) {
                return false;
            }

            return base.IsTriggered(pressedModifMask, ignoreModifiers);
        }


        public override bool Equals(object other) =>
            this == (KeyBindContext)other;

        public bool Equals(KeyBindContext other) =>
            this == other;

        public static bool operator !=(KeyBindContext key1, KeyBindContext key2) =>
            !(key1 == key2);

        public static bool operator ==(KeyBindContext key1, KeyBindContext key2) {
            if (key1 is null) {
                return false;
            }
            if (key2 is null) {
                return false;
            }
            return ReferenceEquals(key1, key2) ||
                 (KeyBindState)key1 == (KeyBindState)key2 && key1.HotkeyContext == key2.HotkeyContext;
        }


        public bool EqualKeycodesAndContext(KeyBindContext otherKeyBindContext) {
            //Compare keys and context while ignoring InputState.
            return ((KeyBind)this) == (KeyBind)otherKeyBindContext && HotkeyContext == otherKeyBindContext.HotkeyContext;
        }


        public override int GetHashCode() => 
            HashCode.Combine(KeyCode, ModifiersMask, TriggerInputState, HotkeyContext);
            

        public override string ToString() => base.ToString() + $" - Context: {HotkeyContext}";

        public string ToStringKeyCodesAndContext() => ((KeyBind)this).ToString() + $" - Context: {HotkeyContext}";

    }

    public class HotkeyContext(string Name, Func<bool> activationCondition, int priority) :
            IEquatable<HotkeyContext> {
        /// <summary>
        /// Name of the context. Must be an unique value since it ll be used for comparison with other HotkeyContext,
        /// </summary>
        public string Name { get; private set; } = Name;
        /// <summary>
        /// Function that returns true if the context is currently active, so the hotkeys assigned to 
        /// it can trigger. If null, its the same as if it always returned true.
        /// WHEN SET, THIS CONDITION METHOD IS POTENTIALLY TRIGGERED EVERY FRAME!! Make sure to have very light logic in it.
        /// </summary>
        public Func<bool> ActivationCondition { get; private set; } = activationCondition;

        public int Priority { get; private set; } = priority;


        public override bool Equals(object other) =>
            this == (HotkeyContext)other;

        public bool Equals(HotkeyContext other) =>
            this == other;

        public static bool operator !=(HotkeyContext key1, HotkeyContext key2) =>
            !(key1 == key2);

        public static bool operator ==(HotkeyContext key1, HotkeyContext key2) {
            if (key1 is null) {
                return false;
            }
            if (key2 is null) {
                return false;
            }
            return ReferenceEquals(key1, key2) || key1.Name == key2.Name;
        }

        /// <summary>Taken from https://stackoverflow.com/a/263416/739345 by Jon Skeet</summary>
        public override int GetHashCode() {
            unchecked { // Overflow is fine, just wrap
                int hash = 83;

                return hash * 3623 + Name.GetHashCode();
            }
        }

        public override string ToString() => $"{Name} ({Priority})";

    }

}

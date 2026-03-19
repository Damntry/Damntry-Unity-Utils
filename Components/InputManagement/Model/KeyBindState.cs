using System;
using UnityEngine;

namespace Damntry.UtilsUnity.Components.InputManagement.Model {

    public class KeyBindState : KeyBind, IEquatable<KeyBindState> {

        public InputState TriggerInputState { get; private set; }


        public KeyBindState(KeyCode keyCode, KeyCode[] modifiers, InputState inputState) :
                base(keyCode, modifiers) {

            TriggerInputState = inputState;
        }

        public bool IsTriggered(ModifierMask pressedModifMask, bool ignoreModifiers) {
            return base.IsTriggered(TriggerInputState, pressedModifMask, ignoreModifiers);
        }


        public override bool Equals(object other) =>
            this == (KeyBindState)other;

        public bool Equals(KeyBindState other) =>
            this == other;

        public static bool operator !=(KeyBindState key1, KeyBindState key2) =>
            !(key1 == key2);

        public static bool operator ==(KeyBindState key1, KeyBindState key2) {
            if (key1 is null) {
                return false;
            }
            if (key2 is null) {
                return false;
            }

            return ReferenceEquals(key1, key2) ||
                (KeyBind)key1 == (KeyBind)key2 && key1.TriggerInputState == key2.TriggerInputState;
        }


        public override int GetHashCode() => HashCode.Combine(KeyCode, ModifiersMask, TriggerInputState);

        public override string ToString() => base.ToString() + $" ({TriggerInputState})";

    }

}

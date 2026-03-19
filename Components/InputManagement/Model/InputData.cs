using Damntry.Utils.Logging;
using System;
using UnityEngine;

namespace Damntry.UtilsUnity.Components.InputManagement.Model {

    public class InputData {

        private readonly double cooldownSeconds;

        private double lastKeyPressTime;

        public string HotkeyName { get; private set; }

        public string GroupName { get; private set; }

        public Action Action { get; private set; }

        public KeyBindContext KeyContext { get; private set; }


        public bool IsFromGroup => !string.IsNullOrEmpty(GroupName);


        public InputData(string hotkeyName, KeyBindContext keyContext, 
                Action action, int cooldownMillis, string groupName) {

            HotkeyName = hotkeyName;
            KeyContext = keyContext;
            Action = action ?? throw new ArgumentNullException($"The {nameof(action)} call cant be null.");
            cooldownSeconds = cooldownSeconds > 0 ? cooldownMillis / 1000d : -1;
            GroupName = groupName;

            lastKeyPressTime = double.MinValue;
        }


        internal void ChangeValidatedKeyCodes(KeyCode newKeyCode, KeyCode[] modifiers) {

            KeyContext = new KeyBindContext(newKeyCode, modifiers, 
                KeyContext.TriggerInputState, KeyContext.HotkeyContext);
        }

        internal bool IsModifierCompatible(ModifierMask modifierMask) =>
            //The modifiers, if any, in this inputData mask, must not have modifiers that are not in the modifierMask param.
            (KeyContext.ModifiersMask & ~modifierMask) == 0;

        internal bool IsNotInCooldown() =>
            cooldownSeconds <= 0 || lastKeyPressTime + cooldownSeconds < Time.timeAsDouble;

        internal bool IsContextActive() => KeyContext.HotkeyContext.ActivationCondition?.Invoke() == true;

        internal void PerformHotkeyAction() {
            lastKeyPressTime = Time.timeAsDouble;

            try {
                Action();
            } catch (Exception ex) {
                TimeLogger.Logger.LogExceptionWithMessage("Exception while trying to execute Action for " +
                    $"hotkey '{HotkeyName}' and input combination: {KeyContext}", ex, LogCategories.KeyMouse);
            }
            
        }

    }
}

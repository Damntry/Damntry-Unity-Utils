using Damntry.Utils.ExtensionMethods;
using Damntry.Utils.Logging;
using Damntry.UtilsUnity.Components.InputManagement.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Damntry.UtilsUnity.Components.InputManagement {

	public enum InputState {
		/// <summary>Triggers when the key is pressed for the first time.</summary>
		KeyDown = 0,
		/// <summary>Triggers when the key is released.</summary>
		KeyUp = 1,
		/// <summary>Triggers constantly while the key is pressed.</summary>
		KeyHeld = 2,
	}

    [Flags]
    public enum ModifierMask {
        None = 0,
        LeftControl = 1,
        RightControl = 2,
        LeftShift = 4,
        RightShift = 8,
        LeftAlt = 16,
        RightAlt = 32,
        AltGr = 64,
        LeftWindows = 128,
        RightWindows = 256
    }

    /// <summary>
    /// What to do when the introduced key code is not valid for any reason.
    /// The OnValidationError event will always be raised no matter the choice.
    /// </summary>
    public enum OnValidationError {
        /// <summary>Keeps the previous keycode value.</summary>
        Rollback,
        /// <summary>Keeps the Hotkey bind, but clears the keycode to 'KeyCode.None'.</summary>
        ClearKeycodes
    }

    public enum HotkeyAddResult {
        Sucess,
        AddedWithFailure,
        Failure,
    }

    /// <summary>
    /// Class for keyboard and mouse input detection. 
    /// Each input is saved with a hotkey name and all its associated data.
    /// If no hotkey is registered, this class remains fully inactive.
    /// Multiple hotkeys can share the same key + modifiers combination, as long as 
    /// each hotkey has different <see cref="InputState"/> values.
    /// When a hotkey conflict is detected, the event <see cref="OnHotkeyValidationError"/>
    /// is called with the error message.
    /// </summary>
    public class InputDetection {

        //TODO 3 - Updating hotkeys is annoying, since I dont know if they exist or not or what.
        //  I need to make a method here like the one in InputBepInEx.ChangeConfigHotkey, so I
        //  dont have to deal with it.

        public static InputDetection Instance => 
            _instance ??= new InputDetection(typeof(InputDetection), OnValidationError.Rollback, false);

        private static InputDetection _instance;


        private static Type typeBinding;

        public event Action<string, string, KeyBind> OnHotkeyValidationError;

        public delegate void HotkeyValidationErrorDel(string hotkeyName, string errorMessage);


        protected virtual void HotkeyValidationError(string hotkeyName, string message, KeyBind keyBind) {
            OnHotkeyValidationError?.Invoke(hotkeyName, message, keyBind);
        }


        private static readonly KeyCode[] SupportedModifiersList = [
            KeyCode.LeftControl,
            KeyCode.RightControl,
            KeyCode.LeftShift,
            KeyCode.RightShift,
            KeyCode.LeftAlt,
            KeyCode.RightAlt,
            KeyCode.AltGr,
            KeyCode.LeftWindows,
            KeyCode.RightWindows,
        ];

        /// <summary>
        /// Minimum cooldown time between keypresses when not specified for the hotkey.
        /// </summary>
        protected const int DefaultKeyPressCooldown = 65;

        private static IgnoreModifierComparer comparer;

        protected Dictionary<string, InputData> KeyPressActions { get; private set; }

        private IEnumerable<IGrouping<KeyBindState, InputData>> groupedKeysCache;
        

        public Dictionary<string, List<string>> NamedGroupsCache { get; private set; }
        
        private List<KeyBind> _restrictedKeyBinds;
        /// <summary>
        /// List of restricted keybinds. This list is only checked when adding or changing keybinds.
        /// If a new restricted entry is added and the keybind already existed, it will continue to work.
        /// </summary>
        public List<KeyBind> RestrictedKeyBinds { 
            get => _restrictedKeyBinds; 
            set => _restrictedKeyBinds = value ?? [];
        }

        private bool restrictModifiers;

        /// <summary>
        /// Function called before checking if any of the hotkeys are pressed.
        /// If it returns true, any pressed hotkeys will be processed. 
        /// Otherwise input is ignored for the current frame.
        /// </summary>
        protected Func<bool> PreKeyCheckFunc { get; private set; }

        /// <summary>
        /// Function executed to get the MonoBehaviour object to attach to.
        /// </summary>
        protected Func<MonoBehaviour> GetBehaviourAttachFunc;

        public OnValidationError OnValidationError { get; set; }


        /// <param name="callingKeypressClass">Type of the calling (derived) class</param>
        /// <param name="restrictModifiers">
        /// True to restrict the use of modifiers as a non modifier key (Ex: "Ctrl + Shift", "Alt", etc..).
        /// </param>
        /// <param name="restrictedKeyBinds">List of keybinds to disallow.</param>
        /// <remarks>Restricted keybinds only affect the specific Modifiers + Keypress combination and no other. 
        /// If a modifier is restricted, keys that include the modifier will still work, but the modifier itself 
        /// as the Keypress will not.</remarks>
        protected InputDetection(Type callingKeypressClass, OnValidationError onValidationError,
                bool restrictModifiers, params List<KeyBind> restrictedKeyBinds) {

            InitializeInputSingleton(callingKeypressClass);

            KeyPressActions = new();
            NamedGroupsCache = new();

            comparer = new IgnoreModifierComparer();

            OnValidationError = onValidationError;

            RestrictedKeyBinds = restrictedKeyBinds;

            //Remove entries with an empty KeyCode
            if (RestrictedKeyBinds.RemoveAll(k => k.KeyCode == KeyCode.None) > 0) {
                TimeLogger.Logger.LogWarning($"The {nameof(restrictedKeyBinds)} parameter contained " +
                    $"keybinds with a 'None' KeyCode and they were removed. Restricting modifiers from being " +
                    $"used in any combination is not supported, you can ony restrict specific Key + Modifier " +
                    $"combinations, or using the modifier by itself. If you only wanted to remove the modifier " +
                    $"as a whole keybind, set it in the {nameof(KeyBind)}.{nameof(KeyBind.KeyCode)}.", 
                    LogCategories.KeyMouse);
            }

            this.restrictModifiers = restrictModifiers;
        }


        protected bool HasTriggerableKeys() =>
            KeyPressActions.Where(k => !k.Value.KeyContext.IsSuppressedKeyCode).Any();

        private bool IsRestrictedKeyBind(KeyBind keyBind) =>
            RestrictedKeyBinds.Any(k => k == keyBind) || 
                restrictModifiers && SupportedModifiersList.Contains(keyBind.KeyCode);


        /// <summary>
        /// Encapsulates initialization so only a single type of keypress detection can use it.
        /// </summary>
        private void InitializeInputSingleton(Type callingKeypressClass) {
            if (callingKeypressClass == null) {
                throw new ArgumentNullException($"The type parameter cannot be null.");
            }
            if (callingKeypressClass != typeof(InputDetection) &&
                    !callingKeypressClass.IsSubclassOf(typeof(InputDetection))) {

                //This isnt really possible since the constructor is protected, but
                //  better be safe in case I change something later.
                throw new InvalidOperationException($"The type parameter must be a " +
                    $"'{nameof(InputDetection)}' or a type deriving from it.");
            }

            if (typeBinding != null) {
                if (typeBinding == callingKeypressClass) {
                    TimeLogger.Logger.LogError($"The class has already been initialized before.",
                        LogCategories.KeyMouse);
                } else {
                    throw new InvalidOperationException($"'{callingKeypressClass.Name}' is currently trying " +
                        $"to get initialized, but {typeBinding.Name} has already done so before." +
                        $"Only a single '{nameof(InputDetection)}' or derived class can be initialized.");
                }
                return;
            }

            InputMonobehaviour.Initialize(this);
            typeBinding = callingKeypressClass;
        }

        private void GenerateKeyGroupCache() {
            //Group by main Keycode and InputState).
            groupedKeysCache = KeyPressActions
                .Select(k => k.Value)
                .GroupBy(i => (KeyBindState)i.KeyContext, comparer);
        }

        /// <param name="getBehaviourAttachFunc">
        /// The function that will return the object to which the key press behaviour will attach.
        /// Can be called while the object is still null, and it will keep trying until it is initialized.
        /// </param>
        /// <param name="preKeyCheckFunc">
        /// Function called before checking if any of the hotkeys are pressed.
        /// If it returns true, any pressed hotkeys will be processed. 
        /// Otherwise input is ignored for the current frame.
        /// </param>
        public async Task InitializeAsync(Func<MonoBehaviour> getBehaviourAttachFunc, Func<bool> preKeyCheckFunc = null) {
            GetBehaviourAttachFunc = getBehaviourAttachFunc;
            PreKeyCheckFunc = preKeyCheckFunc;

            await BeginGetBehaviour();
        }

        /// <summary>
        /// Begins the periodic task to try and find the behaviour where we will attach to, using the provided function.
        /// </summary>
        protected async Task BeginGetBehaviour() {
            MonoBehaviour monoObject = null;
            while ((monoObject = GetBehaviourAttachFunc()) == null || !monoObject.gameObject.activeInHierarchy) {
                await Task.Delay(500);
            }

            AddDetectionComponent(monoObject);
        }

        private void AddDetectionComponent(MonoBehaviour unityObject) {
            if (unityObject == null) {
                throw new ArgumentNullException(nameof(unityObject));
            }
            AddDetectionComponent(unityObject.gameObject);
        }

        private void AddDetectionComponent(GameObject unityGameObject) {
            if (unityGameObject == null) {
                throw new ArgumentNullException(nameof(unityGameObject));
            }
            unityGameObject.AddComponent<InputMonobehaviour>();
        }


        public virtual bool TryAddHotkey(string hotkeyName, KeyCode keyCode, KeyCode[] modifiers,
                InputState inputState, HotkeyContext hotkeyContext, Action action) {

            return AddHotkeyInternal(hotkeyName, keyCode, modifiers, inputState, 
                    hotkeyContext, DefaultKeyPressCooldown, action, groupName: null)
                == HotkeyAddResult.Sucess;
        }

        public virtual bool TryAddHotkey(string hotkeyName, KeyCode keyCode, KeyCode[] modifiers,
                InputState inputState, HotkeyContext hotkeyContext, int cooldownMillis, Action action) {
            return AddHotkeyInternal(hotkeyName, keyCode, modifiers, inputState, 
                    hotkeyContext, cooldownMillis, action, groupName: null)
                == HotkeyAddResult.Sucess;
        }
        
        protected HotkeyAddResult AddHotkeyInternal(string hotkeyName, KeyCode keyCode, KeyCode[] modifiers,
                InputState inputState, HotkeyContext hotkeyContext, int cooldownMillis, Action action, string groupName) {

            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            if (!KeyPressActions.TryGetValue(hotkeyName, out _)) {
                ValidationResult valRes = IsHotkeyCombinationValid(hotkeyName, keyCode, modifiers,
                    inputState, hotkeyContext, out KeyBindContext keyBindContext);
                
                if (!valRes.IsValid) {
                    //Insert the hotkey but keep it empty, so it can be changed later with a valid combination.
                    keyBindContext = new(KeyCode.None, [], inputState, hotkeyContext);
                }

                InputData inputData = new(hotkeyName, keyBindContext, action, cooldownMillis, groupName);
                KeyPressActions.Add(hotkeyName, inputData);

                GenerateKeyGroupCache();

                if (inputData.IsFromGroup) {
                    if (NamedGroupsCache.ContainsKey(groupName)) {
                        NamedGroupsCache[groupName].Add(hotkeyName);
                    } else {
                        NamedGroupsCache.Add(groupName, [hotkeyName]);
                    }
                }

                if (!valRes.IsValid) {
                    HotkeyValidationError(hotkeyName, valRes.ErrorMessage, keyBindContext);
                    return HotkeyAddResult.AddedWithFailure;
                }
            } else {
                //Shouldnt be happening unless I made a mistake somewhere.
                TimeLogger.Logger.LogError($"Hotkey '{hotkeyName}' already exists.", LogCategories.KeyMouse);

                HotkeyValidationError(hotkeyName, $"I fucked up something and hotkey '{hotkeyName}' couldnt be added.",
                    new(keyCode, modifiers));

                return HotkeyAddResult.Failure;
            }

            InputMonobehaviour.Instance?.BehaviourEnabledCheck();

            return HotkeyAddResult.Sucess;
        }

        public virtual bool ChangeHotkey(string hotkeyName, KeyCode keyCode, KeyCode[] modifiers) {
            return ChangeHotkeyInternal(hotkeyName, keyCode, modifiers);
        }

        protected bool ChangeHotkeyInternal(string hotkeyName, KeyCode keyCode, KeyCode[] modifiers) {
            if (KeyPressActions.TryGetValue(hotkeyName, out InputData keyData)) {
                KeyBindContext keyBindCtx = keyData.KeyContext;

                ValidationResult valRes = IsHotkeyCombinationValid(hotkeyName, keyCode, modifiers,
                        keyBindCtx.TriggerInputState, keyBindCtx.HotkeyContext, out _);

                bool updateKeycodes = true;
                if (!valRes.IsValid) {
                    if (OnValidationError == OnValidationError.Rollback) {
                        updateKeycodes = false;
                        keyCode = keyBindCtx.KeyCode;
                        modifiers = keyBindCtx.Modifiers;
                    } else if (OnValidationError == OnValidationError.ClearKeycodes) {
                        keyCode = KeyCode.None;
                        modifiers = [];
                    }
                }

                if (updateKeycodes) {
                    keyData.ChangeValidatedKeyCodes(keyCode, modifiers);

                    GenerateKeyGroupCache();
                }

                if (!valRes.IsValid) {
                    HotkeyValidationError(hotkeyName, valRes.ErrorMessage, keyBindCtx);
                    return false;
                }
            } else {
                TimeLogger.Logger.LogError($"Hotkey '{hotkeyName}' cant be changed because it doesnt exist.",
                    LogCategories.KeyMouse);

                HotkeyValidationError(hotkeyName, $"Something went wrong and hotkey '{hotkeyName}' cant be changed.",
                    new(keyCode, modifiers));

                return false;
            }

            return true;
        }

        public virtual bool RemoveHotkey(string hotkeyName) {
            return RemoveHotkeysInternal(hotkeyName, groupName: null);
        }

        public virtual bool RemoveHotkeyGroup(string groupName) {
            return RemoveHotkeysInternal(hotkeyName: null, groupName);
        }

        protected bool RemoveHotkeysInternal(string hotkeyName, string groupName) {
            bool isEntryRemoved = false;

            if (!string.IsNullOrEmpty(hotkeyName)) {
                if (KeyPressActions.TryGetValue(hotkeyName, out InputData inputData)) {
                    NamedGroupsCache[inputData.GroupName].Remove(hotkeyName);
                    if (NamedGroupsCache.Count == 0) {
                        NamedGroupsCache.Remove(inputData.GroupName);
                    }
                    KeyPressActions.Remove(hotkeyName);

                    isEntryRemoved = true;
                }
            }else if (!string.IsNullOrEmpty(groupName)) {
                if (NamedGroupsCache.TryGetValue(groupName, out var group)) {
                    NamedGroupsCache.Remove(groupName);
                    foreach (string hotkeyFromGroup in group) {
                        KeyPressActions.Remove(hotkeyFromGroup);
                    }
                    isEntryRemoved = true;
                }
            }else {
                throw new ArgumentException($"Both {nameof(hotkeyName)} and {nameof(groupName)} cant be empty.");
            }

            if (isEntryRemoved) {
                GenerateKeyGroupCache();
                InputMonobehaviour.Instance?.BehaviourEnabledCheck();

                return true;
            }
            
            return false;
        }

        /// <summary>Checks that a KeyCode/InputState/HotkeyContext combination is valid.</summary>
        private ValidationResult IsHotkeyCombinationValid(string hotkeyName, KeyCode keyCode, KeyCode[] modifiers,
                InputState inputState, HotkeyContext hotkeyContext, out KeyBindContext keyBindContext) {
            
            keyBindContext = new(keyCode, modifiers, inputState, hotkeyContext);

            if (keyCode == KeyCode.None) {
                //Allow the None keycode, as a feature to register the hotkey but keep it disabled for now.
                //When the key is changed, it will get validated again.
                return ValidationResult.Valid;
            }

            if (IsRestrictedKeyBind(keyBindContext)) {
                string errorMessage = $"Hotkey '{keyBindContext.ToStringKeyCodes()}' is restricted and cant be used.";
                TimeLogger.Logger.LogWarning(errorMessage, LogCategories.KeyMouse);

                return new(false, errorMessage);
            }

            var localKeyBindContext = keyBindContext;
            var sameKeysAndCtxCombinations = KeyPressActions
                .Where(k => k.Value.KeyContext.EqualKeycodesAndContext(localKeyBindContext))
                .Select(k => (k.Key, k.Value.KeyContext.TriggerInputState))
                .ToList();

            if (sameKeysAndCtxCombinations.Count > 0) {
                //Check inputState is not the same
                var sameinputState = sameKeysAndCtxCombinations
                    .Where(k => k.TriggerInputState == inputState);

                if (sameinputState.Any()) {
                    TimeLogger.Logger.LogWarning($"The key '{keyBindContext.ToStringKeyCodesAndContext()}' is " +
                        $"already being used for the same key state: '{inputState}' with the hotkey name " +
                        $"'{sameinputState.First().Key}'", LogCategories.KeyMouse);

                    string errorMessage = $"Hotkey '{keyBindContext.ToStringKeyCodes()}' already in use.";
                    return new(false, errorMessage);
                }

                //KeyDown and KeyHeld are not allowed to be registered at the same time, so at least
                //	one of them must be the remaining TriggerInputState.KeyUp.
                var keyUpAction = sameKeysAndCtxCombinations.Where(k => k.TriggerInputState != InputState.KeyUp);
                if (inputState != InputState.KeyUp && !keyUpAction.Any()) {

                    TimeLogger.Logger.LogError($"{nameof(InputState.KeyDown)} and {nameof(InputState.KeyHeld)} " +
                        $"cant be registered for hotkey ('{keyBindContext.ToStringKeyCodesAndContext()}') " +
                        $"at the same time.", LogCategories.KeyMouse);

                    string errorMessage = $"Hotkey '{keyBindContext.ToStringKeyCodes()}' already in use.";
                    return new(false, errorMessage);
                }
            }

            return ValidationResult.Valid;
        }


        public string GetRegisteredHotkeys() =>
            string.Join(", ", KeyPressActions.Values.Select(d => d.KeyContext.ToString()));



        private static KeyCode[] GetPressedModifiers() => 
            SupportedModifiersList.Where(Input.GetKey).ToArray();

        public static ModifierMask KeyCodeModifiersToMask(KeyCode[] modifiers) {
            ModifierMask modifMask = ModifierMask.None;

            foreach (var key in modifiers) {
                modifMask |= key switch {
                    KeyCode.LeftControl => ModifierMask.LeftControl,
                    KeyCode.RightControl => ModifierMask.RightControl,
                    KeyCode.LeftShift => ModifierMask.LeftShift,
                    KeyCode.RightShift => ModifierMask.RightShift,
                    KeyCode.LeftAlt => ModifierMask.LeftAlt,
                    KeyCode.RightAlt => ModifierMask.RightAlt,
                    KeyCode.AltGr => ModifierMask.AltGr,
                    KeyCode.LeftWindows => ModifierMask.LeftWindows,
                    KeyCode.RightWindows => ModifierMask.RightWindows,
                    _ => ModifierMask.None
                };
            }

            return modifMask;
        }


        private class InputMonobehaviour : MonoBehaviour {

            //TODO Global 4 - Add a mode in which while the key is being held, 
            //	the cooldown gets progressively smaller up to specified certain limit.

            public static InputMonobehaviour Instance { get; private set; }


            private static InputDetection InputInstance;

            
            public static void Initialize(InputDetection keyPressDetectionInstance) {
                InputInstance = keyPressDetectionInstance;
            }

            private bool IsKeyPrecheckPassed() =>
                InputInstance.PreKeyCheckFunc == null || InputInstance.PreKeyCheckFunc();


            private void Awake() {
                Instance = this;
            }

            private void Start() {
                //Wait for added hotkeys before being active.
                BehaviourEnabledCheck();
            }

            private void Update() {
                if (!IsKeyPrecheckPassed() || InputInstance.groupedKeysCache == null || 
                        InputInstance.groupedKeysCache.Count() == 0) {
                    return;
                }

                //Convert pressed modifiers into a mask for quicker comparisons.
                ModifierMask pressedModifMask = KeyCodeModifiersToMask(GetPressedModifiers());

                foreach (var keyGroup in InputInstance.groupedKeysCache) {
                    //A keyGroup has one or more elements with the same combination of KeyCode + InputState.

                    InputData[] inputCombinationData = keyGroup
                        //Only take combinations with the same, or less, modifiers as the ones pressed.
                        .Where(i => i.IsModifierCompatible(pressedModifMask))
                        //Sort by number of modifiers
                        .OrderByDescending(k => CountFlags(k.KeyContext.ModifiersMask))
                        //Order entries that have the same modifiers by priority
                        .ThenByDescending(k => k.KeyContext.HotkeyContext.Priority)
                        .ToArray();

                    //Check triggers
                    foreach (InputData inputData in inputCombinationData) {
                        if (inputData.KeyContext.IsTriggeredWithContext(pressedModifMask, ignoreModifiers: true)) {

                            if (inputData.IsNotInCooldown()) {
                                inputData.PerformHotkeyAction();
                            }
                            
                            break;
                        }
                    }
                }
            }
            
            private static int CountFlags(ModifierMask modifierMask) =>
                new BitArray([(int)modifierMask])
                    .OfType<bool>()
                    .Count(x => x);


            private void OnDestroy() {
                InputInstance.BeginGetBehaviour().FireAndForget(LogCategories.KeyMouse);
            }

            internal void BehaviourEnabledCheck() {
                if (!this) {
                    return;
                }

                enabled = InputInstance.HasTriggerableKeys();
            }
        }


        private sealed class IgnoreModifierComparer : IEqualityComparer<KeyBindState> {
            public bool Equals(KeyBindState o1, KeyBindState o2) =>
                o1.KeyCode == o2.KeyCode && o1.TriggerInputState == o2.TriggerInputState;
            public int GetHashCode(KeyBindState o) => HashCode.Combine(o.KeyCode, o.TriggerInputState);
        }

        private readonly struct ValidationResult(bool isValid, string errorMessage) {

            public static ValidationResult Valid = new (true, "");

            public bool IsValid { get; } = isValid;
            public string ErrorMessage { get; } = errorMessage;
        }

    }

}

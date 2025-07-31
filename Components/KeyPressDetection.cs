using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using Damntry.Utils.ExtensionMethods;
using Damntry.Utils.Logging;
using UnityEngine;

namespace Damntry.UtilsUnity.Components {

	public enum KeyPressAction {
		/// <summary>Triggers when the key is pressed for the first time.</summary>
		KeyDown = 0,
		/// <summary>Triggers when the key is released.</summary>
		KeyUp = 1,
		/// <summary>Triggers constantly while the key is pressed.</summary>
		KeyHeld = 2,
	}

	public class KeyPressDetection : MonoBehaviour {

		private static KeyPressDetection instance;

		private static Dictionary<KeyCode, KeyPressData> keyPressActions;

		private static Func<bool> preKeyCheckFunc;

		private static Func<MonoBehaviour> getBehaviourAttachFunc;

		//TODO Global 4 - Add a mode in which while the key is being held, 
		//	the cooldown gets progressively smaller up to a certain limit.

		/// <summary>
		/// Minimum cooldown time between keypresses when not specified for the hotkey.
		/// </summary>
		private const int DefaultKeyPressCooldown = 65;

		
		static KeyPressDetection() {
			keyPressActions = new();
		}

		private void Awake() {
			instance = this;
		}

		private void Start() {
			//Wait for added hotkeys before being active.
			BehaviourEnabledCheck();
		}

		private void Update() {
			if (!Input.anyKey) {
				return;
			}

			foreach (var keyAction in keyPressActions) {
				KeyPressData keyPressData = keyAction.Value;

				bool isBindedKeyTriggered = keyPressData.KeyAction switch {
					KeyPressAction.KeyDown => Input.GetKeyDown(keyAction.Key),
					KeyPressAction.KeyUp => Input.GetKeyUp(keyAction.Key),
					KeyPressAction.KeyHeld => Input.GetKey(keyAction.Key),
					_ => throw new NotImplementedException(keyPressData.KeyAction.GetDescription()),
				};

				if (isBindedKeyTriggered && (preKeyCheckFunc == null || preKeyCheckFunc())) {
					if (keyPressData.IsNotInCooldown()) {
						keyPressData.UpdateKeyPressTime();
						keyPressData.Action();
					}
				}
			}
		}

		private void OnDestroy() {
			BeginGetBehaviour(getBehaviourAttachFunc)
				.FireAndForget(Utils.Logging.LogCategories.KeyMouse);
		}

		/// <param name="getBehaviourAttachFunc">
		/// The function that will return the object to which the key press behaviour will attach.
		/// Can be called while the object is still null, and it will keep trying until it is initialized.
		/// </param>
		/// <param name="preKeyCheckFunc">
		/// Function called before doing a detected keypress action.
		/// If it returns true, the hotkey will be triggered, otherwise ignored.
		/// </param>
		public async static Task InitializeAsync(Func<MonoBehaviour> getBehaviourAttachFunc, Func<bool> preKeyCheckFunc = null) {
			KeyPressDetection.getBehaviourAttachFunc = getBehaviourAttachFunc;
			await BeginGetBehaviour(getBehaviourAttachFunc);
		}

		/// <summary>
		/// Begins the periodic task to try and find the behaviour where we will attach to, using the provided function.
		/// </summary>
		private static async Task BeginGetBehaviour(Func<MonoBehaviour> getBehaviourAttachFunc) {
			MonoBehaviour monoObject = null;
			while ((monoObject = getBehaviourAttachFunc()) == null || !monoObject.gameObject.activeInHierarchy) {
				await Task.Delay(500);
			}

			Initialize(monoObject, preKeyCheckFunc);
		}

		public static void Initialize(MonoBehaviour unityObject, Func<bool> preKeyCheckFunc = null) {
			if (unityObject == null) {
				throw new ArgumentNullException(nameof(unityObject));
			}
			Initialize(unityObject.gameObject, preKeyCheckFunc);
		}

		public static void Initialize(GameObject unityGameObject, Func<bool> preKeyCheckFunc = null) {	
			if (unityGameObject == null) {
				throw new ArgumentNullException(nameof(unityGameObject));
			}
			unityGameObject.AddComponent<KeyPressDetection>();

			KeyPressDetection.preKeyCheckFunc = preKeyCheckFunc;
		}


		public static bool TryAddHotkey(KeyCode keyCode, KeyPressAction keyAction, Action action) {
			return AddHotkeyInternal(keyCode, DefaultKeyPressCooldown, keyAction, throwIfExists: false, action);
		}

		public static bool TryAddHotkey(KeyCode keyCode, KeyPressAction keyAction, int cooldownMillis, Action action) {
			return AddHotkeyInternal(keyCode, cooldownMillis, keyAction, throwIfExists: false, action);
		}

		public static void AddHotkey(KeyCode keyCode, KeyPressAction keyAction, Action action) {
			AddHotkeyInternal(keyCode, DefaultKeyPressCooldown, keyAction, throwIfExists: true, action);
		}

		public static void AddHotkey(KeyCode keyCode, KeyPressAction keyAction, int cooldownMillis, Action action) {
			AddHotkeyInternal(keyCode, cooldownMillis, keyAction, throwIfExists: true, action);
		}

		private static bool AddHotkeyInternal(KeyCode keyCode, int cooldownMillis,
				KeyPressAction keyAction, bool throwIfExists, Action action) {

			if (action == null) {
				throw new ArgumentNullException(nameof(action));
			}
			if (!keyPressActions.TryGetValue(keyCode, out KeyPressData existingKeyData)) {
				keyPressActions.Add(keyCode, new KeyPressData(action, keyAction, cooldownMillis));
			} else { 
				if (keyAction != existingKeyData.KeyAction &&
						//KeyDown and KeyHeld are not allowed to be registered at the same time.
						(keyAction == KeyPressAction.KeyUp || existingKeyData.KeyAction == KeyPressAction.KeyUp)) {

					//TODO 0 - Here I need to somehow add the indication that another KeyAction is being registered
					keyPressActions.Add(keyCode, new KeyPressData(action, keyAction, cooldownMillis));
				} else {
					string errorMessage = $"Hotkey {keyCode} is already defined.";
					if (throwIfExists) {
						throw new InvalidOperationException(errorMessage);
					}
					else {
						TimeLogger.Logger.LogTimeError(errorMessage, LogCategories.KeyMouse);
					}
					return false;
				}
			}

			instance?.BehaviourEnabledCheck();

			return true;
		}

		public static void RemoveHotkey(KeyCode keyCode) {
			if (keyPressActions.ContainsKey(keyCode)) {
				keyPressActions.Remove(keyCode);
				instance?.BehaviourEnabledCheck();
			}
		}

		public static string GetRegisteredHotkeys() => string.Join(", ", keyPressActions.Keys);


		private void BehaviourEnabledCheck() {
			this.enabled = keyPressActions?.Count > 0;
		}


		private class KeyPressData {

			private readonly double cooldownSeconds;

			private double lastKeyPressTime;

			public KeyPressData(Action action, KeyPressAction keyAction, int cooldownMillis) {
				this.Action = action; 
				this.cooldownSeconds = cooldownSeconds > 0 ? cooldownMillis / 1000d : -1; 
				this.lastKeyPressTime = double.MinValue;
				this.KeyAction = keyAction;
			}

			internal Action Action { get; private set; }

			internal KeyPressAction KeyAction { get; private set; }



			internal bool IsNotInCooldown() {
				return cooldownSeconds <= 0 || lastKeyPressTime + cooldownSeconds < Time.timeAsDouble;
			}

			internal void UpdateKeyPressTime() {
				lastKeyPressTime = Time.timeAsDouble;
			}

		}

	}

}

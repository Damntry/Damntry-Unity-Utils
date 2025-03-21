using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Damntry.Utils.ExtensionMethods;
using Damntry.Utils.Logging;
using UnityEngine;

namespace Damntry.UtilsUnity.Components {

	public class KeyPressDetection : MonoBehaviour {

		private static KeyPressDetection instance;

		private static Dictionary<KeyCode, KeyPressData> keyPressActions;

		private static Func<bool> preKeyCheckFunc;

		private static Func<MonoBehaviour> getBehaviourAttachFunc;

		//TODO Global 4 - Add a mode in which while the key is being held, 
		//	the cooldown gets progressively smaller up to a certain limit.

		//TODO Global 3 - Add a mode of normal behaviour where its just a single click
		//		without repetition. Basically dont allow more until after keyup is detected.

		/// <summary>
		/// Minimum cooldown time between keypresses when not specified for the hotkey.
		/// </summary>
		private const int DefaultKeyPressCooldown = 50;

		
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
				if (Input.GetKey(keyAction.Key) && (preKeyCheckFunc == null || preKeyCheckFunc())) {
					KeyPressData keyPressData = keyAction.Value;
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

		public static void AddHotkey(KeyCode keyCode, Action action) {
			AddHotkey(keyCode, DefaultKeyPressCooldown, action);
		}

		public static void AddHotkey(KeyCode keyCode, int cooldownMillis, Action action) {
			if (action == null) {
				throw new ArgumentNullException(nameof(action));
			}
			if (keyPressActions.ContainsKey(keyCode)) {
				throw new InvalidOperationException($"Hotkey {keyCode} is already defined.");
			}

			keyPressActions.Add(keyCode, new KeyPressData(action, cooldownMillis));

			instance?.BehaviourEnabledCheck();
		}

		public static void RemoveHotkey(KeyCode keyCode) {
			if (keyPressActions.ContainsKey(keyCode)) {
				keyPressActions.Remove(keyCode);
				instance?.BehaviourEnabledCheck();
			}
		}

		private void BehaviourEnabledCheck() {
			this.enabled = keyPressActions?.Count > 0;
		}


		private class KeyPressData {

			public KeyPressData(Action action, int cooldownMillis) {
				this.Action = action; 
				this.cooldownSeconds = cooldownMillis / 1000d; 
				this.lastKeyPressTime = double.MinValue;
			}

			internal Action Action { get; private set; }

			private double cooldownSeconds;

			private double lastKeyPressTime;


			internal bool IsNotInCooldown() {
				return cooldownSeconds <= 0 || lastKeyPressTime + cooldownSeconds < Time.timeAsDouble;
			}

			internal void UpdateKeyPressTime() {
				lastKeyPressTime = Time.timeAsDouble;
			}

		}

	}

}

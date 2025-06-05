using System;
using System.Collections.Generic;
using System.IO;
using Damntry.Utils.Logging;
using Damntry.Utils.Reflection;
using UnityEngine;

namespace Damntry.UtilsUnity.Resources {

	/// <summary>
	/// Handles loading .prefab files at runtime.
	/// </summary>
	public class AssetBundleElement {

		private readonly string assetBundlePath;

		private AssetBundle loadedBundle;

		private readonly Dictionary<string, UnityEngine.Object> loadedPrefabs;


		public AssetBundleElement(Type assemblyType, string assetName) {
			if (assemblyType == null) {
				throw new ArgumentNullException(nameof(assemblyType));
			}
			if (string.IsNullOrEmpty(assetName)) {
				throw new ArgumentNullException($"The parameter {assetName} cant be null or empty.");
			}

			assetBundlePath = AssemblyUtils.GetCombinedPathFromAssemblyFolder(assemblyType, assetName);
			if (!File.Exists(assetBundlePath)) {
				throw new FileNotFoundException(null, assetBundlePath);
			}

			loadedPrefabs = new();
		}

		/// <summary>
		/// Load the bundle from the constructor given path. Useful when you want
		/// to load it earlier so when instanced it avoids the loading cost.
		/// </summary>
		/// <returns>True if it loaded successfully, or was previously already loaded. False otherwise.</returns>
		public bool PreloadBundle() {
			if (loadedBundle == null) {
				loadedBundle = AssetBundle.LoadFromFile(assetBundlePath);
				if (loadedBundle == null) {
					TimeLogger.Logger.LogTimeError($"Failed to load bundle from {assetBundlePath}", LogCategories.Other);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Loads a prefab object for later use.
		/// </summary>
		/// <param name="fullLoadPath">Path to the object within the .</returns>
		private bool TryLoadPrefabObject<T>(string fullLoadPath, out T loadedObject, bool cacheObject)
				where T : UnityEngine.Object {

			loadedObject = default;

			if (!PreloadBundle()) {
				return false;
			}

			loadedObject = loadedBundle.LoadAsset<T>(fullLoadPath);
			if (loadedObject == null) {
				return false;
			}

			if (cacheObject) {
				loadedPrefabs.Add(fullLoadPath, loadedObject);
			}

			return true;
		}

		public bool TryLoadNewPrefabInstance(string prefabName, out GameObject prefabInstance, bool cacheObject = true) {
			prefabInstance = null;
			string fullLoadPath = $"assets/{prefabName}.prefab";

			if (!TryLoadObject(fullLoadPath, out GameObject prefabObj)) {
				return false;
			}

			prefabInstance = UnityEngine.Object.Instantiate(prefabObj);
			return true;
		}

		public bool TryLoadObject<T>(string loadPath, out T objectInstance, bool cacheObject = true)
				where T : UnityEngine.Object {

			objectInstance = default;
			if (!loadedPrefabs.TryGetValue(loadPath, out UnityEngine.Object unityObject)) {
				//If not found, load it from bundle file.
				if (!TryLoadPrefabObject(loadPath, out unityObject, cacheObject)) {
					return false;
				}
			}
			if (unityObject is not T) {
				TimeLogger.Logger.LogTimeError($"The object at {loadPath} is not a {typeof(T).Name} type.", 
					LogCategories.Other);
			}

			objectInstance = (T)unityObject;
			return true;
		}

	}
}

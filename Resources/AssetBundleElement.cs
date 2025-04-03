using System;
using System.Collections.Generic;
using System.IO;
using Damntry.Utils.Reflection;
using UnityEngine;

namespace Damntry.UtilsUnity.Resources {

	/// <summary>
	/// Handles loading .prefab files at runtime.
	/// </summary>
	public class AssetBundleElement {

		private readonly string assetBundlePath;

		private AssetBundle loadedBundle;

		private readonly Dictionary<string, GameObject> loadedPrefabs;



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
		/// Loads a prefab object for later use.
		/// </summary>
		/// <param name="prefabName">Name of the prefab object, without .prefab extension.</param>
		/// <returns>True if the object was loaded successfully.</returns>
		private bool TryLoadPrefabObject(string prefabName, out GameObject prefabObj) {
			prefabObj = null;

			if (loadedBundle == null) {
				loadedBundle = AssetBundle.LoadFromFile(assetBundlePath);
				if (loadedBundle == null) {
					return false;
				}
			}

			prefabObj = loadedBundle.LoadAsset<GameObject>($"assets/{prefabName}.prefab");
			if (prefabObj == null) {
				return false;
			}
			loadedPrefabs.Add(prefabName, prefabObj);

			return true;
		}

		public bool TryLoadNewInstance(string prefabName, out GameObject prefabInstance) {
			prefabInstance = null;
			if (!loadedPrefabs.TryGetValue(prefabName, out GameObject prefabObj)) {
				//If not found, load it from bundle file.
				if (!TryLoadPrefabObject(prefabName, out prefabObj)) {
					return false;
				}
			}

			prefabInstance = UnityEngine.Object.Instantiate(prefabObj);
			return true;
		}
	}
}

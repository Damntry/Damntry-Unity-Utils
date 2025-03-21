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

		private readonly string assetPath;

		private Dictionary<string, GameObject> loadedPrefabs;


		public AssetBundleElement(Type assemblyType, string assetName) {
			if (assemblyType == null) {
				throw new ArgumentNullException(nameof(assemblyType));
			}
			if (string.IsNullOrEmpty(assetName)) {
				throw new ArgumentNullException($"The parameter {assetName} cant be null or empty.");
			}

			assetPath = AssemblyUtils.GetCombinedPathFromAssemblyFolder(assemblyType, assetName);
			if (!File.Exists(assetPath)) {
				throw new FileNotFoundException(null, assetPath);
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

			AssetBundle _bundle = AssetBundle.LoadFromFile(assetPath);
			if (_bundle == null) {
				return false;
			}
			
			prefabObj = _bundle.LoadAsset<GameObject>($"assets/{prefabName}.prefab");
			if (prefabObj == null) {
				return false;
			}
			loadedPrefabs.Add(prefabName, prefabObj);

			return true;
		}

		public bool TryLoadNewInstance(string prefabName, out GameObject prefabInstance) {
			prefabInstance = null;
			bool found = loadedPrefabs.TryGetValue(prefabName, out GameObject prefabObj);
			if (!found) {
				//If not found, load it from disk.
				found = TryLoadPrefabObject(prefabName, out prefabObj);
			}
			if (found) {
				prefabInstance = UnityEngine.Object.Instantiate(prefabObj);
			}

			return found;
		}
	}
}

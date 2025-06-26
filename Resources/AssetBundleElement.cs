using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Damntry.Utils.Logging;
using Damntry.Utils.Reflection;
using UnityEngine;

namespace Damntry.UtilsUnity.Resources {

	/// <summary>
	/// Handles loading .prefab files at runtime.
	/// </summary>
	public class AssetBundleElement {

		private static Dictionary<string, AssetBundle> bundleCacheStorage = new();


		private readonly string assetBundlePath;

		private AssetBundle loadedBundle;


		public AssetBundleElement(Type assemblyType, string assetName) {
			if (assemblyType == null) {
				throw new ArgumentNullException(nameof(assemblyType));
			}
			if (string.IsNullOrEmpty(assetName)) {
				throw new ArgumentNullException($"The parameter {nameof(assetName)} cant be null or empty.");
			}

			assetBundlePath = AssemblyUtils.GetCombinedPathFromAssemblyFolder(assemblyType, assetName);
			if (!File.Exists(assetBundlePath)) {
				throw new FileNotFoundException(null, assetBundlePath);
			}
		}

		public AssetBundleElement(string assetFullPath) {
			if (string.IsNullOrEmpty(assetFullPath)) {
				throw new ArgumentNullException($"The parameter {nameof(assetFullPath)} cant be null or empty.");
			}

			if (!File.Exists(assetFullPath)) {
				throw new FileNotFoundException(null, assetFullPath);
			}
			assetBundlePath = assetFullPath;
		}

		/// <summary>
		/// Load the bundle from the constructor given path. Useful when
		/// you want to load it earlier and avoid that cost later on.
		/// </summary>
		/// <returns>True if it loaded successfully, or was previously already loaded. False otherwise.</returns>
		public bool PreloadBundle() {
			if (loadedBundle == null) {
				//	See if this GetAllLoadedAssetBundles can be used to find an already loaded one.
				//AssetBundle.GetAllLoadedAssetBundles()
				if (!bundleCacheStorage.TryGetValue(assetBundlePath, out loadedBundle) || loadedBundle == null) {
					loadedBundle = AssetBundle.LoadFromFile(assetBundlePath);
					if (loadedBundle == null) {
						throw new LoadBundleFileException(assetBundlePath);
					}
					TimeLogger.Logger.LogTimeDebug($"Bundle loaded successfully from {assetBundlePath}"
						, LogCategories.Loading);

					bundleCacheStorage.Add(assetBundlePath, loadedBundle);
				} else {
					TimeLogger.Logger.LogTimeDebug($"Bundle loaded successfully from storage cache.", LogCategories.Loading);
				}
			}
			return true;
		}
		/*
		public string[] GetAllScenePaths() {
			if (!PreloadBundle()) {
				return null;
			}

			return loadedBundle.GetAllScenePaths();
		}
		*/
		public string[] GetAllAssetNames() {
			if (!PreloadBundle()) {
				return null;
			}

			return loadedBundle.GetAllAssetNames();
		}
		/*
		public string GetAllScenePathsString(string separator = "\n") {
			string[] scenePaths = GetAllScenePaths();
			if (scenePaths == null) {
				return null;
			}
			return string.Join(separator, scenePaths);
		}
		*/
		public string GetAllAssetNamesString(string separator = "\n") {
			string[] assetNames = GetAllAssetNames();
			if (assetNames == null) {
				return null;
			}
			return string.Join(separator, assetNames);
		}

		public async Task<T[]> LoadAllAssetsAsync<T>() where T : UnityEngine.Object {

			if (!PreloadBundle()) {
				return null;
			}

			AssetBundleRequest assetRequest = loadedBundle.LoadAllAssetsAsync(typeof(T));
			await assetRequest;

			return assetRequest.allAssets.Cast<T>().ToArray();
		}

		public T[] LoadAllAssets<T>() where T : UnityEngine.Object {

			if (!PreloadBundle()) {
				return null;
			}

			return loadedBundle.LoadAllAssets<T>().ToArray();
		}

		public bool TryLoadNewPrefabInstance(string prefabName, out GameObject prefabInstance) {
			prefabInstance = null;
			string fullLoadPath = $"assets/{prefabName}.prefab";

			if (!TryLoadObject(fullLoadPath, out GameObject prefabObj)) {
				return false;
			}

			prefabInstance = UnityEngine.Object.Instantiate(prefabObj);
			return true;
		}

		/// <summary>
		/// Loads a Unity object for later use.
		/// </summary>
		/// <param name="bundleInternalUnityPath">Path to the object within the bundle.</returns>
		public bool TryLoadObject<T>(string bundleInternalUnityPath, out T objectInstance)
				where T : UnityEngine.Object {

			objectInstance = default;
			//If not found, load it from bundle file.
			if (!TryLoadPrefabObject(bundleInternalUnityPath, out UnityEngine.Object unityObject)) {
				return false;
			}
			if (unityObject is not T) {
				TimeLogger.Logger.LogTimeError($"The object at {bundleInternalUnityPath} " +
					$"is not a {typeof(T).Name} type.", LogCategories.Loading);
				return false;
			}

			objectInstance = (T)unityObject;
			return true;
		}

		/// <summary>
		/// Loads a prefab object for later use.
		/// </summary>
		/// <param name="bundleInternalUnityPath">Path to the object within the bundle.</returns>
		private bool TryLoadPrefabObject<T>(string bundleInternalUnityPath, out T loadedObject)
				where T : UnityEngine.Object {

			loadedObject = default;

			if (!PreloadBundle()) {
				return false;
			}

			loadedObject = loadedBundle.LoadAsset<T>(bundleInternalUnityPath);
			if (loadedObject == null) {
				TimeLogger.Logger.LogTimeDebug($"Asset loaded with path " +
					$"\"{bundleInternalUnityPath}\" not found.", LogCategories.Loading);
				return false;
			}

			return true;
		}

		public void UnloadBundle() {
			if (loadedBundle == null) {
				TimeLogger.Logger.LogTimeDebug($"No loaded bundle was " +
					$"found while trying to unload.", LogCategories.Loading);
				return;
			}
			loadedBundle.Unload(true);
			loadedBundle = null;
			bundleCacheStorage.Remove(assetBundlePath);


			TimeLogger.Logger.LogTimeDebug($"Asset bundle at {assetBundlePath} " +
				$"unloaded successfully.", LogCategories.Loading);
		}
	}

	public class LoadBundleFileException : Exception {

		public LoadBundleFileException(string assetPath)
				: base($"Failed to load asset bundle from path: {assetPath}.") { }
	}

}
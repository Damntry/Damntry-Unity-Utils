// scr* https://gist.github.com/andrew-raphael-lukasik/df4a36ff2ad89078258fd653c422a021
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performs AABB frustum culling on GameObject renderers.
/// </summary>
public class GpuInstancingFrustumCulling : MonoBehaviour {
	[SerializeField] Camera _camera = null;
	[SerializeField] MeshRenderer[] _meshRenderers = new MeshRenderer[0];

	/// <summary>
	/// Prefer "true" ☑ as "false" ☐ require updates every frame.
	/// It is a good idea to keep lists of still and moving mesh renderers in a separate components.
	/// </summary>
	public bool meshesAreStill = true;

	Dictionary<(Mesh mesh, Material material), (List<Transform> transforms, Bounds aabb)> _sources = new Dictionary<(Mesh, Material), (List<Transform>, Bounds)>();
	Dictionary<(Mesh mesh, Material material), (Matrix4x4[] matrices, Bounds aabb)> _batches = new Dictionary<(Mesh, Material), (Matrix4x4[], Bounds)>();
	Dictionary<int, Stack<Matrix4x4[]>> _freeMatrices = new Dictionary<int, Stack<Matrix4x4[]>>();
	Plane[] _frustum = new Plane[6];

	void Start() {
		Initialize();
		UpdateMatrices();

		if (_camera == null) _camera = Camera.main;
		if (_camera == null) {
			Debug.LogError("no camera, can't continue", this);
			enabled = false;
		}
	}

	void Update() {
		if (!meshesAreStill) UpdateMatrices();

		GeometryUtility.CalculateFrustumPlanes(_camera, _frustum);
		foreach (var batch in _batches) {
			var meshMaterialPair = batch.Key;
			var matricesAabbPair = batch.Value;
			var aabb = matricesAabbPair.aabb;
			if (GeometryUtility.TestPlanesAABB(_frustum, aabb)) {
				Graphics.DrawMeshInstanced(
					mesh: meshMaterialPair.mesh,
					submeshIndex: 0,
					material: meshMaterialPair.material,
					matrices: matricesAabbPair.matrices
				);
			}
		}
	}

#if UNITY_EDITOR
	// void OnDrawGizmosSelected ()
	void OnDrawGizmos ()
	{
		Initialize();

		Gizmos.color = Color.yellow;
		foreach( var source in _sources )
		{
			var transformsAabbPair = source.Value;
			var aabb = transformsAabbPair.aabb;
			Gizmos.DrawWireCube( aabb.center , aabb.size );
			
			if( Application.isPlaying && !GeometryUtility.TestPlanesAABB(_frustum,aabb) )
				UnityEditor.Handles.Label( aabb.center , "(out of camera view)" );
		}
	}
#endif

	void Initialize() {
		_sources.Clear();
		foreach (var meshRenderer in _meshRenderers) {
			if (meshRenderer == null) continue;
			var meshFilter = meshRenderer.GetComponent<MeshFilter>();
			if (meshFilter == null) continue;
			var mesh = meshFilter.sharedMesh;
			if (mesh == null) continue;
			foreach (var material in meshRenderer.sharedMaterials) {
				if (!material.enableInstancing && Application.isPlaying) {
					Debug.LogWarning($"\"{material.name}\" material won't be rendered as it's <b>GPU Instancing</b> is not enabled", meshRenderer);
					continue;
				}
				if (material == null) continue;
				var aabb = meshRenderer.bounds;
				var meshMaterialPair = (mesh, material);
				if (_sources.ContainsKey(meshMaterialPair)) {
					var transforms = _sources[meshMaterialPair].transforms;
					transforms.Add(meshRenderer.transform);

					var newAabb = _sources[meshMaterialPair].aabb;
					newAabb.Encapsulate(aabb);

					_sources[meshMaterialPair] = (transforms, newAabb);
				} else {
					_sources.Add(meshMaterialPair, (new List<Transform>() { meshRenderer.transform }, aabb));
				}
			}
			if (Application.isPlaying)
				meshRenderer.enabled = false;
		}
	}

	void UpdateMatrices() {
		foreach (var batch in _batches) {
			var matricesAabbPair = batch.Value;
			var matrices = matricesAabbPair.matrices;
			if (_freeMatrices.ContainsKey(matrices.Length)) {
				_freeMatrices[matrices.Length].Push(matrices);
			} else {
				var stack = new Stack<Matrix4x4[]>();
				stack.Push(matrices);
				_freeMatrices.Add(matrices.Length, stack);
			}
		}
		_batches.Clear();

		foreach (var source in _sources) {
			var meshMaterialPair = source.Key;
			var transformsAabbPair = source.Value;
			var transforms = transformsAabbPair.transforms;

			int numTransforms = transforms.Count;
			Matrix4x4[] matrices = null;
			if (_freeMatrices.ContainsKey(numTransforms) && _freeMatrices[numTransforms].Count != 0) {
				matrices = _freeMatrices[numTransforms].Pop();
			} else matrices = new Matrix4x4[numTransforms];

			for (int i = 0; i < numTransforms; i++)
				matrices[i] = transforms[i].localToWorldMatrix;
			_batches.Add(meshMaterialPair, (matrices, transformsAabbPair.aabb));
		}
	}

}
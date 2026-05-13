using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PrefabRef {
	#if UNITY_EDITOR
	[SerializeField] private GameObject prefab;
	#endif

	[SerializeField] private string prefabName;
	[SerializeField] private string resourcePath;
	[SerializeField] private AssetBundle bundle;

	public string PrefabName => prefabName;
	public string ResourcePath => bundle ? $"AssetBundles/{bundle.name}/{resourcePath}" : resourcePath;
	public AssetBundle Bundle => bundle;

	public GameObject Prefab {
		get {
			#if UNITY_EDITOR
			if(prefab != null) return prefab;
			#endif

			string cachedPath;
			if(bundle){
				cachedPath = ResourcePath;
			}else{
				cachedPath = resourcePath;
			}

			if(RunManager.instance.singleplayerPool.TryGetValue(cachedPath, out var cachedPrefab)){
				return cachedPrefab;
			}
			if(RunManager.instance.multiplayerPool.ResourceCache.TryGetValue(cachedPath, out cachedPrefab)){
				return cachedPrefab;
			}

			if(bundle){
				cachedPrefab = bundle.LoadAsset<GameObject>(resourcePath);
			}else{
				cachedPrefab = Resources.Load<GameObject>(resourcePath);
			}

			if(cachedPrefab == null){
				if(bundle){
					Debug.LogError("PrefabRef failed to load \"" + resourcePath + "\" from \"" + bundle.name + "\" asset bundle.");
				}else{
					Debug.LogError("PrefabRef failed to load \"" + resourcePath + "\". Make sure it's in a \"Resources\" folder.");
				}
				return null;
			}

			RunManager.instance.singleplayerPool.Add(cachedPath, cachedPrefab);
			return cachedPrefab;
		}
	}

	public bool IsValid(){
		#if UNITY_EDITOR
		if(prefab != null) return true;
		#endif

	    return !string.IsNullOrEmpty(resourcePath);
	}

	public void SetPrefab(GameObject _prefab, string _resourcePath = null){
		#if UNITY_EDITOR
		prefab = _prefab;
		#endif

		if(_prefab != null){
			prefabName = _prefab.name;

			#if UNITY_EDITOR
			var path = UnityEditor.AssetDatabase.GetAssetPath(_prefab);
			var cleanPath = path.Replace("//", "/");
			resourcePath = cleanPath;
			#else
			resourcePath = _resourcePath;
			#endif
		}else{
			prefabName = null;
			resourcePath = null;
		}
	}

	public void SetPrefab(AssetBundle _bundle, string _resourcePath = null){
		bundle = _bundle;
	
		if(bundle != null){
			prefabName = Path.GetFileNameWithoutExtension(_resourcePath);
			resourcePath = _resourcePath;
		}else{
			prefabName = null;
			resourcePath = null;
		}
	}
}
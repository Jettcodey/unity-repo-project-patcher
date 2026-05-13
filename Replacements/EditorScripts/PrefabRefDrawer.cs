using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PrefabRef))]
public class PrefabRefDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		var prefabProp = property.FindPropertyRelative("prefab");
		var prefabNameProp = property.FindPropertyRelative("prefabName");
		var resourcePathProp = property.FindPropertyRelative("resourcePath");

		using(var check = new EditorGUI.ChangeCheckScope()){
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, prefabProp, label);

			if (check.changed) {
				if(prefabProp.objectReferenceValue is GameObject prefab){
					prefabNameProp.stringValue = prefab.name;

					var path = AssetDatabase.GetAssetPath(prefab);
					var cleanPath = path.Replace("//", "/");
					resourcePathProp.stringValue = cleanPath;
				}else{
					prefabNameProp.stringValue = null;
					resourcePathProp.stringValue = null;
				}

				property.serializedObject.ApplyModifiedProperties();
			}
		}

		EditorGUI.EndProperty();
	}
}
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Region)), CanEditMultipleObjects]
public class TerritoryEditor : Editor {

	SerializedProperty points;

	private static GUIContent addPoint = new GUIContent("+", "Add SparkPoint");
	private static GUIContent removePoint = new GUIContent("-", "Remove SparkPoint");

	public void OnEnable () {
		points = serializedObject.FindProperty("regionPoints");
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();
		
		// add/remove regions
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(addPoint, EditorStyles.miniButtonLeft, GUILayout.Width(50f))) {
			points.arraySize+=1;
			points.GetArrayElementAtIndex(points.arraySize-1).objectReferenceValue = null;
		}
		if(GUILayout.Button(removePoint, EditorStyles.miniButtonMid, GUILayout.Width(50f))) {
			points.arraySize-=1;
			points.GetArrayElementAtIndex(points.arraySize-1).objectReferenceValue = null;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.PropertyField(points, true);
		
		serializedObject.ApplyModifiedProperties();
	}
}
using UnityEngine;
using UnityEditor;
using System.Collections;
	
[CustomEditor (typeof(SparkPoint)), CanEditMultipleObjects]
public class SparkPointEditor : Editor {

	SerializedProperty connectedSparkPoints;

	private static GUIContent addPoint = new GUIContent("+", "Add Point");
	private static GUIContent minusPoint = new GUIContent("-", "Remove Point");

	public void OnEnable () {
		connectedSparkPoints = serializedObject.FindProperty("_connections");
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();

		//these buttons adds / removes spark points
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(addPoint, EditorStyles.miniButtonLeft, GUILayout.Width(50f))) {
			connectedSparkPoints.arraySize+=1;
			connectedSparkPoints.GetArrayElementAtIndex(connectedSparkPoints.arraySize-1).objectReferenceValue = null;
		}
		if(GUILayout.Button(minusPoint, EditorStyles.miniButtonMid, GUILayout.Width(50f))) {
			connectedSparkPoints.arraySize-=1;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.PropertyField(connectedSparkPoints, true);

		//checks its children and live updates - if A contains B and B does not contain A, add A to B
		for(int i = 0; i < connectedSparkPoints.arraySize; i++) {
			if(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue != null) {
				SerializedObject so = new SerializedObject(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue);
				SerializedProperty sp = so.FindProperty("_connections");

				bool alreadyConnected = false;

				for(int j = 0; j < sp.arraySize; j++) {
					if(sp.GetArrayElementAtIndex(j).objectReferenceValue == this.serializedObject.targetObject) {
						alreadyConnected = true;
					}
				}

				if(!alreadyConnected) {
					sp.serializedObject.Update();
					sp.arraySize++;
					sp.GetArrayElementAtIndex(sp.arraySize-1).objectReferenceValue = this.serializedObject.targetObject;
					sp.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		serializedObject.ApplyModifiedProperties();
	}

	public void OnSceneGUI () {

		//draw a line to all connected spark points

		for(int i = 0; i < connectedSparkPoints.arraySize; i++) {
			if(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue != null) {
				GameObject ptA = GameObject.Find(target.name);
				GameObject ptB = 
					GameObject.Find(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue.name);

				Handles.DrawLine(ptA.transform.position, ptB.transform.position);
			}
		}
	}

}






using UnityEngine;
using UnityEditor;
using System.Collections;
	
//[CustomEditor (typeof(SparkPoint)), CanEditMultipleObjects]
public class SparkPointEditor : Editor {

	SerializedProperty connectedSparkPoints;
	SerializedProperty connectedLines;

	private static GUIContent addPoint = new GUIContent("+", "Add Point");
	//private static GUIContent minusPoint = new GUIContent("-", "Remove Point");

	public void OnEnable () {
		connectedSparkPoints = serializedObject.FindProperty("_connections");
		connectedLines = serializedObject.FindProperty("connectionLines");
	}

	public override void OnInspectorGUI () {
		serializedObject.Update();

		//these buttons adds spark point entries
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(addPoint, EditorStyles.miniButtonLeft, GUILayout.Width(50f))) {
			connectedSparkPoints.arraySize+=1;
			connectedSparkPoints.GetArrayElementAtIndex(connectedSparkPoints.arraySize-1).objectReferenceValue = null;
		}
//		if(GUILayout.Button(minusPoint, EditorStyles.miniButtonMid, GUILayout.Width(50f))) {
//			connectedSparkPoints.arraySize-=1;
//		}
		EditorGUILayout.EndHorizontal();

		//display _connections list
		for(int i = 0; i < connectedSparkPoints.arraySize; i++) {
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.PropertyField(connectedSparkPoints.GetArrayElementAtIndex(i));

			//delete from connections
			//please use this to remove entries from the list otherwise the live update code below will
			//add it right back into the list
			if(GUILayout.Button("X",GUILayout.MaxWidth(50),GUILayout.MaxHeight(15))){
				//remove self reference from sparkpoint connection you're about to delete
				if(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue != null) {
					SerializedObject so = new SerializedObject(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue);
					SerializedProperty sp = so.FindProperty("_connections");

					Debug.Log (so.targetObject.name);

					for(int j = 0; j < sp.arraySize; j++) {
						if(sp.GetArrayElementAtIndex(j).objectReferenceValue == 
						   this.serializedObject.targetObject) {
							sp.serializedObject.Update();
							sp.DeleteArrayElementAtIndex(j);
							sp.serializedObject.ApplyModifiedProperties();
							break;
						}
					}
				}

				connectedSparkPoints.DeleteArrayElementAtIndex(i);
			}

			EditorGUILayout.EndHorizontal();
		}

		//checks its children and live updates - if A contains B and B does not contain A, add A to B
		for(int i = 0; i < connectedSparkPoints.arraySize; i++) {

			//if !null
			if(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue != null) {
				SerializedObject so = new SerializedObject(connectedSparkPoints.GetArrayElementAtIndex(i).objectReferenceValue);
				SerializedProperty sp = so.FindProperty("_connections");

				bool alreadyConnected = false;
				int emptyField = -1;

				for(int j = 0; j < sp.arraySize; j++) {
					if(sp.GetArrayElementAtIndex(j).objectReferenceValue == this.serializedObject.targetObject) {
						alreadyConnected = true;
					}
					else if(sp.GetArrayElementAtIndex(j).objectReferenceValue == null) {
						if(emptyField < 0) emptyField = j;
					}
				}

				if(!alreadyConnected) {
					sp.serializedObject.Update();
					if(emptyField >= 0) {
						sp.GetArrayElementAtIndex(emptyField).objectReferenceValue = this.serializedObject.targetObject;
					}
					else {
						sp.arraySize++;
						sp.GetArrayElementAtIndex(sp.arraySize-1).objectReferenceValue = this.serializedObject.targetObject;
					}
					sp.serializedObject.ApplyModifiedProperties();
				}
			}
		

		} //end loop


		//end
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






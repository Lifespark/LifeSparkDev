using UnityEngine;
using System.Collections;
using UnityEditor;


// This editor is conflicted with SparkPointEditor
// I merged the content into that script
#if false
[CustomEditor(typeof(SparkPoint)), CanEditMultipleObjects]
public class SparkPointBossAreaEditor : Editor {
	
	/* TODO: DESIGNERS
	 * need to make this load from file,
	 * as of right now it's hard coded. -jk */
	
	//SerializedProperty connectedSparkPoints;
	//SerializedProperty connectedLines;
	
	//private static GUIContent addPoint = new GUIContent("+", "Add Point");
	//private static GUIContent minusPoint = new GUIContent("-", "Remove Point");
	
	//public void OnEnable () {
	//connectedSparkPoints = serializedObject.FindProperty("_connections");
	//connectedLines = serializedObject.FindProperty("connectionLines");
	//}
	
	public override void OnInspectorGUI () {
		DrawDefaultInspector();
		SparkPoint sparkPoint = (SparkPoint)target;
		for (int i = 0; i < 3; i++) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Boss Area: "+(i+1));
			EditorGUILayout.EndHorizontal();
			for (int j = 0; j < 4; j++) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("SP: "+(j+1));
				EditorGUILayout.ObjectField(sparkPoint._bossAreaConnections[i,j],typeof(SparkPoint),true);
				EditorGUILayout.EndHorizontal();
				
			}
		}
	}
}

#endif
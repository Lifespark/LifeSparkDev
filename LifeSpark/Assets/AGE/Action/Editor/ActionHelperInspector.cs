
using UnityEditor;
using UnityEngine;
using AGE;

[CustomEditor(typeof(ActionHelper)), CanEditMultipleObjects]
public class ActionHelperInspector : Editor
{
	public static string strHelperNameFilter = "";
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.BeginVertical();

		//add search bar
	//	EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
		GUILayout.Label("search action:", GUILayout.MaxWidth(88));
		strHelperNameFilter = EditorGUILayout.TextField(strHelperNameFilter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true));
		if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
		{
			// Remove focus if cleared
			strHelperNameFilter = "";
			GUI.FocusControl(null);
		}
		EditorGUILayout.EndHorizontal();
	
		EditorList.Show(serializedObject.FindProperty("actionHelpers"), EditorListOption.All);

		EditorGUILayout.EndVertical();
		
		serializedObject.ApplyModifiedProperties();
	}
}

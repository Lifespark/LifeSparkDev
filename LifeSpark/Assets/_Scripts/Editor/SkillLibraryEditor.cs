using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SkillLibrary)), CanEditMultipleObjects]
public class SkillLibraryEditor : Editor {

    private static GUIContent
    moveUpButtonContent = new GUIContent("\u2191", "move up"),   //eacape character. uparrow
    moveButtonContent = new GUIContent("\u2193", "move down"),
    duplicateButtonContent = new GUIContent("+", "duplicate"),
    deleteButtonContent = new GUIContent("-", "delete"),
    addButtonContent = new GUIContent("+", "add element");

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.BeginVertical();
#if false
        

        //add search bar
        //	EditorGUIUtility.LookLikeInspector();
        EditorGUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        GUILayout.Label("search action:", GUILayout.MaxWidth(88));
        strHelperNameFilter = EditorGUILayout.TextField(strHelperNameFilter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true));
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
            // Remove focus if cleared
            strHelperNameFilter = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
#endif
        //EditorList.Show(serializedObject.FindProperty("actionHelpers"), EditorListOption.All);

        SerializedProperty list = serializedObject.FindProperty("m_skillSet");

        EditorGUILayout.PropertyField(list);

        if (list.isExpanded) {
            SerializedProperty size = list.FindPropertyRelative("Array.size");
            if (size.hasMultipleDifferentValues) {
                EditorGUILayout.HelpBox("Not showing lists with different sizes.", UnityEditor.MessageType.Info);
            }
            else {
                EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
                ShowElements(list);
            }
        }


        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private static void ShowElements(SerializedProperty list) {

        for (int i = 0; i < list.arraySize; ++i) {
            SerializedProperty listMember = list.GetArrayElementAtIndex(i);
            SerializedProperty helperNameProperty = listMember.FindPropertyRelative("m_actionName");
            string thisHelperName = helperNameProperty.stringValue;

            SerializedProperty parameterProperty = listMember.FindPropertyRelative("m_parameterNames");
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), true);
//             for (int j = 0; j < parameterProperty.arraySize; j++) {
//                 EditorGUILayout.PropertyField(parameterProperty.GetArrayElementAtIndex(j), true);
//                 //EditorGUILayout.BeginHorizontal();
//                 if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth)) {
//                     parameterProperty.InsertArrayElementAtIndex(j);
// 
//                 }
//                 //EditorGUILayout.EndHorizontal();
//             }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("    ");
            if (GUILayout.Button(addButtonContent, EditorStyles.miniButtonLeft, GUILayout.Width(20f))) {
                parameterProperty.InsertArrayElementAtIndex(parameterProperty.arraySize);
            }
            if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(20f))) {
                parameterProperty.DeleteArrayElementAtIndex(parameterProperty.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();

            ShowButtons(list, i);
            EditorGUILayout.EndVertical();
        }

        if (list.arraySize == 0 && GUILayout.Button(addButtonContent, EditorStyles.miniButton)) {
            list.arraySize += 1;
        }
    }

    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20.0f);
    private static void ShowButtons(SerializedProperty list, int index) {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth)) {
            list.InsertArrayElementAtIndex(index);
        }
        if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth)) {
            int oldSize = list.arraySize;
            list.DeleteArrayElementAtIndex(index);
            if (list.arraySize == oldSize) {
                list.DeleteArrayElementAtIndex(index);
            }
        }

        if (GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth)) {
            list.MoveArrayElement(index, index - 1);
        }
        if (GUILayout.Button(moveButtonContent, EditorStyles.miniButtonRight, miniButtonWidth)) {
            list.MoveArrayElement(index, index + 1);
        }
        EditorGUILayout.EndHorizontal();

    }
}

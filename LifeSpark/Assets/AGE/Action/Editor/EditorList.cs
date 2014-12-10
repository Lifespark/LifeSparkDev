
using UnityEditor;
using UnityEngine;
using System;
using AGE;

[Flags]
public enum EditorListOption
{
	None = 0,
	ListSize = 1,
	ListLabel = 2,
	ElementLabels = 4,
	Buttons = 8,
	Default = ListSize | ListLabel | ElementLabels,
	NoElementLabels = ListSize | ListLabel,
	All = Default | Buttons

}

public static class EditorList
{
	private static GUIContent
		moveUpButtonContent = new GUIContent("\u2191", "move up"),   //eacape character. uparrow
		moveButtonContent = new GUIContent("\u2193", "move down"),
		duplicateButtonContent = new GUIContent("+", "duplicate"),
		deleteButtonContent = new GUIContent("-", "delete"),
		addButtonContent = new GUIContent("+", "add element");


	public static void Show (SerializedProperty list, EditorListOption options /*= EditorListOption.Default*/ )  
	{
		if (!list.isArray)
		{
			EditorGUILayout.HelpBox(list.name + " is neither an array nor a list", UnityEditor.MessageType.Error);
			return;
		}

		EditorGUILayout.PropertyField(list);
	
		if (list.isExpanded)
		{
			SerializedProperty size = list.FindPropertyRelative("Array.size");
			if (size.hasMultipleDifferentValues)
			{
				EditorGUILayout.HelpBox("Not showing lists with different sizes.", UnityEditor.MessageType.Info );
			}
			else
			{
				EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
				ShowElements(list, options);
			}
		}
	}

	private static void ShowElements(SerializedProperty list, EditorListOption options)
	{
		bool bShowElementLables = (options & EditorListOption.ElementLabels) != 0;
		bool bShowButtons = (options & EditorListOption.Buttons) != 0;

		for (int i=0; i<list.arraySize; ++i)
		{
			SerializedProperty listMember = list.GetArrayElementAtIndex(i);
			SerializedProperty helperNameProperty = listMember.FindPropertyRelative("helperName");
			string thisHelperName = helperNameProperty.stringValue;

			if (ActionHelperInspector.strHelperNameFilter != "" && !StringMatch.IsMatchString(thisHelperName, ActionHelperInspector.strHelperNameFilter))
			{
				continue;
			}
			EditorGUILayout.BeginVertical();
			EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), true);
			ShowButtons(list, i);
			EditorGUILayout.EndVertical();
		}

		if (bShowButtons && list.arraySize == 0 && GUILayout.Button (addButtonContent, EditorStyles.miniButton))
		{
			list.arraySize += 1;
		}
	}

	private static GUILayoutOption miniButtonWidth = GUILayout.Width(20.0f);
	private static void ShowButtons (SerializedProperty list, int index) 
	{
		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth) )
		{
			list.InsertArrayElementAtIndex(index);
		}
		if ( GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth) )
		{
			int oldSize = list.arraySize;
			list.DeleteArrayElementAtIndex(index);
			if (list.arraySize == oldSize)
			{
				list.DeleteArrayElementAtIndex(index);
			}
		}

		if ( GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth) )
		{
			list.MoveArrayElement(index, index - 1);
		}
		if ( GUILayout.Button(moveButtonContent, EditorStyles.miniButtonRight, miniButtonWidth) )
		{
			list.MoveArrayElement(index, index + 1);
		}
		EditorGUILayout.EndHorizontal();

	}

}

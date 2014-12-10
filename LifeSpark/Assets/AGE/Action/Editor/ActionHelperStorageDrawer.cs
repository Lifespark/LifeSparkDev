
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using AGE;

[CustomPropertyDrawer(typeof(ActionHelperStorage))]
public class ActionHelperStorageDrawer : PropertyDrawer
{
	private Dictionary<string, Dictionary<string, int>> mActionLoaded = new Dictionary<string, Dictionary<string, int>>();
	private Dictionary<string, List<string>> mActionTargets = new Dictionary<string, List<string>>();
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		int oldIndentLevel = EditorGUI.indentLevel;
		EditorGUI.indentLevel = oldIndentLevel ; 

		EditorGUI.BeginProperty(position, label, property);
		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		property.isExpanded =  EditorGUI.Foldout(position, property.isExpanded, label); 
		EditorGUILayout.EndHorizontal();

		if (property.isExpanded)
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.PropertyField(property.FindPropertyRelative("helperName"), new GUIContent("Helper Name")); 

			TextAsset selAction = null;
			string actionNameVal = property.FindPropertyRelative("actionName").stringValue;
			if (actionNameVal != "")
			{
				TextAsset ageAction =  Resources.Load(actionNameVal) as TextAsset;
				if (ageAction != null)
				{
					selAction = ageAction;
				}
			}
			selAction = EditorGUILayout.ObjectField("Action(xml)", selAction, typeof(TextAsset), false) as TextAsset;
			property.FindPropertyRelative("actionName").stringValue = AGE.AgeEditorUtility.GetActionNameByAsset(selAction);
		//	EditorGUILayout.PropertyField(property.FindPropertyRelative("actionName"), new GUIContent("Action Name"));

			EditorGUILayout.PropertyField(property.FindPropertyRelative("playOnStart"), new GUIContent("Play On Start"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("detectStatePath"), new GUIContent("Detect State Path"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("waitForEvents"), new GUIContent("Wait For Events"));

			List<string> targetNames = new List<string>();
			SerializedProperty targets = property.FindPropertyRelative("targets");
			targets.isExpanded = EditorGUILayout.Foldout(targets.isExpanded, new GUIContent("Targets"));
			EditorGUI.indentLevel += 1;
			if (targets.isExpanded)
			{
				string actionName = property.FindPropertyRelative("actionName").stringValue;
				targetNames = GetTargetNames(actionName);
				targets.arraySize = targetNames.Count;
				for (int i=0; i<targets.arraySize; ++i)
				{
					EditorGUILayout.PropertyField(targets.GetArrayElementAtIndex(i), new GUIContent(targetNames[i]));
				}
			}
			EditorGUI.indentLevel -= 1;

			EditorGUILayout.EndVertical();
		}
	

		EditorGUILayout.EndVertical();
		EditorGUI.EndProperty();

		EditorGUI.indentLevel = oldIndentLevel;

	}
	
	private List<string> GetTargetNames(string actionName)
	{
		List<string> outtargetNames = new List<string>();
		if (mActionTargets.TryGetValue(actionName, out outtargetNames))
		{
			return outtargetNames;
		}

		List<string> targetNames = new List<string>();
	//	targetNames.Clear();
		Dictionary<string, int> mObjIds  = LoadTemplateObjectList(actionName);
		foreach (KeyValuePair<string, int> pair in mObjIds)
		{
			targetNames.Add(pair.Key);
		}
		mActionTargets.Add(actionName, targetNames);

		return targetNames;
	}


	private Dictionary<string, int> LoadTemplateObjectList(string actionName)
	{
		Dictionary<string, int> mObjIds = new Dictionary<string, int>();

		Dictionary<string, int> result = new Dictionary<string, int>();
		if ( mActionLoaded.TryGetValue(actionName, out result))
			return result;

		TextAsset textAsset;
		if( ActionManager.Instance != null && ActionManager.Instance.ResLoader != null )
			textAsset = ActionManager.Instance.ResLoader.Load(actionName) as TextAsset;
		else
			textAsset = Resources.Load(actionName) as TextAsset;
		if (textAsset == null)
			return new Dictionary<string, int>();

		//read from xml
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(textAsset.text);
		
		XmlNode projectNode = doc.SelectSingleNode("Project");
		XmlNode templateObjectListNode = projectNode.SelectSingleNode("TemplateObjectList");
	
		//load TemplateObjectList
		if (templateObjectListNode != null)
		{
			mObjIds.Clear();
			foreach (XmlNode templateObjectNode in templateObjectListNode.ChildNodes)
			{
				string str = templateObjectNode.Attributes["objectName"].Value;
				string idStr = templateObjectNode.Attributes["id"].Value;
				int id = int.Parse(idStr);
				string isTempStr = templateObjectNode.Attributes["isTemp"].Value;
				if (isTempStr == "false")
				{
					mObjIds.Add(str, id);
				}
			}
		}
		mActionLoaded.Add(actionName, mObjIds);

		return mObjIds;
	}
}

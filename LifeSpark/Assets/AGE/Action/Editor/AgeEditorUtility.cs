using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AGE
{
public class AgeEditorUtility  
{
	public static string  GetActionNameByAsset( TextAsset asset )
	{
		string actionName = "";
		if (asset == null) return actionName;
		
		actionName = AssetDatabase.GetAssetPath(asset);
		int resIndex = actionName.LastIndexOf("Resources");
		int xmlIndex = actionName.LastIndexOf (".xml");
		if(resIndex < 0 || xmlIndex < 0)
		{
			actionName = "";
			AgeLogger.LogError(": please put action files(.xml) in [Resources] directory!");
		}
		else
		{
			actionName = actionName.Substring(resIndex+10, xmlIndex - resIndex - 10); // strip "Resources/" and extension ".xml"
		}
		
		return actionName;
	}
}
}
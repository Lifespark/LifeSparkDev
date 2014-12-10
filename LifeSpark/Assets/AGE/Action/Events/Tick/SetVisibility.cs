using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

[EventCategory("Utility")]
public class SetVisibility : TickEvent
{
	public bool enabled = true;
	public string[] excludeMeshes = new string[0];

	Dictionary<string, bool> excludeMeshNames = new Dictionary<string, bool>();

	[ObjectTemplate]
	public int targetId = 0;

	public override bool SupportEditMode ()
	{
		return true;
	}
	
	public override void Process (Action _action, Track _track)
	{
		if (excludeMeshNames.Count != excludeMeshes.Length)
		{
			foreach (string name in excludeMeshes)
			{
				string meshName = name;
				excludeMeshNames.Add(meshName, true);
			}
		}

		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null) return;

		SetChild(targetObject);
	}

	void SetChild(GameObject _obj)
	{
		string objectName = _obj.name;
		if (excludeMeshNames.ContainsKey(objectName))
			return;

		if (_obj.renderer)
			_obj.renderer.enabled = enabled;

		foreach (Transform child in _obj.transform) 
		{
			SetChild(child.gameObject);
		}
	}
}
}

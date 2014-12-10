using UnityEngine;
using System.Collections;

namespace AGE{

public class GetSubObjectTick : TickEvent {
	
	[ObjectTemplate]
	public int targetId = -1;
	
	[ObjectTemplate]
	public int parentId = -1;

	public bool isGetByName = false;
	public string subObjectName = "Mesh";

	public override bool SupportEditMode ()
	{
		return true;
	}
	
	public override void Process (Action _action, Track _track)
	{
		GameObject parentObject = _action.GetGameObject(parentId);
		if (parentObject == null)
		{
			if (isGetByName)
			{
				GameObject targetObject = GameObject.Find(subObjectName);
				if (targetObject != null)
				{
					while (_action.gameObjects.Count<=targetId)
						_action.gameObjects.Add(null);
					_action.gameObjects[targetId] = targetObject;
				}
			}
			else
			{
				while (_action.gameObjects.Count<=targetId)
					_action.gameObjects.Add(null);
				_action.gameObjects[targetId] = Camera.main.gameObject;
			}
			return;
		}

		GameObject subObject = _action.GetGameObject(targetId);
		if(isGetByName)
		{
			Transform[] transforms = parentObject.GetComponentsInChildren<Transform>();
			for (int i=0; i<transforms.Length; ++i)
			{
			    if(transforms[i].gameObject.name == subObjectName)
				{
					subObject = transforms[i].gameObject;
					break;
				}
			}
			
            if (subObject == null)
            {
                AgeLogger.Log(" Warning: Failed to find sub object by name: <color=red>[ " + subObjectName + " ]</color>! " + " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
                   "by Action:<color=yellow>[ " + _action.name + " ] </color>");
            }
		}
		else
		{
			if(parentObject.transform.childCount > 0)
				subObject = parentObject.transform.GetChild(0).gameObject;
		}
		while (_action.gameObjects.Count<=targetId)
			_action.gameObjects.Add(null);
		_action.gameObjects[targetId] = subObject;
	}
}

}

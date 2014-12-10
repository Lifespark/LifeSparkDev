using UnityEngine;
using System.Collections;

namespace AGE{
[EventCategory("Utility")]
public class GetSubObjectDuration : DurationEvent {

	public override bool SupportEditMode ()
	{
		return true;
	}
	[ObjectTemplate(true)]
	public int targetId = -1;
	
	[ObjectTemplate]
	public int parentId = -1;
	
	public bool isGetByName = false;
	public string subObjectName = "";

	public override void Enter(Action _action, Track _track)
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

		if (targetId >= 0)
		{
			while (targetId >= _action.gameObjects.Count)
				_action.gameObjects.Add(null);

			GameObject subObject = _action.GetGameObject(targetId);
			if( subObject != null ) return;
			
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
		
	public override void Leave(Action _action, Track _track)
	{
		if (targetId >= 0 && _action.GetGameObject(targetId))
			_action.gameObjects[targetId] = null;
	}
	
}

}
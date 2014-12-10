using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Utility")]
public class PlaySubAction : DurationEvent
{
	[ActionReference]
	public string actionName = "";
	public int[] gameObjectIds = new int[0];
	
	GameObject[] gameObjects = null;
	Action subAction = null;
	
	public override void Enter (Action _action, Track _track)
	{
		if (subAction != null)
			subAction.Stop();

		if (gameObjects == null)
		{
			gameObjects = new GameObject[gameObjectIds.Length];
			for (int i=0; i<gameObjectIds.Length; i++)
				gameObjects[i] = _action.GetGameObject(gameObjectIds[i]);
		}
		
		subAction = ActionManager.Instance.PlaySubAction(_action, actionName, length, gameObjects);
	}
	
	public override void Leave (Action _action, Track _track)
	{
		if (subAction != null)
		{
			subAction.Stop();
			subAction = null;
		}
	}
}
}

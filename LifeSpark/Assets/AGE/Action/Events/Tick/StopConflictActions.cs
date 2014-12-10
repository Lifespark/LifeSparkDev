using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

[EventCategory("ActionControl")]
public class StopConflictActions : TickEvent
{
	public int[] gameObjectIds = new int[0];

	public override void Process (Action _action, Track _track)
	{
		List<GameObject> gameObjects = new List<GameObject>();
		foreach (int id in gameObjectIds)
			gameObjects.Add(_action.GetGameObject(id));

		List<Action> conflictActions = new List<Action>();

		foreach (GameObject gameObject in gameObjects)
		{
			foreach (Action conflictAction in ActionManager.Instance.objectReferenceSet[gameObject])
			{
				if (conflictAction != _action && !conflictAction.unstoppable)
					conflictActions.Add(conflictAction);
			}
		}

		foreach (Action conflictAction in conflictActions)
			conflictAction.Stop();
	}
}
}

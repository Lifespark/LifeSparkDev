using UnityEngine;
using System.Collections;

namespace AGE
{	
	[EventCategory("Action Control")]
	public class StopAction : TickEvent
	{
		public bool transitAction = false;
		[ActionReference]
		public string actionName = "";
		public int[] gameObjectIds = new int[0];

		public override void Process (Action _action, Track _track)
		{
			if (transitAction && actionName.Length > 0)
			{
				GameObject[] gameObjects = new GameObject[gameObjectIds.Length];
				for (int i=0; i<gameObjectIds.Length; i++)
				{
					gameObjects[i] = _action.GetGameObject(gameObjectIds[i]);
					//dereference temp object to prevent from being destroyed and transfer it to new action
					if (gameObjects[i] != null && gameObjectIds[i]>=_action.refGameObjectsCount)
						_action.gameObjects[gameObjectIds[i]] = null;
				}
				ActionManager.Instance.PlayAction(actionName, true, false, gameObjects);
			}

			_action.Stop();
		}
	}
}

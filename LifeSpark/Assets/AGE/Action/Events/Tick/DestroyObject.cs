using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Utility")]
public class DestroyObject : TickEvent
{
	[ObjectTemplate]
	public int targetId = -1;
	
	public override void Process (Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null) return;
		ActionManager.Instance.DestroyObject(targetObject);
	}
}
}

using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Physics")]
public class SampleVelocity : DurationEvent
{
	[ObjectTemplate(typeof(Rigidbody))]
	public int targetId = 0;
	
	Vector3 startPos = Vector3.zero;
	
	public override void Enter (Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null) return;
		startPos = targetObject.transform.position;
	}
	
	public override void Leave (Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null || targetObject.rigidbody == null) return;
		Vector3 endPos = targetObject.transform.position;
		Vector3 velocity = (endPos - startPos) / length;
		targetObject.rigidbody.velocity = velocity;
	}
}
}

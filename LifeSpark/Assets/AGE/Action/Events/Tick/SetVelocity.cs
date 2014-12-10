using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Physics")]
public class SetVelocity : TickEvent
{
	static Vector3 axisWeight = new Vector3(1, 0, 1);

	public Vector3 velocity = new Vector3(0, 0, 0);
	public Vector3 angularVelocity = new Vector3(0, 0, 0);
	public bool additive = false;
	
	[ObjectTemplate(typeof(Rigidbody))]
	public int targetId = 0;

	[ObjectTemplate]
	public int objectSpaceId = -1;

	[ObjectTemplate]
	public int fromId = -1;
	[ObjectTemplate]
	public int toId = -1;

	public override void Process (Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null || targetObject.rigidbody == null) return;

		GameObject fromObject = _action.GetGameObject(fromId);
		GameObject toObject = _action.GetGameObject(toId);
		if (fromObject == null || toObject == null)
		{
			Vector3 transformedVelocity = velocity;

			GameObject objectSpace = _action.GetGameObject(objectSpaceId);
			if (objectSpace)
			{
				//in given space
				transformedVelocity = objectSpace.transform.localToWorldMatrix.MultiplyVector(velocity);
			}
			else
			{
				if (targetObject.transform.parent)
				{
					//in parent space
					transformedVelocity = targetObject.transform.parent.localToWorldMatrix.MultiplyVector(velocity);
				}
				else
				{
					//in world space
					transformedVelocity = velocity;
				}
			}

			if (additive)
				_action.GetGameObject(targetId).rigidbody.velocity += transformedVelocity;
			else
				_action.GetGameObject(targetId).rigidbody.velocity = transformedVelocity;
		}
		else
		{
			Vector3 lookDir = toObject.transform.position - fromObject.transform.position;
			lookDir = Vector3.Normalize(new Vector3(lookDir.x*axisWeight.x, lookDir.y*axisWeight.y, lookDir.z*axisWeight.z));
			Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
			
			Vector3 transformedVelocity = lookRotation * velocity;
			
			if (additive)
				_action.GetGameObject(targetId).rigidbody.velocity += transformedVelocity;
			else
				_action.GetGameObject(targetId).rigidbody.velocity = transformedVelocity;
		}

		if (additive)
			_action.GetGameObject(targetId).rigidbody.angularVelocity += angularVelocity;
		else
			_action.GetGameObject(targetId).rigidbody.angularVelocity = angularVelocity;
	}
}
}


using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Movement")]
public class ModifyTransform : TickEvent
{
	public override bool SupportEditMode ()
	{
		return true;
	}

	public bool enableTranslation = true;
	public bool currentTranslation = false;
	public Vector3 translation = Vector3.zero;
	public bool enableRotation = true;
	public bool currentRotation = false;
	public Quaternion rotation = Quaternion.identity;
	public bool enableScaling = false;
	public bool currentScaling = false;
	public Vector3 scaling = Vector3.one; //scaling is always local!!!
	
	[ObjectTemplate]
	public int targetId = 0;
	
	//won't change targetObject's hierachy, but calculate transform in it
	[ObjectTemplate]
	public int objectSpaceId = -1;

	//use in relative mode
	[ObjectTemplate]
	public int fromId = -1;
	[ObjectTemplate]
	public int toId = -1;
	public bool normalizedRelative = false;
	public static Vector3 axisWeight = new Vector3(1, 0, 1);

	public bool cubic = false;

	bool currentInitialized = false;

	public override void Process (Action _action, Track _track)
	{
		if (_action.GetGameObject(targetId) == null) return;
		
		currentInitialized = false;
		SetCurrentTransform(_action.GetGameObject(targetId).transform);

		if (enableTranslation)
			_action.GetGameObject(targetId).transform.position = GetTranslation(_action);
		if (enableRotation)
			_action.GetGameObject(targetId).transform.rotation = GetRotation(_action);
		if (enableScaling)
			_action.GetGameObject(targetId).transform.localScale = scaling;
	}

	public void CubicVectorBlend(Action _action, Track _track, TickEvent _prevEvent, float _blendWeight, bool isPos)
	{
		int prevID = _track.GetIndexOfEvent(_prevEvent);
		int curID = _track.GetIndexOfEvent(this);
		int evtCount = _track.GetEventsCount();
		int formerID = prevID-1;
		if( formerID < 0 )
		{
			if( _action.loop )
			{
				formerID = evtCount-1;
				if( formerID < 0 )
					formerID = 0;
			}
			else
				formerID = 0;
		}
		int latterId = curID+1;
		if( latterId >= evtCount )
		{
			if( _action.loop )
				latterId = 0;
			else
				latterId = curID;
		}

		ModifyTransform prevEvent = _prevEvent as ModifyTransform;
		ModifyTransform formerEvent = _track.GetEvent(formerID) as ModifyTransform;
		ModifyTransform latterEvent = _track.GetEvent(latterId) as ModifyTransform;
		
		Vector3 prevPoint = isPos ? prevEvent.GetTranslation(_action)   : prevEvent.scaling;
		Vector3 curnPoint = isPos ? GetTranslation(_action)             : scaling;
		Vector3 formPoint = isPos ? formerEvent.GetTranslation(_action) : formerEvent.scaling;
		Vector3 lattPoint = isPos ? latterEvent.GetTranslation(_action) : latterEvent.scaling;

		Vector3 ctrlPoint1;
		Vector3 ctrlPoint2;
		CurvlData.CalculateCtrlPoint(formPoint, prevPoint, curnPoint, lattPoint, out ctrlPoint1, out ctrlPoint2);
		
		float t1 = 1.0f - _blendWeight;
		float t2 = _blendWeight;
		Vector3 resultPos = prevPoint       * t1 * t1 * t1 +
							ctrlPoint1 * 3  * t1 * t1 * t2 +
							ctrlPoint2 * 3  * t1 * t2 * t2 +
							curnPoint       * t2 * t2 * t2;

		if( isPos )
			_action.GetGameObject(targetId).transform.position = resultPos;
		else
			_action.GetGameObject(targetId).transform.localScale = resultPos;
	}

	public override void ProcessBlend(Action _action, Track _track, TickEvent _prevEvent, float _blendWeight)
	{
		if (_action.GetGameObject(targetId) == null || _prevEvent == null) return;
		if (enableTranslation)
		{
			if (cubic)
			{
				CubicVectorBlend( _action, _track, _prevEvent, _blendWeight, true );
			}
			else
			{
				_action.GetGameObject(targetId).transform.position = GetTranslation(_action) * _blendWeight + (_prevEvent as ModifyTransform).GetTranslation(_action) * (1.0f - _blendWeight);
			}
		}
		if (enableRotation)
		{
			_action.GetGameObject(targetId).transform.rotation = Quaternion.Slerp((_prevEvent as ModifyTransform).GetRotation(_action), GetRotation(_action), _blendWeight);
		}
		if (enableScaling)
		{
			if (cubic)
			{
				CubicVectorBlend( _action, _track, _prevEvent, _blendWeight, false );
			}
			else
			{
				_action.GetGameObject(targetId).transform.localScale = scaling * _blendWeight + (_prevEvent as ModifyTransform).scaling * (1.0f - _blendWeight);
			}
		}
	}

	public bool HasTempObj(Action _action)
	{
		if( fromId >= 0 )
		{
			if( _action.GetGameObject(fromId) == null )
				return true;
		}
		if( toId >= 0 )
		{
			if( _action.GetGameObject(toId) == null )
				return true;
		}
		if( objectSpaceId >= 0 )
		{
			if( _action.GetGameObject(objectSpaceId) == null )
				return true;
		}
		return false;
	}

	// 0 : no depedent object
	// 1 : find depedent object
	// -1 : has depedent obejct, but not find in action
	public int HasDependObject(Action _action)
	{
		if (currentTranslation || currentRotation || currentScaling)
			return 1;
		if( fromId >= 0 )
		{
			if( _action.GetGameObject(fromId) != null )
				return 1;
			return -1;
		}
		if( toId >= 0 )
		{
			if( _action.GetGameObject(toId) != null )
				return 1;
			return -1;
		}
		if( objectSpaceId >= 0 )
		{
			if( _action.GetGameObject(objectSpaceId) != null )
				return 1;
			return -1;
		}
		return 0;
	}
	
	//calculate translation based on given object space
	public Vector3 GetTranslation(Action _action)
	{
		if (_action.GetGameObject(targetId))
			SetCurrentTransform(_action.GetGameObject(targetId).transform);

		GameObject fromObject = _action.GetGameObject(fromId);
		GameObject toObject = _action.GetGameObject(toId);

		if (fromObject && toObject)
		{
			//relative mode
			Vector3 result = new Vector3();
			Vector3 lookDir = toObject.transform.position - fromObject.transform.position;
			float length = (new Vector2(lookDir.x, lookDir.z)).magnitude;
			lookDir = Vector3.Normalize(new Vector3(lookDir.x*axisWeight.x, lookDir.y*axisWeight.y, lookDir.z*axisWeight.z));
			Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
			if (normalizedRelative)
			{
				result = lookRotation * translation;
				result = fromObject.transform.position + new Vector3(result.x * length, result.y, result.z * length);
				result += new Vector3(0.0f, translation.z * (toObject.transform.position.y - fromObject.transform.position.y), 0.0f);
			}
			else
			{
				result = fromObject.transform.position + lookRotation * translation;
				result += new Vector3(0.0f, (translation.z / length) * (toObject.transform.position.y - fromObject.transform.position.y), 0.0f);
			}
			return result;
		}
		else
		{
			GameObject objectSpace = _action.GetGameObject(objectSpaceId);
			if (objectSpace)
			{
				//in given space
				return objectSpace.transform.localToWorldMatrix.MultiplyPoint(translation);
			}
			else
			{
				GameObject targetObject = _action.GetGameObject(targetId);
				if (targetObject && targetObject.transform.parent)
				{
					//in parent space
					return targetObject.transform.parent.localToWorldMatrix.MultiplyPoint(translation);
				}
				else
				{
					//in world space
					return translation;
				}
			}	
		}
	}
	
	//calculate rotation based on given object space
	public Quaternion GetRotation(Action _action)
	{
		if (_action.GetGameObject(targetId))
			SetCurrentTransform(_action.GetGameObject(targetId).transform);

		GameObject fromObject = _action.GetGameObject(fromId);
		GameObject toObject = _action.GetGameObject(toId);
		
		if (fromObject && toObject)
		{
			//relative mode
			Vector3 lookDir = toObject.transform.position - fromObject.transform.position;
			float length = lookDir.magnitude;
			lookDir = Vector3.Normalize(new Vector3(lookDir.x*axisWeight.x, lookDir.y*axisWeight.y, lookDir.z*axisWeight.z));
			Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
			return lookRotation * rotation;
		}
		else
		{
			GameObject objectSpace = _action.GetGameObject(objectSpaceId);
			if (objectSpace)
			{
				//in given space
				return objectSpace.transform.rotation * rotation;
			}
			else
			{
				GameObject targetObject = _action.GetGameObject(targetId);
				if (targetObject && targetObject.transform.parent)
				{
					//in parent space
					return targetObject.transform.parent.rotation * rotation;
				}
				else
				{
					//in world space
					return rotation;
				}
			}	
		}
	}

	void SetCurrentTransform(Transform _transform)
	{
		if (currentInitialized) return;

		if (currentTranslation)
		{
			objectSpaceId = fromId = toId = -1;
			translation = _transform.localPosition;
		}

		if (currentRotation)
		{
			objectSpaceId = fromId = toId = -1;
			rotation = _transform.localRotation;
		}

		if (currentScaling)
			scaling = _transform.localScale;

		currentInitialized = true;
	}
}
}


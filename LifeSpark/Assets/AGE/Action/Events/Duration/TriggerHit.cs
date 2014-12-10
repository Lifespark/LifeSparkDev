using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AGE{

[EventCategory("Utility")]
public class TriggerHit : DurationCondition
{
	[ObjectTemplate]
	public int triggerId = 0;

	public string scriptName = "TriggerHelper";
	public string methodName = "GetCollisionSet";

	[ObjectTemplate]
	public int attackerId = 0;

	[ActionReference]
	public string actionName = "";

	public string[] tags = new string[0];
	public float triggerInterval = 0.5f;

	bool hit = false;

	Dictionary<GameObject, float> collideTimeMap = new Dictionary<GameObject, float>();

	public override void Enter(Action _action, Track _track)
	{
		base.Enter(_action, _track);
		collideTimeMap.Clear();
	}

	public override void Process (Action _action, Track _track, float _localTime)
	{
		GameObject triggerObject = _action.GetGameObject(triggerId);
		if (triggerObject == null) return;

		Component comp = triggerObject.GetComponent(scriptName);
		if (comp == null) return;

		hit = false;

		Type calledType = comp.GetType();
		System.Object[] args = new object[] {};
		var hitSet = calledType.InvokeMember(methodName,
		                        BindingFlags.NonPublic | 
		                        BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod | BindingFlags.Public | 
		                        BindingFlags.Static,
		                        null,
		                        comp,
		                        args) as List<GameObject>;
		
		foreach (GameObject collidedObject in hitSet)
		{
			if (collidedObject == null) continue;

			//check for tags
			if (tags.Length > 0)
			{
				bool fit = false;
				foreach (string tag in tags)
				{
					if (collidedObject.tag == tag)
					{
						fit = true;
						break;
					}
				}
				if (!fit) continue;
			}

			float collideTime = 0.0f;
			if (collideTimeMap.TryGetValue(collidedObject, out collideTime))
			{
				collideTime += Time.deltaTime * _action.PlaySpeed;
				while (collideTime > triggerInterval)
				{
					collideTime -= triggerInterval;
					TriggerAction(_action, collidedObject);					
					hit = true;
				}
				collideTimeMap[collidedObject] = collideTime;
			}
			else
			{
				collideTimeMap.Add(collidedObject, 0.0f);
				TriggerAction(_action, collidedObject);
				hit = true;
			}
		}

		base.Process(_action, _track, _localTime);
	}

	void TriggerAction(Action _action, GameObject _target)
	{
		//AgeLogger.Log("Hit" + _target.name);
		if (actionName.Length == 0) return;
		GameObject attackerObject = _action.GetGameObject(attackerId);
		GameObject triggerObject = _action.GetGameObject(triggerId);
		Action action = ActionManager.Instance.PlayAction(actionName, true, false, _target, attackerObject, triggerObject);
        //action.CopyRefParam(_action);
	}

	public override bool Check (Action _action, Track _track)
	{
		return hit;
	}
}
}

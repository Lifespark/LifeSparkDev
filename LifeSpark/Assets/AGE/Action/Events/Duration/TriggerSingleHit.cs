using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AGE{

[EventCategory("Utility")]
public class TriggerSingleHit : DurationCondition
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

	GameObject hitObject = null;

	public override void Enter(Action _action, Track _track)
	{
		base.Enter(_action, _track);
		hitObject = null;
	}
	
	public override void Process (Action _action, Track _track, float _localTime)
	{
		if (hitObject != null)
			return;

		GameObject triggerObject = _action.GetGameObject(triggerId);
		if (triggerObject == null) return;
		
		Component comp = triggerObject.GetComponent(scriptName);
		if (comp == null) return;

		Type calledType = comp.GetType();
		System.Object[] args = new object[] {};
		var hitSet = calledType.InvokeMember(methodName,
		                                     BindingFlags.NonPublic | 
		                                     BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod | BindingFlags.Public | 
		                                     BindingFlags.Static,
		                                     null,
		                                     comp,
		                                     args) as List<GameObject>;

		hitObject = null;
		
		foreach (GameObject collidedObject in hitSet)
		{
			if (collidedObject == null) continue;
			
			//check for tags
			if (tags.Length > 0)
			{
				foreach (string tag in tags)
				{
					if (collidedObject.tag == tag)
					{
						hitObject = collidedObject;
						break;
					}
				}
			}
			else
			{
				hitObject = collidedObject;
				break;
			}
		}

		if (hitObject != null)
			TriggerAction(_action, hitObject);
			
		base.Process(_action, _track, _localTime);
	}
	
	void TriggerAction(Action _action, GameObject _target)
	{
		//AgeLogger.Log("Hit" + _target.name);
		if (actionName.Length == 0) return;
		GameObject attackerObject = _action.GetGameObject(attackerId);
		GameObject triggerObject = _action.GetGameObject(triggerId);
		ActionManager.Instance.PlayAction(actionName, true, false, _target, attackerObject, triggerObject);
	}
	
	public override bool Check (Action _action, Track _track)
	{
		return hitObject != null;
	}
}

}

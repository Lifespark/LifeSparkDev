using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AGE{

[EventCategory("Condition")]
public class OnTrigger : DurationCondition
{
	[ObjectTemplate(typeof(Collider))]
	public int targetId = 0;
		
	public string scriptName = "TriggerHelper";
	public string methodName = "GetCollisionSet";

	public string[] tags = new string[0];

	public override bool Check (Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null) return false;
			
		Component comp = targetObject.GetComponent(scriptName);
		if (comp == null) return false;
			
		Type calledType = comp.GetType();
		System.Object[] args = new object[] {};
		var hitSet = calledType.InvokeMember(methodName,
		                                     BindingFlags.NonPublic | 
		                                     BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod | BindingFlags.Public | 
		                                     BindingFlags.Static,
		                                     null,
		                                     comp,
		                                     args) as List<GameObject>;

		if (tags.Length > 0)
		{
			foreach (GameObject collidedObject in hitSet)
			{
				if (collidedObject == null) continue;
				
				//check for tags
				foreach (string tag in tags)
				{
					if (collidedObject.tag == tag)
						return true;
				}
			}
		}
		else
		{
			foreach (GameObject collidedObject in hitSet)
			{
				if (collidedObject != null)
					return true;
			}
		}
		return false;
	}
}
}

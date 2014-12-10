using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace AGE{

[EventCategory("Utility")]
public class InvokeMethod : TickEvent
{
	public string scriptName = "";   //script name 
	public string methodName = "";      //method name 

	public enum ParamType
	{ 
		NoParam = 0, 
		IntParam = 1, 
		FloatParam = 2, 
		StringParam = 3,
		BoolParam = 4,
		GameObjectParam = 5,
	};

	public ParamType paramType = ParamType.NoParam;

	public int intParam = 0;
	public float floatParam = 0.0f;
	public string stringParam = "";
	public bool boolParam = false;
	public int gameObjectParam = -1;
	
	[ObjectTemplate]
	public int targetId = 0;
	
	public override void Process (Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null) return;
		
		Component comp = targetObject.GetComponent(scriptName) as Component;
		if (comp == null)
		{
			AgeLogger.LogError(targetObject.name + " doesn't have a " + scriptName + " !");
			return;
		}

		System.Object[] args = new object[] {};
		switch (paramType)
		{
			case ParamType.IntParam:
			{
				args = new System.Object[] { intParam };
				break;
			}
			case ParamType.FloatParam:
			{
				args = new System.Object[] { floatParam };
				break;
			}
			case ParamType.StringParam:
			{
				args = new System.Object[] { stringParam };
				break;
			}
			case ParamType.BoolParam:
			{
				args = new System.Object[] { boolParam };
				break;
			}
			case ParamType.GameObjectParam:
			{
				args = new System.Object[] { _action.GetGameObject(gameObjectParam) };
				break;
			}
		}

		Type calledType = comp.GetType();
		calledType.InvokeMember(methodName,
// 		                        BindingFlags.NonPublic | 
// 		                        BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod | BindingFlags.Public |
//                                 BindingFlags.Static | BindingFlags.FlattenHierarchy,
                                BindingFlags.Public 
                                | BindingFlags.Instance 
                                | BindingFlags.InvokeMethod,
		                        null,
		                        comp,
		                        args);
	}
}
}


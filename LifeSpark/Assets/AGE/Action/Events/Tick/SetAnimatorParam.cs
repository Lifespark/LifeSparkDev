using UnityEngine;

namespace AGE{

[EventCategory("Utility")]	
public class SetAnimatorParam : TickEvent
{
	public string paramName = "";
	public string paramType = "float";
	public string paramValue = "0";
	
	bool valueParsed = false;
	enum ValueType { InvalidType, FloatType, IntType, BoolType, TriggerType };
	
	float floatValue = 0.0f;
	int intValue = 0;
	bool boolValue = false;
	ValueType valueType = ValueType.InvalidType;
	
	[ObjectTemplate(typeof(Animator))]
	public int targetId = 0;

	void ParseValue()
	{
		if (!valueParsed)
		{
			paramType = paramType.ToLower();
			if (paramType == "float")
			{
				if (float.TryParse(paramValue, out floatValue))
				{
					valueType = ValueType.FloatType;
				}
				else
				{
					AgeLogger.LogError("Can't parse \"" + paramValue + "\" as float!");
					valueType = ValueType.InvalidType;
				}
			}
			else if (paramType == "int")
			{
				if (int.TryParse(paramValue, out intValue))
				{
					valueType = ValueType.IntType;
				}
				else
				{
					AgeLogger.LogError("Can't parse \"" + paramValue + "\" as int!");
					valueType = ValueType.InvalidType;
				}
			}
			else if (paramType == "bool")
			{
				if (bool.TryParse(paramValue, out boolValue))
				{
					valueType = ValueType.BoolType;
				}
				else
				{
					AgeLogger.LogError("Can't parse \"" + paramValue + "\" as bool!");
					valueType = ValueType.InvalidType;
				}
			}
			else if (paramType == "trigger")
			{
				if (bool.TryParse(paramValue, out boolValue))
				{
					valueType = ValueType.TriggerType;
				}
				else
				{
					AgeLogger.LogError("Can't parse \"" + paramValue + "\" as trigger!");
					valueType = ValueType.InvalidType;
				}
			}
			else
			{
				AgeLogger.LogError("Unsupported type \"" + paramType + "\"!");
				valueType = ValueType.InvalidType;
			}
			valueParsed = true;
		}
	}

	public override void Process (Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject == null) return;
		Animator animator = targetObject.GetComponent<Animator>();
		if (animator == null) return;

		ParseValue();
		if (valueType == ValueType.InvalidType) return;
		
		switch (valueType)
		{
		case ValueType.FloatType:
			animator.SetFloat(paramName, floatValue);
			break;
		case ValueType.IntType:
			animator.SetInteger(paramName, intValue);
			break;
		case ValueType.BoolType:
			animator.SetBool(paramName, boolValue);
			break;
		case ValueType.TriggerType:
			if (boolValue)
				animator.SetTrigger(paramName);
			else
				animator.ResetTrigger(paramName);
			break;
		}
	}
}
}

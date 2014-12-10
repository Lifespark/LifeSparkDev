using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; 

namespace AGE
{

	//base class of all track events
	public abstract class BaseEvent
	{
		public float time = 0;

		public virtual bool SupportEditMode() { return false; }

		public virtual Dictionary<string, bool> GetAssociatedActions() 
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();
			
			System.Type myType = this.GetType();
			System.Type stringType = typeof(string);
			System.Type stringArrayType = typeof(string[]);
			while (myType == typeof(BaseEvent) || myType.IsSubclassOf(typeof(BaseEvent)))
			{
				FieldInfo[] fieldInfos = myType.GetFields(BindingFlags.Instance | BindingFlags.Public); 
				foreach(System.Reflection.FieldInfo fieldInfo in fieldInfos)
				{
					if (System.Attribute.IsDefined(fieldInfo, typeof(ActionReference)))
					{
						if (fieldInfo.FieldType == stringType)
						{
							result.Add(fieldInfo.GetValue(this) as string, true);
						}
						else if (fieldInfo.FieldType == stringArrayType)
						{
							string[] array = fieldInfo.GetValue(this) as string[];
							foreach (string str in array)
								result.Add(str, true);
						}
					}
				}
				myType = myType.BaseType;
			}
			
			return result;
		}

		public virtual Dictionary<string, bool> GetAssociatedResources() 
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();

			System.Type myType = this.GetType();
			System.Type stringType = typeof(string);
			System.Type stringArrayType = typeof(string[]);
			while (myType == typeof(BaseEvent) || myType.IsSubclassOf(typeof(BaseEvent)))
			{
				FieldInfo[] fieldInfos = myType.GetFields(BindingFlags.Instance | BindingFlags.Public); 
				foreach(System.Reflection.FieldInfo fieldInfo in fieldInfos)
				{
					if (System.Attribute.IsDefined(fieldInfo, typeof(AssetReference)))
					{
						if (fieldInfo.FieldType == stringType)
						{
							result.Add(fieldInfo.GetValue(this) as string, true);
						}
						else if (fieldInfo.FieldType == stringArrayType)
						{
							string[] array = fieldInfo.GetValue(this) as string[];
							foreach (string str in array)
								result.Add(str, true);
						}
					}
				}
				myType = myType.BaseType;
			}

			return result;
		}

		public BaseEvent Clone()
		{
			System.Type myType = this.GetType();
			BaseEvent result = System.Activator.CreateInstance(myType) as BaseEvent;
			while (myType == typeof(BaseEvent) || myType.IsSubclassOf(typeof(BaseEvent)))
			{
				FieldInfo[] fieldInfos = myType.GetFields(BindingFlags.Instance | BindingFlags.Public); 
				foreach(System.Reflection.FieldInfo fieldInfo in fieldInfos)
		            fieldInfo.SetValue(result, fieldInfo.GetValue(this));
				myType = myType.BaseType;
			}
			result.waitForConditions = waitForConditions;
			return result;
		}

		public bool CheckConditions(Action _action)
		{
			foreach (int conditionId in waitForConditions.Keys)
			{
				if (conditionId >= 0 && conditionId < _action.GetConditionCount())
				{
					if (_action.GetCondition(_action.tracks[conditionId] as Track) != waitForConditions[conditionId])
						return false;
				}
			}
			return true;
		}

		public Dictionary<int, bool> waitForConditions = new Dictionary<int, bool>();

		public Track track = null;
	}
}

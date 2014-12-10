using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Utility")]
public class CreateObjectTick : TickEvent
{
	public override bool SupportEditMode ()
	{
		return true;
	}

	[ObjectTemplate(true)]
	public int targetId = -1;
	
	[ObjectTemplate]
	public int parentId = -1;

	[ObjectTemplate]
	public int objectSpaceId = -1;

	//use in relative mode
	[ObjectTemplate]
	public int fromId = -1;

	[ObjectTemplate]
	public int toId = -1;

	public bool normalizedRelative = false;

	[AssetReference]
	public string prefabName = "";

	public bool recreateExisting = true;
	
	public bool modifyTranslation = true;
	public Vector3 translation = Vector3.zero;
	public bool modifyRotation = false;
	public Quaternion rotation = Quaternion.identity;
	public bool modifyScaling = false;
	public Vector3 scaling = Vector3.one;

	[ActionReference]
	public string actionName = "";
	public int[] gameObjectIds = new int[0];

	public bool   enableLayer = false;
	public int    layer = 0;
	public bool   enableTag = false;
	public string tag = ""; 

	public bool applyActionSpeedToAnimation = true;
	public bool applyActionSpeedToParticle = true;
	
	public override void Process (Action _action, Track _track)
	{
		GameObject parentObj = _action.GetGameObject(parentId);
		GameObject objectSpace = _action.GetGameObject(objectSpaceId);			
		GameObject fromObj = _action.GetGameObject(fromId);
		GameObject toObj = _action.GetGameObject(toId);
		Vector3 newPos = new Vector3(0,0,0);
		Quaternion newRot = new Quaternion(0,0,0,1);
		
		if (fromObj != null && toObj != null )
		{
			CalRelativeTransform( fromObj, toObj, ref newPos, ref newRot );
		}
		else if ( parentObj )
		{
			if (modifyTranslation)
				newPos = parentObj.transform.localToWorldMatrix.MultiplyPoint(translation);
			if (modifyRotation)
				newRot = parentObj.transform.rotation * rotation;
		}
		else if (objectSpace)
		{
			if (modifyTranslation)
				newPos = objectSpace.transform.localToWorldMatrix.MultiplyPoint(translation);
			if (modifyRotation)
				newRot = objectSpace.transform.rotation * rotation;
		}
		else
		{
			if (modifyTranslation)
				newPos = translation;
			if (modifyRotation)
				newRot = rotation;
		}

		if (targetId >= 0)
		{
			while (targetId >= _action.gameObjects.Count)
				_action.gameObjects.Add(null);

			if (recreateExisting && _action.gameObjects[targetId] != null)
			{
				if( applyActionSpeedToAnimation )
					_action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_Anim, _action.gameObjects[targetId]);
				if( applyActionSpeedToParticle )
					_action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, _action.gameObjects[targetId]);
				if( !applyActionSpeedToAnimation && !applyActionSpeedToParticle )
					_action.RemoveTempObject(Action.PlaySpeedAffectedType.ePSAT_SelfSpeed, _action.gameObjects[targetId]);

				ActionManager.DestroyGameObject(_action.gameObjects[targetId]);
				_action.gameObjects[targetId] = null;
			}

			GameObject newObject = null;
			if (_action.gameObjects[targetId] == null)
			{
				GameObject prefab = (GameObject)ActionManager.Instance.ResLoader.Load(prefabName);
				if (prefab != null)
				{
					if( !modifyRotation )
						newRot = prefab.transform.rotation;
					newObject = ActionManager.InstantiateObject(prefab, newPos, newRot) as GameObject;
				}
				else
				{
					newObject = new GameObject("TempObject");
					newObject.transform.position = newPos;
					newObject.transform.rotation = newRot;
				}
				_action.gameObjects[targetId] = newObject;

				if( applyActionSpeedToAnimation )
					_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Anim, newObject);
				if( applyActionSpeedToParticle )
					_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, newObject);
				if( !applyActionSpeedToAnimation && !applyActionSpeedToParticle )
					_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_SelfSpeed, newObject);
			}
			else
			{
				//use existing object
				return;
//				newObject = _action.gameObjects[targetId];
//				if (newObject.GetComponent<ActionHelper>())
//					newObject.GetComponent<ActionHelper>().Restart();
			}

			if(enableLayer)
			{
				newObject.layer = layer;

				Transform[] transforms = newObject.GetComponentsInChildren<Transform>();
				for (int i=0; i<transforms.Length; ++i)
				{
					transforms[i].gameObject.layer = layer;
				}
			}
			
			if(enableTag)
			{
				newObject.tag = tag;

				Transform[] transforms = newObject.GetComponentsInChildren<Transform>();
				for (int i=0; i<transforms.Length; ++i)
				{
					transforms[i].gameObject.tag = tag;
				}
			}
			
			if(modifyScaling)
			{
				ParticleSystem[] particsys = newObject.GetComponentsInChildren<ParticleSystem>();
				if (newObject.particleSystem || particsys != null)
				{	
					for( int i = 0; i < particsys.Length; i++)
					{
						particsys[i].startSize     *= scaling.x;
						particsys[i].startLifetime *= scaling.y;
						particsys[i].startSpeed    *= scaling.z;
						particsys[i].transform.localScale *= scaling.x;
					}
				}
			}
			
			//LifeTimeHelper lifeTimeHelper = newObject.AddComponent<LifeTimeHelper>();
			LifeTimeHelper lifeTimeHelper = LifeTimeHelper.CreateTimeHelper(newObject);
			lifeTimeHelper.startTime = _action.CurrentTime;

			newObject.transform.parent = (parentObj ? parentObj.transform : null);

			if (modifyScaling)
				newObject.transform.localScale = scaling; //scaling is always local!!!

			if (actionName.Length > 0)
			{
				GameObject[] gameObjects = new GameObject[gameObjectIds.Length];
				for (int i=0; i<gameObjectIds.Length; i++)
				{
					if (gameObjectIds[i] < 0)
						gameObjects[i] = newObject;
					else
						gameObjects[i] = _action.GetGameObject(gameObjectIds[i]);
				}
				ActionManager.Instance.PlayAction(actionName, true, false, gameObjects);
			}
		}
		else
		{
			//no auto recycling
			//always create new object
			GameObject prefab = (GameObject)ActionManager.Instance.ResLoader.Load(prefabName);
			if( prefab == null )
			{
				AgeLogger.LogError(" Failed to Load prefab: <color=red>[ " + prefabName + " ]</color>! "+ " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
				               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
				return;
			}
			if( !modifyRotation )
				newRot = prefab.transform.rotation;
			GameObject newObject = ActionManager.InstantiateObject(prefab, newPos, newRot) as GameObject;
			if (newObject == null)
			{
				AgeLogger.LogError("Failed to create object. Prefab \"" + prefabName + "\" doesn't exist!");
				return;
			}

			if( applyActionSpeedToAnimation )
				_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Anim, newObject);
			if( applyActionSpeedToParticle )
				_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, newObject);
			if( !applyActionSpeedToAnimation && !applyActionSpeedToParticle )
				_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_SelfSpeed, newObject);

			if(enableLayer)
			{
				newObject.layer = layer;

				Transform[] transforms = newObject.GetComponentsInChildren<Transform>();
				for (int i=0; i<transforms.Length; ++i)
				{
					transforms[i].gameObject.layer = layer;
				}
			}
			
			if(enableTag)
			{
				newObject.tag = tag;

				Transform[] transforms = newObject.GetComponentsInChildren<Transform>();
				for (int i=0; i<transforms.Length; ++i)
				{
					transforms[i].gameObject.tag = tag;
				}
			}

			if(modifyScaling)
			{
				ParticleSystem[] particsys = newObject.GetComponentsInChildren<ParticleSystem>();
				if (newObject.particleSystem || particsys != null)
				{	
					for( int i = 0; i < particsys.Length; i++)
					{
						particsys[i].startSize     *= scaling.x;
						particsys[i].startLifetime *= scaling.y;
						particsys[i].startSpeed    *= scaling.z;
						particsys[i].transform.localScale *= scaling.x;
					}
				}
			}

			//LifeTimeHelper lifeTimeHelper = newObject.AddComponent<LifeTimeHelper>();
			LifeTimeHelper lifeTimeHelper = LifeTimeHelper.CreateTimeHelper(newObject);
			lifeTimeHelper.startTime = _action.CurrentTime;
			if (newObject.particleSystem != null)
				lifeTimeHelper.checkParticleLife = true;

			newObject.transform.parent = parentObj ? parentObj.transform : null;
			
			if (modifyScaling)
				newObject.transform.localScale = scaling; //scaling is always local!!!

			if (actionName.Length > 0)
			{
				GameObject[] gameObjects = new GameObject[gameObjectIds.Length];
				for (int i=0; i<gameObjectIds.Length; i++)
				{
					if (gameObjectIds[i] < 0)
						gameObjects[i] = newObject;
					else
						gameObjects[i] = _action.GetGameObject(gameObjectIds[i]);
				}
				ActionManager.Instance.PlayAction(actionName, true, false, gameObjects);
			}
		}
	}

	void CalRelativeTransform( GameObject fromObj, GameObject toObj, ref Vector3 pos, ref Quaternion rot )
	{
		if( modifyTranslation )
		{
			//relative mode
			Vector3 result = new Vector3();
			Vector3 lookDir = toObj.transform.position - fromObj.transform.position;
			float length = (new Vector2(lookDir.x, lookDir.z)).magnitude;
			lookDir = Vector3.Normalize(new Vector3(lookDir.x*ModifyTransform.axisWeight.x, lookDir.y*ModifyTransform.axisWeight.y, lookDir.z*ModifyTransform.axisWeight.z));
			Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
			if (normalizedRelative)
			{
				result = lookRotation * translation;
				result = fromObj.transform.position + new Vector3(result.x * length, result.y, result.z * length);
				result += new Vector3(0.0f, translation.z * (toObj.transform.position.y - fromObj.transform.position.y), 0.0f);
			}
			else
			{
				result = fromObj.transform.position + lookRotation * translation;
				result += new Vector3(0.0f, (translation.z / length) * (toObj.transform.position.y - fromObj.transform.position.y), 0.0f);
			}
			pos = result;
		}
		if( modifyRotation )
		{
			//relative mode
			Vector3 lookDir = toObj.transform.position - fromObj.transform.position;
			float length = lookDir.magnitude;
			lookDir = Vector3.Normalize(new Vector3(lookDir.x*ModifyTransform.axisWeight.x, lookDir.y*ModifyTransform.axisWeight.y, lookDir.z*ModifyTransform.axisWeight.z));
			Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
			Quaternion result = lookRotation * rotation;
			rot = result;
		}
	}
}
}

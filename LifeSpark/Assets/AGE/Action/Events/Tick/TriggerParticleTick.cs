using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Animation")]
public class TriggerParticleTick : TickEvent
{
	public override bool SupportEditMode ()
	{
		return true;
	}

	[ObjectTemplate]
	public int targetId = 0;
	
	[ObjectTemplate]
	public int objectSpaceId = -1;

	[AssetReference]
	public string resourceName = "";
	
	[SubObject]
	public string bindPointName = "";
	
	public Vector3 bindPosOffset = new Vector3(0, 0, 0);
	public Quaternion bindRotOffset = new Quaternion(0, 0, 0, 1);
	public Vector3 scaling = new Vector3(1.0f, 1.0f, 1.0f); //size scale, lifetime scale, speed scale

	public bool   enableLayer = false;
	public int    layer = 0;
	public bool   enableTag = false;
	public string tag = ""; 

	public bool applyActionSpeedToParticle = true;
	
	GameObject particleObject = null;
	
	public override void Process(Action _action, Track _track)
	{
		Vector3 newPos = bindPosOffset;
		Quaternion newRot = bindRotOffset;
		GameObject targetObject = _action.GetGameObject(targetId);
		GameObject objectSpace = _action.GetGameObject (objectSpaceId);
		Transform parent = null;
		Transform virtualParent = null;
		if( bindPointName.Length == 0 )
		{
			if( targetObject != null )
				parent = targetObject.transform;
			else if( objectSpace != null )
				virtualParent = objectSpace.transform;
		}
		else
		{
			Transform bindPoint = null;
			if( targetObject != null )
			{
                GameObject bindObject = SubObject.FindSubObject(targetObject, bindPointName);
                if (bindObject != null)
                {
                    bindPoint = bindObject.transform;
                    if (bindPoint != null)
                        parent = bindPoint;
                }
                else
                {
                    AgeLogger.Log(" Warning: Failed to find parent: [" + targetObject.name + "] --bindPointName: <color=red>[ " + bindPointName + " ]</color>! " + " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
                   "by Action:<color=yellow>[ " + _action.name + " ] </color>");

                    parent = targetObject.transform;
                }
			}
			else if( objectSpace != null )
			{
                GameObject bindObject = SubObject.FindSubObject(objectSpace, bindPointName);
                if (bindObject != null)
                {
                    bindPoint = bindObject.transform;
                    if (bindPoint != null)
                        virtualParent = bindPoint;
                }
                else
                {
                    AgeLogger.Log(" Warning: Failed to find objectSpace:[" + objectSpace.name + "] --bindPointName: <color=red>[ " + bindPointName + " ]</color>! " + " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
                   "by Action:<color=yellow>[ " + _action.name + " ] </color>");

                    virtualParent = objectSpace.transform;
                }
			}
		}
		if( parent != null )
		{			
			newPos = parent.localToWorldMatrix.MultiplyPoint(bindPosOffset);
			newRot = parent.rotation * bindRotOffset;
		}
		else if( virtualParent != null )
		{
			newPos = virtualParent.localToWorldMatrix.MultiplyPoint(bindPosOffset);
			newRot = virtualParent.rotation * bindRotOffset;
		}
		GameObject prefab = (GameObject)ActionManager.Instance.ResLoader.Load(resourceName);
		if( !prefab )
		{
			AgeLogger.LogError(" Failed to find prefab: <color=red>[ " + resourceName + " ]</color>! "+ " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
			               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
			return;
		}
		particleObject = ActionManager.InstantiateObject(prefab, newPos, newRot) as GameObject;
		particleObject.transform.parent = parent;

//		particleObject = (GameObject)ActionManager.Instance.resLoader.Load(resourceName) as GameObject;
//		GameObject targetObject = _action.GetGameObject(targetId);
//		if (bindPointName.Length == 0)
//		{
//			particleObject.transform.parent = targetObject.transform;
//			particleObject.transform.localPosition = bindPosOffset;
//			particleObject.transform.localRotation = bindRotOffset;
//		}
//		else
//		{
//			Transform bindPoint = SubObject.FindSubObject(targetObject, bindPointName).transform;//targetObject.transform.Find(bindPointName);
//			if (bindPoint != null)
//				particleObject.transform.parent = bindPoint;
//			else
//				particleObject.transform.parent = targetObject.transform;
//			particleObject.transform.localPosition = bindPosOffset;
//			particleObject.transform.localRotation = bindRotOffset;
//		}

		//particleObject.transform.localScale = scaling;

		if(enableLayer)
		{
			particleObject.layer = layer;
			
			Transform[] transforms = particleObject.GetComponentsInChildren<Transform>();
			for (int i=0; i<transforms.Length; ++i)
			{
				transforms[i].gameObject.layer = layer;
			}
		}
		
		if(enableTag)
		{
			particleObject.tag = tag;
			
			Transform[] transforms = particleObject.GetComponentsInChildren<Transform>();
			for (int i=0; i<transforms.Length; ++i)
			{
				transforms[i].gameObject.tag = tag;
			}
		}

		//LifeTimeHelper lifeTimeHelper = particleObject.AddComponent<LifeTimeHelper>();
		LifeTimeHelper lifeTimeHelper = LifeTimeHelper.CreateTimeHelper(particleObject);
		lifeTimeHelper.startTime = _action.CurrentTime;

		if (particleObject.particleSystem)
		{
			ParticleSystem[] particsys = particleObject.GetComponentsInChildren<ParticleSystem>();
			for( int i = 0; i < particsys.Length; i++)
			{
				particsys[i].startSize     *= scaling.x;
				particsys[i].startLifetime *= scaling.y;
				particsys[i].startSpeed    *= scaling.z;
				particsys[i].transform.localScale *= scaling.x;
			}

			particleObject.particleSystem.Play();
			lifeTimeHelper.checkParticleLife = true;
		}

		if( particleObject != null )
		{
			if( applyActionSpeedToParticle )
				_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_Fx, particleObject);
			else
				_action.AddTempObject(Action.PlaySpeedAffectedType.ePSAT_SelfSpeed, particleObject);
		}
	}
}
}

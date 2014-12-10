using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class TriggerHelper : MonoBehaviour
{
	void OnTriggerEnter(Collider _other)
	{
		GameObject collisionObject = _other.gameObject;
		if (!collisionSet.ContainsKey(collisionObject))
			collisionSet.Add(collisionObject, Time.realtimeSinceStartup);
	}
	
	void OnTriggerExit(Collider _other)
	{
		GameObject collisionObject = _other.gameObject;
		if (collisionSet.ContainsKey(collisionObject))
			collisionSet.Remove(collisionObject);
	}
	
	public List<GameObject> GetCollisionSet()
	{
		//fix collisionSet
		List<GameObject> badResults = new List<GameObject>();
		foreach (GameObject obj in collisionSet.Keys)
		{
			if (obj == null || obj.collider==null || obj.collider.enabled==false)
				badResults.Add(obj);
		}
		foreach (GameObject obj in badResults)
			collisionSet.Remove(obj);

		List<GameObject> result = new List<GameObject>();
		foreach (GameObject obj in collisionSet.Keys)
			result.Add(obj);
		return result;
	}
	
	Dictionary<GameObject, float> collisionSet = new Dictionary<GameObject, float>();
}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class LifeTimeHelper : MonoBehaviour
{
	public float startTime = 0.0f;
	public bool checkParticleLife = false;
	private ParticleSystem []particles = null;

	void Start()
	{
		particles = GetComponentsInChildren<ParticleSystem>();
	}

	void Update()
	{
		if (checkParticleLife && particleSystem.playOnAwake && particleSystem!=null && particleSystem.isStopped)
			ActionManager.DestroyGameObject(gameObject);
	}

	public static LifeTimeHelper CreateTimeHelper(GameObject obj)
	{
		LifeTimeHelper comp = obj.GetComponent<LifeTimeHelper>();
		if (comp == null)
		{
			comp = obj.AddComponent<LifeTimeHelper>();
		}
		else
		{	
			comp.particles = comp.GetComponentsInChildren<ParticleSystem>();
		}
		comp.active = true;
		comp.enabled = true;
		comp.startTime = 0.0f;
		comp.checkParticleLife = false;
		return comp;
	}
}
}

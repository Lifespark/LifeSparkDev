using UnityEngine;
using System.Collections;

/// <summary>
/// Derived from Attack class, instantiated as a ranged attack object with relevant fields
/// </summary>
public class RangedAttack : Attack {

	public int m_attackSpeed;
	public int m_range;

	public RangedAttack(AttackType attackType, HitObject hitObject, int attackSpeed, int range): base(attackType, hitObject){
		m_attackSpeed = attackSpeed;
		m_range = range;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

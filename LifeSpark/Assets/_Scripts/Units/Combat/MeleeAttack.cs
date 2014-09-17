using UnityEngine;
using System.Collections;

/// <summary>
/// Derived from Attack class, instantiated as a melee attack object with relevant fields
/// </summary>
public class MeleeAttack : Attack {

	public int m_attackSpeed;

	public MeleeAttack(AttackType attackType, HitObject hitObject, int attackSpeed): base(attackType, hitObject){
		m_attackSpeed = attackSpeed;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Derived from Attack class, instantiated as a melee attack object with relevant fields
/// </summary>
public class MeleeAttack : Attack {

	public int m_attackSpeed;

	public void CreateMeleeAttack(AttackType attackType, Hit hitObject, int attackSpeed){
		CreateAttack(attackType, hitObject);
		m_attackSpeed = attackSpeed;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

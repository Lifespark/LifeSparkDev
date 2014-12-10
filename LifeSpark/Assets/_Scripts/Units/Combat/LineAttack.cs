using UnityEngine;
using System.Collections;

/// <summary>
/// Derived from Attack class, instantiated as a line attack object with relevant fields
/// </summary>
public class LineAttack : Attack {

	public int m_range;
	public int m_duration;
	public int m_cooldown;

	public void CreateLineAttack(AttackType attackType, Hit hitObject, int range, int duration, int cooldown){

		CreateAttack(attackType, hitObject);
		m_range = range;
		m_duration = duration;
		m_cooldown = cooldown;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

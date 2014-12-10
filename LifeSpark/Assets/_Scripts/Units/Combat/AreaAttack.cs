using UnityEngine;
using System.Collections;

/// <summary>
/// Derived from Attack class, instantiated as an area attack object with relevant fields
/// </summary>
public class AreaAttack : Attack {

	public int m_radius;
	public int m_duration;
	public int m_cooldown;
	public bool m_isPlayerOrigin;

	public void CreateAreaAttack(AttackType attackType, Hit hitObject, int radius, int duration, int cooldown, bool isPlayerOrigin){
		CreateAttack(attackType, hitObject);
		m_radius = radius;
		m_duration = duration;
		m_cooldown = cooldown;
		m_isPlayerOrigin = isPlayerOrigin;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

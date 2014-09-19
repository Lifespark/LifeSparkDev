using UnityEngine;
using System.Collections;

/// <summary>
/// Encapsulates a certain attack behavior with its associated hit object and type
/// </summary>
 public class Attack : MonoBehaviour {



	public enum AttackType {
		Melee,
		Ranged,
		Line,
		Area
	};

	public AttackType m_attackType;
	public HitObject m_hitObject;
	// Use this for initialization

	public Attack(AttackType attackType, HitObject hitObject){
		m_attackType = attackType;
		m_hitObject = hitObject;
	}

	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

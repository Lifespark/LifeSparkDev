using UnityEngine;
using System.Collections;

public class Projectile : LSMonoBehaviour {

	private CombatManager combatManager;

	// Use this for initialization
	void Start () {
		GameObject manager = GameObject.Find ("Manager");
		combatManager = (CombatManager) manager.GetComponent ("CombatManager");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

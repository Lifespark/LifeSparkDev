using UnityEngine;
using System.Collections;

public class UnitObject : LSMonoBehaviour {
	
	protected CombatManager combatManager;

	// Use this for initialization
	void Start () {
		combatManager = (CombatManager) GameObject.Find ("Manager").GetComponent ("CombatManager");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

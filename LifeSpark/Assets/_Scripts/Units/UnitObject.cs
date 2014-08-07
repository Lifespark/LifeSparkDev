using UnityEngine;
using System.Collections;

public class UnitObject : LSMonoBehaviour {
	
	protected CombatManager combatManager;
	
	public int unitHealth;
	public int baseAttack;
	public int DPSReceived;
	
	public int DPSTimer;
	const int DPSTime = 100;

	// Use this for initialization
	void Start () {
		combatManager = (CombatManager) GameObject.Find ("Manager").GetComponent ("CombatManager");
		DPSReceived = 0;
		DPSTimer = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void UnitUpdate () {
		DPSTimer++;
		
		if (DPSTimer >= DPSTime) {//Time to inflict 'em damages
			DPSTimer = 0;
			
			unitHealth -= DPSReceived;
		}
		
		if (unitHealth <= 0) {
			this.enabled = false;
			
			Debug.Log ("Player died!");
			
		}
		
	}
	
	public void setUnitBeingAttacked(int attackValue, bool incoming)
	{
		if (incoming)
			DPSReceived += attackValue;
		else
			DPSReceived -= attackValue;
		
	}
}

using UnityEngine;
using System.Collections;

public class UnitObject : LSMonoBehaviour {
	
	protected CombatManager combatManager;
	
	public float unitHealth;
	public int baseAttack;

	// Use this for initialization
	void Start () {
		combatManager = (CombatManager) GameObject.Find ("Manager").GetComponent ("CombatManager");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void UnitUpdate () {
		if (unitHealth <= 0) {
			this.enabled = false;
			
			Debug.Log ("Player died!");
		}
	}

	//Distributes damage evenly over a duration of time. It keeps looping untill there is no time left.
	//Input: The amount of damage to distribute and how long it should run
	IEnumerator DamageOverTime(int damageRemaining, int timeRemaining) {
		if (timeRemaining>0) {
			float damageToInflict = damageRemaining / timeRemaining;
			unitHealth -= damageToInflict;
			yield return new WaitForSeconds (1);
			StartCoroutine (DamageOverTime((int)(damageRemaining-damageToInflict),(int)(timeRemaining-1)));
		} else {
			unitHealth -= damageRemaining;
		}
	}

	//Starts distributing damage over time
	//Input: Origional amount of damage to be done and origional amount of time to deal it in
	public void receiveAttack(int attackValue, int timeDelt) {
		StartCoroutine (DamageOverTime(attackValue,timeDelt));
	}
}

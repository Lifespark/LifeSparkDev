using UnityEngine;
using System.Collections;

public class CombatManager : LSMonoBehaviour {

    private GameObject tempPlayer;
    private GameObject tempTarget;

	// Use this for initialization
	void Start () {
		NetworkManager tempNM = GameObject.Find ("Manager").GetComponent<NetworkManager>();
		MetaPlayer[] tempMPs = tempNM.GetMetaPlayers ();
		MetaPlayer masterPlayer = tempMPs [0];
		// decide masterPlayer by smallest ID, he manage other players like MasterClient
		foreach (MetaPlayer forMP in tempMPs) {
			if(forMP.ID < masterPlayer.ID) {
				masterPlayer = forMP;
			}
		}
		//Debug.Log ("The MasterPlayer is " + masterPlayer.name);
		
		// if this is the masterPlayer, manage and set each client to 1 player (only has 1 masterPlayer)
		if (tempNM.playerName.Equals (masterPlayer.name)) {
			//Debug.Log ("I'm the MasterPlayer.");
			// 
			int playerNum = 1;
			
			foreach (MetaPlayer forMP in tempMPs) {
				photonView.RPC ("RPC_setMine", PhotonTargets.All, "Player"+playerNum, forMP.name);
				//
				playerNum = playerNum + 1;
			}
			// check
			/*for (int i=1; i<=4; i++){
				Debug.Log ("Player" + i + ":" + GameObject.Find("Players/Player"+i).GetComponent<Player>().playerName);
			}*/
		}	
	}
	
	// Update is called once per frame
	void Update () {

	}

	/// <summary>
	/// Create missile targeted at player with targetName shot by attacker Name
	/// </summary>
	[RPC]
	void RPC_ShootMissile(string attackerName, string targetName){
		
		//Create missile targeted at player with targetName, store attacking player name in the missile
		GameObject attacker = GameObject.Find("Players/" + attackerName);
		GameObject target = GameObject.Find("Players/" + targetName);

		Player attackerPlayer = attacker.GetComponent<Player>();
		
		//missile prefab stored in player or missile itself?
		GameObject missile = (GameObject)Instantiate(attackerPlayer.missilePrefab, attackerPlayer.transform.position, 
		                                            Quaternion.LookRotation(target.transform.position - attackerPlayer.transform.position));
		missile.GetComponent<Renderer>().material.color = (attackerPlayer.team == 1) ? Color.red : Color.blue;

		Projectile missileProjectile = missile.GetComponent<Projectile>();
		missileProjectile.m_owner = attacker;
		missileProjectile.m_target = target;
		//other properties set according to attacker properties
	}

	/// <summary>
	/// projectile hits, target of the projectile takes damage based on the attack
	/// </summary>
	void MissileHit(){
		string attackerName = "";
		string targetName = "";
		
		GameObject tempPlayer;
		tempPlayer = GameObject.Find ("Players/" + "***attackerName to be passed from the projectile***");
		
		GameObject tempTarget;
		tempTarget = GameObject.Find("Players/" + "***targetName to be determined from the missile hit***");
		
		GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
		                                                 PhotonTargets.All,
		                                                 targetName,
		                                                 attackerName,
		                                                 tempPlayer.GetComponent<Player>().baseAttack,
		                                                 0);
		
	}

	/// <summary>
	/// Visualization of line attack
	/// </summary>
	/// <param name="attackerName">Attacker, where we retrieve position, prefab to use, and attack distance .</param>
	/// <param name="targetDir">Target direction to cast attack.</param>
	/// <param name="targetSrc">Target source - for now the attack originates from the attacker, but this may change in the future.</param>
	[RPC]
	public void RPC_lineAttackVisualization (string attackerName, Vector3 targetSrc, Vector3 targetDir) {

		//now uses prefabs for visual effect
		//consider using projections to handle irregular terrain?
		GameObject attacker = GameObject.Find ("Players/" + attackerName);
		Player attackerPlayer = attacker.GetComponent<Player>();

		//player pos is 1.2 units above ground, reset prefab y value to zero
		Vector3 realPos = targetSrc; 
		realPos.y = 0.0f;	

		//instantiate, scales, and kills the effect - scaling needs to be moved to update for animated effects (eg. growing)
		//would not be necessary if prefab itself is an animated object
		//GameObject lineAttack = (GameObject)PhotonNetwork.Instantiate(attackerPlayer.lineAttackPrefab.name, realPos, Quaternion.LookRotation(targetDir), 0);
		GameObject lineAttack = (GameObject)Instantiate(attackerPlayer.lineAttackPrefab, realPos, Quaternion.LookRotation(targetDir));
		lineAttack.transform.localScale = (new Vector3(1,1,attackerPlayer.lineAttackDist));

		//temporary coloring mechanic
		lineAttack.GetComponentInChildren<Renderer>().material.color = (attackerPlayer.team == 1) ? Color.red : Color.blue;

		//lifetime is temporary set to 1 second - should set a value either in Player component or an attackType Object
		Destroy(lineAttack, 1.0f);
		//PhotonNetwork.Destroy(lineAttack.GetPhotonView());

	}

	/// <summary>
	/// Visualization of Area Attacks
	/// </summary>
	/// <param name="attackerName">Attacker - radius of AOE is retrieved.</param>
	/// <param name="targetPt">Target location.</param>
	[RPC]
	public void RPC_areaAttackVisualization(string attackerName, Vector3 targetPt) {

		GameObject attacker = GameObject.Find ("Players/" + attackerName);
		Player attackerPlayer = attacker.GetComponent<Player>();

		Vector3 realPos = targetPt; 
		realPos.y = 0.0f;	

		//GameObject areaAttack = (GameObject)PhotonNetwork.Instantiate(attackerPlayer.areaAttackPrefab.name, realPos, Quaternion.identity, 0);
		GameObject areaAttack = (GameObject)Instantiate(attackerPlayer.areaAttackPrefab, realPos, Quaternion.identity);
		areaAttack.transform.localScale = (new Vector3(attackerPlayer.areaAttackRadius, 0.01f, attackerPlayer.areaAttackRadius));

		//temporary coloring mechanic
		areaAttack.renderer.material.color = (attackerPlayer.team == 1) ? Color.red : Color.blue;

		Destroy(areaAttack, 1.0f);

	}

	public void startCombat (string attackerName, PlayerInput.TargetType combatType, Vector3 location) {
		tempPlayer = GameObject.Find ("Players/" + attackerName);
		//Makes sure the ray doesnt hit the ground
		location.y = tempPlayer.transform.position.y;

		if (combatType == PlayerInput.TargetType.LineAttack) {
			RaycastHit[] hits;

			Vector3 heading = location - tempPlayer.transform.position;
			Vector3 direction = (heading / heading.magnitude);//*tempPlayer.GetComponent<Player>().lineAttackDist;
			//Just to show how far the attack reaches for now
			Debug.DrawRay (tempPlayer.transform.position, direction * tempPlayer.GetComponent<Player>().lineAttackDist, Color.black);

			photonView.RPC ("RPC_lineAttackVisualization",
			                 PhotonTargets.All,
                             attackerName,
                             tempPlayer.transform.position,
                             direction);

			hits = Physics.RaycastAll (tempPlayer.transform.position, direction, tempPlayer.GetComponent<Player>().lineAttackDist);

			for (int n=0; n <hits.Length; n++) {
				if (hits[n].collider.tag=="Player") {
					GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
				                                                 	PhotonTargets.All,
				                                                 	hits[n].collider.name,
				                                                 	attackerName,
				                                                 	tempPlayer.GetComponent<Player>().baseAttack,
				                                                 	0);
				}
			}
		} else {
			GameObject[] entities = GameObject.FindGameObjectsWithTag("Player");
			Debug.DrawRay (location, Vector3.forward * tempPlayer.GetComponent<Player>().areaAttackRadius, Color.black);
			Debug.DrawRay (location, Vector3.left * tempPlayer.GetComponent<Player>().areaAttackRadius, Color.red);
			Debug.DrawRay (location, Vector3.back * tempPlayer.GetComponent<Player>().areaAttackRadius, Color.red);
			Debug.DrawRay (location, Vector3.right * tempPlayer.GetComponent<Player>().areaAttackRadius, Color.black);

			photonView.RPC ("RPC_areaAttackVisualization",
			                PhotonTargets.All,
			                attackerName,
			                location);

			for (int x=0; x<entities.Length; x++) {
				float dist = Vector3.Distance(location,entities[x].transform.position);
				if (dist < tempPlayer.GetComponent<Player>().areaAttackRadius) {
					GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
					                                                 PhotonTargets.All,
					                                                 entities[x].collider.name,
					                                                 attackerName,
					                                                 tempPlayer.GetComponent<Player>().baseAttack,
					                                                 0);
				}
			}
		}


	}
	
}

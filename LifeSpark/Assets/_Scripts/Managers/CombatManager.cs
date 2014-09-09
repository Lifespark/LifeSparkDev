using UnityEngine;
using System.Collections;

public class CombatManager : LSMonoBehaviour {

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

	[RPC]
	void RPC_ShootMissile(string attackerName, string targetName){
		
		//Create missile targeted at player with targetName, store attacking player name in the missile
		
		
	}
	
	void MissileHit()
	{
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

	public void startCombat (string attackerName, PlayerInput.TargetType combatType, Vector3 location) {
		GameObject tempPlayer;
		tempPlayer = GameObject.Find ("Players/" + attackerName);
		//Makes sure the ray doesnt hit the ground
		location.y = tempPlayer.transform.position.y;

		if (combatType == PlayerInput.TargetType.LineAttack) {
			RaycastHit[] hits;

			Vector3 heading = location - tempPlayer.transform.position;
			Vector3 direction = heading / heading.magnitude;
			//Just to show how far the attack reaches for now
			Debug.DrawRay (tempPlayer.transform.position, direction * tempPlayer.GetComponent<Player>().lineAttackDist, Color.black);

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

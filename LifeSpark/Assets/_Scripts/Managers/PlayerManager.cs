using UnityEngine;
using System.Collections;

public class PlayerManager : LSMonoBehaviour {
	public Material OriginalLane;
	public Material BrokenLane;
	public Material Team1Lane;
	public Material Team2Lane;

	
	GameObject tempPlayer;
	GameObject tempSparkPoint;
	
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
	void RPC_setMine (string playerObject, string playerName) {
		NetworkManager tempNM = GameObject.Find ("Manager").GetComponent<NetworkManager>();
		tempPlayer = GameObject.Find ("Players/" + playerObject);
		if (tempNM.playerName.Equals (playerName)) {
			tempPlayer.GetComponent<PlayerInput>().isMine = true;
			Debug.Log ("I'm " + playerName + ", control " + playerObject);
		}
		tempPlayer.GetComponent<Player> ().playerName = playerName;
	}
	
	[RPC]
	void RPC_setPlayerTarget (string playerObject, Vector3 target, string targetName) {
		tempPlayer = GameObject.Find ("Players/" + playerObject);
		tempPlayer.GetComponent<Player>().UpdateTarget(target,targetName);
	}
	
	[RPC]
	void RPC_setSparkPointCapture (string sparkPointName, string playerName, int team, bool b) {
		tempSparkPoint = GameObject.Find("SparkPoints/"+sparkPointName);
		tempSparkPoint.GetComponent<SparkPoint>().SetSparkPointCapture(playerName,team,b);
	}
	
	[RPC]
	void RPC_setPlayerSparkPointCaptured (string playerName) {
		tempPlayer = GameObject.Find("Players/"+playerName);
		tempPlayer.GetComponent<Player>().CapturedObjective();
	}

	[RPC]
	void RPC_setPlayerAttack(string attackedName, string attackerName, bool b){
		tempPlayer = GameObject.Find ("Players/" + attackedName);
		tempPlayer.GetComponent<Player>().setUnitBeingAttacked(GameObject.Find ("Players/" + attackerName).GetComponent<Player>().baseAttack,b);

	}

}

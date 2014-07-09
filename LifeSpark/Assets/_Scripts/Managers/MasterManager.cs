using UnityEngine;
using System.Collections;

public class MasterManager : LSMonoBehaviour {
	public Material OriginalLine;
	public Material BrokenLine;
	public Material Team1Line;
	public Material Team2Line;

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
		Debug.Log ("The MasterPlayer is " + masterPlayer.name);

		// if this is the masterPlayer, manage and set each client to 1 player (only has 1 masterPlayer)
		if (tempNM.playerName.Equals (masterPlayer.name)) {
			Debug.Log ("I'm the MasterPlayer.");
			// 
			int playerNum = 1;

			foreach (MetaPlayer forMP in tempMPs) {
				photonView.RPC ("RPC_setMine", PhotonTargets.All, "Player"+playerNum, forMP.name);
				//
				playerNum = playerNum + 1;
			}
			// check
			for (int i=1; i<=4; i++){
				Debug.Log ("Player" + i + ":" + GameObject.Find("Players/Player"+i).GetComponent<Player>().playerName);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	[RPC]
	void RPC_setMine (string playerObject, string playerName) {
		NetworkManager tempNM = GameObject.Find ("Manager").GetComponent<NetworkManager>();
		GameObject tempPlayer = GameObject.Find ("Players/" + playerObject);
		if (tempNM.playerName.Equals (playerName)) {
			tempPlayer.GetComponent<PlayerInput>().isMine = true;
			Debug.Log ("I'm " + playerName + ", control " + playerObject);
		}
		tempPlayer.GetComponent<Player> ().playerName = playerName;
	}

	[RPC]
	void RPC_setPlayerTarget (string playerObject, Vector3 target) {
		GameObject tempPlayer = GameObject.Find ("Players/" + playerObject);
		tempPlayer.GetComponent<Player> ().target = target;
	}
}

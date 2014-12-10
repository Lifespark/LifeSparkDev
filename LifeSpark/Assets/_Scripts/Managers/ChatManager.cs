using UnityEngine;
using System.Collections;

public class ChatManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		// Press T to send test chat message to all other players in game
		if (Input.GetKeyDown("t")) {
			// Send a chat message
			string testMessage = "Hello World!";
			GameObject.Find ("Manager").GetPhotonView().RPC ("RPC_receiveChatMessage", 
			                                                 PhotonTargets.All,
			                                                 this.name,
			                                                 testMessage);
		}
	}
	
	[RPC]
	void RPC_receiveChatMessage(string playerName, string message) {
		Debug.Log(playerName + ":   " + message);
	}
}

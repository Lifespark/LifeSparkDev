using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ServerManager : MonoBehaviour {
	
	public int listenPort = 25000;
	public int maxClients = 6;
	//private DatabaseManager dbManager;
	//private MetricsManager mmManager;
	
	// Use this for initialization
	void Start () {
		//dbManager = GetComponent<DatabaseManager>();
		//mmManager = GetComponent<MetricsManager>();
		StartServer();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void StartServer() {
		Network.InitializeServer(maxClients, listenPort, false);
	}
	
	void StopServer() {
		Network.Disconnect();
	}
	
	void OnGUI () {
		ShowServerInformation();
		ShowClientInformation();
	}
	
	/* displays servers ip address and port. -jk */
	void ShowServerInformation() {
		GUILayout.Label("IP: " + Network.player.ipAddress + " Port: " + Network.player.port);  
	}
	
	/* displays info from clients that are connected to server. -jk */
	void ShowClientInformation() {
		GUILayout.Label("Clients: " + Network.connections.Length + "/" + maxClients);
		foreach(NetworkPlayer p in Network.connections) {
			GUILayout.Label(" Player from ip/port: " + p.ipAddress + "/" + p.port); 
		}
	}
	
	/* Authenticates player login data. -jk */
	bool AuthenticateLoginData (string usernameAttempt, string passwordAttempt) {
		//return dbManager.DatabaseValidateLogin (usernameAttempt, passwordAttempt);
		return true;
	}
	
	/* allows clients to send data to server. -jk */
	[RPC]
	void Message (string text) {
		Debug.Log("Client: "+text);
	}
	
	/* Receives login data from clients. -jk */
	[RPC]
	void AuthenticateLogin (string usernameAttempt, string passwordAttempt, NetworkMessageInfo info) {
		//networkView.RPC("ValidateLogin",info.sender,AuthenticateLoginData(usernameAttempt,passwordAttempt));
	}
	
	/* Receives ping and updates player's data. -jk */
	[RPC]
	void ReceivePlayerPing (string username, int ping) {
		//mmManager.CalculateAveragePing(username, ping);
	}
	
	/* Updates ping at end of session. -jk */
	[RPC]
	void UpdatePlayerPing (string username) {
		//mmManager.CalculateHighestPing(username);
	}
}
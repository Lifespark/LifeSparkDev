using UnityEngine;
using System.Collections;

public class ClientManager : MonoBehaviour {
	
	public bool serverTest = true;
	public string ipAddress = "68.181.96.34";	/* ip address of server machine. -jk */
	int port = 25000;							/* default port number. -jk */
	string portString = "25000";
	string connectionStatus = "Not connected to server.";
	string username = "Guest";
	string password = "Password";
	public float timerPing = 5.0f;
	float timerCurrent = 0.0f;
	bool connecting = false;
	bool connected = false;
	bool loggedin = false;
	bool loginfailed = false;
    bool showLogin = true;
	enum ClientState {
		CS_Client,
		CS_Game,
	};
	ClientState clientState = ClientState.CS_Client;
	
	// Use this for initialization
	void Start () {
		
	}

	void Awake () {
		DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
		if (clientState == ClientState.CS_Game && connected) {
			timerCurrent += Time.deltaTime;
			//Debug.Log ("current timer: "+timerCurrent + "----"+timerPing);
			if (timerCurrent > timerPing) {
				//Debug.Log ("sent");
				SendPing((int)PhotonNetwork.GetPing());
				timerCurrent = 0.0f;
			}
		}
	}
	
	/* Connects to server on same machine with designated port num. -jk */
	void ConnectToServer() {
		Network.Connect(ipAddress,int.Parse(portString));
		connectionStatus = "Connecting...";
		connecting = true;
	}

	/* called when connection to server succeeds. -jk */
	void OnConnectedToServer() {
		connected = true;
		connecting = false;
	}

	/* called when connection to server fails. -jk */
	void OnFailedToConnect(NetworkConnectionError error) {
		connecting = false;
		connectionStatus = "Invalid port number.";
	}
	
	void DisconnectFromServer() {
		Network.Disconnect();
		username = "Guest";
		password = "Password";
		connectionStatus = "Not connected to server.";
		connected = false;
		loggedin = false;
		loginfailed = false;
	}
	
	/* creates button for network commands. -jk */
	void OnGUI ()
	{
		if (serverTest && showLogin) {
			if (clientState == ClientState.CS_Client) { 
				if (connected) {
					GUILayout.BeginArea(new Rect(250,10,200,200));
					GUILayout.Label("Server Commands");
					if (!loggedin) {
						if (loginfailed) {
							GUILayout.Label("Login failed - username/password\ncombination incorrect.");
						}
						username = GUILayout.TextField(username,20);
						password = GUILayout.TextField(password,20);
						if (GUILayout.Button ("Login"))
							SendLoginInfo();
					}
					else {
						GUILayout.Label("Welcome: "+username);
					}
					if (GUILayout.Button ("Disconnect"))
						DisconnectFromServer();
					GUILayout.EndArea();
				}
				else {
					GUILayout.BeginArea(new Rect(250,10,200,200));
					GUILayout.Label(connectionStatus);
					if (connecting) {
						GUI.enabled = false;
					}
					if (GUILayout.Button ("Connect"))
						ConnectToServer();
					GUILayout.Label("Port Number:");
					portString = GUILayout.TextField(portString,20);
					GUI.enabled = true;
					GUILayout.EndArea();
				}
			}
		}
	}

	/* start game. -jk */
	public void ClientStartGame() {
		clientState = ClientState.CS_Game;
	}

	/* loads client. -jk */
	public void ClientLoadClient() {
		clientState = ClientState.CS_Client;
	}
	
	/* calls rpc for authenticating login data. -jk */
	void SendLoginInfo() {
		networkView.RPC("AuthenticateLogin", RPCMode.Server, username, password);
	}
	
	/* calls rpc for sending whatever data. -jk */
	void SendData() {
		networkView.RPC("Message", RPCMode.Server, "Client Message.");
	}

	/* calls rpc for sending ping. -jk */
	void SendPing(int ping) {
        if (Network.connections.Length > 0)
		    networkView.RPC("ReceivePlayerPing", RPCMode.Server, username, ping);
	}
	
	[RPC]
	void ValidateLogin (bool loginstatus) {
		loggedin = loginstatus;
		if (!loggedin) {
			loginfailed = true;
		}
	}

    void OnLevelWasLoaded(int level) {
        if (level > 0)
            showLogin = false;
    }
}
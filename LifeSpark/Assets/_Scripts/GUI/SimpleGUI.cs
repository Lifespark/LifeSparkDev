using UnityEngine;
using System.Collections;

public enum GuiStage {
	mainMenu,
	multiMenu,
	inGame,
	joiningLobby
};

public class SimpleGUI : LSMonoBehaviour {
	private GuiStage guiStage = GuiStage.mainMenu; 
	private NetworkManager networkManager;
	private GameObject m_startUpUIObject;
	private Vector2 scrollPos = Vector2.zero;
	private string lobbyName = "default";
	private string sceneName = "MainMap";
	private StartUpUI m_startUI;

	// Use this for initialization
	void Start () {
		Debug.Log ("SimpleGUI.cs start");
		GameObject manager = GameObject.Find ("Manager");
		Object.DontDestroyOnLoad (manager);
		m_startUI = GetComponent<UIManager>().AddGui("StartAndLobby").GetComponent<StartUpUI>();
		m_startUI.SetSimpleGuiObejct(this);
		networkManager = (NetworkManager) manager.GetComponent("NetworkManager");
		m_startUI.reset();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	[RPC]
	public void RPC_setGUIStage(int stage) { //RPC doesn't understand GuiStage type
		this.guiStage = (GuiStage) stage;
	}

	private void StartQuickTest() {
//		Debug.Log ("Loading MainMap.unity...");
//		guiStage = GuiStage.inGame;
//		networkManager.mode = NetMode.quick; 
//		Application.LoadLevel ("MainMap");

		networkManager.lobbyName = this.lobbyName;
		guiStage = GuiStage.inGame;
		networkManager.mode = NetMode.quick;
		networkManager.playerName = "Quicktest Player";
		networkManager.CreateLobby(lobbyName, new RoomOptions() { maxPlayers = 4});
	}

	public void StartMultiPlayer ()
	{
		guiStage = GuiStage.multiMenu;
		//GUI_MultiMenu();
	}
	

	private void OnGUI () {
		if (networkManager.hasLobby() && guiStage != GuiStage.inGame) {
			GUI_InLobby();
		} else if (guiStage == GuiStage.mainMenu) {
			GUI_MainMenu();
		} else if (guiStage == GuiStage.multiMenu) {
			GUI_MultiMenu();
		} else if (guiStage == GuiStage.joiningLobby) {
			GUI_JoiningLobby();
		}
		GUILayout.BeginArea(new Rect(0, Screen.height-20, 400, 300));
		//GUILayout.BeginArea(new Rect(Screen.width, Screen.height + 50, 50, 50));
		//GUILayout.Label("Ping to server: " + PhotonNetwork.GetPing());
		if(m_startUI!=null)
		{
			m_startUI.DisplayPing("Ping to server: " + PhotonNetwork.GetPing());
		}

		if(guiStage == GuiStage.inGame)
		{
			if(m_startUI!=null)
			{
				m_startUI.clossAll();
			}
		}
		GUILayout.EndArea ();
	}
	
	private void GUI_MainMenu () {

		return;
		int buttonWidth = 250;
		int buttonHeight = 100;
		int space = 10;

		if (!networkManager.connected) {
			GUI_Disconnected();
		} else if (GUI.Button (new Rect ((Screen.width-buttonWidth)/2, ((Screen.height-buttonHeight)/2)-((buttonHeight+space)/2), 250, 100), "Quick Test Mode (1P)")) {
			StartQuickTest ();
		} else if (GUI.Button (new Rect ((Screen.width-buttonWidth)/2, ((Screen.height-buttonHeight)/2)+((buttonHeight+space)/2), 250, 100), "Multiplayer")) {
			guiStage = GuiStage.multiMenu;
		}
	}

	private void OnPlayerNameChanged()
	{
		PlayerPrefs.SetString("playerName", networkManager.playerName);
	}

	public string NetworkPlayerName
	{
		get{
		return networkManager.playerName;
		}
		set{
			networkManager.playerName = value;
		}
	}



	void CreateLobby ()
	{
		networkManager.lobbyName = this.lobbyName;
		networkManager.CreateLobby (lobbyName, new RoomOptions () {
			maxPlayers = 4
		});
	}

	public void CreateLobby (string lobbyName)
	{
		networkManager.lobbyName = lobbyName;
		networkManager.CreateLobby (lobbyName, new RoomOptions () {
			maxPlayers = 4
		});
	}

	public void join (string lobbyName)
	{
		guiStage = GuiStage.joiningLobby;
		networkManager.lobbyName = lobbyName;
		networkManager.JoinRoom (lobbyName);
	}

	private void GUI_MultiMenu() {
		m_startUI.MultiMenu();

		return;
		int menuWidth = 400;
		int menuHeight = 300;

		GUILayout.BeginArea(new Rect((Screen.width - menuWidth) / 2, (Screen.height - menuHeight) / 2, menuWidth, menuHeight));
		
		// Player name
		GUILayout.BeginHorizontal();
		GUILayout.Label("Player name:", GUILayout.Width((2 * menuWidth) / 5));
		networkManager.playerName = (GUILayout.TextField (networkManager.playerName));
		if (GUI.changed) {
			 OnPlayerNameChanged();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(15);

		// Create a lobby (fails if already exists!)
		GUILayout.BeginHorizontal();
		this.lobbyName = GUILayout.TextField(this.lobbyName);
		if (GUILayout.Button("CREATE")) {
			CreateLobby ();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(25);

		//Show a list of all current lobbies
		GUILayout.Label("Active Lobbies:");
		if (networkManager.GetLobbies().Length == 0) {
			GUILayout.Label("(none)");
		}
		else
		{
			// Room listing: simply call GetRoomList: no need to fetch/poll whatever!
			this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
			foreach (Lobby lobby in networkManager.GetLobbies ())
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(lobby.name + " " + lobby.playerCount + "/" + lobby.maxPlayers);
				if (GUILayout.Button("JOIN"))
				{
					join (lobby.name);
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndArea();
	}

	public Lobby[] GetLobbyList()
	{
		return networkManager.GetLobbies();
	}

	private void GUI_InLobby() {
		//

		m_startUI.InLobby(networkManager.lobbyName,networkManager.GetMetaPlayers (),this.sceneName);
		return;
		GUILayout.Label("We are connected to room: "+networkManager.lobbyName);
		GUILayout.Label("Players: ");
		foreach (MetaPlayer player in networkManager.GetMetaPlayers ())
		{
			GUILayout.Label("ID: "+player.ID+" Name: "+player.name);
		}


		GUILayout.Label("Scene To Load:");
		this.sceneName = GUILayout.TextField(this.sceneName);

		if (GUILayout.Button("Start game"))
		{
			networkManager.startNetworkedGame(this.sceneName);
		}

		if (GUILayout.Button("Leave room"))
		{
			guiStage = GuiStage.multiMenu;
			PhotonNetwork.LeaveRoom();
		}
	}

	public void startGame(string sceneName)
	{
		networkManager.startNetworkedGame(sceneName);
	}


	public void leavelRoom()
	{
		guiStage = GuiStage.multiMenu;
		PhotonNetwork.LeaveRoom();
	}

	private void GUI_JoiningLobby() {
		GUILayout.Label ("Connecting to " + networkManager.lobbyName + "...");
	}
	
	private void GUI_Disconnected() {
		if (networkManager.connecting) {
			GUILayout.Label("Connecting...");
		}
		else {
			GUILayout.Label("Not connected.");
		} 

		if (networkManager.connectFailed){
			GUILayout.Label("Connection failed.");
			GUILayout.Label(string.Format("Server: {0}", networkManager.serverAddress));
			GUILayout.Label(string.Format("AppId: {0}", networkManager.appID));
			
			if (GUILayout.Button("Try Again", GUILayout.Width(100)))
			{
				networkManager.connectFailed = false;
				PhotonNetwork.ConnectUsingSettings("1.0");
			}
		}
	}
}

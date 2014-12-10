using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;

//Mode constants
public enum NetMode { initial, quick, full };

public class MetaPlayer {
	//currently just a wrapper for PUN's Player

	public int ID { get; private set; }
	public string name { get; set; }

	public MetaPlayer (int pID, string pName){
		this.ID = pID;
		this.name = pName;
	}
}

public class Lobby {
	//currently just a wrapper for PUN's RoomInfo

	public string name { get; set; }
	public int playerCount { get; set; }
	public int maxPlayers { get; set; }

	public Lobby(string pName, int pPlayerCount, int pMaxPlayers) {
		this.name = pName;
		this.playerCount = pPlayerCount;
		this.maxPlayers = pMaxPlayers;
	}
}

public class NetworkManager : LSMonoBehaviour {

#if UNITY_IPHONE
    iPhoneGeneration m_deviceType;
#elif UNITY_STANDALONE_WIN
    string m_deviceType;
#endif

    private GameObject serverManager;
    static private NetworkManager _instance;
    static public NetworkManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(NetworkManager)) as NetworkManager;
            return _instance;
        }
    }

	public NetMode mode { get; set; }
	public bool connectFailed { get; set; }
		
	//PUN wrappers
	public bool connected { 
		get { return PhotonNetwork.connected;}
	}
	public bool connecting { 
		get { return PhotonNetwork.connecting;}
	}
	public string serverAddress { 
		get { return PhotonNetwork.PhotonServerSettings.ServerAddress;}
	}
	public string appID { 
		get { return PhotonNetwork.PhotonServerSettings.AppID;}
	}
	public string playerName { 
		get { return PhotonNetwork.playerName;}
		set { PhotonNetwork.playerName = value;}
	} 
	public string lobbyName { get; set; }

    public bool canJoin = false;

	private void Awake() {
#if UNITY_IPHONE
        m_deviceType = iPhone.generation;
        if (m_deviceType != iPhoneGeneration.iPadMini2Gen)
            m_deviceType = iPhoneGeneration.iPad5Gen;
#elif UNITY_STANDALONE_WIN
        m_deviceType = SystemInfo.operatingSystem;
        Debug.Log(m_deviceType);
#endif
        _instance = this;
        DebugClient dc = new DebugClient(1);
        DebugClient dc2 = new DebugClient(2);
        dc.Run();
        dc2.Run();

		mode = NetMode.initial;
		connectFailed = false;
		// Connect to the main photon server.
		if (!PhotonNetwork.connected)
		{
			PhotonNetwork.ConnectUsingSettings("1.0");
		}	
		
		//Load our name from PlayerPrefs
		PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);

        PhotonNetwork.sendRate = 15;
        PhotonNetwork.sendRateOnSerialize = 15;
	}
	
	// Use this for initialization
	void Start () {
		//Set scene to automatically update on all clients
		PhotonNetwork.automaticallySyncScene = true; 
		serverManager = GameObject.Find("ServerManager");
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void startNetworkedGame(string scene) {
		serverManager.GetComponent<ClientManager>().ClientStartGame();
		photonView.RPC("RPC_setGUIStage", PhotonTargets.All, (int)GuiStage.inGame);
        PhotonNetwork.LoadLevel(scene);
        PhotonNetwork.room.open = false;
        PhotonNetwork.room.visible = false;
	}


//	//Not needed (autosync + PhotonNetwork.LoadLevel takes care of this for us)
//	[RPC]
//	public void RPC_startGame() {
//		Debug.Log ("Loading MainMap.unity...");
//		this.mode = NetMode.full; 
//		Application.LoadLevel ("MainMap");
//	}

	public void setPlayerName(string name) {
		PhotonNetwork.playerName = name;
	}

	public bool hasLobby() {
		return PhotonNetwork.room != null;
	}
	
	public void CreateLobby(string room, RoomOptions options){
		PhotonNetwork.CreateRoom(room, options, null);
	}

	public Lobby[] GetLobbies() {
		// Using Lobby as a wrapper for RoomInfo to 
		//  restrict PUN-specific things to this file
		RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
		int numLobbies = rooms.Length;
		Lobby[] lobbies = new Lobby[numLobbies];
		for (int i = 0; i < numLobbies; i++){
            if (rooms[i].visible && rooms[i].open) {
                lobbies[i] = new Lobby(rooms[i].name, rooms[i].playerCount, rooms[i].maxPlayers);
            }
			
		}
		
		return lobbies;
	}

	public void JoinRoom(string name) {
		PhotonNetwork.JoinRoom(name);
	}

	public MetaPlayer[] GetMetaPlayers() {
		// Using MetaPlayer as a wrapper for Player to 
		//  restrict PUN-specific things to this file
		PhotonPlayer[] players  = PhotonNetwork.playerList;
		MetaPlayer[] metaPlayers = new MetaPlayer[players.Length];
		for (int i = 0; i < players.Length; i++){
			metaPlayers[i] = new MetaPlayer(players[i].ID, players[i].name);
		}


		return metaPlayers;
	}

	private void OnFailedToConnectToPhoton(object parameters) {
		connectFailed = true;
		Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters);
	}

	void OnJoinedRoom() {
        lobbyName = PhotonNetwork.room.name;
        //SimpleGUI.Instance.guiStage = GuiStage.joiningLobby;
        SimpleGUI.Instance.m_startUI.pickWindow.SetActive(true);
        SimpleGUI.Instance.m_startUI.gameObject.SetActive(true);
        if (!PhotonNetwork.isMasterClient)
            SimpleGUI.Instance.m_startUI.startButton.gameObject.SetActive(true);
        return;

        // yeah I break it no i don't just don't use it for demo day
		if (mode == NetMode.quick) {
			startNetworkedGame ("MainMap");
		}
	}

	public bool IsMasterClient() {
		return PhotonNetwork.isMasterClient;
	}

    public void CreateRoomBasedOnDevice() {
        RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 4 };
#if UNITY_IPHONE
        PhotonNetwork.JoinOrCreateRoom(m_deviceType.ToString(), roomOptions, TypedLobby.Default);
#elif UNITY_STANDALONE_WIN
        PhotonNetwork.JoinOrCreateRoom(m_deviceType, roomOptions, TypedLobby.Default);
#endif
    }

    IEnumerator DeferJoin() {
        yield return new WaitForSeconds(4.0f);
        canJoin = true;
    }

//     void OnCreatedRoom() {
//         SimpleGUI.Instance.guiStage = GuiStage.joiningLobby;
// #if UNITY_IPHONE
//         PhotonNetwork.JoinRoom(m_deviceType.ToString());
// #elif UNITY_STANDALONE_WIN
//         PhotonNetwork.JoinRoom(m_deviceType);
// #endif
//     }
}

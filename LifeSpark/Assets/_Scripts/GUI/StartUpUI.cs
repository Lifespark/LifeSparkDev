/// <summary>
/// This Class is terrible!!!!!!!
/// I will combie this class with SimpleGUI when doing the formal GUI
/// Or I have spare time to do this
/// </summary>


using UnityEngine;
using System.Collections;
//[ExecuteInEditMode]
public class StartUpUI : MonoBehaviour {

	public GameObject start, lobby;
	public UILabel pingLabel;
	public SimpleGUI m_sGui;
	public UIInput playerName;
	public UIInput m_lobbyName;
    public UIInput m_joinLobbyName;
	public UILabel m_none;
	private Lobby[] lobbyList;
	public GameObject m_cellObject;
	public LobbyCell[] m_lobbyCell;
	public UILabel[] m_playerLabelList;
	public GameObject m_room;
	public UILabel m_sceneName;
    public MetaPlayer[] t_playersList;
    public GameObject pickWindow;

    private NetworkManager m_networkManager;

	public void SetSimpleGuiObejct(SimpleGUI sgui){
		m_sGui = sgui;
        //transform.localScale = new Vector3(1.5f, 1.5f, 1);
	}

	bool inRoom;

	private bool t_needUpdate;
	private float t_timer = 0;

	// Use this for initialization
	void Start () {
        GameObject manager = GameObject.Find("Manager");
        Object.DontDestroyOnLoad(manager);
        m_networkManager = (NetworkManager)manager.GetComponent("NetworkManager");

		//Debug.Log("UI Root, It's a new one!");
	}

	public void clossAll()
	{
		start.SetActive(false);
		lobby.SetActive(false);
		m_room.SetActive(false);
		t_needUpdate = false;
//		start.SetActive(false);
//		start.SetActive(false);
	}

	public void close ()
	{
		this.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	//	if(m_lobbyCell==null)
//		{
//		m_lobbyCell = m_cellObject.GetComponentsInChildren<LobbyCell>();
//		}


		if(t_timer > 1)
		{
			t_timer = 0;
			UpdateLobby();
            updatePlayer();
		}else{
			t_timer += Time.deltaTime;
		}
	}

    /// <summary>
    /// this var is to record is the player join other's room or create the room their own
    /// </summary>
    bool isJoinedRoom;

	public void joinLobby (string lobbyName)
	{
//		lobby.gameObject.SetActive(false); clossAll()
		clossAll();
        isJoinedRoom = true;
		m_room.SetActive(true);
		m_sGui.join(lobbyName);
        startButton.gameObject.SetActive(!isJoinedRoom);
		inRoom = true;
	}

	public void leaveLobby ()
	{
		//		lobby.gameObject.SetActive(false); clossAll()
		clossAll();
		isJoinedRoom = false;
		m_sGui.leavelRoom();
		m_room.SetActive(false);
		start.SetActive (true);
		//startButton.gameObject.SetActive(true);
		inRoom = false;
	}
	
	void OnCreatePublic()
    {
        OnCreate(true);
    }

    void OnCreatePrivate()
    {
        OnCreate(false);
    }
	void OnCreate(bool visibility)
	{
//		m_sGui.SendMessage();
		lobby.gameObject.SetActive(false);
		if(m_sGui.NetworkPlayerName == playerName.text)
		{

		}else{
			m_sGui.NetworkPlayerName = playerName.text;
			m_sGui.SendMessage("OnPlayerNameChanged");
		}
		if(string.IsNullOrEmpty(m_lobbyName.text))
		{

		}else{
			m_sGui.CreateLobby(m_lobbyName.text,visibility );
		}
        isJoinedRoom = false;
		inRoom = true;
	}
    /// <summary>
    /// Called when the JoinPrivateGame button is pressed
    /// </summary>
    void OnJoin()
    {
        clossAll();
        isJoinedRoom = true;
        m_room.SetActive(true);
		inRoom = true;
        if (!string.IsNullOrEmpty( m_joinLobbyName.text))
            m_sGui.join(m_joinLobbyName.text);
        

    }
    /// <summary>
    /// Called when the Refresh Button is Pressed
    /// </summary>
    void OnRefreshButton()
    {
        lobbyList = m_sGui.GetLobbyList();
    }

	void OnMulti()
	{
		clossAll();
		t_needUpdate = true;
		m_sGui.StartMultiPlayer();
		start.SetActive(false);
		m_lobbyName.text = "default";
		lobbyList = m_sGui.GetLobbyList();
		for(int i =0;i< m_lobbyCell.Length;i++)
		{
			m_lobbyCell[i].gameObject.SetActive(false);
		}
		if(lobbyList == null)
		{
			Debug.LogError("Cannot get the lobby list from network manager");
		}else{

			if(lobbyList.Length == 0)
			{
				m_none.gameObject.SetActive(true);
			}else{
				m_none.gameObject.SetActive(false);

				for(int i =0 , j = lobbyList.Length;i<j;i++)
				{
					m_lobbyCell[i].gameObject.SetActive(true);
					m_lobbyCell[i].lobbyName = lobbyList[i].name;
					m_lobbyCell[i].m_label.text = lobbyList[i].name + 
						" " + lobbyList[i].playerCount + "/" + lobbyList[i].maxPlayers;

				}





			}
		}

	}

	void UpdateLobby ()
	{

        if (lobby.activeSelf) {
            lobbyList = m_sGui.GetLobbyList();
            //Debug.Log("Lobby updated");
            if (lobbyList == null) {
                //			Debug.LogError("Cannot get the lobby list from network manager");
            } else {

                //Check for host


                for (int i = 0; i < m_lobbyCell.Length; i++) {
                    m_lobbyCell[i].gameObject.SetActive(false);
                }
                if (lobbyList.Length == 0) {
                    m_none.gameObject.SetActive(true);
                } else {
                    m_none.gameObject.SetActive(false);

                    for (int i = 0, j = lobbyList.Length; i < j; i++) 
                    {
                        m_lobbyCell[i].gameObject.SetActive(true);
                        m_lobbyCell[i].lobbyName = lobbyList[i].name;
                        m_lobbyCell[i].m_label.text = lobbyList[i].name +
                            " " + lobbyList[i].playerCount + "/" + lobbyList[i].maxPlayers;

                    }





                }
            }
        }
	}


    void updatePlayer() {
        if (m_room.activeSelf) {
            t_playersList = m_sGui.getMetaPlayers();
            if (t_playersList == null) {
                return;
            } else {

                Showplayer(t_playersList);
            }
        }
    }

	void OnQuickTest()
	{
		m_sGui.SendMessage("StartQuickTest");
		//this.gameObject.SetActive(false);
        //pickWindow.SetActive(true);
	}
	/// <summary>
	/// this will be called by UIManager after use the func AddGUI
	/// this method served as reset method
	/// </summary>
	void OnDisplay()
	{
		clossAll();
		start.SetActive(true);
		lobby.SetActive(false);
		if(m_sGui!=null)
		{
			playerName.text = m_sGui.NetworkPlayerName;
		}
	}

	public void MultiMenu()
	{
		if(lobby.activeSelf)
			return;
		clossAll();
		start.SetActive(false);
		lobby.SetActive(true);
	}


	public void DisplayPing(string ping)
	{
		pingLabel.text = ping;
	}


	public void reset ()
	{
		OnDisplay();
		needPick = true;
	}

	void Showplayer(MetaPlayer[] mp)
	{
		for(int i =0; i< m_playerLabelList.Length;i++){
			m_playerLabelList[i].gameObject.SetActive(false);
		}
		for(int i =0; i< mp.Length;i++){
			m_playerLabelList[i].gameObject.SetActive(true);
			m_playerLabelList[i].text = "ID: "+mp[i].ID+" Name: "+mp[i].name;
		}

	}

    public UIButton startButton;

	public bool needPick;

	//public void InLobby ()
	//{
	//	m_room .gameObject.SetActive(true);
	//}

	public void InLobby (string lobbyName, MetaPlayer[] mplayer,string scenename)
	{

				m_sceneName.text = lobbyName;
				scenenNameInput.text = scenename;

				if (m_room.activeSelf) {
						//Grab the start button
						Component start = m_room.transform.FindChild ("start");

						if (start != null) {   //If this instance is not the host
								if (m_networkManager.IsMasterClient () == false) {
										start.gameObject.SetActive (false);
								} else if (!start.gameObject.activeSelf) {
										start.gameObject.SetActive (true);
								}
						}
				} else {
						clossAll ();
						m_room.gameObject.SetActive (true);
						Showplayer (mplayer);

				}
				//startButton.gameObject.SetActive (!isJoinedRoom);
		if (needPick && inRoom ) {
						pickWindow.SetActive (true);
				}
	}

	public UIInput scenenNameInput;
	void StartGame()
	{
		m_sGui.startGame(scenenNameInput.text);
	}

	void leaveRoom()
	{
		inRoom = false;
		m_sGui.leavelRoom();
		needPick = true;

	}


    void OpenFriendList() {
        UIManager.mgr.OpenFriendForm();
    }
}

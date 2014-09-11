using UnityEngine;
using System.Collections;
//[ExecuteInEditMode]
public class StartUpUI : MonoBehaviour {

	public GameObject start, lobby;
	public UILabel pingLabel;
	private SimpleGUI m_sGui;
	public UIInput playerName;
	public UIInput m_lobbyName;
	public UILabel m_none;
	private Lobby[] lobbyList;
	public GameObject m_cellObject;
	public LobbyCell[] m_lobbyCell;
	public UILabel[] m_playerLabelList;
	public GameObject m_room;
	public UILabel m_sceneName;

	public void SetSimpleGuiObejct(SimpleGUI sgui){
		m_sGui = sgui;

	}
	// Use this for initialization
	void Start () {
		
	}

	public void clossAll()
	{
		start.SetActive(false);
		lobby.SetActive(false);
		m_room.SetActive(false);
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
	}

	public void joinLobby (string lobbyName)
	{
//		lobby.gameObject.SetActive(false); clossAll()
		clossAll();
		m_room.SetActive(true);
		m_sGui.join(lobbyName);
	}

	void OnCreateButton()
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
			m_sGui.CreateLobby(m_lobbyName.text );
		}

	}
	void OnMulti()
	{
		clossAll();
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

	void OnQuickTest()
	{
		m_sGui.SendMessage("StartQuickTest");
		this.gameObject.SetActive(false);
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

	//public void InLobby ()
	//{
	//	m_room .gameObject.SetActive(true);
	//}

	public void InLobby (string lobbyName, MetaPlayer[] mplayer,string scenename)
	{
		if(m_room.activeSelf)
			return;
		clossAll();
		m_room .gameObject.SetActive(true);
		Showplayer(mplayer);
		m_sceneName.text = lobbyName;
		scenenNameInput.text = scenename;
	}

	public UIInput scenenNameInput;
	void StartGame()
	{
		m_sGui.startGame(scenenNameInput.text);
	}

	void leaveRoom()
	{
		m_sGui.leavelRoom();
	}
}

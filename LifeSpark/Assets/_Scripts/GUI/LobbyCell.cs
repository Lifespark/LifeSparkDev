using UnityEngine;
using System.Collections;

public class LobbyCell : MonoBehaviour {
	public UILabel m_label;
	public string lobbyName;
	public StartUpUI m_sui;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void join()
	{
		m_sui.joinLobby(lobbyName);
	}
}

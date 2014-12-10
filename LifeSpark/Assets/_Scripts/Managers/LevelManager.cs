using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : LSMonoBehaviour {

	public GameObject m_levelUpEffect;
	public GameObject m_levelUpEffect02;

    static private LevelManager _instance;
    static public LevelManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(LevelManager)) as LevelManager;
            return _instance;
        }
    }

    public Dictionary<int, int> m_playerHero = new Dictionary<int, int>();

    public bool m_playerInited = false;
    public bool m_sparkPointInited = false;
    public bool m_jungleCreepInited = false;
    private bool m_levelInited = false;

    private int m_initedNetworkPlayer = 0;
    public bool m_startedGame = false;

    void OnLevelWasLoaded(int level) {
        if (level == 0) return;

//         if (PhotonNetwork.isMasterClient) {
//             photonView.RPC("RPC_startInitGame", PhotonTargets.AllViaServer, 0);
//         }

    }

    // Use this for initialization
    void Awake() {
        _instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (!m_levelInited) {
            if (m_playerInited && m_sparkPointInited && m_jungleCreepInited) {
                m_levelInited = true;
                photonView.RPC("NetworkLevelInited", PhotonTargets.MasterClient, null);
            }
        }

        if (PhotonNetwork.isMasterClient && !m_startedGame) {
            if (m_initedNetworkPlayer == PhotonNetwork.playerList.Length) {
                photonView.RPC("StartNetworkGame", PhotonTargets.AllBuffered, null);
                //m_startedGame = true;
            }
        }
    }

    public void AddPlayerHero(int ID, int index) {
        m_playerHero.Add(ID, index);

        if (m_playerHero.Count == PhotonNetwork.playerList.Length) {
            if (PhotonNetwork.isMasterClient) {
                photonView.RPC("RPC_startInitGame", PhotonTargets.AllViaServer);
            }
        }
    }

    [RPC]
    public void NetworkLevelInited() {
        m_initedNetworkPlayer++;
    }

    [RPC]
    public void StartNetworkGame() {
        Debug.Log("Network Game Starts!");
        m_startedGame = true;
        UIManager.mgr.OnGameStarted();
        GameObject.Find("Manager").GetComponent<UIManager>().closeLoadForm();
        // focus on player
       
        CameraManager.Instance.FocusOnPlayer();
    }

    [RPC]
    public void RPC_startInitGame() {
        // init sparkPoint
        SparkPointManager.Instance.InitSparkPoint();

		// init bossCage
		BossCageManager.Instance.InitBossCage();

        // init jungle creep
        MonsterClient.Instance.InitMonster();

        // init player
        PlayerManager.Instance.InitPlayer();

        // change level music
        //MusicManager.Instance.setLevelMusic(1);
    }
}

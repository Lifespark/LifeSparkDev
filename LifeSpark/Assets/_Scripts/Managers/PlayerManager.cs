using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerManager : LSMonoBehaviour {

    static private PlayerManager _instance;
    static public PlayerManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(PlayerManager)) as PlayerManager;
            return _instance;
        }
    }


    public Material OriginalLane;
    public Material BrokenLane;
    public Material Team1Lane;
    public Material Team2Lane;

    GameObject tempPlayer;
    GameObject tempSparkPoint;
	GameObject cursor; /* used for pings. -jk */
	Animator tempAnim; /* used for pings. -jk */

    int playerNum = 0;

    public List<Player> allPlayers = new List<Player>(); // use this to iterate over all players
    public Dictionary<string, Player> PlayerLookUp = new Dictionary<string,Player>(); // use this to look up player by name
    public GameObject myPlayer = null; // use this to get yourself

    void Awake() {
        _instance = this;
    }

	public List<Player> GetFriendlyPlayers(int team){
		List<Player> friendlyPlayers = new List<Player>();;

		for (int i = 0; i < allPlayers.Count; i++)
			if (allPlayers[i].team == team)
				friendlyPlayers.Add(allPlayers[i]);

		return friendlyPlayers;
	}

    public void InitPlayer() {
        playerNum = NetworkManager.Instance.GetMetaPlayers().Length;
        if (PhotonNetwork.isMasterClient) {
            GameObject[] playerPoses = GameObject.FindGameObjectsWithTag("PlayerPos");
            foreach (KeyValuePair<int, int> playerHero in LevelManager.Instance.m_playerHero) {
                object[] initData = { (playerHero.Key - 1) % 2 == 0 ? 2 : 1, "Player" + (playerHero.Key).ToString(), playerHero.Value };

                if (playerHero.Value == 0)
                    PhotonNetwork.InstantiateSceneObject("Player", playerPoses[playerHero.Key - 1].transform.position, Quaternion.identity, 0, initData);
                else if (playerHero.Value == 1)
                    PhotonNetwork.InstantiateSceneObject("LevantisNewer", playerPoses[playerHero.Key - 1].transform.position, Quaternion.identity, 0, initData);
                else if (playerHero.Value == 2)
                    PhotonNetwork.InstantiateSceneObject("Delia", playerPoses[playerHero.Key - 1].transform.position, Quaternion.identity, 0, initData);
            }
        }
    }



    public void OnPlayerInstantiated(Player player) {
        PlayerLookUp.Add(player.name, player);
        allPlayers.Add(player);
        playerNum--;
        if (playerNum == 0) {
            LevelManager.Instance.m_playerInited = true;
            InitPlayerControl();
        }
    }

    // Use this for initialization
    void InitPlayerControl() {
        NetworkManager tempNM = GameObject.Find("Manager").GetComponent<NetworkManager>();
        MetaPlayer[] tempMPs = tempNM.GetMetaPlayers();
        MetaPlayer masterPlayer = tempMPs[0];
        // decide masterPlayer by smallest ID, he manage other players like MasterClient
        foreach (MetaPlayer forMP in tempMPs) {
            if (forMP.ID < masterPlayer.ID) {
                masterPlayer = forMP;
            }
        }
        //Debug.Log ("The MasterPlayer is " + masterPlayer.name);

        // if this is the masterPlayer, manage and set each client to 1 player (only has 1 masterPlayer)
        if (tempNM.playerName.Equals(masterPlayer.name)) {
            //Debug.Log ("I'm the MasterPlayer.");
            // 
            int playerNum = 1;

            foreach (MetaPlayer forMP in tempMPs) {
                photonView.RPC("RPC_setMine", PhotonTargets.All, "Player" + playerNum, forMP.name);
                //
                playerNum = playerNum + 1;
            }
            // check
            /*for (int i=1; i<=4; i++){
                Debug.Log ("Player" + i + ":" + GameObject.Find("Players/Player"+i).GetComponent<Player>().playerName);
            }*/
        }
    }

    // Update is called once per frame
    void Update() {

    }

    [RPC]
    void RPC_setMine(string playerObject, string playerName) {
        NetworkManager tempNM = GameObject.Find("Manager").GetComponent<NetworkManager>();
        tempPlayer = GameObject.Find(playerObject);
        if (tempNM.playerName.Equals(playerName)) {
            tempPlayer.GetComponent<PlayerInput>().isMine = true;
            //Debug.Log("I'm " + playerName + ", control " + playerObject);
            myPlayer = tempPlayer;
            GameObject.Find("Manager").GetComponent<UIManager>().setPlayer(tempPlayer.GetComponent<Player>());
        }
        tempPlayer.GetComponent<Player>().playerName = playerName;
    }

    [RPC]
    void RPC_setPlayerTarget(string playerObject, Vector3 target, string targetName, int typeAsInt) {
        //Note: Enums are sent as ints in RPCs, so we actually need to receive the PlayerInput.TargetType as an int and then cast it
        PlayerInput.TargetType type = (PlayerInput.TargetType)typeAsInt;

        tempPlayer = GameObject.Find(playerObject);

		UnitObject targetUnit = null;
		GameObject tempTarget = GameObject.Find (targetName);

		if (tempTarget != null)
			targetUnit = tempTarget.GetComponent<UnitObject>();

        if (type == PlayerInput.TargetType.Position) {
            tempPlayer.GetComponent<Player>().UpdateTarget(target, targetName, targetUnit);//This may not be necessary now that we're using navmesh. Will leave in for now.
			// tempPlayer.GetComponent<NavMeshAgent>().SetDestination(target);
        }
        //		else if(type == PlayerInput.TargetType.PositionFromJoystick) {
        //
        //		}
        else {
            tempPlayer.GetComponent<Player>().EngageCombat(type, target);
        }
        UIManager.mgr.OnTargetChanged();
    }

	[RPC]
	void RPC_setPlayerPing(string playerObject, Vector3 target, string targetName, int typePing) {
		tempPlayer = GameObject.Find(playerObject);
		if (tempPlayer.GetComponent<Player>().GetTeam() != myPlayer.GetComponent<Player>().GetTeam()) {
			return;
		}
		UnitObject targetUnit = null;
		GameObject tempTarget = GameObject.Find(targetName);

		if (tempTarget != null)
			targetUnit = tempTarget.GetComponent<UnitObject>();

		/* activate colored ping by type. -jk */
		cursor = GameObject.Find("CursorGround");
		if (typePing/8 >= 1) {
			cursor.GetComponent<Animator>().SetTrigger("pingPurple");
		}
		else if (typePing/4 >= 1) {
			cursor.GetComponent<Animator>().SetTrigger("pingBlue");
		}
		else if (typePing/2 >= 1) {
			cursor.GetComponent<Animator>().SetTrigger("pingOrange");
		}
		else {
			cursor.GetComponent<Animator>().SetTrigger("pingGreen");
		}

		/* make arrow point towards camera. -jk */
		cursor = GameObject.Find("CursorAir");
		Vector3 camPos = GameObject.Find("Main Camera").transform.position;
		camPos.y = target.y;
		cursor.transform.rotation = Quaternion.LookRotation(camPos - target);
		cursor.GetComponent<Animator>().SetTrigger("cursorBob");

		/* reposition cursor. -jk */
		cursor = GameObject.Find("Cursor");
		cursor.transform.position = target;

		UIManager.mgr.OnTargetChanged();
	}

    [RPC]
    void RPC_setSparkPointCapture(string sparkPointName, string playerName, int team, bool b, float rate) {
		tempPlayer = GameObject.Find(playerName);

        tempSparkPoint = SparkPointManager.Instance.sparkPointsDict[sparkPointName].gameObject;

		tempPlayer.GetComponent<Player> ().m_currentlyCapturedSparkPoint = b ? tempSparkPoint.GetComponent<SparkPoint>() : null;

        tempSparkPoint.GetComponent<SparkPoint>().SetSparkPointCapture(playerName, team, b, rate);
    }

//     [RPC]
//     void RPC_setSparkPointDestroy(string sparkPointName, string playerName, int team) {
//         //tempSparkPoint = GameObject.Find("SparkPoints/"+sparkPointName);
//         tempSparkPoint = SparkPointManager.Instance.sparkPointsDict[sparkPointName].gameObject;
//         tempSparkPoint.GetComponent<SparkPoint>().SetSparkPointDestroy(playerName, team);
//     }

    [RPC]
    void RPC_setPlayerSparkPointCaptured(string playerName, string sparkPointName, int team) {
        tempPlayer = GameObject.Find(playerName);
        tempPlayer.GetComponent<Player>().CapturedObjective(sparkPointName);

		tempPlayer.GetComponent<Player> ().m_currentlyCapturedSparkPoint = null;

        tempSparkPoint = SparkPointManager.Instance.sparkPointsDict[sparkPointName].gameObject;
        tempSparkPoint.GetComponent<SparkPoint>().SetSparkPointCaptured(team);
    }


    //Sends signal to the unit object to dish out damage for a specified player
    //Input: Who's attacking, who's being attacked, how much damage to deal, how long it should take for damage to be taken completely
    [RPC]
    void RPC_setPlayerAttack(string attackedName, string attackerName, int attackIndex) {
		Debug.Log("In PlayerManager:attackedName=" + attackedName);
        UnitObject attackedUnit = (GameObject.Find(attackedName)).GetComponent<UnitObject>();
        Player attackerPlayer = PlayerManager.Instance.PlayerLookUp[attackerName];

        //If attack index points to BASIC, get the hit object from the basic attack, otherwise get it from the special attacks list with the given index
        Hit hit = ((CombatManager.AttackIndex)attackIndex) == CombatManager.AttackIndex.BASIC ? attackerPlayer.m_basicAttack.m_hitObject : attackerPlayer.m_specialAttacks[(int)attackIndex].m_hitObject;

        //attackedUnit.receiveAttack(hit, attackerPlayer.transform);
        //tempPlayer.GetComponent<Player>().receiveAttack(hit);
        attackerPlayer.transform.LookAt(attackedUnit.transform);
        attackerPlayer.m_action = attackerPlayer.GetComponent<AGE.ActionHelper>().PlayAction("Basic");
        attackerPlayer.m_action.SetGameObject(1, attackedUnit.gameObject);
    }

    [RPC]
    void RPC_setPlayerDeath(string playerName, int team) {
        tempPlayer = GameObject.Find(playerName);
        tempPlayer.GetComponent<Player>().KillPlayer();
    }

    [RPC]
    void RPC_setPlayerRespawn(string playerName, Vector3 location) {
        // Debug.Log("respawning " + playerName);
        tempPlayer = GameObject.Find(playerName);
        Player t_playerComponent = tempPlayer.GetComponent<Player>();
        t_playerComponent.RespawnPlayer(location);
    }

    /*[RPC]
    void RPC_addPlayerEffect(string playerName, Effect effect) {
        Debug.Log("applying hit effect to " + playerName);
        tempPlayer = GameObject.Find("Players/" + playerName);
        tempPlayer.GetComponent<Player>().AddEffect(effect);
    }*/

    [RPC]
    void RPC_setPlayerDeltaMove(string playerName, Vector3 delta) {
        tempPlayer = GameObject.Find(playerName);
        tempPlayer.GetComponent<Player>().SetPlayerPosAndRot(delta);
    }

	[RPC]
	void RPC_sendPlayerMessage(string playerName, string msg) {
		tempPlayer = GameObject.Find(playerName);
		tempPlayer.GetComponent<Player>().ShowMessage(msg);
		//tempPlayer.GetComponent<Player>().SetPlayerPosAndRot(delta);
	}

	[RPC]
	void RPC_setTeamVictory(int winningTeam) {
		Player player = myPlayer.GetComponent<Player>();
		player.SetVictorious(player.team == winningTeam);
	}

    [RPC]
    void RPC_pickupLore(string playerName, LoreObject lore) {
        tempPlayer = GameObject.Find(playerName);
        tempPlayer.GetComponent<Player>().AcquireLoreItem(lore.m_loreItem);
    }

}

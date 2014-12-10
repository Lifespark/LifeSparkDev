using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

public class Player : UnitObject {

    //Player singleton used to get default
    static private Player m_instance;
    static public Player Instance {
        get {
            if (!m_instance) {
                m_instance = new Player();
            }
            return m_instance;
        }
    }

	#region ENUM_FIELD
	public enum PlayerState {
		Idle,
		Moving,
		Capturing,
		Attacking,
		Dead,
        ActionAttacking,
	};

	public enum GameState {
		GameStart,
		InGame,
		Victory,
		Defeat
	};

    public enum PlayerHero {
        CAPSULE,
        LEVANTIS,
        DELIA,
	};

	public enum TargetType {
		NONE,						// Default
		DIRECTION,					// For joystick movement
		DESTINATION,				// For target movement
		PLAYER,
		CREEP,
		MONSTER,
		SPARKPOINT,
	}
    #endregion
	#region PLAYER_STATE
	//Finite state machines
	public PlayerStateIdle		    m_idleFSM;
	public PlayerStateMove		    m_moveFSM;
	public PlayerStateAttack	    m_attackFSM;
	public PlayerStateCapture       m_captureFSM;
	public PlayerStateDead          m_deadFSM;
    public PlayerStateActionAttack  m_actionAttackFSM;
	
	private PlayerStateBase     m_playerState;
	#endregion

	#region PLAYER_LEVEL_UP_STAT_INCREASE_VALUES
	public int m_physicalAttackPerLvl;
	public int m_physicalDefensePerLvl;
	public int m_magicAttackPerLvl;
	public int m_magicDefensePerLvl;
	public float m_critChancePerLvl;
	#endregion

	#region PLAYER_INFORMATIONS
	public int team;
	public int playerID;
	private static int ID = 0;
    int spawnPoint;
	public string playerName;
	public float speed;
	public float realspeed;
	#endregion
	#region PLAYER_GAME_PARAMS
	public float totalRespawnTime;
	public float remainingRespawnTime;
	#endregion
	#region PLAYER_TARGET_PARAMS
	public Vector3 m_delta;								// Specific for Joystick movement.
	public bool m_deltaMoveFlag = false;

	public TargetType m_targetType = TargetType.NONE;
	public TargetType m_moveTargetType;
	public string m_moveTargetName;
	UnitObject m_moveTargetUnit;

	public string targetName;

    public Vector3 moveTo;
	public Vector3 target; 
    public Transform targetTransform;
	public PlayerInput.TargetType targetType;
	public float m_targetTolerance = 0.5f;				// May be different for different heroes' walking animation.
    public Vector3 combatTarget;

	public UnitObject targetUnit;
	#endregion
	#region PLAYER_CHAT_PARAMS
    // Should have a chat manager handle this.
	public float chatTimerLength = 5;
	float chatTimer;
	bool isChatMsgShown;
	string chatMsg = "";
	#endregion

	#region GAME_INFORMATIONS
	ArrayList teamSparkPoints;
	SparkPoint[] sparkPoints;
	List<Player> players;
	#endregion


	Vector3 tempPosition;
	Vector3 tempValue;
    //int tempInt;
	float totalSqrLength;
    

	//public float lineAttackDist;
	//public float areaAttackRadius;
	public UnityEngine.Object lineAttackPrefab;
	public UnityEngine.Object areaAttackPrefab;
	public UnityEngine.Object missilePrefab;

	public float m_attackRange = 100.0f;
    public float m_meleeRange;
    public float maxMana;
    public float unitMana;
    public float regenManaPerSec;
    private System.DateTime regenManaTime;
    private long lastCalculatedTime = 0;

	public Attack.AttackType m_baseAttackType;
	public Attack[] m_specialAttacks;
	public Element.ElementType m_primaryElementalEnchantment;
	public Element.ElementType m_elementalEnchantment = Element.ElementType.None;
    public Hit tempHit;
	public Transform m_rightHand;
	private GameObject m_currentEnchantment;

    public GameObject m_hpUnit;
    public PlayerInput m_playerInput;
    public AGE.Action m_action = null;

    public Dictionary<string, float> m_lastSkillCastTime = new Dictionary<string, float>();

	//Initial values for attack objects to be tinkered with from the inspector
	public int m_basicAttackSpeed;
	public int m_basicAttackCooldownTime;
	public bool isBasicAttackCool;
	//public Effect
	public int m_rangedAttackRange;
	public int m_rangedAttackCooldownTime;
	public bool isRangedAttackCool;
	public int m_lineAttackRange;
	public int m_lineAttackDuration;
	public int m_lineAttackCooldownTime;
	public bool isLineAttackCool;
	public int m_areaAttackRadius;
	public int m_areaAttackDuration;
	public int m_areaAttackCooldownTime;
	public bool isAreaAttackCool;


	private Animator anim;
	private bool isAnimated;
    
    private Timer m_slowTimer;
    private bool isSlowed;
	public static float m_XP = 50;
	public static float m_XPradius = 35;
    public float currentXP;
    public float m_LvlXP;
    private int m_Level;
    public int m_MaxLevel = 10;
	public float[] m_LevelsXP = {25, 100, 100, 100, 100, 100, 100, 100, 100, 100};
    public bool m_usePlayerDefaults = true;

	public PlayerState playerState;
    public PlayerHero m_heroType;

    InputManager t_inputManager;
	public GameState gameState;
	
	/* Calculating Player and LS.Region Variables. -jk */
	LS.Region[] region;
	float regionArea;
	float regionSign;
	float regionBarS;
	float regionBarT;
	float regionPointOffset;
	Vector3 regionPoint0;
	Vector3 regionPoint1;
	Vector3 regionPoint2;
	Vector3 regionTempPos;
	Vector3 tempVector;

    public bool isAutoAttacking = false;

	public const float elementDropRate = 1.0f;

    public float m_sparkPointCapturingTime = 5.0f;

	public int CurrLevel{
		get {
			return m_Level;
			}
	}

	private bool m_seenByCamera;

	public SparkPoint m_currentlyCapturedSparkPoint;

	// Use this for initialization
	public override void Awake () {
		// Init CombatManager, NavMeshAgent, EffectList in UnitObject.cs
        base.Awake();
        m_unitType = UnitObjectType.DYNAMIC;
		//Set player IDs
		playerID = ID;
		ID++;
		// Set unity objects hierarchy.
		transform.parent = GameObject.Find("Players").transform;

		// Initialize all states, switch to Idle.
		m_idleFSM = new PlayerStateIdle(this);
		m_moveFSM = new PlayerStateMove(this);
		m_attackFSM = new PlayerStateAttack(this);
		m_captureFSM = new PlayerStateCapture(this);
		m_deadFSM = new PlayerStateDead(this);
        m_actionAttackFSM = new PlayerStateActionAttack(this); 
		SwitchState(PlayerState.Idle);

		// Init player informations.
		team = (int)photonView.instantiationData[0];
		gameObject.name = (string)photonView.instantiationData[1];
		m_heroType = (PlayerHero)photonView.instantiationData[2];
		speed = 5;
		
		//Initialize XP system
		currentXP = 0;
		m_Level = 1;
        SetXPtoLevel();

		// Init target params.
		m_moveTargetType = TargetType.NONE;
		targetUnit = null;
		target = this.transform.position;
		target.y = 0;

		// Init player ability values.
		this.maxHealth = 50;
		this.unitHealth = 50;
		maxMana = 100;
		unitMana = 100 ;
		regenManaPerSec = 5;
		regenManaTime = DateTime.Now;


		// Hook to InputManagwe
		t_inputManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<InputManager>();



		// Initialize chat variables.
		chatTimer = chatTimerLength;
		isChatMsgShown = false;
		chatMsg = "";
        
        
        

		anim = GetComponent<Animator>();
		if(anim == null) isAnimated = false;
		else isAnimated = true;

		gameState = GameState.GameStart;
        

		//this.baseAttack = 5;
        totalRespawnTime = 5;
        sparkPoints = FindObjectsOfType<SparkPoint>();
        teamSparkPoints = new ArrayList();
        
		// Grab all sparkpoints on your team
		for (int i = 0; i < sparkPoints.Length; i++) {
			if (sparkPoints[i].GetOwner() == team) {
				teamSparkPoints.Add(sparkPoints[i]);
			}
		}



		//lineAttackDist = 30.0f;
		//areaAttackRadius = 10.0f;

		//m_meleeRange = 25;

        // initialize line renderer for drawing path to false
        GetComponent<LineRenderer>().enabled = false;

		/* initialize region variables. -jk */
		region = GameObject.FindGameObjectWithTag("Ground").GetComponents<LS.Region>();
		regionArea = 0;
		regionSign = 0;
		regionBarS = 0;
		regionBarT = 0;
		regionPointOffset = 100.0f;
		regionPoint0 = Vector3.zero;
		regionPoint1 = Vector3.zero;
		regionPoint2 = Vector3.zero;
		regionTempPos = Vector3.zero;
		tempVector = new Vector3(100.0f,0.0f,100.0f);


		//Set player's basic attack
		m_baseAttackType = Attack.AttackType.Melee;
		m_basicAttackSpeed = 2;
		m_basicAttackCooldownTime = 2;
		isBasicAttackCool = true;
		m_rangedAttackRange = 100;
		m_rangedAttackCooldownTime = 1;
		isRangedAttackCool = true;
		m_lineAttackRange = 10;
		m_lineAttackDuration = 1;
		m_lineAttackCooldownTime = 1;
		isLineAttackCool = true;
		m_areaAttackRadius = 10;
		m_areaAttackDuration = 1;
		m_areaAttackCooldownTime = 1;
		isAreaAttackCool = true;


		HitObjLookUp hitobj = GetComponent<HitObjLookUp>();
	
		tempHit = new Hit();
		switch((int)m_primaryElementalEnchantment) {
			case (int)Element.ElementType.Fire: 
			tempHit.SetEffect(new Effect(Hit.EffectType.Burn,0)); 
			for(tempInt = 0; tempInt < 5; tempInt++) {
				hitobj.hits[tempInt].m_primaryEffectType = Hit.EffectType.Burn;
			}
			break;
			case (int)Element.ElementType.Air: 
			tempHit.SetEffect(new Effect(Hit.EffectType.Bounce,0)); 
			for(tempInt = 0; tempInt < 5; tempInt++) {
				hitobj.hits[tempInt].m_primaryEffectType = Hit.EffectType.Bounce;
			}
			break;
			case (int)Element.ElementType.Earth: 
			tempHit.SetEffect(new Effect(Hit.EffectType.Slow,0)); 
			for(tempInt = 0; tempInt < 5; tempInt++) {
				hitobj.hits[tempInt].m_primaryEffectType = Hit.EffectType.Slow;
			}
			break;
			case (int)Element.ElementType.Light: 
			tempHit.SetEffect(new Effect(Hit.EffectType.Regen,0)); 
			for(tempInt = 0; tempInt < 5; tempInt++) {
				hitobj.hits[tempInt].m_primaryEffectType = Hit.EffectType.Regen;
			}
			break;
			case (int)Element.ElementType.Dark: 
			tempHit.SetEffect(new Effect(Hit.EffectType.Leech,0)); 
			for(tempInt = 0; tempInt < 5; tempInt++) {
				hitobj.hits[tempInt].m_primaryEffectType = Hit.EffectType.Leech;
			}
			break;
			case (int)Element.ElementType.Ice: 
			tempHit.SetEffect(new Effect(Hit.EffectType.Freeze,0)); 
			for(tempInt = 0; tempInt < 5; tempInt++) {
				hitobj.hits[tempInt].m_primaryEffectType = Hit.EffectType.Freeze;
			}
			break;
		}

		
		//Hit tempHit = this.gameObject.AddComponent<Hit>();

		if (m_baseAttackType == Attack.AttackType.Melee) {
			m_basicAttack = gameObject.AddComponent<MeleeAttack>();
			((MeleeAttack)m_basicAttack).CreateMeleeAttack(Attack.AttackType.Melee, tempHit, m_basicAttackSpeed);
		}
		else {
			m_basicAttack = gameObject.AddComponent<RangedAttack>();
			((RangedAttack)m_basicAttack).CreateRangedAttack(Attack.AttackType.Ranged, tempHit, m_basicAttackSpeed, m_rangedAttackRange);
		}

		//set player's special attacks

       // m_hpUnit = UIManager.mgr.getHPCircle(this);
        UIManager.mgr.addHp(this);
        //if(this.GetComponent<PlayerInput>().isMine) {
        //    GameObject.Find("Manager").GetComponent<UIManager>().setPlayer(this);
        //}

		// Set default correctPos
		// this.correctUnitPos = this.transform.position;

        PlayerManager.Instance.OnPlayerInstantiated(this);

		m_specialAttacks[0] = gameObject.AddComponent<LineAttack>();
		((LineAttack)m_specialAttacks[0]).CreateLineAttack(Attack.AttackType.Line, tempHit, m_lineAttackRange, m_lineAttackDuration, m_lineAttackCooldownTime);
		m_specialAttacks[1] = gameObject.AddComponent<AreaAttack>();
		((AreaAttack)m_specialAttacks[1]).CreateAreaAttack(Attack.AttackType.Area, tempHit, m_areaAttackRadius, m_areaAttackDuration, m_areaAttackCooldownTime, true);
		m_specialAttacks [2] = gameObject.AddComponent<AreaAttack>();
		((AreaAttack)m_specialAttacks[2]).CreateAreaAttack(Attack.AttackType.Area, tempHit, m_areaAttackRadius, m_areaAttackDuration, m_areaAttackCooldownTime, false);
	
		//GameObject.Find ("Manager").GetComponent<UIManager>().m_virtualControlForm.m_joyStick.OnDropped += SwitchToIdle;

		m_currentEnchantment = new GameObject();
		m_currentEnchantment.transform.parent = m_rightHand;
        

        m_playerInput = GetComponent<PlayerInput>();

		m_seenByCamera = false;

		this.UpgradeAttacks();//Applies the damage and crit chance bonuses(from hero stats) to hit objects

		this.m_currentlyCapturedSparkPoint = null;
	}



    void OnTriggerExit(Collider col) {
        if (collider.name.Contains("Lane")) {
            TriangulatePosition(null, collider);
        }
    }


	// Update is called once per frame
	new void Update () {
        // don't sync position until we have good local prediction method
		//base.Update();

		if (Input.GetKeyDown(KeyCode.V)) {
			AnnounceGameEnd();
		}

        //Debug.Log("updating player " + this.name);
		if(gameState == GameState.GameStart) {
			gameState = GameState.InGame;
			
			// set player's starting sparkpoint
			/*if(teamSparkPoints.Capacity == 0) {
				players = PlayerManager.Instance.allPlayers;
				List<int> usedValues = new List<int>();
				for(int a = 0; a < players.Count; a++) {
					int rand = UnityEngine.Random.Range(0, sparkPoints.Length);
					while(usedValues.Contains(rand)) {
						rand = UnityEngine.Random.Range(0, sparkPoints.Length);	
					}
					usedValues.Add(rand);
					players[a].GetComponent<Player>().teamSparkPoints.Add(sparkPoints[rand]);
					sparkPoints[rand].isStartingSparkpoint = true;
				}
			}*/
		}
        realspeed = speed * (100.0f - slowAmount) / 100.0f;
        // Add effects from temp list into actual list
        foreach (Effect e in tempEffectList) {
            Debug.Log("adding from temp effect list");
            AddEffect(e);
        }
        tempEffectList.Clear();
        UpdateEffects();
		UnitUpdate ();

        if (PhotonNetwork.isMasterClient)
            regenMana();

        // Health calculation is currently inside UnitUpdate() so I'll be using a second check for now
        if (unitHealth <= 0 && m_playerState.State != PlayerState.Dead) {

            PlayerManager.Instance.photonView.RPC("RPC_setPlayerDeath",
                                                   PhotonTargets.All,
                                                   this.name, 
                                                   team);
            if (m_hpUnit) {
                m_hpUnit.SetActive(false);
            }

            return;
        }

		//Debug.Log("player state = " + playerState);
		//Debug.Log("animation playing = " + anim.GetCurrentAnimatorStateInfo(0).nameHash);


		// 
        if (!isFrozen && !isKnockedUp) {
			/*if(PhotonNetwork.isMasterClient == true || m_playerInput.isMine == true) {
				// MasterClient(Server) update and that client prediction. 
				m_playerState.OnUpdate();
				if(m_playerInput.isMine) {
					// Still need specific syncronization.

				}
			} else {
				base.Update();
			}*/

			if(m_playerInput.isMine == true) {
				m_playerState.OnUpdate();
				////
				photonView.RPC("RPC_syncTransform", PhotonTargets.OthersBuffered, this.transform.position, this.transform.rotation);
				////
			} else {
				transform.position = Vector3.Lerp(transform.position, this.correctUnitPos, Time.deltaTime * 5);
				transform.rotation = Quaternion.Lerp(transform.rotation, this.correctUnitRot, Time.deltaTime * 5);
			}
        }
        playerState = m_playerState.State;

		//base.Update();

        //UpdateEffects();
		
        // Draw Path
        /*if (GetComponent<NavMeshAgent>().hasPath && playerState != PlayerState.Dead)
        {
            DrawPath(GetComponent<NavMeshAgent>().path);
            GetComponent<LineRenderer>().enabled = true;
        }
        else 
        {
            GetComponent<LineRenderer>().enabled = false;
        }*/

		if(isChatMsgShown) {
			chatTimer -= Time.deltaTime;
			if(chatTimer <= 0){
				chatTimer = chatTimerLength;
				isChatMsgShown = false;
			}
		}


		// TEMPORARY, used to test players ability to acquire a piece of lore
		if (Input.GetKeyDown("l")) {
            LoreManager.LoreItem lore = new LoreManager.LoreItem();
            lore.name = "Temp Lore Title";
            lore.text = "Temp Lore Text";
			AcquireLoreItem(lore);
		}

		//Camera focus needed?
		//CameraManager = PhotonNetwork
		Camera cam = CameraManager.Instance.camera;
		Vector3 viewPortDif = cam.WorldToViewportPoint(this.gameObject.transform.position);
		bool seenByCamera = (viewPortDif.x >= 0.2f) && (viewPortDif.x <= .8f) && (viewPortDif.y >= 0.2f) && (viewPortDif.y <= .8f);

		if (this.GetComponent<PlayerInput> ().isMine && m_seenByCamera && !seenByCamera && cam.velocity.magnitude == 0.0f){
			CameraManager.Instance.FollowPlayer();
		}

		m_seenByCamera = seenByCamera;
	}

	void LateUpdate() {
		/*if(m_deltaMoveFlag) {
			m_delta.Normalize();
			m_delta = m_delta * (Mathf.Sqrt((anim.deltaPosition / Time.deltaTime * (100.0f - slowAmount) / 100.0f).sqrMagnitude) / 100f);
			Debug.Log("In Player:Move Delta=" + m_delta);
			GetComponent<NavMeshAgent>().Move(m_delta);
			m_deltaMoveFlag = false;
		}*/
	}

	/// <summary>
	/// Basic unity OnGUI func, just for showing temp information and unfinished functionality.
	/// Should be empty at final version.
	/// </summary>
	void OnGUI () {
        //if (this.GetComponent<PlayerInput> ().isMine) {
        //    GUI.TextArea (new Rect (10, 200, 200, 40), chatMsg);
        //}
		//		if (this.GetComponent<PlayerInput> ().isMine) {
//			GUI.TextArea (new Rect (10, 10, 200, 20), "Position:" + this.transform.position.x + ":" + this.transform.position.y + ":" + this.transform.position.z);
//			GUI.TextArea (new Rect (10, 30, 200, 20), "Target:" + target.x + ":0:" + target.z);
//			GUI.TextArea (new Rect (10, 50, 220, 20), "A for area attack, S for line attack, D for self area attack");
//            GUI.TextArea(new Rect(10, 70, 200, 20), "F to kill yourself");
//			
//			//Gui debug message for state of player in-game/post-game - BR
//			switch(gameState) {
//			case GameState.GameStart:
//				GUI.TextArea(new Rect(10, 90, 200, 20), "Starting Game."); 
//				break;
//			case GameState.InGame:
//				GUI.TextArea(new Rect(10, 90, 200, 20), "Game in Progress."); 
//				break;
//			case GameState.Victory:
//				GUI.TextArea(new Rect(10, 90, 200, 20), "Victory!");
//				break;
//			case GameState.Defeat:
//				GUI.TextArea(new Rect(10, 90, 200, 20), "Defeat...");
//				break;
//			}
//		}
	}

	public void SendMessage(string msg) {
		//Debug.Log ("Sending Message: " + msg);
		msg = this.name + ": " + msg;

		players = PlayerManager.Instance.allPlayers;
		for(int a = 0; a < players.Count; a++) {
			if(players[a].team == team) {
				GameObject.Find("Ground").GetPhotonView().RPC("RPC_sendPlayerMessage", PhotonTargets.All, 
		                    								  players[a].name, msg);
			}
		}
	}

	public void ShowMessage(string msg) {
		isChatMsgShown = true;
		chatMsg = msg;
		chatTimer = chatTimerLength;
	}

    // fit animation speed with navmesh movement
    void OnAnimatorMove() {
        //if (PhotonNetwork.isMasterClient)
		GetComponent<NavMeshAgent>().velocity = anim.deltaPosition / Time.deltaTime * (100.0f - slowAmount) / 100.0f;
    }

	//called when player state switches
	//sets animation
	public void SwitchState(PlayerState nextState) {

		//What is this line supposed to do?
		//if(!this.GetComponent<PlayerInput>().isMine) return;

        if (nextState != PlayerState.ActionAttacking && m_action != null) m_action.Stop();


		//Debug.Log("Player animation switching to: " + nextState);

		//reset anim triggers
        // do it in set animation param
		//this.photonView.RPC("RPC_resetAnimTriggers", PhotonTargets.AllBuffered);


		if (m_playerState != null && m_playerState.State == nextState)
			return;

		
		if (m_playerState != null)
			m_playerState.OnExit();

//		m_playerState.State = nextState;
		
		switch (nextState) {
		case PlayerState.Idle:
			m_playerState = m_idleFSM;
			break;
		case PlayerState.Moving:
			m_playerState = m_moveFSM;
			break;
		case PlayerState.Attacking:
			m_playerState = m_attackFSM;
			break;
		case PlayerState.Capturing:
			m_playerState = m_captureFSM;
			break;
		case PlayerState.Dead:
			m_playerState = m_deadFSM;
			break;
        case PlayerState.ActionAttacking:
			m_playerState = m_actionAttackFSM;
			break;
		}
        playerState = nextState;
		m_playerState.OnEnter();

	}

	//used when joystick is dragged / dropped
	public void SwitchToIdle() {
        if (m_playerState != null && m_playerState.State != PlayerState.Attacking && m_playerState.State != PlayerState.Capturing) {
            SwitchState(PlayerState.Idle);
            return;
        }
	}
	public void SwitchToDirectMoving() {
		//Debug.Log("switching to moving");
		targetType = PlayerInput.TargetType.None;


		m_moveTargetType = TargetType.DIRECTION;
		SwitchState(PlayerState.Moving);
        return;
	}



	//Dont let the player use basic attacks until the timer has finished
	public IEnumerator coolBasicAttack () {
		isBasicAttackCool = false;
		yield return new WaitForSeconds (m_basicAttackCooldownTime);
		isBasicAttackCool = true;
	}

	//Dont let the player use ranged attacks until the timer has finished
	public IEnumerator coolRangedAttack () {
		isRangedAttackCool = false;
		yield return new WaitForSeconds (m_rangedAttackCooldownTime);
		isRangedAttackCool = true;
	}

	//Dont let the player use basic attacks until the timer has finished
	public IEnumerator coolLineAttack () {
		isLineAttackCool = false;
		yield return new WaitForSeconds (m_lineAttackCooldownTime);
		isLineAttackCool = true;
	}

	//Dont let the player use ranged attacks until the timer has finished
	public IEnumerator coolAreaAttack () {
		isAreaAttackCool = false;
		yield return new WaitForSeconds (m_areaAttackCooldownTime);
		isAreaAttackCool = true;
	}

	//Sends the call for special attack off to the combat manager
	//Input: Type of attack (area or line) and where the combat is located
	public void EngageCombat(PlayerInput.TargetType combatType, Vector3 location) {
		//Combat manager was null so I get it here for now
		GameObject manager = GameObject.Find ("Manager");
		combatManager = (CombatManager) manager.GetComponent ("CombatManager");
		if (combatType == PlayerInput.TargetType.LineAttack) {

			LineAttack tempLine = null;

			for (int i = 0; i < m_specialAttacks.GetLength(0); i++) {
				if (m_specialAttacks [i].m_attackType == Attack.AttackType.Line) {
					tempLine = (LineAttack) m_specialAttacks [i];
					break;
				}
			}

			if (tempLine != null)
				combatManager.LineAttack (this.name, location, tempLine);
		}
		else if (combatType == PlayerInput.TargetType.TargetAreaAttack) {
			AreaAttack tempArea = null;
			
			for (int i = 0; i < m_specialAttacks.GetLength(0); i++) {
				if (m_specialAttacks [i].m_attackType == Attack.AttackType.Area && !((AreaAttack)m_specialAttacks[i]).m_isPlayerOrigin) {
					tempArea = (AreaAttack) m_specialAttacks [i];
					break;
				}
			}

			if (tempArea != null)
				combatManager.AreaAttack(this.name, location, tempArea);
		}
		else if (combatType == PlayerInput.TargetType.SelfAreaAttack) {
			AreaAttack tempArea = null;
			
			for (int i = 0; i < m_specialAttacks.GetLength(0); i++) {
				if (m_specialAttacks [i].m_attackType == Attack.AttackType.Area && ((AreaAttack)m_specialAttacks[i]).m_isPlayerOrigin) {
					tempArea = (AreaAttack) m_specialAttacks [i];
					break;
				}
			}
			
			if (tempArea != null)
				combatManager.AreaAttack(this.name, location, tempArea);
		}
	}

	public void RefreshTargetUnit()
	{
		//Apparently started causing trouble after some changes, previously made player follow its target
		return;

		if (targetUnit == null)
			return;

		Player tempTargetPlayer = targetUnit.GetComponent<Player>();

		if (tempTargetPlayer != null){

			if (tempTargetPlayer.getPlayerState() == PlayerState.Dead){
				ResetTarget();

				return;
			}

		}

		UpdateTarget(targetUnit.gameObject.transform.position, targetUnit.name, targetUnit);
	}

	public void ResetTarget(){
		targetTransform = null;
		targetUnit = null;
		UIManager.mgr.targetSprite.setObject(null);
		UIManager.mgr.targetSprite.gameObject.SetActive(false);
	}

	public void UpdateTarget(Vector3 p_target, string targetName, UnitObject p_targetUnit) {
		bool targetChanged = !p_target.Equals(target);
		this.targetUnit = p_targetUnit;

		this.target = p_target;
		this.targetName = targetName;
        this.targetTransform = GameObject.Find(targetName).transform;
		this.targetType = PlayerInput.TargetType.Position;
		//m_playerState = PlayerState.Moving;
		if (targetChanged)
			GetComponent<NavMeshAgent>().SetDestination(target);

		SwitchState(PlayerState.Moving);
        return;
	}

    public List<GameObject> GetTarget() {
        List<GameObject> result = new List<GameObject>();
        result.Add(GameObject.Find(targetName));
        return result;
    }

	public void dropElementItem(Vector3 position){

		photonView.RPC ("RPC_DropElement", PhotonTargets.MasterClient, position);

	}

    // Turn player invisible and disable their input and start their respawn timer 
    public void KillPlayer() {
        //m_playerState.State = PlayerState.Dead;
        if (m_action != null)
            m_action.Stop();
            
		Debug.Log ("Player killed!");

		float doesElementDrop = UnityEngine.Random.Range(0.0f, 1.0f);
		if(/*this.GetComponent<PlayerInput>().isMine && */doesElementDrop <= elementDropRate && PhotonNetwork.isMasterClient)
		{
			dropElementItem(this.transform.position);
		}

		SwitchState(PlayerState.Dead);
        target = this.transform.position;
//		Debug.LogError ("Player state switched to dead renderer and collider disabled!");
        renderer.enabled = false;
		collider.enabled = false;
        unitHealth = 0;
        remainingRespawnTime = totalRespawnTime;
    }

    // Turn player visible again, heal them, spawn them at a sparkpoint, and reenable their input.
    public void RespawnPlayer(Vector3 location) {
        //m_playerState = PlayerState.Idle;
		Debug.Log ("Player respawned at " + location.ToString());

        m_navAgent.enabled = false;
        this.transform.position = location;
        target = location;
        this.enabled = true;
        collider.enabled = true;
        renderer.enabled = true;
        unitHealth = maxHealth;
        if (m_hpUnit) {
            m_hpUnit.SetActive(true);
        } else {
            m_hpUnit = GameObject.Find("Manager").GetComponent<UIManager>().getHPCircle(this);
        }

        if (m_playerInput.isMine) {
            CameraManager.Instance.FocusOnPlayer();
        }
        m_navAgent.enabled = true;
        SwitchState(PlayerState.Idle);
    }

	public void CapturedObjective(string sparkPointName) {
// 		m_playerState = PlayerState.Idle;
// 		SwitchState(PlayerState.Idle);
		TriangulatePosition(sparkPointName,null);
	}
	
	public int GetTeam() {
		return team;
	}

    public PlayerState GetState() {
		if (m_playerState == null)
			return PlayerState.Idle;

        return m_playerState.State;
    }
	
	/* Called when game is over and Victory gameState is determined*/
	public void SetVictorious(bool isVictorious) {
		if(isVictorious) {
			gameState = GameState.Victory;
		}
		else {
			gameState = GameState.Defeat;	
		}
		GameObject.Find("Manager").GetComponent<UIManager>().EndGame (isVictorious);
	}
	
    public void DrawPath(NavMeshPath path)
    {
        LineRenderer LR = GetComponent<LineRenderer>();
        

        LR.SetVertexCount(path.corners.Length);

        int i = 0;
        foreach (Vector3 v in path.corners)
        {
            LR.SetPosition(i, v);
            i++;
        }
     

    }

	/* Used to give player access to a piece of lore */
	public void AcquireLoreItem(LoreManager.LoreItem lore) {

		// Until we figure out how to have persistent player data, just display a debug message
		Debug.Log ("New Lore Acquired: " + lore.name);
        ShowMessage("You picked up the lore: " + lore.name);
        //ShowMessage(lore.text);
	}

	public void setElementalEnchantment(Element.ElementType elementType) {
		m_elementalEnchantment = elementType;

//		if(m_currentEnchantment) {
//			GameObject temp = m_currentEnchantment;
//			Destroy(temp);
//		}
		photonView.RPC ("RPC_RemoveEnchantment", PhotonTargets.All);

		photonView.RPC ("RPC_AttachEnchantment", PhotonTargets.All, (int)elementType);

//		DroppableManager dm = GameObject.Find("Manager").GetComponent<DroppableManager>();	
//		m_currentEnchantment = (GameObject)GameObject.Instantiate(dm.m_ElementFX[(int)elementType]);
//		m_currentEnchantment.GetComponent<ParticleSystem>().startLifetime = 0.5f;
//		if(m_currentEnchantment) {
//			m_currentEnchantment.transform.parent = m_rightHand;
//			m_currentEnchantment.transform.localPosition = Vector3.zero;
//		}

        // Apply elemental effect;
		HitObjLookUp hitobj = GetComponent<HitObjLookUp>();

		UIManager.mgr.psu.refresh ();
        switch (elementType) {
            case (Element.ElementType.None):
                m_basicAttack.m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                for (tempInt = 0; tempInt < m_specialAttacks.Length; tempInt++) {
                    m_specialAttacks[tempInt].m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                }
                break;
            case (Element.ElementType.Fire):
                m_basicAttack.m_hitObject.SetEffect(new Effect(Hit.EffectType.Burn, 5, 5));
                for (tempInt = 0; tempInt < m_specialAttacks.Length; tempInt++) {
                    m_specialAttacks[tempInt].m_hitObject.SetEffect(new Effect(Hit.EffectType.Burn, 5, 5));
                }
				for(tempInt = 0; tempInt < 5; tempInt++) {
					hitobj.hits[tempInt].m_secondaryEffectType = Hit.EffectType.Burn;
				}
                break;
            case (Element.ElementType.Ice):
                m_basicAttack.m_hitObject.SetEffect(new Effect(Hit.EffectType.Freeze, 2));
                for (tempInt = 0; tempInt < m_specialAttacks.Length; tempInt++) {
                    m_specialAttacks[tempInt].m_hitObject.SetEffect(new Effect(Hit.EffectType.Freeze, 2));
                }
				for(tempInt = 0; tempInt < 5; tempInt++) {
					hitobj.hits[tempInt].m_secondaryEffectType = Hit.EffectType.Freeze;
				}
				break;
            case (Element.ElementType.Earth):
                // Lightning isnt an element listed in the 1st tier
                m_basicAttack.m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                for (tempInt = 0; tempInt < m_specialAttacks.Length; tempInt++) {
                    m_specialAttacks[tempInt].m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                }
				for(tempInt = 0; tempInt < 5; tempInt++) {
					hitobj.hits[tempInt].m_secondaryEffectType = Hit.EffectType.Slow;
				}
				break;
            case (Element.ElementType.Light):
                m_basicAttack.m_hitObject.SetEffect(new Effect(Hit.EffectType.Regen, 5, 5));
                for (tempInt = 0; tempInt < m_specialAttacks.Length; tempInt++) {
                    m_specialAttacks[tempInt].m_hitObject.SetEffect(new Effect(Hit.EffectType.Regen, 5, 5));
                }
				for(tempInt = 0; tempInt < 5; tempInt++) {
					hitobj.hits[tempInt].m_secondaryEffectType = Hit.EffectType.Regen;
				}
				break;
            case (Element.ElementType.Dark):
                // Leech not yet implemented
                m_basicAttack.m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                for (tempInt = 0; tempInt < m_specialAttacks.Length; tempInt++) {
                    m_specialAttacks[tempInt].m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                }
				for(tempInt = 0; tempInt < 5; tempInt++) {
					hitobj.hits[tempInt].m_secondaryEffectType = Hit.EffectType.Leech;
				}
				break;
            default:
                m_basicAttack.m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                for (tempInt = 0; tempInt < m_specialAttacks.Length; tempInt++) {
                    m_specialAttacks[tempInt].m_hitObject.SetEffect(new Effect(Hit.EffectType.None, 0));
                }
				for(tempInt = 0; tempInt < 5; tempInt++) {
					hitobj.hits[tempInt].m_secondaryEffectType = Hit.EffectType.None;
				}
			break;
        }
	}


	/* Turns on player's health regen when in own region. -jk */
	private void TriangulatePosition(string sparkPointName, Collider collider) {
		/* called when stepping across lane into region. -jk */
		if (sparkPointName == null) {
			for (int i = 0; i < region.Length; i++) {
				if (region[i].GetActivated() && region[i].GetTeam() == team) {
					for (int j = 0; j < region[i].regionPoints.Length; j++) {
						if (!collider.name.Contains(region[i].regionPoints[j].name)) {
							continue;
						}
						else {
							regionPoint0 = region[i].regionPoints[0].transform.position;
							regionPoint0 += tempVector;
							regionPoint1 = region[i].regionPoints[1].transform.position;
							regionPoint1 += tempVector;
							regionPoint2 = region[i].regionPoints[2].transform.position;
							regionPoint2 += tempVector;
							regionTempPos = transform.position + tempVector;
							regionArea = 0.5f * (-regionPoint1.z * regionPoint2.x + regionPoint0.z * 
							                     (-regionPoint1.x + regionPoint2.x) + regionPoint0.x *
							                     (regionPoint1.z - regionPoint2.z) + regionPoint1.x * 
							                     regionPoint2.z);
							regionSign = regionArea < 0 ? -1 : 1;
							regionBarS = (regionPoint0.z * regionPoint2.x - regionPoint0.x * 
							              regionPoint2.z + (regionPoint2.z - regionPoint0.z) *
							              regionTempPos.x + (regionPoint0.x - regionPoint2.x) * 
							              regionTempPos.z) * regionSign;
							regionBarT = (regionPoint0.x * regionPoint1.z - regionPoint0.z * 
							              regionPoint1.x + (regionPoint0.z - regionPoint1.z) * 
							              regionTempPos.x + (regionPoint1.x - regionPoint0.x) * 
							              regionTempPos.z) * regionSign;
							if (regionBarS > 0 && regionBarT > 0 && 
							    (regionBarS + regionBarT) < 2 * regionArea * regionSign) {
								particleSystem.Play();
							}
							else {
								particleSystem.Stop();
							}
							return;
						}
					}
				}
			}
		}
		/* called when in region and capturing. -jk */
		else {
			for (int i = 0; i < region.Length; i++) {
				if (region[i].GetActivated() && region[i].GetTeam() == team) {
					for (int j = 0; j < region[i].regionPoints.Length; j++) {
						if (!region[i].regionPoints[j].name.Contains(sparkPointName)) {
							Debug.Log (region[i].regionPoints[j].name + " " +sparkPointName);
							continue;
						}
						else {
							regionPoint0 = region[i].regionPoints[0].transform.position;
							regionPoint0 += tempVector;
							regionPoint1 = region[i].regionPoints[1].transform.position;
							regionPoint1 += tempVector;
							regionPoint2 = region[i].regionPoints[2].transform.position;
							regionPoint2 += tempVector;
							regionTempPos = transform.position + tempVector;
							regionArea = 0.5f * (-regionPoint1.z * regionPoint2.x + regionPoint0.z * 
							                     (-regionPoint1.x + regionPoint2.x) + regionPoint0.x *
							                     (regionPoint1.z - regionPoint2.z) + regionPoint1.x * 
							                     regionPoint2.z);
							regionSign = regionArea < 0 ? -1 : 1;
							regionBarS = (regionPoint0.z * regionPoint2.x - regionPoint0.x * 
							              regionPoint2.z + (regionPoint2.z - regionPoint0.z) *
							              regionTempPos.x + (regionPoint0.x - regionPoint2.x) * 
							              regionTempPos.z) * regionSign;
							regionBarT = (regionPoint0.x * regionPoint1.z - regionPoint0.z * 
							              regionPoint1.x + (regionPoint0.z - regionPoint1.z) * 
							              regionTempPos.x + (regionPoint1.x - regionPoint0.x) * 
							              regionTempPos.z) * regionSign;
							if (regionBarS > 0 && regionBarT > 0 && 
							    (regionBarS + regionBarT) < 2 * regionArea * regionSign) {
								particleSystem.Play();
							}
							else {
								particleSystem.Stop();
							}
							return;
						}
					}
				}
			}
		}
	}



	public void SetPlayerPosAndRot(Vector3 pos){
		// this.correctUnitPos = pos;
		// this.correctUnitRot = Quaternion.LookRotation(pos - this.transform.position);
	}

    new void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (!LevelManager.Instance.m_startedGame)
            return;
        // base.OnPhotonSerializeView(stream, info);
    }

	public LineAttack GetLineAttack(){
		return (LineAttack) m_specialAttacks[0];
	}

	public AreaAttack GetAreaAttack(){
		return (AreaAttack) m_specialAttacks[2];
	}

	public AreaAttack GetSelfAreaAttack(){
		return (AreaAttack) m_specialAttacks[1];
	}

    /// <summary>
    /// Used when the Player experiences some sort of slow effect
    /// </summary>
    /// <param name="slow">The % to slow the player down by</param>
    /// <param name="duration">The duration of the slow effect.</param>
    public void SlowEffect(float slow, int duration)
    {
        if (!isSlowed)
        {
            isSlowed = true;

			photonView.RPC("RPC_ApplySlowEffect", PhotonTargets.All, null);

			//Oldspeed = speed;
			slowAmount = slow;
            Timer m_slowTimer = new Timer(duration);
            m_slowTimer.Elapsed += On_SlowEnd;
            m_slowTimer.Start();
        }
        else
        {
            m_slowTimer = new Timer(duration);
            m_slowTimer.Start();
        }

    }

	[RPC]
	public void RPC_ApplySlowEffect() {
		GameObject go = (GameObject)Instantiate(EffectManager.Instance.m_slowEffect);
		go.transform.parent = this.transform;
		go.transform.localPosition = Vector3.zero;
	}

    //Called when the slow duration timer is up -> Resets the original player speed
    public void On_SlowEnd(System.Object source, ElapsedEventArgs e)
    {
        isSlowed = false;
        //speed = Oldspeed;
		slowAmount = 0;
        m_slowTimer.Enabled = false;
        m_slowTimer.Close();
    }


	/// <summary>
	/// Applies the damage and crit chance bonuses(from hero stats) to hit objects
	/// called once initially and each time the hero levels up
	/// </summary>
	public void UpgradeAttacks(){

		HitObjLookUp hitlookup = gameObject.GetComponent<HitObjLookUp>();
		if (hitlookup == null){
			Debug.LogError("HitObjLookUp component null!");
			return;
		}

		for (int i = 0; i < hitlookup.hits.Length; i++){
			Hit hit = hitlookup.hits[i];
			hit.SetDamageBonusCritChance(hit.m_isMagicDamage ? this.m_magicAttack : this.m_physicalAttack,
			                             this.m_critChance);

		}


	}

	/// <summary>
	/// Levels up the player and plays some sort of Particle Effect
	/// </summary>
    [RPC]
    public void RPC_LevelUp()
    {

        if (m_Level < m_MaxLevel)
        {
            m_Level++;
			currentXP = 0;

            SetXPtoLevel();

			//Increase stats
			this.m_physicalAttack += this.m_physicalAttackPerLvl;
			this.m_physicalDefense += this.m_physicalDefensePerLvl;
			this.m_magicAttack += this.m_magicAttackPerLvl;
			this.m_magicDefense += this.m_magicDefensePerLvl;
			this.m_critChance += this.m_critChancePerLvl;

			UpgradeAttacks();

        }
		//Some sort of visual feedback on level up
		//ParticleSystem lvlEffect = GetComponent<ParticleSystem> ();
		//lvlEffect.loop = false;
		//lvlEffect.enableEmission = true;
		//lvlEffect.Play ();
		LevelUpAnim();
    }

	[RPC]
	void LevelUpAnim() {
		GameObject lvUp = (GameObject)GameObject.Instantiate(LevelManager.Instance.m_levelUpEffect);
		lvUp.transform.parent = this.transform;
		lvUp.transform.localPosition = Vector3.zero;

		GameObject lvUp2 = (GameObject)GameObject.Instantiate(LevelManager.Instance.m_levelUpEffect02);
		lvUp2.transform.parent = this.transform;
		lvUp2.transform.localPosition = Vector3.zero;
	}

    /// <summary>
    /// Helper function for setting experience needed for next level.
    /// </summary>
    private void SetXPtoLevel() {
        Player xpReference;
        if (m_usePlayerDefaults)
        {
            xpReference = Player.Instance;
        }
        else
        {
            xpReference = this;
        }
        m_LvlXP = xpReference.m_LevelsXP[m_Level];
    }

	/// <summary>
	/// Called from a dying unit. Gives the player XP from killing the unit. 
	/// </summary>
	/// <param name="xp">Experience granted from the dying unit.</param>
    public void GetXP(float xp)
    {
        currentXP += xp;
        //If you've reached the required experience for that level then level up
        if (PhotonNetwork.isMasterClient && currentXP >= m_LvlXP)
        {
            photonView.RPC("RPC_LevelUp", PhotonTargets.AllBufferedViaServer);
        }

    }

	public void AnnounceGameEnd() {
		gameState = GameState.Defeat;
		int winningTeam;
		if (team == 1) {
			winningTeam = 2;
		} else {
			winningTeam = 1;
		}
        PlayerManager.Instance.photonView.RPC("RPC_setTeamVictory", PhotonTargets.AllBufferedViaServer, winningTeam);

	}


	[RPC]
	void RPC_DropElement(Vector3 position) {
		int element = UnityEngine.Random.Range(0, Element.ElementTypeCount);
		object[] initData = {(Element.ElementType) element};
		//PhotonNetwork.Instantiate("Element.prefab", this.transform.position, this.transform.rotation, this.photonView.group);
		PhotonNetwork.InstantiateSceneObject("Element", position, Quaternion.identity, 0, initData);

	}

	[RPC]
	void RPC_AttachEnchantment(int element) {

		DroppableManager dm = GameObject.Find("Manager").GetComponent<DroppableManager>();	
		m_currentEnchantment = (GameObject)GameObject.Instantiate(dm.m_ElementFX[element]);
		if(m_currentEnchantment) {
			if(m_currentEnchantment.GetComponent<ParticleSystem>()) 
				m_currentEnchantment.GetComponent<ParticleSystem>().startLifetime = 0.5f;

			m_currentEnchantment.transform.parent = m_rightHand;
			m_currentEnchantment.transform.localPosition = Vector3.zero;
		}
		//PhotonNetwork.InstantiateSceneObject(dm.m_ElementNames[element], pos, Quaternion.identity, 0, null);
	}

	[RPC]
	void RPC_RemoveEnchantment() {
		if(m_currentEnchantment) {
			GameObject temp = m_currentEnchantment;
			Destroy(temp);
		}
	}

	//reset all animation triggers
	[RPC]
	public void RPC_resetAnimTriggers() {
		if(anim) {
			anim.ResetTrigger("stateIdle");
			anim.ResetTrigger("stateWalk");
			anim.ResetTrigger("stateDead");
			anim.ResetTrigger("stateAttack");

            if (m_heroType == PlayerHero.LEVANTIS) {
                anim.ResetTrigger("attackSmashySmashy");
                anim.ResetTrigger("attackWhirlingDervish");
                anim.ResetTrigger("attackSnakeHook");
                //anim.ResetTrigger("stateAttack");
            }
            else if (m_heroType == PlayerHero.DELIA) {
                anim.ResetTrigger("attackAngelFlight");
                anim.ResetTrigger("attackSmite");
                anim.ResetTrigger("attackHolyFire");
                anim.ResetTrigger("attackDivineFury");
            }
		}
	}

	/// <summary>
	/// change animator parameter over network. must cast animType to int and value to float when calling
	/// </summary>
	/// <param name="animType"></param>
	/// <param name="param"></param>
	/// <param name="value"></param>
	[RPC]
	void RPC_setAnimParam(int animType, string param, float value = 0) {
        RPC_resetAnimTriggers();
		if(anim) {
			switch ((AnimatorType)animType) {
			case AnimatorType.BOOL:
				anim.SetBool(param, value == 1);
				break;
			case AnimatorType.INT:
				anim.SetInteger(param, (int)value);
				break;
			case AnimatorType.FLOAT:
				anim.SetFloat(param, value);
				break;
			case AnimatorType.TRIGGER:
				anim.SetTrigger(param);
				break;
			}
		}
	}
	public void SetAnimParam(int animType, string param, float value = 0){
		if(m_playerInput.isMine) {
			// RPC
			photonView.RPC("RPC_setAnimParam", PhotonTargets.All, animType, param, value);
		}
	}

	/// <summary>
	///  Turns the player to face a point
	/// </summary>
	/// <param name="target">Point to turn towards</param>
	[RPC]
	void RPC_turnToFace(Vector3 target) {
		//transform.LookAt (target);
	}

    /// <summary>
    /// synchronize health between all client from master client. No non-master client should manage health on its own
    /// </summary>
    /// <param name="health"></param>
    [RPC]
    public void UpdateAllClientUnitMana(float mana)
    {
        unitMana = mana;
    }

    [RPC]
    public void UpdateCD(string actionName, float lastTimeCast) {
        m_lastSkillCastTime[actionName] = lastTimeCast;
    }

    private void regenMana()
    {
        DateTime currentTime = DateTime.Now;
        long last = (currentTime.ToFileTime() - regenManaTime.ToFileTime()) / 10000000;
        regenManaTime.AddSeconds(last);

        float tmpMana = unitMana + regenManaPerSec * (last-lastCalculatedTime);
        if (tmpMana > maxMana)
            tmpMana = maxMana;
        if (tmpMana != unitMana)
        {
            unitMana = tmpMana;
            photonView.RPC("UpdateAllClientUnitMana", PhotonTargets.OthersBuffered, unitMana);
        }

        lastCalculatedTime = last;
    }

    public bool castSkill(string actionName)
    {
        float manaCost = SkillLibrary.Instance.m_skills[actionName].manaCost;
        DateTime currentTime = DateTime.Now;
        float theTime = (currentTime.ToFileTime() - regenManaTime.ToFileTime()) / 10000000;
        if (manaCost <= unitMana)
        {
            if (!m_lastSkillCastTime.ContainsKey(actionName) ||
                theTime - m_lastSkillCastTime[actionName] >= SkillLibrary.Instance.m_skills[actionName].coolDownTime) {
                //m_lastSkillCastTime[actionName] = Time.time;
                photonView.RPC("UpdateCD", PhotonTargets.All, actionName, theTime);
                unitMana -= manaCost;
                photonView.RPC("UpdateAllClientUnitMana", PhotonTargets.OthersBuffered, unitMana);
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }

	public bool IsPlayerAlive(){


		return m_playerState != m_deadFSM;

	}

	public PlayerState getPlayerState(){
		if (m_playerState == null)
			return PlayerState.Idle;

		return m_playerState.State;
	}

	#region PLAYER_STATE_CLASSES


	/// <summary>
	/// Base state class. Can only be inherited
	/// </summary>
	[Serializable]
	public abstract class PlayerStateBase {
		// state name
		protected PlayerState m_state;
		// player instance
		protected Player player;
		// when do we start this state?
		protected float startTime;
		
		public PlayerState State { get { return m_state; } }
		
		/// <summary>
		/// This func will be called once when state starts
		/// <para>Put all initialization code here</para> 
		/// </summary>
		public abstract void OnEnter();
		
		/// <summary>
		/// This func will be called every frame when state is activated
		/// <para>Put all Update code here</para> 
		/// </summary>
		public abstract void OnUpdate();
		
		/// <summary>
		/// This func will be called once when state ends
		/// <para>Put all cleanup code here</para> 
		/// </summary>
		public abstract void OnExit();
		
		public PlayerStateBase() { startTime = Time.time; }
	}
	[Serializable]
	public class PlayerStateIdle : PlayerStateBase {
		public PlayerStateIdle(Player pPlayer) {
			startTime = Time.time;
			m_state = PlayerState.Idle;
			player = pPlayer;
		}

		public override void OnEnter() {
			startTime = Time.time;
			if (player.isAnimated) {
				//player.anim.SetTrigger ("stateIdle");
				player.SetAnimParam((int)Player.AnimatorType.TRIGGER, "stateIdle");
// 				player.photonView.RPC ("RPC_setAnimParam", PhotonTargets.AllBuffered, 
// 				                    (int)LaneCreep.AnimatorType.TRIGGER, "stateIdle", 0f);

			}

			player.RefreshTargetUnit();

			// Remove cursor effect
			if(player.m_playerInput != null) {
				if(player.m_playerInput.isMine){
					player.m_playerInput.m_cursorGround.GetComponent<Animator>().SetBool("pingKeep", false);
					player.m_playerInput.m_cursorAir.GetComponent<Animator>().SetBool("cursorKeep", false);
				}
			}
		}
		
		public override void OnUpdate() {
            float attackRange = 0;
            if (player.m_basicAttack.m_attackType == Attack.AttackType.Ranged) {
                //attackRange=m_attackRange;
                attackRange = ((RangedAttack)player.m_basicAttack).m_range;
            }
            if (player.m_basicAttack.m_attackType == Attack.AttackType.Melee) {
                attackRange = player.m_meleeRange;
            }
            //            Debug.Log(attackRange);
            player.tempPosition = player.transform.position;
            player.tempPosition.y = 0;
            if (player.targetTransform != null) {
                player.tempValue = player.target - player.tempPosition;
                player.totalSqrLength = player.tempValue.sqrMagnitude;
                player.tempValue = Vector3.Normalize(player.tempValue) * player.speed * (100.0f - player.slowAmount) / 100.0f * Time.deltaTime;
                player.realspeed = player.speed * (100.0f - player.slowAmount) / 100.0f;
                if (player.totalSqrLength <= (attackRange) * (attackRange)) {
                    if (player.targetUnit.name.StartsWith("Player"))
                    {
                        if (PlayerManager.Instance.PlayerLookUp[player.targetUnit.name].m_playerState.State != PlayerState.Dead && player.isAutoAttacking)
                        {
                            //m_playerState = PlayerState.Attacking;	
                            //PlayerSwitchState(pPlayer.m_attackFSM);
                            player.SwitchState(PlayerState.Attacking);
                            return;
                        }
                    }
                    else if (player.targetUnit.name.StartsWith("JungleC"))
                    {
                        if (MonsterClient.Instance.MonsterLookUp.ContainsKey(player.targetUnit.name) &&
                            MonsterClient.Instance.MonsterLookUp[player.targetUnit.name].curState != JungleMonster.JungleMonsterState.DEAD && 
                            player.isAutoAttacking) {
                            Debug.Log("BRANCHB");
                            player.SwitchState(PlayerState.Attacking);
                            return;
                        }
                    }
                    else if (player.targetUnit.name.StartsWith("LaneC"))
                    {
                        if (CreepManager.Instance.LaneCreepLookUp.ContainsKey(player.targetUnit.name) &&
                            CreepManager.Instance.LaneCreepLookUp[player.targetUnit.name].GetCreepState().State != LaneCreep.CreepState.DEAD && 
                            player.isAutoAttacking) {
                            player.SwitchState(PlayerState.Attacking);
                            return;
                        }
                    }
                    else if (player.targetName.StartsWith("SparkPo")) {
                        if (SparkPointManager.Instance.sparkPointsDict.ContainsKey(player.targetUnit.name) &&
                            SparkPointManager.Instance.sparkPointsDict[player.targetUnit.name].sparkPointState == SparkPoint.SparkPointState.CAPTURED &&
                            player.isAutoAttacking) {
                            player.SwitchState(PlayerState.Attacking);
                            return;
                        }
                    }
                    else if (player.targetUnit.name.StartsWith("Bo"))
                    {
                        player.SwitchState(PlayerState.Attacking);
                    }
                }

                /*else {
                    //this.transform.position = this.transform.position + tempValue;
                    // Draw Path
                    // do not move like this!!
                    //GetComponent<NavMeshAgent>().Move(tempValue);
                    player.GetComponent<NavMeshAgent>().Resume();
                    if (player.GetComponent<NavMeshAgent>().hasPath) {
                        player.DrawPath(player.GetComponent<NavMeshAgent>().path);
                        player.GetComponent<LineRenderer>().enabled = true;
                    }
                }*/
            } else {
				// Debug.Log("Idle in here.");
			}
		}
		
		public override void OnExit() {
			
		}
	}
	[Serializable]
	public class PlayerStateMove : PlayerStateBase {
		public PlayerStateMove(Player pPlayer) {
			startTime = Time.time;
			m_state = PlayerState.Moving;
			player = pPlayer;
		}
		
		public override void OnEnter() {
			startTime = Time.time;
            player.m_navAgent.Resume();
			if (player.isAnimated) {
				//player.anim.SetTrigger ("stateWalk");
				player.SetAnimParam((int)Player.AnimatorType.TRIGGER, "stateWalk");
// 				player.photonView.RPC ("RPC_setAnimParam", PhotonTargets.AllBuffered, 
// 			                    (int)LaneCreep.AnimatorType.TRIGGER, "stateWalk", 0f);
			}
		}
		
		public override void OnUpdate() {
			float attackRange = 0;
			if (player.m_basicAttack.m_attackType == Attack.AttackType.Ranged) {
				//attackRange=m_attackRange;
				attackRange = ((RangedAttack) player.m_basicAttack).m_range;
			}
			if (player.m_basicAttack.m_attackType == Attack.AttackType.Melee) {
                attackRange = player.m_meleeRange;
			}

			player.tempPosition = player.transform.position;
			player.tempPosition.y = 0;

			// If vector3 target not equal.
			// Otw: all option use NavMesh distance.
			if(player.m_moveTargetType != Player.TargetType.NONE) {
				if(player.m_moveTargetType == Player.TargetType.DIRECTION) {
					// Debug.Log("In Player:Move by Direction.");
					player.m_delta.Normalize();
					player.m_delta = player.m_delta * (Mathf.Sqrt((player.anim.deltaPosition / Time.deltaTime * (100.0f - player.slowAmount) / 100.0f).sqrMagnitude) / 100f);
					player.GetComponent<NavMeshAgent>().Move(player.m_delta);
					/*m_deltaMoveFlag = false;
					player.m_deltaMoveFlag = true;*/
					player.GetComponent<LineRenderer>().enabled = false;
				} else {
					player.transform.LookAt(player.m_navAgent.steeringTarget);
					// Debug.Log("In Player:Move by Target.");
					player.RefreshTargetUnit();

					if (!player.tempPosition.Equals(player.target)) {
						player.tempValue = player.target - player.tempPosition;
						player.totalSqrLength = player.tempValue.sqrMagnitude;

						// If target in attack range.
						// Opt: 0.8 should change as a parameter.
		                if (player.totalSqrLength <= (0.8 * attackRange) * (0.8 * attackRange)) {
		                    if (player.targetName.StartsWith("Player"))
							{
		                        if (PlayerManager.Instance.PlayerLookUp[player.targetName].m_playerState.State != PlayerState.Dead && player.isAutoAttacking) {
									player.SwitchState(PlayerState.Attacking);
		                            return;
								} else {
									player.SwitchState(PlayerState.Idle);
		                            return;
								}
							} else if (player.targetName.StartsWith("JungleCreep")) {
		                        if (MonsterClient.Instance.MonsterLookUp.ContainsKey(player.targetName) && MonsterClient.Instance.MonsterLookUp[player.targetName].curState != JungleMonster.JungleMonsterState.DEAD && player.isAutoAttacking) {
		                            player.SwitchState(PlayerState.Attacking);
		                            return;
		                        } else {
		                            player.SwitchState(PlayerState.Idle);
		                            return;
		                        }
		                    } else if (player.targetName.StartsWith("LaneCreep")) {
		                        if (CreepManager.Instance.LaneCreepLookUp.ContainsKey(player.targetName) && CreepManager.Instance.LaneCreepLookUp[player.targetName].GetCreepState().State != LaneCreep.CreepState.DEAD && player.isAutoAttacking) {
		                            player.SwitchState(PlayerState.Attacking);
		                            return;
		                        } else {
		                            player.SwitchState(PlayerState.Idle);
		                            return;
		                        }
		                    } else if (player.targetName.StartsWith("SparkPoint")) {
		                        if (SparkPointManager.Instance.sparkPointsDict.ContainsKey(player.targetName) && SparkPointManager.Instance.sparkPointsDict[player.targetName].sparkPointState == SparkPoint.SparkPointState.CAPTURED && player.isAutoAttacking) {
		                            player.SwitchState(PlayerState.Attacking);
		                            return;
		                        } else {
		                            player.SwitchState(PlayerState.Idle);
		                            return;
		                        }
                            }
                            else if (player.targetName.StartsWith("Bo")) {
                                if (player.isAutoAttacking) {
                                    player.SwitchState(PlayerState.Attacking);
                                    return;
                                }
                                else {
                                    player.SwitchState(PlayerState.Idle);
                                    return;
                                }
                            }
						}


						if(player.totalSqrLength < 0.4) {
							player.transform.position.Set(player.target.x, player.transform.position.y, player.target.z);
							//m_playerState = PlayerState.Idle;
							player.SwitchState(PlayerState.Idle);
		                    return;
						}
						else {
							//this.transform.position = this.transform.position + tempValue;
							// Draw Path
							// do not move like this!!
							//GetComponent<NavMeshAgent>().Move(tempValue);
							//player.GetComponent<NavMeshAgent>().Resume();

							// *** COMMENTED OUT FOR DEMO 12/4 ***
//							if (player.GetComponent<NavMeshAgent>().hasPath) {
//								player.DrawPath(player.GetComponent<NavMeshAgent>().path);
//								player.GetComponent<LineRenderer>().enabled = true;
//							}
						}
					} else {
						player.SwitchState(PlayerState.Idle);
					}
				}
			} else {
				player.SwitchState(PlayerState.Idle);
			}
		}
		
		public override void OnExit() {
			player.GetComponent<NavMeshAgent>().Stop();
			player.GetComponent<LineRenderer>().enabled = false;
		}
	}
	[Serializable]
	public class PlayerStateCapture : PlayerStateBase {
		public PlayerStateCapture(Player pPlayer) {
			startTime = Time.time;
			m_state = PlayerState.Capturing;
			player = pPlayer;
		}
		
		public override void OnEnter() {
			startTime = Time.time;
			if (player.isAnimated) {
				//player.anim.SetTrigger ("stateIdle");
				player.SetAnimParam((int)Player.AnimatorType.TRIGGER, "stateIdle");
// 				player.photonView.RPC ("RPC_setAnimParam", PhotonTargets.AllBuffered, 
// 			                    (int)LaneCreep.AnimatorType.TRIGGER, "stateIdle", 0f);
			}
		}
		
		public override void OnUpdate() {
			

		}
		
		public override void OnExit() {

		}
	}
	[Serializable]
	public class PlayerStateAttack : PlayerStateBase {

        float lastAttackTime = 0;

		public PlayerStateAttack(Player pPlayer) {
			startTime = Time.time;
			m_state = PlayerState.Attacking;
			player = pPlayer;
		}
		
		public override void OnEnter() {
			Debug.Log("In Player:Enter Attack.");
            if (player.m_action != null)
                player.m_action.Stop();
            lastAttackTime = -10;
            player.m_navAgent.Stop();
            player.m_navAgent.ResetPath();
            if (player.isAnimated) {
                player.anim.SetTrigger("stateIdle");
				player.SetAnimParam((int)Player.AnimatorType.TRIGGER, "stateIdle");
            }
			startTime = Time.time;
		}
		
		public override void OnUpdate() {
            if (player.m_action != null) return;
			float attackRange=0;
			if (player.m_basicAttack.m_attackType == Attack.AttackType.Ranged) {
				//attackRange=m_attackRange;
				attackRange = ((RangedAttack) player.m_basicAttack).m_range;
			}
			if (player.m_basicAttack.m_attackType == Attack.AttackType.Melee) {
				attackRange = player.m_meleeRange;
			}
            //attackRange = 50;
			//if target is alive, attack
            //Debug.Log("Before attack check");
            //Debug.Log(player.isAutoAttacking);
            //Debug.Log(player.targetName.StartsWith("Bo"));
            if (player.isAutoAttacking && (player.targetName.StartsWith("Player") && PlayerManager.Instance.PlayerLookUp[player.targetName].IsPlayerAlive() ||
                player.targetName.StartsWith("JungleCreep") && MonsterClient.Instance.MonsterLookUp.ContainsKey(player.targetName) && MonsterClient.Instance.MonsterLookUp[player.targetName].GetJungleMonsterState().State != JungleMonster.JungleMonsterState.DEAD ||
                player.targetName.StartsWith("LaneCreep") && CreepManager.Instance.LaneCreepLookUp.ContainsKey(player.targetName) && CreepManager.Instance.LaneCreepLookUp[player.targetName].GetCreepState().State != LaneCreep.CreepState.DEAD ||
                player.targetName.StartsWith("SparkPoint") && SparkPointManager.Instance.sparkPointsDict.ContainsKey(player.targetName) && SparkPointManager.Instance.sparkPointsDict[player.targetName].sparkPointState == SparkPoint.SparkPointState.CAPTURED && SparkPointManager.Instance.sparkPointsDict[player.targetName].owner != player.team ||
                player.targetName.StartsWith("Bo"))) {
                Debug.Log("Inner branch attack check");
				//player.RefreshTargetUnit();
				player.tempPosition = player.transform.position;
				player.tempPosition.y = 0;
				//if (!player.tempPosition.Equals(player.target)) {
				player.tempValue = player.target - player.tempPosition;
				player.totalSqrLength = player.tempValue.sqrMagnitude;

                if (player.totalSqrLength > attackRange * attackRange) {//Our target ran away/became distant,we can no longer deal DPS
					//m_playerState = PlayerState.Moving;//Chase the target
					player.SwitchState(PlayerState.Moving);
                    Debug.Log("Chase!");
                    return;
				}
                if (player.targetTransform == null || (player.transform.position - player.targetTransform.position).sqrMagnitude > attackRange * attackRange) {//Our target ran away/became distant,we can no longer deal DPS
                    //m_playerState = PlayerState.Moving;//Chase the target
                    player.SwitchState(PlayerState.Idle);
                    Debug.Log("Idle!");
                    return;
                }
				// Opt: 1.2 should be a parameter.
                else if (Time.time - lastAttackTime > 1.2f) {
                    Debug.Log("Attack; R o M?");
                    if (player.m_basicAttack.m_attackType == Attack.AttackType.Ranged) {
                        Debug.Log("DoRangedBasic; Cool?");
                        if (player.isRangedAttackCool) {
                            lastAttackTime = Time.time;
                            Debug.Log("DoRangedBasic");
                            PlayerManager.Instance.photonView.RPC("RPC_ShootMissile",
                                                                   PhotonTargets.All,
                                                                   player.name,
                                                                   player.targetName
                                                                   );
                            player.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered,
                                (int)LaneCreep.AnimatorType.TRIGGER, "stateAttack", 0f);
                            player.transform.LookAt(player.targetTransform);
                            if (player.isAnimated) {
                                //player.anim.SetTrigger("stateAttack");
								player.SetAnimParam((int)Player.AnimatorType.TRIGGER, "stateAttack");
                            }
                            player.StartCoroutine(player.coolRangedAttack());
                        }
                    }
                    else if (player.m_basicAttack.m_attackType == Attack.AttackType.Melee) {
                        Debug.Log("DoMeleeBasic; Cool?");
                        if (player.isBasicAttackCool) {                       
                            lastAttackTime = Time.time;
                            player.RPC_resetAnimTriggers();
                            Debug.Log("DoMeleeBasic");
                            PlayerManager.Instance.photonView.RPC("RPC_setPlayerAttack",
                                                                   PhotonTargets.All,
                                                                   player.targetName,
                                                                   player.name,
                                                                   (int)CombatManager.AttackIndex.BASIC);
//                             player.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered,
//                                 (int)UnitObject.AnimatorType.TRIGGER, "stateAttack", 0f);
//                             player.transform.LookAt(player.targetTransform);
//                             if (player.isAnimated) {
//                                 //player.anim.SetTrigger("stateAttack");
// 								player.SetAnimParam((int)Player.AnimatorType.TRIGGER, "stateAttack");
//                             }
                            player.StartCoroutine(player.coolBasicAttack());
                        }
                    }
                }
				//}
			}
			else {
				//m_playerState = PlayerState.Idle;
				player.SwitchState(PlayerState.Idle);
                Debug.Log("Idle2!");
                return;
			}

		}
		
		public override void OnExit() {

		}
	}

    [Serializable]
    public class PlayerStateActionAttack : PlayerStateBase {
        private AGE.Action action;

        public PlayerStateActionAttack(Player pPlayer) {
            startTime = Time.time;
            m_state = PlayerState.ActionAttacking;
            player = pPlayer;
            //action = ;
        }

        public override void OnEnter() {
			Debug.Log("In Player:Enter Action Attack.");
            if (player.m_action != null)
                player.m_action.Stop();
            player.m_navAgent.ResetPath();
            startTime = Time.time;
//             if (player.isAnimated) {
//                 player.anim.SetTrigger("stateAttack");
//             }
            player.RPC_resetAnimTriggers();
        }

        public override void OnUpdate() {
            if (player.m_action == null) {
                //m_player.playerState = Player.PlayerState.Idle;
                player.SwitchState(Player.PlayerState.Idle);
                return;
            }
        }

        public override void OnExit() {

        }
    }

	[Serializable]
	public class PlayerStateDead : PlayerStateBase {
		public PlayerStateDead(Player pPlayer) {
			startTime = Time.time;
			m_state = PlayerState.Dead;
			player = pPlayer;
		}
		
		public override void OnEnter() {
			// Debug.Log("In Player:Enter Dead.");
            player.m_navAgent.ResetPath();
			startTime = Time.time;
			Transform lastHit = player.lastAttacker;
            if (lastHit != null && lastHit.tag == "Player") 
			{
				Player attacker = lastHit.GetComponent<Player>();
				int teamXP = attacker.team;	

				Vector3 position = player.transform.position;
				Collider[] objectsAroundMe = Physics.OverlapSphere(position, m_XPradius);
				Collider temp;
				for (int i = 0; i < objectsAroundMe.Length; i++)
				{
					temp = objectsAroundMe[i];
					if (temp.CompareTag("Player"))
					{	//Only give XP to players on the same team as the killing player
						if (temp.GetComponent<Player>().team == teamXP)
							temp.GetComponent<Player>().GetXP(m_XP);
					}
					
				}

			}

			if (player.isAnimated) {
				//player.anim.SetTrigger ("stateDead");
				player.SetAnimParam((int)Player.AnimatorType.TRIGGER, "stateDead");
// 				player.photonView.RPC ("RPC_setAnimParam", PhotonTargets.AllBuffered, 
// 			                    (int)LaneCreep.AnimatorType.TRIGGER, "stateDead", 0f);
			}

			if (player.m_hpUnit) {
				player.m_hpUnit.SetActive(false);
			}

			player.m_isDead = true;

			//If any spark point was being captured make sure it no longer is by this player
			if (player.m_currentlyCapturedSparkPoint != null){
				GameObject.Find("Ground").GetPhotonView().RPC("RPC_setSparkPointCapture",
				                                              PhotonTargets.All,
				                                              player.m_currentlyCapturedSparkPoint.name,
				                                              player.name,
				                                              player.team,
				                                              false,
				                                              0f);

			}

		}
		
		public override void OnUpdate() {


			if (player.remainingRespawnTime <= 0.0f) {
				// Grab all sparkpoints on your team
				Debug.Log ("Player died respawn < 0!");

				player.teamSparkPoints.Clear();

				for (int i = 0; i < player.sparkPoints.Length; i++) {


					if (player.sparkPoints[i].GetOwner() == player.team) {
						player.teamSparkPoints.Add(player.sparkPoints[i]);
					}
				}
                
				// Select random spawn point from list of team spark points
				if (player.teamSparkPoints.Count > 0) {

					UnityEngine.Random.seed = (int)Time.time;

					player.spawnPoint = UnityEngine.Random.Range(0, player.teamSparkPoints.Count-1);
					player.tempPosition = ((SparkPoint)player.teamSparkPoints[player.spawnPoint]).transform.position;
					player.tempPosition.y = 1.2f;
					player.tempPosition.z -= 2.0f;

					player.gameState = GameState.Defeat;
					player.players = PlayerManager.Instance.allPlayers;

                    GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerRespawn",
                                                              PhotonTargets.All,
                                                              player.name,
                                                              player.tempPosition);
                    //m_playerState = PlayerState.Idle;
                    player.SwitchState(PlayerState.Idle);
				}
				else {
					// Lose condition upon reaching 0 sparkpoints. Aaaand you don't have any friends left (alive)
					List<Player> friendlyPlayers = new List<Player>();
					friendlyPlayers = PlayerManager.Instance.GetFriendlyPlayers(player.team);

					bool allDead = true;

					for (int i = 0; i < friendlyPlayers.Count; i++)
						if (friendlyPlayers[i].getPlayerState() != PlayerState.Dead){
							allDead = false;
							break;
						}

					if (allDead)
						player.AnnounceGameEnd();

					
					//player.tempPosition = new Vector3(32.0f, 1.2f, 0.0f); // arbitrary value
				}

				/*player.photonView.RPC("RPC_setPlayerRespawn",
				                      PhotonTargets.All,
				                      player.name,
				                      player.tempPosition);*/

                return;
			}
			else {
				player.remainingRespawnTime -= Time.deltaTime;
			}

		}
		
		public override void OnExit() {
			player.m_isDead = false;
		}
	}
	#endregion

	#region RPC_FUNCS
	[RPC]
	void RPC_syncTransform(Vector3 position, Quaternion rotation) {
		correctUnitPos = position;
		correctUnitRot = rotation;
	}

	[RPC]
	void RPC_syncState(int state) {
		SwitchState((PlayerState)state);
	}
	#endregion

	#region RPC_PAIR_FUNCS
	/// <summary>
	/// Sets the player move target.
	/// </summary>
	/// <param name="targetType">Target type.</param>
	/// <param name="targetOrDelta">Target or delta.</param>
	/// <param name="targetName">Target name.</param>
	/// <param name="isUnit">If set to <c>true</c> is unit.</param>
	public void SetPlayerMoveTarget(TargetType targetType, Vector3 targetOrDelta, string targetName, bool isUnit) {
		//					 RPC_setPlayerMoveTarget
		this.photonView.RPC("RPC_setPlayerMoveTarget",
		                    PhotonTargets.All,
		                    (int)targetType,
		                    targetOrDelta,
		                    targetName,
		                    isUnit);
	}
	[RPC]
	void RPC_setPlayerMoveTarget(int targetType, Vector3 targetOrDelta, string targetName, bool isUnit) {
		m_targetType = (TargetType)targetType;
		m_moveTargetType = (TargetType)targetType;

		// If targetType equal none, back.
		if(m_moveTargetType == TargetType.NONE){return;}

		// Click movement.
		if(m_moveTargetType != TargetType.DIRECTION) {
			// Set to 0, now not consider y-axis.
			targetOrDelta.y = 0f;

			// If unit check.
			if(isUnit) {
				GameObject tempTarget = GameObject.Find (targetName);
				if (tempTarget != null) {
					this.targetUnit = tempTarget.GetComponent<UnitObject>();
					this.targetTransform = tempTarget.transform;
                    UIManager.mgr.OnAttackStatusChanged(true);
                    PlayerManager.Instance.myPlayer.GetComponent<Player>().isAutoAttacking = true;
                    this.targetName = targetName;
				}
			} else {
				//this.targetUnit = null;
				if(m_moveTargetType == TargetType.DESTINATION) {
                    UIManager.mgr.OnAttackStatusChanged(false);
                    PlayerManager.Instance.myPlayer.GetComponent<Player>().isAutoAttacking = false;
				}
			}

			// Always be position.
			this.targetType = PlayerInput.TargetType.Position;
			// Neede for outline check.

			bool targetChanged = false;
			if(m_moveTargetType != TargetType.DIRECTION) {
				targetChanged = !targetOrDelta.Equals(target);
				this.target = targetOrDelta;
			}

			if (targetChanged) {
				GetComponent<NavMeshAgent>().SetDestination(target);
			}

			UIManager.mgr.OnTargetChanged();
		} else {
			m_navAgent.ResetPath();
		}
		
		SwitchState(PlayerState.Moving);
	}

	public void MovePlayerByDelta(Vector3 delta){
		this.transform.rotation = Quaternion.LookRotation(delta);
		m_delta = delta;
		this.photonView.RPC("RPC_setPlayerMoveDelta",
		                    PhotonTargets.OthersBuffered,
		                    delta);
	}
	[RPC]
	void RPC_setPlayerMoveDelta(Vector3 delta) {
		this.transform.rotation = Quaternion.LookRotation(delta);
		m_delta = delta;
	}
	#endregion

	#region USELESS_CODES
	/*void movePlayer () {
        // need local prediction, this is not exactly one but could work, need to add server data auth
//         if (!PhotonNetwork.isMasterClient) {
//             return;
//         }
		// Intopolation from network, temporary used, can delete and rewrite for merging two type of movement.
        //if(!this.GetComponent<PlayerInput>().isMine) {
        //	transform.position = Vector3.Lerp(transform.position, this.correctUnitPos, Time.deltaTime * 5);
        //}


		//Stop NavMeshAgent, if Player should actually move will be enables in the Moving PlayerState
		GetComponent<NavMeshAgent>().Stop();
        GetComponent<NavMeshAgent>().updateRotation = false;
		GetComponent<LineRenderer>().enabled = false;

		//if(name == "Player1")
			//Debug.Log("Current player state: " + playerState);

		float attackRange=0;
		if (m_basicAttack.m_attackType == Attack.AttackType.Ranged) {
			//attackRange=m_attackRange;
			attackRange = ((RangedAttack) m_basicAttack).m_range;
		}
		if (m_basicAttack.m_attackType == Attack.AttackType.Melee) {
			attackRange = m_meleeRange;
		}

		//Debug.Log("player state = " + playerState);

		switch (m_playerState.State) {
		case PlayerState.Idle:
			break;
		case PlayerState.Moving:
			tempPosition = this.transform.position;
			tempPosition.y = 0;
			if (!tempPosition.Equals(target)) {
				tempValue = target - tempPosition;
				totalSqrLength = tempValue.sqrMagnitude;
				tempValue = Vector3.Normalize(tempValue) * speed * (100.0f - slowAmount) / 100.0f * Time.deltaTime;
                realspeed = speed * (100.0f - slowAmount) / 100.0f;

				if (transform.forward != Vector3.Normalize (tempValue)) {
					this.photonView.RPC ("RPC_turnToFace", PhotonTargets.All, target);
				}

				if(totalSqrLength <= attackRange) {
					if (targetName.Contains("Player"))
					{
						if(GameObject.Find(targetName).GetComponent<Player>().m_playerState.State != PlayerState.Dead) {
							//m_playerState.State = PlayerState.Attacking;	
							SwitchState(PlayerState.Attacking);
                            return;
						}
						else {
							//m_playerState.State = PlayerState.Idle;
							SwitchState(PlayerState.Idle);
                            return;
						}
					}
				}

				if (totalSqrLength <= 10.0f && targetName.Contains ("SparkPoint")) {
					
					if (targetName.Contains("SparkPoint"))
					{
						GameObject.Find("Ground").GetPhotonView().RPC("RPC_setSparkPointCapture",
						                                              PhotonTargets.All,
						                                              targetName,
						                                              this.name,
						                                              team,
						                                              true,
                                                                      0.2f);
						//m_playerState.State = PlayerState.Capturing;
						SwitchState(PlayerState.Capturing);
                        return;
					}
//					else if (targetName.Contains("Player"))
//					{
//						playerState = PlayerState.Attacking;
//						
//					}
				}
				else if(tempValue.sqrMagnitude > 0.05 * totalSqrLength) {
					this.transform.position.Set(target.x, this.transform.position.y, target.z);
					//m_playerState.State = PlayerState.Idle;
					SwitchState(PlayerState.Idle);
                    return;
				}
				else {
					//this.transform.position = this.transform.position + tempValue;
                    // Draw Path
                    // do not move like this!!
                    //GetComponent<NavMeshAgent>().Move(tempValue);
                    GetComponent<NavMeshAgent>().Resume();
					//


					//
                    if (GetComponent<NavMeshAgent>().hasPath) {
                        DrawPath(GetComponent<NavMeshAgent>().path);
                        GetComponent<LineRenderer>().enabled = true;
                        GetComponent<NavMeshAgent>().updateRotation = true;


                    }
				}
			}
			break;
		case PlayerState.Capturing:
			break;
		case PlayerState.Attacking:

			//if target is alive, attack
			if(GameObject.Find(targetName).GetComponent<Player>().m_playerState.State != PlayerState.Dead) {

				tempPosition = this.transform.position;
				tempPosition.y = 0;
				if (!tempPosition.Equals(target)) {
					tempValue = target - tempPosition;
					totalSqrLength = tempValue.sqrMagnitude;

					if (totalSqrLength > attackRange) {//Our target ran away/became distant,we can no longer deal DPS
						//m_playerState = PlayerState.Moving;//Chase the target
						SwitchState(PlayerState.Moving);
                        return;
					} 
                    else if (m_basicAttack.m_attackType == Attack.AttackType.Ranged) {
						if(isRangedAttackCool) {
							GameObject.Find("Manager").GetPhotonView().RPC("RPC_ShootMissile",
							                                               PhotonTargets.All,
							                                               this.name,
							                                               targetName
							                                               );
							StartCoroutine(coolRangedAttack());
						}
					} else {
						if (isBasicAttackCool) {
							GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
							                                                 PhotonTargets.All,
							                                                 targetName,
							                                                 name,
							                                                 (int)CombatManager.AttackIndex.BASIC);
							StartCoroutine(coolBasicAttack());
						}
							
					}
				}
			}
			else {
				//m_playerState = PlayerState.Idle;
				SwitchState(PlayerState.Idle);
                return;
			}

			break;
        case PlayerState.Dead:

            if (remainingRespawnTime <= 0.0f) {
                // Grab all sparkpoints on your team
                for (int i = 0; i < sparkPoints.Length; i++) {
                    if (sparkPoints[i].GetOwner() == team) {
                        teamSparkPoints.Add(sparkPoints[i]);
                    }
                }
                // Select random spawn point from list of team spark points
                if (teamSparkPoints.Count > 0) {
                    spawnPoint = UnityEngine.Random.Range(0, teamSparkPoints.Count);
                    tempPosition = ((SparkPoint)teamSparkPoints[spawnPoint]).transform.position;
                    tempPosition.y = 1.2f;
                    tempPosition.z -= 2.0f;
                }
				// Dying with no sparkpoints currently causes a loss - Should change in the future
                else {
					
					// Lose condition code. Moved to Update for no sparkpoint lose condition
					// gameState = GameState.Defeat; 
					//otherPlayers = GameObject.FindGameObjectsWithTag("Player");
					
					//for(int a = 0; a < otherPlayers.Length; a++) {
					//	if(!otherPlayers[a].GetComponent<Player>().playerName.Equals (playerName))	{
					//		if(otherPlayers[a].GetComponent<Player>().team == team)
					//			otherPlayers[a].GetComponent<Player>().SetVictorious(false);
					//		else
					//			otherPlayers[a].GetComponent<Player>().SetVictorious(true);
					//	}
					//} 
					
                    tempPosition = new Vector3(32.0f, 1.2f, 0.0f); // arbitrary value
                }
                
                GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerRespawn",
                                                              PhotonTargets.All,
                                                              this.name,
                                                              tempPosition);
                //m_playerState = PlayerState.Idle;
				SwitchState(PlayerState.Idle);
                return;
            }
            else {
                remainingRespawnTime -= Time.deltaTime;
            }
            break;
		}

	}*/
	#endregion
}

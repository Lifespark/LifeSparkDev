using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

public class BossCage : LSMonoBehaviour {
    //TODO: This feels more like a "BossBehavior" class now, almost. We may want to change its name. -Kaleb

    #region OUTER_INTERFACE
    // Warning, assume only have one boss on stage
    public delegate void TierAction(int tier);
    public static event TierAction OnTierChanged;
    public delegate void PeriodicAction();
    public static event PeriodicAction OnPeriodicEvent;
    #endregion

	public enum BossCageState {
		IDLE,
		CHARGING,
		BREAKING,
		DEFAULT
	}

	// public string[] m_connectionNames;
	public SparkPoint[] m_connections;

	public int[] m_lineCreatedMemo;
	public int[] m_teamCounts = new int[2];

    public GameObject tomb;

	private List<string> initConnected = new List<string>();

	public float m_chargeRate;
	public float m_breakValue;

	public float m_chargedValue;

    public Bosstype.Name m_BossName;
    private Bosstype _BossType;
    public Bosstype m_BossType { get { return _BossType; } }

	public bool shouldDebugCageSize;
	public float bossCageSize = 130.0f;

	#region BOSSCAGE_STATE
	private BossCageStateIdle m_bossCageStateIdle;
	private BossCageStateCharging m_bossCageStateCharging;
	private BossCageStateBreaking m_bossCageStateBreaking;

	private BossCageStateBase m_bossCageState;
	#endregion

    // Time parameters
    private int m_tier;
    public float m_tierInterval = 5; // seconds
    private float m_tierIntervalMS; // miliseconds
    public int m_maxTier = 3;
    private bool m_incTier = false;
    private bool m_doPeriodicEvent = false;
    public float m_periodicActionInterval = 3; // seconds
    private float m_periodicActionIntervalMS; // miliseconds
    private Timer m_tierTimer;
    private Timer m_perTimer;

	private GameObject t_gameObject;
	private SparkPoint t_sparkPoint;
	private float t_chargedValue;

	void Awake() {
		m_bossCageStateIdle = new BossCageStateIdle(this);
		m_bossCageStateCharging = new BossCageStateCharging(this);
		m_bossCageStateBreaking = new BossCageStateBreaking(this);
		SwitchState(BossCageState.IDLE);

        m_tierIntervalMS = m_tierInterval * 1000;
        m_periodicActionIntervalMS = m_periodicActionInterval * 1000;

		/*if(photonView != null && photonView.instantiationData != null) {
			gameObject.name = (string)photonView.instantiationData[0];
			gameObject.tag = (string)photonView.instantiationData[1];
			m_chargeRate = (float)photonView.instantiationData[2];
			m_breakValue = (float)photonView.instantiationData[3];
			List<int> initLineCreatedMemo = new List<int>();
			for(int i = 4; i < photonView.instantiationData.GetLength(0); i++){
				initConnected.Add((string)photonView.instantiationData[i]);
				initLineCreatedMemo.Add(0);
			}
			m_connectionNames = initConnected.ToArray();
			m_lineCreatedMemo = initLineCreatedMemo.ToArray();
			BossCageManager.GetInstance().OnBossCageInstantiated();
		}*/
	}

    public void Start() {
        InitBosstype();
        if (PhotonNetwork.isMasterClient) {
            InitTierSystem();
            OnPeriodicEvent += _BossType.OnPeriodicEvent;
        }
        OnTierChanged += _BossType.OnTierChange;
    }

    /// <summary>
    /// Initialize all values relating to the tier system
    /// </summary>
    public void InitTierSystem() {
        // Set time interval and tier, value from BossCage     
        m_tier = 0;
        TierInterval();
        if (_BossType.m_hasPeriodicEvent) {
            PeriodicInterval();
        }
    }

    /// <summary>
    /// Initialize Bosstype object according to m_BossName
    /// </summary>
    public void InitBosstype() {
        switch (m_BossName) {
            case Bosstype.Name.ZUTSU:
                _BossType = new Zutsu();
                _BossType.Initialize();
                break;
            default:
                break;
        }
    }

	/// <summary>
	/// Initial network data.
	/// </summary>
	public void InitNetworkPassedData() {
	}
	
	// Update is called once per frame
	void Update() {
		if(PhotonNetwork.isMasterClient) {
			m_bossCageState.OnUpdate();

            // for fast debug...
            if (Input.GetKeyUp(KeyCode.P))
                photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossCageState.BREAKING);
		} 
        else {
			m_chargedValue = Mathf.Lerp(m_chargedValue, t_chargedValue, Time.deltaTime * 5);
		}
		// Show debug cage size lines
		if (shouldDebugCageSize) {
			Debug.DrawLine(gameObject.transform.position,gameObject.transform.position+Vector3.right*bossCageSize,Color.red);
			Debug.DrawLine(gameObject.transform.position,gameObject.transform.position+Vector3.left*bossCageSize,Color.red);
			Debug.DrawLine(gameObject.transform.position,gameObject.transform.position+Vector3.forward*bossCageSize,Color.red);
			Debug.DrawLine(gameObject.transform.position,gameObject.transform.position+Vector3.back*bossCageSize,Color.red);
		}

        // Increment tier if the timer has ticked
        if (m_incTier)
        {
            Debug.Log("A");
            m_incTier = false;
            if (m_tier <= m_maxTier)
            {
                Debug.Log("B: " + m_tier);
                m_tier++;
                if (OnTierChanged != null)
                {
                    Debug.Log("C");
                    OnTierChanged(m_tier);
                }
                Debug.Log("D");
            }
        }

        if (m_doPeriodicEvent) {
            m_doPeriodicEvent = false;
            if (PhotonNetwork.isMasterClient && _BossType.m_hasPeriodicEvent && OnPeriodicEvent != null) {
                Debug.Log("OnPeriodicEvent");
                OnPeriodicEvent();
            }
        }
	}

    public void DoPeriodicAction() {
        photonView.RPC("RPC_DoPeriodicAction", PhotonTargets.AllBufferedViaServer);
    }

    [RPC]
    public void RPC_DoPeriodicAction() {
        _BossType.OnPeriodicEvent();
    }
	
	private void SwitchState(BossCageState toState) {
		if(m_bossCageState != null) {
			if (m_bossCageState.State == toState)
				return;
			m_bossCageState.OnExit();
		}
		
		switch(toState) {
			case BossCageState.IDLE:
				m_bossCageState = m_bossCageStateIdle;
				break;
			case BossCageState.CHARGING:
				m_bossCageState = m_bossCageStateCharging;
				break;
			case BossCageState.BREAKING:
				m_bossCageState = m_bossCageStateBreaking;
				shouldDebugCageSize = true;
				break;
		}
		
		m_bossCageState.OnEnter();
	}
	
	/// <summary>
	/// photon syncing
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="info"></param>
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if(stream.isWriting) {
			stream.SendNext(m_chargedValue);
		} else {
			// From 0, no initialize.
			t_chargedValue = (float)stream.ReceiveNext();
		}
	}

	/// <summary>
	/// Check sparkpoint state every frame.
	/// Set line and delete line if need.
	/// <returns>Bool for charge or not.</returns>
	/// </summary>
	private bool checkSparkPointState() {
		/*for(int i = 0; i < m_connectionNames.GetLength(0); i++) {
			t_gameObject = GameObject.Find("SparkPoints/"+m_connectionNames[i]);
			t_sparkPoint = t_gameObject.GetComponent<SparkPoint>();
			if(t_sparkPoint.GetOwner() > 0){
				if(m_lineCreatedMemo[i] == 0){
					if(t_sparkPoint.GetOwner() == 1){
						m_teamOneCount = m_teamOneCount + 1;
						m_lineCreatedMemo[i] = 1;
					} else if(t_sparkPoint.GetOwner() == 2){
						m_lineCreatedMemo[i] = 2;
						m_teamTwoCount = m_teamTwoCount + 1;
					}



					// build lane
					GameObject tempObject = PhotonNetwork.InstantiateSceneObject("Lane", new Vector3(), new Quaternion(), 0, null);
					Lane tempLane = tempObject.GetComponent<Lane>();
					// set lane name
					tempObject.name = "Lane" + "BossCage" + m_connectionNames[i];
					tempObject.transform.parent = transform.parent;
					// set lane material
					tempLane.photonView.RPC("RPC_setLaneMaterial", PhotonTargets.All, 3);
					// set line position, location and scale
					tempLane.photonView.RPC("RPC_setInitialTransform", PhotonTargets.All, transform.position, t_gameObject.transform.position);
				}
			} else {
				if(m_lineCreatedMemo[i] > 0){
					if(m_lineCreatedMemo[i] == 1){
						m_teamOneCount = m_teamOneCount - 1;
					} else if(m_lineCreatedMemo[i] == 2){
						m_teamTwoCount = m_teamTwoCount - 1;
					}
					m_lineCreatedMemo[i] = 0;


					t_gameObject = transform.parent.gameObject;
					Lane tempLane = GameObject.Find(t_gameObject.name + "/LaneBossCage" + m_connectionNames[i]).GetComponent<Lane>();
					PhotonNetwork.Destroy(tempLane.photonView);
				}
			}
		}*/
		//for(int i = 0; i < m_connectionNames.GetLength(0); i++) {
		//	t_gameObject = GameObject.Find("SparkPoints/"+m_connectionNames[i]);
		//	t_sparkPoint = t_gameObject.GetComponent<SparkPoint>();
		for(int i = 0; i < m_connections.GetLength(0); i++) {
			t_sparkPoint = m_connections[i];
			if(t_sparkPoint.GetOwner() > 0){
				if(m_lineCreatedMemo[i] == 0){
                    int t_sharedConnections = 0;
                    foreach(SparkPoint bossRegionPoint in t_sparkPoint.m_bossAreaConnections) {
					    if(t_sparkPoint.GetOwner() == bossRegionPoint.GetOwner()){
                            t_sharedConnections += 1;
                        }
                    }
                    if (t_sharedConnections == t_sparkPoint.m_bossAreaConnections.Length) {
						m_teamCounts[t_sparkPoint.GetOwner()-1] = m_teamCounts[t_sparkPoint.GetOwner()-1] + t_sparkPoint.GetOwner();
                        m_lineCreatedMemo[i] = t_sparkPoint.GetOwner();


                        // build lane
//                         GameObject tempObject = PhotonNetwork.InstantiateSceneObject("Lane", new Vector3(), new Quaternion(), 0, null);
//                         Lane tempLane = tempObject.GetComponent<Lane>();
//                         tempObject.name = "Lane" + "BossCage" + m_connections[i].name;
//                         tempLane.photonView.RPC("RPC_setLaneMaterial", PhotonTargets.All, 3);
//                         tempLane.photonView.RPC("RPC_setInitialTransform", PhotonTargets.All, transform.position, t_sparkPoint.transform.position);
                        object[] initData = { t_sparkPoint.owner };
                        t_sparkPoint.m_bossCageConnectionLane = PhotonNetwork.InstantiateSceneObject("SparkPointLink", t_sparkPoint.m_sparkPointTip.transform.position, Quaternion.identity, 0, initData);
                        //tempLine.AddComponent<LineRenderer>();
                        //tempLine.GetComponent<LineRenderer>().material = m_sparkPointConnectionMaterial;
                        //tempLine.AddComponent<SparkPointConnection>();
                        t_sparkPoint.m_bossCageConnectionLane.transform.parent = t_sparkPoint.transform;
                        //tempLine.GetComponent<SparkPointConnection>().startConnection(m_sparkPointTip.transform, 
                        //_connections[i].m_sparkPointTip.transform);
                        t_sparkPoint.m_bossCageConnectionLane.GetPhotonView().RPC("RPC_startSparkPointConnection", PhotonTargets.All, t_sparkPoint.m_sparkPointTip.transform.position, t_sparkPoint.m_bossCageConnectionPoint.position);
								
                    }
				}
			} 
            else {
				if(m_lineCreatedMemo[i] > 0){
                    m_teamCounts[t_sparkPoint.GetOwner()-1] = m_teamCounts[t_sparkPoint.GetOwner()-1] - 1;
					m_lineCreatedMemo[i] = 0;
					
					
					t_gameObject = transform.parent.gameObject;
					//Lane tempLane = GameObject.Find(t_gameObject.name + "/LaneBossCage" + m_connectionNames[i]).GetComponent<Lane>();
					Lane tempLane = GameObject.Find(t_gameObject.name + "/LaneBossCage" + m_connections[i].name).GetComponent<Lane>();
					if(tempLane != null){
						PhotonNetwork.Destroy(tempLane.photonView);
					}
				}
			}
		}
		if((m_teamCounts[0] > 0) && (m_teamCounts[1] > 0)) {
			return true;
		}
        else {
			return false;
		}
	}

	#region BOSSCAGE_STATE_CLASSES
	/// <summary>
	/// Base state class. Can only be inherited
	/// </summary>
	abstract class BossCageStateBase {
		// state name
		protected BossCageState m_state;
		// lane creep instance
		protected BossCage m_bossCage;
		// when do we start this state?
		protected float m_startTime;
		
		public BossCageState State { get { return m_state; } }

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
		
		public BossCageStateBase() { m_startTime = Time.time; }
	}
	
	class BossCageStateIdle : BossCageStateBase {
		public BossCageStateIdle(BossCage bossCage) {
			m_startTime = Time.time;
			m_state = BossCageState.IDLE;
			m_bossCage = bossCage;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
		}
		
		public override void OnUpdate() {
			if(m_bossCage.checkSparkPointState()){
				m_bossCage.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossCageState.CHARGING);
			}
		}
		
		public override void OnExit() {
			
		}


	}
	
	class BossCageStateCharging : BossCageStateBase {
		public BossCageStateCharging(BossCage bossCage) {
			m_startTime = Time.time;
			m_state = BossCageState.CHARGING;
			m_bossCage = bossCage;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Now charging.");
		}
		
		public override void OnUpdate() {
			if(m_bossCage.checkSparkPointState()) {
				if(m_bossCage.m_chargedValue > m_bossCage.m_breakValue) {
					m_bossCage.m_chargedValue = m_bossCage.m_breakValue;
					m_bossCage.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossCageState.BREAKING);
				} else {
					m_bossCage.m_chargedValue = m_bossCage.m_chargedValue + (m_bossCage.m_chargeRate * Time.deltaTime);
				}
			} else {
				if(m_bossCage.m_chargedValue > 0) {
					m_bossCage.m_chargedValue = m_bossCage.m_chargedValue - (m_bossCage.m_chargeRate * Time.deltaTime);
				} else {
					m_bossCage.m_chargedValue = 0;
					m_bossCage.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossCageState.IDLE);
				}
			}
		}
		
		public override void OnExit() {
			if(m_bossCage.m_chargedValue >= m_bossCage.m_breakValue) {
				// Break, Remove capsule collider here.
				m_bossCage.gameObject.GetComponent<CapsuleCollider>().enabled = !m_bossCage.gameObject.GetComponent<CapsuleCollider>().enabled;
			}
		}
	}

	class BossCageStateBreaking : BossCageStateBase {
        bool hasBoss = false;
		public BossCageStateBreaking(BossCage bossCage) {
			m_startTime = Time.time;
			m_state = BossCageState.BREAKING;
			m_bossCage = bossCage;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			// Debug.Log("Now breaking.");
			// Create boss here.
            m_bossCage.tomb.GetComponent<TombSink>().Break();

            if (PhotonNetwork.isMasterClient) {
                for (int i = 0; i < m_bossCage.m_lineCreatedMemo.GetLength(0); i++) {
                    if (m_bossCage.m_lineCreatedMemo[i] > 0) {
                        m_bossCage.m_lineCreatedMemo[i] = 0;
                        //GameObject temp = GameObject.Find ("Terrain/LaneBossCage" + m_bossCage.m_connectionNames [i]);
                        //GameObject temp = GameObject.Find("LaneBossCage" + m_bossCage.m_connections[i].name);
                        //PhotonNetwork.Destroy(temp.GetPhotonView());
                        //DestroyImmediate (temp);
                        Debug.Log("Destroy connection");
                        PhotonNetwork.Destroy(m_bossCage.m_connections[i].m_bossCageConnectionLane);
                    }
                }
            }
		}
		
		public override void OnUpdate() {
            if (!hasBoss && Time.time - m_startTime > 8) {
                if (PhotonNetwork.isMasterClient) {
                    hasBoss = true;
                    // Need change to general way.
                    Vector3 tempVec = new Vector3(0f, -1.9f, 0f);
                    tempVec = tempVec + m_bossCage.transform.position;
                    //Initial Data for Boss provided by Scene
                    //Grab Boss specific Data

                    List<object> initData = new List<object> { m_bossCage.m_BossName.ToString() };
                    PhotonNetwork.InstantiateSceneObject("RealZutsu",
                                                         new Vector3(-28.7f, 0, -25f),
                                                         m_bossCage.transform.rotation,
                                                         0,
                                                         initData.ToArray());
                }
            }
		}
		
		public override void OnExit() {
			
		}
	}

	#endregion

	#region BOSSCAGE_RPC
	[RPC]
	void RPC_switchState(int bossCageState){
		SwitchState((BossCageState)bossCageState);
	}

	#endregion
    #region TIME_TIER_FUNCS
    /// <summary>Initializes the timer that periodically increases the boss tier </summary>
    private void TierInterval()
    {
        Debug.Log("TierInterval");
        m_tierTimer = new Timer(m_tierIntervalMS);
        m_tierTimer.AutoReset = true;
        m_tierTimer.Elapsed += OnTimedEvent;
        m_tierTimer.Enabled = true;
    }

    /// <summary>Handler for the Timer Event </summary>
    /// <param name="source">unused</param>
    /// <param name="e">unused</param>
    private void OnTimedEvent(System.Object source, ElapsedEventArgs e)
    {
        Debug.Log("OnTimedEvent");
        m_incTier = true;
    }

    /// <summary>
    /// Initializes the timer that is used for periodical boss behavior (within tiers)
    /// </summary>
    private void PeriodicInterval() {
        Debug.Log("PeriodicInterval");
        m_perTimer = new Timer(m_periodicActionIntervalMS);
        m_perTimer.AutoReset = true;
        m_perTimer.Elapsed += QueuePeriodicEvent;
        m_perTimer.Enabled = true;
    }

    private void QueuePeriodicEvent(System.Object source, ElapsedEventArgs e)
    {
        Debug.Log("QueuePeriodicEvent");
        m_doPeriodicEvent = true;
    }
    #endregion 
}

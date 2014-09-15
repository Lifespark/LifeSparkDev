using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossCage : LSMonoBehaviour {

	public enum BossCageState {
		IDLE,
		CHARGING,
		BREAKING,
		DEFAULT
	}

	public string[] m_connectionNames;
	public SparkPoint[] m_connections;
	public bool[] m_lineCreatedMemo;
	private List<string> initConnected = new List<string>();

	public float m_chargeRate;
	public float m_breakValue;

	public float m_chargedValue;
	
	public bool m_teamOneCharged;
	public bool m_teamTwoCharged;

	#region BOSSCAGE_STATE
	private BossCageStateIdle m_bossCageStateIdle;
	private BossCageStateCharging m_bossCageStateCharging;
	private BossCageStateBreaking m_bossCageStateBreaking;

	private BossCageStateBase m_bossCageState;
	#endregion
	
	public GameObject t_gameObject;
	public SparkPoint t_sparkPoint;

	void Awake() {
		m_bossCageStateIdle = new BossCageStateIdle(this);
		m_bossCageStateCharging = new BossCageStateCharging(this);
		m_bossCageStateBreaking = new BossCageStateBreaking(this);
		SwitchState(BossCageState.IDLE);
		if(photonView != null && photonView.instantiationData != null) {
			gameObject.name = (string)photonView.instantiationData[0];
			gameObject.tag = (string)photonView.instantiationData[1];
			m_chargeRate = (float)photonView.instantiationData[2];
			m_breakValue = (float)photonView.instantiationData[3];
			List<bool> initLineCreatedMemo = new List<bool>();
			for(int i = 4; i < photonView.instantiationData.GetLength(0); i++){
				initConnected.Add((string)photonView.instantiationData[i]);
				initLineCreatedMemo.Add(false);
			}
			m_connectionNames = initConnected.ToArray();
			m_lineCreatedMemo = initLineCreatedMemo.ToArray();
			BossCageManager.GetInstance().OnBossCageInstantiated();
		}
	}

	/// <summary>
	/// Initial network data.
	/// </summary>
	public void InitNetworkPassedData() {
	}
	
	// Update is called once per frame
	void Update() {
		m_bossCageState.OnUpdate();
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
			checkSparkPointState();
		}
		
		public override void OnExit() {
			
		}

		/// <summary>
		/// Check sparkpoint state every frame.
		/// Set line and switch to charging is requirement is satisfied.
		/// OPT need.
		/// </summary>
		private void checkSparkPointState() {
			if (PhotonNetwork.isMasterClient) {
				for(int i = 0; i < m_bossCage.m_connectionNames.GetLength(0); i++) {
					m_bossCage.t_gameObject = GameObject.Find("SparkPoints/"+m_bossCage.m_connectionNames[i]);
					m_bossCage.t_sparkPoint = m_bossCage.t_gameObject.GetComponent<SparkPoint>();
					if(m_bossCage.t_sparkPoint.GetOwner() > 0){
						if(!m_bossCage.m_lineCreatedMemo[i]){
							m_bossCage.m_lineCreatedMemo[i] = true;

							GameObject tempObject = PhotonNetwork.InstantiateSceneObject("Lane", new Vector3(), new Quaternion(), 0, null);
							Lane tempLane = tempObject.GetComponent<Lane>();
							//// set lane name
							tempObject.name = "Lane" + "BossCage" + m_bossCage.m_connectionNames[i];
							tempObject.transform.parent = m_bossCage.transform.parent;
							//// set lane material
							tempLane.photonView.RPC("RPC_setLaneMaterial", PhotonTargets.All, 3);
							//// set line position, location and scale
							tempLane.photonView.RPC("RPC_setInitialTransform", PhotonTargets.All, m_bossCage.transform.position, m_bossCage.t_gameObject.transform.position);
						}
						if(m_bossCage.t_sparkPoint.GetOwner() == 1){
							m_bossCage.m_teamOneCharged = true;
						} else if(m_bossCage.t_sparkPoint.GetOwner() == 2){
							m_bossCage.m_teamTwoCharged = true;
						} else {
						}
					}
				}
				if(m_bossCage.m_teamOneCharged && m_bossCage.m_teamTwoCharged) {
					m_bossCage.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossCageState.CHARGING);
				}
			}
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
			if(m_bossCage.m_chargedValue > m_bossCage.m_breakValue) {
				m_bossCage.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossCageState.BREAKING);
			} else {
				m_bossCage.m_chargedValue = m_bossCage.m_chargedValue + (m_bossCage.m_chargeRate * Time.deltaTime);
			}
		}
		
		public override void OnExit() {
			// Remove capsule collider here.
			m_bossCage.gameObject.GetComponent<CapsuleCollider>().enabled = !m_bossCage.gameObject.GetComponent<CapsuleCollider>().enabled;
		}
	}

	class BossCageStateBreaking : BossCageStateBase {
		public BossCageStateBreaking(BossCage bossCage) {
			m_startTime = Time.time;
			m_state = BossCageState.BREAKING;
			m_bossCage = bossCage;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Now breaking.");
			// Create boss here.
			if(PhotonNetwork.isMasterClient) {
				// Need change to general way.
				Vector3 tempVec = new Vector3(0f, -1.9f, 0f);
				tempVec = tempVec + m_bossCage.transform.position;
				PhotonNetwork.InstantiateSceneObject("Boss", 
				                                     tempVec,
				                                     m_bossCage.transform.rotation,
				                                     0,
				                                     null);
			}
			// Just for test, disable this gameobject.
			m_bossCage.gameObject.GetComponent<MeshRenderer>().enabled = !m_bossCage.gameObject.GetComponent<MeshRenderer>().enabled;
			// Deleted all lines conncted to bossCage.
			if (PhotonNetwork.isMasterClient) {
				for (int i = 0; i < m_bossCage.m_lineCreatedMemo.GetLength(0); i++) {
					if (m_bossCage.m_lineCreatedMemo [i]) {
						m_bossCage.m_lineCreatedMemo [i] = false;
						GameObject temp = GameObject.Find ("Terrain/LaneBossCage" + m_bossCage.m_connectionNames [i]);
						PhotonNetwork.Destroy(temp.GetPhotonView());
						//DestroyImmediate (temp);
					}
				}
			}
		}
		
		public override void OnUpdate() {

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
}

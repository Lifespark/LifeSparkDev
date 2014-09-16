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
	public int[] m_lineCreatedMemo;
	//public bool m_teamOneCharged;
	//public bool m_teamTwoCharged;
	public int m_teamOneCount;
	public int m_teamTwoCount;



	private List<string> initConnected = new List<string>();

	public float m_chargeRate;
	public float m_breakValue;

	public float m_chargedValue;
	




	#region BOSSCAGE_STATE
	private BossCageStateIdle m_bossCageStateIdle;
	private BossCageStateCharging m_bossCageStateCharging;
	private BossCageStateBreaking m_bossCageStateBreaking;

	private BossCageStateBase m_bossCageState;
	#endregion
	
	public GameObject t_gameObject;
	public SparkPoint t_sparkPoint;
	private float t_chargedValue;

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
			List<int> initLineCreatedMemo = new List<int>();
			for(int i = 4; i < photonView.instantiationData.GetLength(0); i++){
				initConnected.Add((string)photonView.instantiationData[i]);
				initLineCreatedMemo.Add(0);
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
		if(PhotonNetwork.isMasterClient) {
			m_bossCageState.OnUpdate();
		} else {
			m_chargedValue = Mathf.Lerp(m_chargedValue, t_chargedValue, Time.deltaTime * 5);
		}
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
		for(int i = 0; i < m_connectionNames.GetLength(0); i++) {
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
		}
		if((m_teamOneCount > 0) && (m_teamTwoCount > 0)) {
			return true;
		} else {
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
					if (m_bossCage.m_lineCreatedMemo[i] > 0) {
						m_bossCage.m_lineCreatedMemo [i] = 0;
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

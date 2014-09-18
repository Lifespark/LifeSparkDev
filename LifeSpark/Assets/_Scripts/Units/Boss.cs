using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Boss : UnitObject {
	
	public enum BossState {
		IDLE,
		MOVING,
		CHASING,
		ATTACKING,
		ATTACKED,
		RETURNING,
		DEAD,
		DEFAULT
	}

	#region BOSS_STATE
	private BossStateIdle m_bossStateIdle;
	private BossStateMove m_bossStateMove;
	private BossStateChase m_bossStateChase;
	private BossStateAttack m_bossStateAttack;
	private BossStateReturn m_bossStateReturn;

	private BossStateBase m_bossState;
	#endregion

	public Vector3 m_bossIdlePosition;
	public float m_IdleSearchRadius;
	public float m_moveRange;


	private float m_speed;
	public float m_chaseSpeed;
	public float m_returnSpeed;
	public Transform m_target;
	private Vector3 m_correctBossPos;
	private Quaternion m_correctBossRot;
	
	private bool m_appliedInitialUpdate = false;


	private float t_float;
	private Transform t_transform;
	private Vector3 t_vector3;
	
	// Use this for initialization
	void Awake() {
		m_bossStateIdle = new BossStateIdle(this);
		m_bossStateMove = new BossStateMove(this);
		m_bossStateChase = new BossStateChase(this);
		m_bossStateAttack = new BossStateAttack(this);
		m_bossStateReturn = new BossStateReturn(this);
		SwitchState(BossState.IDLE);

		m_bossIdlePosition = transform.position;
	}
	
	// Update is called once per frame
	void Update() {
		// if I'm not in control, sync position from network
		if (!PhotonNetwork.isMasterClient) {
			transform.position = Vector3.Lerp(transform.position, m_correctBossPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, m_correctBossRot, Time.deltaTime * 5);
		}
		// otherwise process current state's update
		else {
			if (m_bossState != null)
				m_bossState.OnUpdate();
		}
	}
	
	private void SwitchState(BossState toState) {
		if (m_bossState != null) {
			if (m_bossState.State == toState)
				return;
			m_bossState.OnExit();
		}
		
		switch (toState) {
		case BossState.IDLE:
			m_bossState = m_bossStateIdle;
			break;
		case BossState.MOVING:
			m_bossState = m_bossStateMove;
			break;
		case BossState.CHASING:
			m_bossState = m_bossStateChase;
			break;
		case BossState.ATTACKING:
			m_bossState = m_bossStateAttack;
			break;
		case BossState.RETURNING:
			m_bossState = m_bossStateReturn;
			break;
		}
		
		m_bossState.OnEnter();
	}

	public bool InMoveRange(){
		if(Vector3.SqrMagnitude(transform.position - m_bossIdlePosition) > (m_moveRange * m_moveRange)){
			return false;
		} else {
			return true;
		}
	}

	public bool SearchAround(){
		t_float = m_IdleSearchRadius * m_IdleSearchRadius;
		for(int i = 1; i <= 4; i++) {
			t_transform = GameObject.Find("Players/Player" + i).transform;
			t_vector3 = t_transform.position - transform.position;
			t_vector3.y = 0;
			if(Vector3.SqrMagnitude(t_vector3) < t_float){
				t_float = Vector3.SqrMagnitude(t_vector3);
				m_target = t_transform;
			}
		}
		if(t_float < (m_IdleSearchRadius * m_IdleSearchRadius)) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// photon syncing
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="info"></param>
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			//stream.SendNext(rigidbody.velocity);
		}
		else {
			m_correctBossPos = (Vector3)stream.ReceiveNext();
			m_correctBossRot = (Quaternion)stream.ReceiveNext();
			//rigidbody.velocity = (Vector3)stream.ReceiveNext();
			
			if (!m_appliedInitialUpdate) {
				m_appliedInitialUpdate = true;
				transform.position = m_correctBossPos;
				transform.rotation = m_correctBossRot;
				rigidbody.velocity = Vector3.zero;
			}
		}
	}

	#region BOSS_STATE_CLASSES
	/// <summary>
	/// Base state class. Can only be inherited
	/// </summary>
	[Serializable]
	abstract class BossStateBase {
		// state name
		protected BossState m_state;
		// lane creep instance
		protected Boss m_boss;
		// when do we start this state?
		protected float m_startTime;
		
		public BossState State { get { return m_state; } }

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
		
		public BossStateBase() { m_startTime = Time.time; }
	}

	[Serializable]
	class BossStateIdle : BossStateBase {
		public BossStateIdle(Boss boss) {
			m_startTime = Time.time;
			m_state = BossState.IDLE;
			m_boss = boss;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Idle.");
		}
		
		public override void OnUpdate() {
			if(m_boss.SearchAround()) {
				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.CHASING);
			}
		}
		
		public override void OnExit() {
			
		}
	}

	[Serializable]
	class BossStateMove : BossStateBase {
		public BossStateMove(Boss boss) {
			m_startTime = Time.time;
			m_state = BossState.MOVING;
			m_boss = boss;

			// Now just for temp.
			// OPT need.
			m_boss.m_target = GameObject.Find("Players/Player1").transform;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Move.");
		}
		
		public override void OnUpdate() {

		}
		
		public override void OnExit() {
			
		}
	}

	[Serializable]
	class BossStateChase : BossStateBase {
		public BossStateChase(Boss boss) {
			m_startTime = Time.time;
			m_state = BossState.CHASING;
			m_boss = boss;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Chase.");
			m_boss.m_speed = m_boss.m_chaseSpeed;
		}
		
		public override void OnUpdate() {
			if(m_boss.InMoveRange()){
				if(m_boss.SearchAround()){
					if (Vector3.SqrMagnitude(m_boss.m_target.position - m_boss.transform.position) > 6.0) {
						Vector3 targetPos = m_boss.m_target.position;
						Vector3 direction = (targetPos - m_boss.transform.position).normalized;
						direction.y = 0;
						
						m_boss.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
						m_boss.transform.position += direction * m_boss.m_speed * Time.deltaTime;
					} else {
						m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.ATTACKING);
					}
				}
			} else {
				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.RETURNING);
			}
		}
		
		public override void OnExit() {
			
		}
	}

	[Serializable]
	class BossStateAttack : BossStateBase {
		public BossStateAttack(Boss boss) {
			m_startTime = Time.time;
			m_state = BossState.ATTACKING;
			m_boss = boss;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Attack.");
		}
		
		public override void OnUpdate() {
			// do attack

			//
			if(Time.time - m_startTime > 1){
				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.CHASING);
			}
		}
		
		public override void OnExit() {

		}
	}

	[Serializable]
	class BossStateReturn : BossStateBase {
		public BossStateReturn(Boss boss) {
			m_startTime = Time.time;
			m_state = BossState.RETURNING;
			m_boss = boss;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Return.");
			m_boss.m_speed = m_boss.m_returnSpeed;
		}
		
		public override void OnUpdate() {
			if (Vector3.SqrMagnitude(m_boss.m_bossIdlePosition - m_boss.transform.position) > 4.0) {
				Vector3 targetPos = m_boss.m_bossIdlePosition;
				Vector3 direction = (targetPos - m_boss.transform.position).normalized;
				direction.y = 0;
				
				m_boss.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
				m_boss.transform.position += direction * m_boss.m_speed * Time.deltaTime;
			} else {
				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.IDLE);
			}
		}
		
		public override void OnExit() {
			
		}
	}

	#endregion

	#region BOSS_RPC
	[RPC]
	void RPC_switchState(int bossState){
		SwitchState((BossState)bossState);
	}

	#endregion
}
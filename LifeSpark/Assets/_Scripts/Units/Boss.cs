using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Boss : UnitObject {
	
	public enum BossState {
		IDLE,
		MOVING,
		ATTACKING,
		ATTACKED,
		DEAD,
		DEFAULT
	}

	#region BOSS_STATE
	private BossStateIdle m_bossStateIdle;
	private BossStateMove m_bossStateMove;

	private BossStateBase m_bossState;
	#endregion

	public float maxSpeed = 5.0f;
	public Transform m_target;
	private Vector3 m_correctBossPos;
	private Quaternion m_correctBossRot;
	
	private bool m_appliedInitialUpdate = false;
	
	// Use this for initialization
	void Awake() {
		m_bossStateIdle = new BossStateIdle(this);
		m_bossStateMove = new BossStateMove(this);
		SwitchState(BossState.IDLE);
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
		}
		
		m_bossState.OnEnter();
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
		}
		
		public override void OnUpdate() {
			// Now just for temp.
			if(PhotonNetwork.isMasterClient) {
				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.MOVING);
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
			Debug.Log("Boss:Moving.");
		}
		
		public override void OnUpdate() {
			// target not reached, continue approaching
			if (Vector3.SqrMagnitude(m_boss.m_target.position - m_boss.transform.position) > 2.0) {
				Vector3 targetPos = m_boss.m_target.position;
				Vector3 direction = (targetPos - m_boss.transform.position).normalized;
				direction.y = 0;
				
				m_boss.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
				m_boss.transform.position += direction * m_boss.maxSpeed * Time.deltaTime;
			}
			else {
				// Re get target.
				// OPT need.
				m_boss.m_target = GameObject.Find("Players/Player1").transform;
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
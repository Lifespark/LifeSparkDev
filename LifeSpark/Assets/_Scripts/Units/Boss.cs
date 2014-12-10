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
	private BossStateDead m_bossStateDead;


	private BossStateBase m_bossState;
	#endregion
	
	public Vector3 m_bossIdlePosition;		
	public float m_IdleSearchRadius;		
	public float m_moveRange;
	public float m_XP = 200;
	public float m_XPradius = 35;

	// Boss move parameters
	private float m_speed;
	public float m_chaseSpeed;
	public float m_returnSpeed;
	//public Transform m_target;
	public Transform m_target;				// Now this for both move and attack target.
	public UnitObject m_targetUnit = null;	// Now this for both move and attack target.
	private Vector3 m_correctBossPos;
	private Quaternion m_correctBossRot;

	// Boss attack parameters
	public float m_basicAttackRange = 10f;

	private GameObject m_boss;
	private string m_bossName;
	private Bosstype m_bossType;
	private Animator m_anim;
	public Hit m_basicAttackHit;
	public AGE.Action m_action;
	private AGE.ActionHelper m_actionHelper;

	private bool m_appliedInitialUpdate = false;

	#region TEMP_PARAMS
	private float t_float;
	private Transform t_transform;
	private Vector3 t_vector3;
	#endregion
	#region BASIC_FUNCS

	/// <summary>
	/// Creates the appropriate boss type.
	/// </summary>
	/// <param name="boss">The name of the boss.</param>
	private void selectBoss(string boss)
	{

		m_boss = new GameObject();
		if (boss == "Zutsu") {
			m_boss = GameObject.Find("Zutsu");
			m_bossType = m_boss.GetComponent<Zutsu>();
		}
		//Additional bosses
	}

	public override void Awake() {
		base.Awake();
		m_anim = GetComponent<Animator>();
		m_actionHelper = GetComponent<AGE.ActionHelper>();
		gameObject.name = "Boss";
		m_bossStateIdle = new BossStateIdle(this);
		m_bossStateMove = new BossStateMove(this);
		m_bossStateChase = new BossStateChase(this);
		m_bossStateAttack = new BossStateAttack(this);
		m_bossStateReturn = new BossStateReturn(this);
		m_bossStateDead = new BossStateDead(this);
		SwitchState(BossState.IDLE);


		m_bossIdlePosition = transform.position;
		m_bossIdlePosition.y = 0;
		// Set time interval and tier, value from BossCage
		if(photonView != null && photonView.instantiationData != null) {
			m_bossName = ((string)photonView.instantiationData[0]);
			selectBoss(m_bossName);
		}

		//m_Zutsu.Tier1_Behavior ();
		GameObject hpBar = UIManager.mgr.getHpBar(this, new Vector3(0, 1f, 0));
	   // hpBar.transform.localScale /= (transform.localScale.x / 2);
	}
	
	// Update is called once per frame
	new void Update() {
		// For position sync
		base.Update();

		// Master client update state only
		if (PhotonNetwork.isMasterClient) {
			if (m_bossState != null) {

				if (m_bossState.GetState() != BossState.DEAD && unitHealth <= 0) {
					SwitchState(BossState.DEAD);
					return;
				}

				m_bossState.OnUpdate();
			}
		}
	}

	void ResetAnimTrigger() {
		m_anim.ResetTrigger("goWalk");
		m_anim.ResetTrigger("goDead");
		m_anim.ResetTrigger("goIdle");
		m_anim.ResetTrigger("goAttack");
		m_anim.ResetTrigger("goSpell");
	}

	[RPC]
	void RPC_setAnimParam(int animType, string param, float value = 0) {
		//Debug.Log(param + "\t" + Time.time);
		switch ((AnimatorType)animType) {
			case AnimatorType.BOOL:
				m_anim.SetBool(param, value == 1);
				break;
			case AnimatorType.INT:
				m_anim.SetInteger(param, (int)value);
				break;
			case AnimatorType.FLOAT:
				m_anim.SetFloat(param, value);
				break;
			case AnimatorType.TRIGGER:
				// reset all other trigger
				ResetAnimTrigger();
				m_anim.SetTrigger(param);
				break;
		}
	}
	#endregion
	#region BOSS_BEHAVIOR_FUNCS
	public bool InMoveRange(){
		if(Vector3.SqrMagnitude(transform.position - m_bossIdlePosition) > (m_moveRange * m_moveRange)){
			return false;
		} else {
			return true;
		}
	}

	public bool SearchAround(){
		t_float = m_IdleSearchRadius * m_IdleSearchRadius;
		for(int i = 0; i < CreepManager.Instance.creepList.Count; i++) {
            //TODO: This should probably be updated to use Physics.OverlapSphere instead of looping through all creeps.
			if (CreepManager.Instance.creepList[i]) {
                t_transform = CreepManager.Instance.creepList[i].transform;
            }
			t_vector3 = t_transform.position - transform.position;
			t_vector3.y = 0;
			if(Vector3.SqrMagnitude(t_vector3) < t_float){
				t_float = Vector3.SqrMagnitude(t_vector3);
				m_target = t_transform;
				m_targetUnit = t_transform.gameObject.GetComponent<LaneCreep>();
			}
		}
		for(int i = 0; i < PlayerManager.Instance.allPlayers.Count; i++) {
			t_transform = PlayerManager.Instance.allPlayers[i].transform;
			t_vector3 = t_transform.position - transform.position;
			t_vector3.y = 0;
            if (Vector3.SqrMagnitude(t_vector3) < t_float && t_transform.gameObject.GetComponent<Player>().GetState() != Player.PlayerState.Dead) {
				t_float = Vector3.SqrMagnitude(t_vector3);
				m_target = t_transform;
				m_targetUnit = t_transform.gameObject.GetComponent<Player>();
			}
		}
		if(t_float < (m_IdleSearchRadius * m_IdleSearchRadius)) {
			return true;
		} else {
			return false;
		}
	}



	#endregion
	#region BOSS_STATE_CLASSES
	/// <summary>
	/// Switchs the state.
	/// </summary>
	/// <param name="toState">To state.</param>
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
		case BossState.DEAD:
			m_bossState =  m_bossStateDead;
			break;
		}
		
		m_bossState.OnEnter();
	}

	/// <summary>
	/// Returns the current boss state.
	/// </summary>
	public BossState GetBossState() {
		return m_bossState.State;
	}

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

		public Boss.BossState GetState() { return m_state; }
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
			m_boss.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)Boss.AnimatorType.TRIGGER, "goIdle", 0f);

			//
			m_boss.GetComponent<NavMeshAgent>().ResetPath();
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


		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Move.");
			m_boss.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)Boss.AnimatorType.TRIGGER, "goWalk", 0f);
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
			m_boss.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)Boss.AnimatorType.TRIGGER, "goWalk", 0f);

			//
			m_boss.GetComponent<NavMeshAgent>().SetDestination(m_boss.m_target.position);
		}
		
		public override void OnUpdate() {
			if(m_boss.InMoveRange()){
				if(m_boss.SearchAround()){
					//if (Vector3.SqrMagnitude(m_boss.m_target.position - m_boss.transform.position) > 10.0 * 10.0) {
					m_boss.GetComponent<NavMeshAgent>().SetDestination(m_boss.m_target.position);
					if (Vector3.SqrMagnitude(m_boss.m_target.position - m_boss.transform.position) > (m_boss.m_basicAttackRange)) {
						Vector3 targetPos = m_boss.m_target.position;
						Vector3 direction = (targetPos - m_boss.transform.position).normalized;
						direction.y = 0;
						
						//m_boss.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
						//m_boss.transform.position += direction * m_boss.m_speed * Time.deltaTime;
						// m_boss.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
						// m_boss.transform.position += direction * m_boss.m_speed * Time.deltaTime;
					} 
					else {
						m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.ATTACKING);
					}
				} else {
					// Can't find any players.
					Debug.Log("In Boss:Can't find any player, return.");
					m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.RETURNING);
				}
			} 
			else {
				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.RETURNING);
			}
		}
		
		public override void OnExit() {
			
		}
	}

	[Serializable]
	class BossStateAttack : BossStateBase {
		bool hasAttack;
		public BossStateAttack(Boss boss) {
			m_startTime = Time.time;
			m_state = BossState.ATTACKING;
			m_boss = boss;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Attack.");
			//
			m_boss.GetComponent<NavMeshAgent>().ResetPath();

			hasAttack = false;
			//Debug.Log("Boss Attack.");
			//m_boss.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)Boss.AnimatorType.TRIGGER, "goAttack", 0f);
			if (m_boss.m_action != null || m_boss.m_target == null) {
				m_boss.SwitchState(Boss.BossState.IDLE);
				return;
			}
			else {
				m_boss.ResetAnimTrigger();
                m_boss.transform.LookAt(m_boss.m_target);
				m_boss.m_action = m_boss.m_actionHelper.PlayAction("Basic");
				m_boss.m_action.SetGameObject(1, m_boss.m_target.gameObject);
			}
		}
		
		public override void OnUpdate() {

			if (m_boss.m_targetUnit == null || m_boss.m_targetUnit.m_isDead){
				Debug.Log("boss target dead");
				m_boss.SwitchState(BossState.IDLE);
				return;

			}
// 			// do attack
//             if (Time.time - m_startTime > 1.6 && !hasAttack) {
//                 m_boss.m_target.GetComponent<Player>().receiveAttack(m_boss.m_basicAttackHit, m_boss.transform);
//                 hasAttack = true;
//             }
// 			//
// 			if(Time.time - m_startTime > 2.9){
// 				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.CHASING);
// 			}
			//if (m_boss.m_action == null) {
				//m_boss.SwitchState(Boss.BossState.IDLE);
			if (m_boss.m_action == null) {
				m_boss.SwitchState(Boss.BossState.CHASING);
				// m_boss.SwitchState(Boss.BossState.IDLE);
				return;
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
			//m_boss.m_speed = m_boss.m_returnSpeed;
			// m_boss.m_speed = m_boss.m_returnSpeed;
			m_boss.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)Boss.AnimatorType.TRIGGER, "goWalk", 0f);

			//
			m_boss.GetComponent<NavMeshAgent>().SetDestination(m_boss.m_bossIdlePosition);
		}
		
		public override void OnUpdate() {
			Vector3 dir = m_boss.m_bossIdlePosition - m_boss.transform.position;
			dir.y = 0;
			if (Vector3.SqrMagnitude(dir) > 6.0 * 6.0) {
				//Vector3 targetPos = m_boss.m_bossIdlePosition;
				/*Vector3 targetPos = m_boss.m_bossIdlePosition;
				Vector3 direction = (targetPos - m_boss.transform.position).normalized;
				direction.y = 0;
				if (direction != Vector3.zero)
					m_boss.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
				//m_boss.transform.position += direction * m_boss.m_speed * Time.deltaTime;
				m_boss.transform.position += direction * m_boss.m_speed * Time.deltaTime;*/
			} else {
				m_boss.photonView.RPC("RPC_switchState", PhotonTargets.All, (int)BossState.IDLE);
			}
		}
		
		public override void OnExit() {
			
		}
	}

	[Serializable]
	//Still needs to be fleshed out 
	//Added to remain consistent with xp system framework
	class BossStateDead : BossStateBase {
		public BossStateDead(Boss boss) {
			m_startTime = Time.time;
			m_state = BossState.DEAD;
			m_boss = boss;
		}
		
		public override void OnEnter() {
			m_startTime = Time.time;
			Debug.Log("Boss Dead.");

			Vector3 position = m_boss.transform.position;
			Collider[] objectsAroundMe = Physics.OverlapSphere(position, m_boss.m_XPradius);
			Collider temp;
			for (int i = 0; i < objectsAroundMe.Length; i++)
			{
				temp = objectsAroundMe[i];
				if (temp.CompareTag("Player"))
				{						
					Player curPlayer = temp.GetComponent<Player>();
					//XP is adjusted based on damage dealt
					float adjustedXP =  (float) m_boss.m_XP * curPlayer.GetTeamDMG(curPlayer.team);
					curPlayer.GetXP((int)adjustedXP);
				}
					
			}
			m_boss.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)Boss.AnimatorType.TRIGGER, "goDead", 0f);
			m_boss.m_isDead = true;
		}
		
		public override void OnUpdate() {
			if (Time.time - m_startTime > 3)
				PhotonNetwork.Destroy(m_boss.gameObject);
		}
		
		public override void OnExit() {
			m_boss.m_isDead = false;
		}
	}


	#endregion
	#region BOSS_RPC
	[RPC]
	void RPC_switchState(int bossState){
		SwitchState((BossState)bossState);
	}
	#endregion
	#region PHOTON_SYNC
	/// <summary>
	/// photon syncing
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="info"></param>
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		base.OnPhotonSerializeView(stream, info);
	}
	#endregion
}
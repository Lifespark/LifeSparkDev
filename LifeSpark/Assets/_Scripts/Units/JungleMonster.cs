using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AGE;

public class JungleMonster : UnitObject//FlockingUnitObject
{
    #region ENUM_FIELD
    public enum JungleMonsterState
    {
        IDLE,
        MOVING,
        CHASING,
        RETURNING,
        ATTACKING,
        ATTACKED,
        DEAD,
        ROAMING,
        GATHERING,
        DEFAULT
    }

    public enum AnimatorType
    {
        BOOL = 1,
        INT = 2,
        FLOAT = 3,
        TRIGGER = 4
    }

    public enum CreepType 
    {
        ORC,
        KOBOLD,
        GOBLIN,
    }

    [Flags]
    public enum PacketMask
    {
        NONE = 0,
        POSITION = 1,
        ROTATION = 2,
        VELOCITY = 4,
    }
    #endregion

    #region JungleMonster_ATTRIBUTE
    public float maxSpeed = 5.0f;
    public float detectRadius = 20;
    public float attackRadius = 2;
	public static float m_XPradius = 35;
	public static float m_XP = 25;
    public Transform target;
    public Vector3 source;
    public int originCamp;
    public int currentCamp;
    public Vector3[] targets;
    public int[] targetID;
    public Vector3 deviatePos;
    public bool isDeviated = false;
    public GameObject lockOnEnemy;
    public JungleMonsterState curState;
    public Vector3 spreadDir;
    public Hit m_hit;
    public CreepType m_creepType;
    public JungleCamp m_myCamp;
    #endregion

    #region JungleMonster_STATE
    public JungleMonsterStateIdle jungleMonsterStateIdle;
    public JungleMonsterStateAttack jungleMonsterStateAttack;
    public JungleMonsterStateAttacked jungleMonsterStateAttacked;
    public JungleMonsterStateChase jungleMonsterStateChase;
    public JungleMonsterStateReturn jungleMonsterStateReturn;
    public JungleMonsterStateDead jungleMonsterStateDead;
    public JungleMonsterStateRoaming jungleMonsterStateRoaming;
    public JungleMonsterStateGathering jungleMonsterStateGathering;

    private JungleMonsterStateBase jungleMonsterState;
    #endregion

    private Animator anim;
    private NavMeshAgent navAgent;
    private NavMeshPath mainNavPath = new NavMeshPath();
    private int animPackageNum = 0;

    public bool isOffensive = false;
    public bool isRoaming = false;
    public bool isGathering = false;
    public bool isInOffensive = false;
    public bool isInRoaming = false;
    public bool isInGathering = false;

    private AGE.Action m_action = null;
    private ActionHelper m_actionHelper;
    // store the enemy so we do not search them during game
    // in case of a player die or drop, could just move it out of vision and not destroy it in case of null reference
    public List<Player> enemyPlayers = new List<Player>();

    // Use this for initialization
    public override void Awake()
    {
        base.Awake();
        m_unitType = UnitObjectType.DYNAMIC;
        // initialize all states
        jungleMonsterStateIdle = new JungleMonsterStateIdle(this);
        jungleMonsterStateAttack = new JungleMonsterStateAttack(this);
        jungleMonsterStateAttacked = new JungleMonsterStateAttacked(this);
        jungleMonsterStateChase = new JungleMonsterStateChase(this);
        jungleMonsterStateReturn = new JungleMonsterStateReturn(this);
        jungleMonsterStateDead = new JungleMonsterStateDead(this);
        jungleMonsterStateRoaming = new JungleMonsterStateRoaming(this);
        jungleMonsterStateGathering = new JungleMonsterStateGathering(this);

        // initialize Findable stuff
        SwitchState(JungleMonsterState.IDLE);
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        anim = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        source = (Vector3)photonView.instantiationData[0];
        currentCamp = originCamp = (int)photonView.instantiationData[1];
        targets = (Vector3[])photonView.instantiationData[2];
        targetID = (int[])photonView.instantiationData[3];
        gameObject.name = (string)photonView.instantiationData[4];
        deviatePos = targets[currentCamp];

        // calculate and store the nav path from source to target
        //navAgent.CalculatePath(target.position + spreadDir * 2.0f, mainNavPath);

        MonsterClient.Instance.allMonsters.Add(this);
        MonsterClient.Instance.MonsterLookUp.Add(gameObject.name, this);
        MonsterClient.Instance.OnJungleMonsterInstantiated();

		// Connect TierAction func to Boss
        //if(PhotonNetwork.isMasterClient) {
        //    Boss.OnTierChanged += TierAction;
        //}
        UIManager.mgr.getHpBar(this, new Vector3(0, 10f, 0) / transform.localScale.x);
        m_actionHelper = GetComponent<ActionHelper>();
    }

    // Update is called once per frame
    new void Update()
    {
        if (!LevelManager.Instance.m_startedGame)
            return;

        base.Update();


        if (unitHealth <= 0)
            SwitchState(JungleMonsterState.DEAD);
        // if I'm not in control, sync position from network
        if (PhotonNetwork.isMasterClient) {
			
            //if (targetSparkPoint.owner == owner && curState != jungleMonsterState.DEAD
            //    || targetSparkPoint.sparkPointState == SparkPoint.SparkPointState.DESTROYED)
            //{
            //    SwitchState(jungleMonsterState.DEAD);
            //    return;
            //}

            //temp!!!!!!!!!!!!!!!!!!!!
            if (Input.GetKeyUp("q"))
            {
                isInOffensive = false;
            }
            else if (Input.GetKeyUp("w"))
            {
                isInRoaming = false;
            }
            else if (Input.GetKeyUp("e"))
            {
                isInGathering = false;
            }
            else if (Input.GetKeyDown("q"))
            {
                if (!isInOffensive)
                {
                    isOffensive = !isOffensive;
                    isInOffensive = true;
                }
            }
            else if (Input.GetKeyDown("w"))
            {

                if (!isInRoaming)
                {
                    isRoaming = !isRoaming;
                    isGathering = false;
                    isInRoaming = true;

                    if (isRoaming)
                        SwitchState(JungleMonsterState.ROAMING);
                    else
                        SwitchState(JungleMonsterState.RETURNING);
                }
            }
            else if (Input.GetKeyDown("e"))
            {
                if (!isInGathering)
                {
                    isGathering = !isGathering;
                    isRoaming = false;
                    isInGathering = true;

                    if (isGathering)
                        SwitchState(JungleMonsterState.GATHERING);
                    else
                        SwitchState(JungleMonsterState.RETURNING);
                }
            }

            if (jungleMonsterState != null)
                jungleMonsterState.OnUpdate();
        }
    }

	public void TierAction(int tier){
		switch(tier) {
			case 1:
				if(!isOffensive) {
					isOffensive = true;
				}
				break;
			case 2:
				if(!isRoaming) {
					isRoaming = true;
					SwitchState(JungleMonsterState.ROAMING);
				}
				break;
			case 3:
				if(!isGathering) {
					isGathering = true;
					isRoaming = false;
					SwitchState(JungleMonsterState.GATHERING);
				}
				break;
		}

	}

    /// <summary>
    /// switch to another state
    /// </summary>
    /// <param name="toState">destination state</param>
    public void SwitchState(JungleMonsterState toState)
    {
        if (jungleMonsterState != null && jungleMonsterState.State == toState)
            return;
        curState = toState;
        if (jungleMonsterState != null)
            jungleMonsterState.OnExit();
        //Debug.Log(toState);
        switch (toState)
        {
            case JungleMonsterState.IDLE:
                jungleMonsterState = jungleMonsterStateIdle;
                break;
            case JungleMonsterState.ATTACKING:
                jungleMonsterState = jungleMonsterStateAttack;
                break;
            case JungleMonsterState.CHASING:
                jungleMonsterState = jungleMonsterStateChase;
                break;
            case JungleMonsterState.RETURNING:
                jungleMonsterState = jungleMonsterStateReturn;
                break;
            case JungleMonsterState.DEAD:
                jungleMonsterState = jungleMonsterStateDead;
                break;
            case JungleMonsterState.GATHERING:
                jungleMonsterState = jungleMonsterStateGathering;
                break;
            case JungleMonsterState.ROAMING:
                jungleMonsterState = jungleMonsterStateRoaming;
                break;
            case JungleMonsterState.ATTACKED:
                jungleMonsterState = jungleMonsterStateAttacked;
                break;
        }
        jungleMonsterState.OnEnter();
    }
    
	public JungleMonsterStateBase GetJungleMonsterState() { return jungleMonsterState; }

    /// <summary>
    /// photon syncing
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    new void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!LevelManager.Instance.m_startedGame)
            return;
        base.OnPhotonSerializeView(stream, info);
    }

    void OnAnimatorMove() {
        //if (PhotonNetwork.isMasterClient)
            navAgent.velocity = anim.deltaPosition / Time.deltaTime;
    }

    /// <summary>
    /// change animator parameter over network. must cast animType to int and value to float when calling
    /// </summary>
    /// <param name="animType"></param>
    /// <param name="param"></param>
    /// <param name="value"></param>
    [RPC]
    void RPC_setAnimParam(int animType, string param, float value = 0)
    {
        //Debug.Log(param + "\t" + Time.time);
        switch ((AnimatorType)animType)
        {
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
                // reset all other trigger
                anim.ResetTrigger("goWalk");
                anim.ResetTrigger("goDead");
                anim.ResetTrigger("goIdle");
                anim.ResetTrigger("goAttack");
                anim.ResetTrigger("goHit");
                anim.SetTrigger(param);
                break;
        }
    }

    [RPC]
    void RPC_setNavAgentState(bool enabled, bool updateDest, Vector3 destination) {
        if (enabled) {
            navAgent.Resume();
            if (updateDest) {
                // or do we pass the calculated path here?
                navAgent.SetDestination(destination);
            }
        }
        else {
            navAgent.Stop();
            navAgent.ResetPath();
        }
    }

    /// <summary>
    /// Base state class. Can only be inherited
    /// </summary>
    [Serializable]
    public abstract class JungleMonsterStateBase
    {
        // state name
        protected JungleMonsterState state;
        //  jungleMonster instance
        protected JungleMonster jungleMonster;
        // when do we start this state?
        protected float startTime;

        public JungleMonsterState State { get { return state; } }

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

        public JungleMonsterStateBase() { startTime = Time.time; }
    }

    [Serializable]
    public class JungleMonsterStateIdle : JungleMonsterStateBase
    {
        public JungleMonsterStateIdle(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.IDLE;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            //Debug.Log("Idle");
            startTime = Time.time;
            if (jungleMonster.anim)
                //jungleMonster.anim.SetTrigger("goIdle");
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goIdle", 0f);
        }

        public override void OnUpdate()
        {
            if (jungleMonster.isOffensive)
            {
                //GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
                int index = -1;
                float minDistance = 0;

                for (int i = 0; i < PlayerManager.Instance.allPlayers.Count; i++)
                {
                    float currentDistance = Vector3.SqrMagnitude(jungleMonster.transform.position - PlayerManager.Instance.allPlayers[i].transform.position);
                    if (index == -1 ||
                        currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        index = i;
                    }
                }
                jungleMonster.lockOnEnemy = PlayerManager.Instance.allPlayers[index].gameObject;

                if (Vector3.SqrMagnitude(jungleMonster.transform.position - PlayerManager.Instance.allPlayers[index].transform.position) < jungleMonster.detectRadius * jungleMonster.detectRadius)
                {
                    jungleMonster.SwitchState(JungleMonsterState.ATTACKING);
                }
            }

            //if (Vector3.SqrMagnitude(jungleMonster.source - jungleMonster.transform.position) > 1.0)
            //{
            //    jungleMonster.SwitchState(JungleMonsterState.ATTACKING);
            //    return;
            //}
        }

        public override void OnExit()
        {

        }
    }

    [Serializable]
    public class JungleMonsterStateAttack : JungleMonsterStateBase
    {
        public float attackIntervial = 4;
        private float lastAttackTime = -2;
        private Quaternion lookAtPlayer;
        public JungleMonsterStateAttack(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.ATTACKING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Attack");
            startTime = Time.time;
            //jungleMonster.navAgent.Stop();
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, false, false, Vector3.zero);
            /*
            if (Vector3.SqrMagnitude(jungleMonster.transform.position - jungleMonster.lockOnEnemy.transform.position) > jungleMonster.attackRadius * jungleMonster.attackRadius)
            {
                jungleMonster.SwitchState(JungleMonsterState.CHASING);
                return;
            }
            */
        }

        public override void OnUpdate()
        {
			UnitObject target = jungleMonster.lockOnEnemy.GetComponent<UnitObject>();

			if (target != null && target.m_isDead){
				jungleMonster.SwitchState(JungleMonsterState.IDLE);
				return;
			}

            float sqrDistance = Vector3.SqrMagnitude(jungleMonster.lockOnEnemy.transform.position - jungleMonster.transform.position);
            // if enemy not in attack range but in chasing range, return to tracing state
            if (sqrDistance > jungleMonster.attackRadius * jungleMonster.attackRadius && jungleMonster.m_action == null)
            {
                jungleMonster.SwitchState(JungleMonsterState.CHASING);
                return;
            }
            // attack enemy
            if (Time.time - lastAttackTime > attackIntervial)
            {
				lastAttackTime = Time.time;
                if (jungleMonster.anim) {
                    //jungleMonster.anim.SetTrigger("goAttack");
                    //jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goAttack", 0f);
                    lookAtPlayer = Quaternion.LookRotation(jungleMonster.lockOnEnemy.transform.position - jungleMonster.transform.position);
                    jungleMonster.transform.rotation = lookAtPlayer;
                    jungleMonster.m_action = jungleMonster.m_actionHelper.PlayAction("Attack");
                    jungleMonster.StartCoroutine(DealDamage());
                }
            }
        }

        public override void OnExit()
        {
            //jungleMonster.navAgent.Resume();
            //jungleMonster.StopCoroutine("DealDamage");
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, false, Vector3.zero);
        }

        IEnumerator DealDamage() {
            yield return new WaitForSeconds(1.11f);
            jungleMonster.lockOnEnemy.GetComponent<Player>().receiveAttack(jungleMonster.m_hit, jungleMonster.transform);
        }
    }

    [Serializable]
    public class JungleMonsterStateAttacked : JungleMonsterStateBase {

        private bool hasHit;

        public JungleMonsterStateAttacked(JungleMonster pjungleMonster) {
            startTime = Time.time;
            state = JungleMonsterState.ATTACKED;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter() {
            //Debug.Log("Attack");
            startTime = Time.time;
            hasHit = false;
            //jungleMonster.navAgent.Stop();
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, false, false, Vector3.zero);
        }

        public override void OnUpdate() {
            if (Time.time - startTime > 0.5f && !hasHit) {
                hasHit = true;
                if (jungleMonster.anim)
                    //jungleMonster.anim.SetTrigger("goWalk");
                    jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goHit", 0f);
            }

            if (Time.time - startTime > 1.5f) {
                jungleMonster.SwitchState(JungleMonsterState.IDLE);
                return;
            }
        }

        public override void OnExit() {

        }
    }

    [Serializable]
    public class JungleMonsterStateChase : JungleMonsterStateBase
    {
        public JungleMonsterStateChase(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.CHASING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Chase");
            startTime = Time.time;
            if (jungleMonster.anim)
                //jungleMonster.anim.SetTrigger("goWalk");
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goWalk", 0f);
            //jungleMonster.navAgent.Resume();
            //jungleMonster.navAgent.SetDestination(jungleMonster.lockOnEnemy.transform.position);
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, jungleMonster.lockOnEnemy.transform.position);
            //jungleMonster.transform.LookAt(jungleMonster.lockOnEnemy.transform.position);
            Vector3 target = jungleMonster.m_navAgent.steeringTarget;
            target.y = 0;
            jungleMonster.transform.LookAt(target);
        }

        public override void OnUpdate()
        {
            Vector3 target = jungleMonster.m_navAgent.steeringTarget;
            target.y = 0;
            jungleMonster.transform.LookAt(target);
            float sqrDistance = 100000;
            if (!jungleMonster.isRoaming)
                sqrDistance  = Vector3.SqrMagnitude(jungleMonster.transform.position - jungleMonster.source);
            else
                sqrDistance = Vector3.SqrMagnitude(jungleMonster.transform.position - jungleMonster.deviatePos);

            if (sqrDistance < jungleMonster.detectRadius *jungleMonster.detectRadius)
            {
                // still too far away to attack
                if (Vector3.SqrMagnitude(jungleMonster.lockOnEnemy.transform.position - jungleMonster.transform.position) > 0.8 * jungleMonster.attackRadius * 0.8 * jungleMonster.attackRadius)
                {
#if false                
					// Move via flocking algorithm (if flock leader) or navmesh pathfinding (if follower)
					GameObject[] jungleCreeps = GameObject.FindGameObjectsWithTag("JungleCreep");
					List<FlockingUnitObject> eligibleFlockMates = new List<FlockingUnitObject>();
					for (int i = 0; i < jungleCreeps.Length; i++) {
						if (jungleCreeps[i].GetComponent<JungleMonster>().GetJungleMonsterState().State == JungleMonsterState.CHASING && jungleCreeps[i].GetComponent<JungleMonster>() != jungleMonster) {
							eligibleFlockMates.Add((FlockingUnitObject)jungleCreeps[i].GetComponent<JungleMonster>());
						}
					}
					if (!jungleMonster.MoveWithFlockUnlessLeader(eligibleFlockMates, jungleMonster.maxSpeed)){
						//jungleMonster.navAgent.SetDestination(jungleMonster.lockOnEnemy.transform.position);
						jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, jungleMonster.lockOnEnemy.transform.position);
					}
#else
                    jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, jungleMonster.lockOnEnemy.transform.position);
#endif

                }
                else
                {
                    jungleMonster.SwitchState(JungleMonsterState.ATTACKING);
                    return;
                }
            }
            else
            {
                if (!jungleMonster.isRoaming)
                    jungleMonster.SwitchState(JungleMonsterState.RETURNING);
                else
                    jungleMonster.SwitchState(JungleMonsterState.ROAMING);
                return;
            }
        }

        public override void OnExit()
        {
            //jungleMonster.navAgent.Stop();
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, false, false, Vector3.zero);

        }
    }

    [Serializable]
    public class JungleMonsterStateReturn : JungleMonsterStateBase
    {
        public JungleMonsterStateReturn(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.RETURNING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Return");
            startTime = Time.time;
            if (jungleMonster.anim)
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goWalk", 0f);

            //jungleMonster.navAgent.Resume();
            //jungleMonster.navAgent.SetDestination(jungleMonster.source);
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, jungleMonster.source);
            Vector3 target = jungleMonster.m_navAgent.steeringTarget;
            target.y = 0;
            jungleMonster.transform.LookAt(target);
        }

        public override void OnUpdate()
        {
            if (true)
            {
                Vector3 target = jungleMonster.m_navAgent.steeringTarget;
                target.y = 0;
                jungleMonster.transform.LookAt(target);
                if (Vector3.SqrMagnitude(jungleMonster.source - jungleMonster.transform.position) > 2)
                {
                    //jungleMonster.navAgent.SetDestination(jungleMonster.source);
                    jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, jungleMonster.source);
                }
                else
                {
                    jungleMonster.SwitchState(JungleMonsterState.IDLE);
                    return;
                }
            }
            else
            {
                // what if the target has been destroyed?
                jungleMonster.SwitchState(JungleMonsterState.IDLE);
                return;
            }
        }

        public override void OnExit()
        {
            //jungleMonster.navAgent.Stop();
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, false, false, Vector3.zero);
        }
    }

    

    [Serializable]
    public class JungleMonsterStateDead : JungleMonsterStateBase
    {
        public float corpseRemainTime = 5f;
        float disappearDuration = 1.5f;
        bool hasChangedShader = false;
        SkinnedMeshRenderer skinedMeshRrenderer;

        public JungleMonsterStateDead(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.DEAD;
            jungleMonster = pjungleMonster;
            skinedMeshRrenderer = jungleMonster.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        public override void OnEnter()
        {
            startTime = Time.time;
            jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goDead", 0f);
			//Grant XP to players on the same team as the killer
			//NOTE: CAN BE changed to grant all players 
			Transform lastHit = jungleMonster.lastAttacker;
			if (lastHit!= null)
			{
				Player attacker = lastHit.GetComponent<Player>();
				int teamXP = attacker.team;	
				
				Vector3 position = jungleMonster.transform.position;
				Collider[] objectsAroundMe = Physics.OverlapSphere(position, m_XPradius);
				Collider temp;
				for (int i = 0; i < objectsAroundMe.Length; i++)
				{
					temp = objectsAroundMe[i];
					if (temp.CompareTag("Player"))
					{
						if (temp.GetComponent<Player>().team == teamXP)
						temp.GetComponent<Player>().GetXP(m_XP);
					}
					
				}
			}
			jungleMonster.m_isDead = true;
        }

        public override void OnUpdate()
        {
            if (Time.time - startTime >= corpseRemainTime)
            {
               // jungleMonsterManager.Instance.jungleMonsterDict[jungleMonster.source.gameObject].Remove(jungleMonster);

                // TODO: will need to pass the following event to non-master client
                if (!hasChangedShader) {
                    hasChangedShader = true;
                    skinedMeshRrenderer.material.shader = Shader.Find("Transparent/Diffuse");
                }
                float disappearPercent = (Time.time - startTime - corpseRemainTime) / disappearDuration;
                if (disappearPercent >= 1 && PhotonNetwork.isMasterClient) {
                    PhotonNetwork.Destroy(jungleMonster.gameObject);
                    jungleMonster.m_myCamp.m_myJungleCreeps.Remove(jungleMonster);
                }
                else {
                    Color c = Color.white;
                    c.a = (1 - disappearPercent);
                    skinedMeshRrenderer.material.color = c;
                }
            }
        }

        public override void OnExit()
        {
			jungleMonster.m_isDead = false;
        }
    }

    [Serializable]
    public class JungleMonsterStateRoaming : JungleMonsterStateBase
    {
        Vector3 target;
        public JungleMonsterStateRoaming(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.ROAMING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Roaming");
            startTime = Time.time;
            if (jungleMonster.anim)
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goWalk", 0f);

            target = jungleMonster.targets[jungleMonster.targetID[jungleMonster.currentCamp]];
            //jungleMonster.navAgent.SetDestination(target);
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, target);
        }

        public override void OnUpdate()
        {
            if (jungleMonster.isOffensive)
            {
                GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
                int index = -1;
                float minDistance = 0;

                for (int i = 0; i < allPlayers.Length; i++)
                {
                    float currentDistance = Vector3.SqrMagnitude(jungleMonster.transform.position - allPlayers[i].transform.position);
                    if (index == -1 ||
                        currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        index = i;
                    }
                }
                jungleMonster.lockOnEnemy = allPlayers[index].gameObject;

                if (Vector3.SqrMagnitude(jungleMonster.transform.position - allPlayers[index].transform.position) < jungleMonster.attackRadius * jungleMonster.attackRadius)
                {
                    Vector3 startPos = jungleMonster.targets[jungleMonster.currentCamp];
                    Vector3 endPos = target;
                    Vector3 currentPos = jungleMonster.transform.position;
                    Vector3 dir = Vector3.Normalize(endPos - startPos);
                    Vector3 projectedPos = Vector3.Dot(currentPos - startPos, dir) * dir + startPos;

                    float lastDistance = Vector3.Magnitude(endPos - jungleMonster.deviatePos);
                    float currentDistance = Vector3.Magnitude(endPos - projectedPos);

                    if (currentDistance < lastDistance)
                    {
                        jungleMonster.deviatePos = projectedPos;
                        jungleMonster.SwitchState(JungleMonsterState.ATTACKING);
                    }
                }
            }

            if(Vector3.SqrMagnitude(target - jungleMonster.transform.position) < 1.0)
            {
                jungleMonster.currentCamp = jungleMonster.targetID[jungleMonster.currentCamp];
                jungleMonster.deviatePos = target;
                target = jungleMonster.targets[jungleMonster.targetID[jungleMonster.currentCamp]];
                //jungleMonster.navAgent.SetDestination(target);
                jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, target);
            }
        }

        public override void OnExit()
        {
            if (jungleMonster.isRoaming == false)
                jungleMonster.currentCamp = jungleMonster.originCamp;
        }
    }

    [Serializable]
    public class JungleMonsterStateGathering : JungleMonsterStateBase
    {
        public JungleMonsterStateGathering(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.IDLE;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Gathering");
            startTime = Time.time;
            if (jungleMonster.anim)
                //jungleMonster.anim.SetTrigger("goIdle");
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goWalk", 0f);
            //jungleMonster.navAgent.Stop();
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, false, false, Vector3.zero);
        }

        public override void OnUpdate()
        {
            //jungleMonster.navAgent.SetDestination(new Vector3(0,0,0));
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, true, true, Vector3.zero);
        }

        public override void OnExit()
        {
            //jungleMonster.navAgent.Stop();
            jungleMonster.photonView.RPC("RPC_setNavAgentState", PhotonTargets.AllViaServer, false, false, Vector3.zero);
        }
    }

}

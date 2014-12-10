using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LaneCreep : UnitObject {
    #region ENUM_FIELD
    public enum CreepState {
        IDLE,
        MOVING,
        CHASING,
        RETURNING,
        ATTACKING,
        DISTANCE_ATTACKING,
        ATTACKED,
        CAPTURING,
        DEAD,
        DEFAULT
    }

    public enum CreepType {
        Ranged,
        Melee,
    }

	public enum PrevPath {
		SELECTED,
		DEFAULT,
		ALTERNATE
	}

    #endregion

    #region CREEP_ATTRIBUTE
    public int          owner;
    public float        maxSpeed = 5.0f;
	public string       playerName;
    public float        detectRadius = 10;
    public float        attackRadius = 2;
    //public float        captureSpeed = 0.05f;
    public float        captureTimePerWave = 20 / 4;
    public Transform    target;
    public SparkPoint   targetSparkPoint;
	public Dictionary<string, PrevPath> m_previousPaths; //Keeps track of what path (sparkpoint) the lane creep used the last time it reached each sparkpoint.
	public bool			onAlternatePath = false;
    public Transform    source;
    public GameObject   lockOnEnemy;
    public CreepState   curState;
    public Vector3      spreadDir;
    public CreepType    m_creepType;
    public Hit          m_basicHit;
	public float m_timetoLive = 360; //In seconds
	public float m_XP = 10;
	public float m_XPradius = 35;
    
	public Texture2D m_blueTexture;
    public Texture2D m_redTexture;
    #endregion

    #region CREEP_STATE
    public CreepStateIdle           creepStateIdle;
    public CreepStateMove           creepStateMove;
    public CreepStateAttack         creepStateAttack;
    public CreepStateDistanceAttack creepStateDistanceAttack;
    public CreepStateChase          creepStateChase;
    public CreepStateReturn         creepStateReturn;
    public CreepStateCapture        creepStateCapture;
    public CreepStateDead           creepStateDead;
    public CreepStateAttacked       creepStateAttacked;

    private CreepStateBase      creepState;
    #endregion
    
    private string          m_fireBallName;
    private Animator        anim;
    private NavMeshAgent    navAgent;
    private NavMeshPath     mainNavPath = new NavMeshPath();
	private float creepSpawn;
    
    private bool			isLeader = false;	// Only used when creepState == chasing.
    											// Determines whether creep should use navmesh for movement (TRUE) or flocking calculations (FALSE)

    // store the enemy so we do not search them during game
    // in case of a player die or drop, could just move it out of vision and not destroy it in case of null reference
    public List<Player> enemyPlayers = new List<Player>();

	// Use this for initialization
	public override void Awake () {
        base.Awake();
		creepSpawn = Time.time;
        m_unitType = UnitObjectType.DYNAMIC;
        // initialize all states
        creepStateIdle              = new CreepStateIdle(this);
        creepStateMove              = new CreepStateMove(this);
        creepStateAttack            = new CreepStateAttack(this);
        creepStateDistanceAttack    = new CreepStateDistanceAttack(this);
        creepStateChase             = new CreepStateChase(this);
        creepStateReturn            = new CreepStateReturn(this);
        creepStateCapture           = new CreepStateCapture(this);
        creepStateDead              = new CreepStateDead(this);
        creepStateAttacked          = new CreepStateAttacked(this);

        // initialize Findable stuff
        SwitchState(CreepState.IDLE);
        target = SparkPointManager.Instance.sparkPointsDict[(string)photonView.instantiationData[0]].transform;
        targetSparkPoint = target.GetComponent<SparkPoint>();
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        // initialize creep property passed in by photon network instantiation 
        owner = (int)photonView.instantiationData[1];

        if (owner == 2) {
            m_silhouetteRenderer.material.SetTexture("_MainTex", m_redTexture);
            m_fireBallName = "FireBall";
        }
        else {
            m_silhouetteRenderer.material.SetTexture("_MainTex", m_blueTexture);
            m_fireBallName = "FireBall2";
        }

        playerName = (string)photonView.instantiationData[2];

        anim = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        
//         GetComponentInChildren<SkinnedMeshRenderer>().material.color 
//             = new Color((float)photonView.instantiationData[3],
//                         (float)photonView.instantiationData[4],
//                         (float)photonView.instantiationData[5],
//                         (float)photonView.instantiationData[6]);
        source = SparkPointManager.Instance.sparkPointsDict[(string)photonView.instantiationData[3]].transform;

        spreadDir = (Vector3)photonView.instantiationData[4];

        // calculate and store the nav path from source to target
        navAgent.CalculatePath(target.position + spreadDir * 2.0f, mainNavPath);

        gameObject.name = "LaneCreep" + ((int)photonView.instantiationData[5]).ToString() /*+ "_" + owner.ToString()*/;

        // TODO: delete itself when die
        CreepManager.Instance.LaneCreepLookUp.Add(gameObject.name, this);

        m_creepType = (CreepType)photonView.instantiationData[6];

        // initialize enemy list
        foreach (var p in allPlayers) {
            Player playerScript = p.GetComponent<Player>();
            if (playerScript.team != owner) {
                enemyPlayers.Add(playerScript);
            }
        }

        if (!CreepManager.Instance.creepDict.ContainsKey(source.gameObject)) {
            CreepManager.Instance.creepDict.Add(source.gameObject, new List<LaneCreep>());
        }

        CreepManager.Instance.creepDict[source.gameObject].Add(this);
        UIManager.mgr.getHpBar(this,new Vector3(0,8.6f,0));

		m_previousPaths = new Dictionary<string, PrevPath>();
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
        if (PhotonNetwork.isMasterClient) {
            if (unitHealth <= 0  && curState != CreepState.DEAD) {
                SwitchState(CreepState.DEAD);
            }
			//Creeps have a limited time to live to save resources
			if (Time.time - creepSpawn >= m_timetoLive)
				SwitchState(CreepState.DEAD);
//             if (targetSparkPoint.owner == owner && curState != CreepState.DEAD
//                 || targetSparkPoint.sparkPointState == SparkPoint.SparkPointState.DESTROYED && curState != CreepState.DEAD) {
//                 SwitchState(CreepState.DEAD);
//                 return;
//             }
            if (creepState != null)
                creepState.OnUpdate();
        }
	}

    void OnAnimatorMove() {
        navAgent.velocity = anim.deltaPosition / Time.deltaTime;
    }

    /// <summary>
    /// switch to another state
    /// </summary>
    /// <param name="toState">destination state</param>
    public void SwitchState(CreepState toState) {
        if (creepState != null && creepState.State == toState)
            return;
        curState = toState;
        if (creepState != null)
            creepState.OnExit();

        switch (toState) {
            case CreepState.IDLE:
                creepState = creepStateIdle;
                break;
            case CreepState.MOVING:
                creepState = creepStateMove;
                break;
            case CreepState.ATTACKING:
                creepState = creepStateAttack;
                break;
            case CreepState.DISTANCE_ATTACKING:
                creepState = creepStateDistanceAttack;
                break;
            case CreepState.CHASING:
                creepState = creepStateChase;
                break;
            case CreepState.RETURNING:
                creepState = creepStateReturn;
                break;
            case CreepState.CAPTURING:
                creepState = creepStateCapture;
                break;
            case CreepState.DEAD:
                creepState = creepStateDead;
                break;
            case CreepState.ATTACKED:
                creepState = creepStateAttacked;
                break;
        }
        creepState.OnEnter();
    }

    void OnDestroy() {
        if (source && CreepManager.Instance.creepDict.ContainsKey(source.gameObject))
            CreepManager.Instance.creepDict[source.gameObject].Remove(this);
        if (CreepManager.Instance.LaneCreepLookUp.ContainsKey(name))
            CreepManager.Instance.LaneCreepLookUp.Remove(name);
    }
    
    public bool NotLastUsed(PrevPath path) {
		return (!m_previousPaths.ContainsKey(targetSparkPoint.name) || m_previousPaths[targetSparkPoint.name] != path);
	}

	public bool DefaultNotSelected() {
		return targetSparkPoint.m_selectedNextSparkPoint == null || targetSparkPoint.m_selectedNextSparkPoint != targetSparkPoint.m_defaultNextSparkPoint;
	}

    public CreepStateBase GetCreepState() { return creepState; }
    

    /// <summary>
    /// photon syncing
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    new void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
    }

    /// <summary>
    /// change animator parameter over network. must cast animType to int and value to float when calling
    /// </summary>
    /// <param name="animType"></param>
    /// <param name="param"></param>
    /// <param name="value"></param>
    [RPC]
    void RPC_setAnimParam(int animType, string param, float value = 0) {
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
                anim.ResetTrigger("goWalk");
                anim.ResetTrigger("goDead");
                anim.ResetTrigger("goIdle");
                anim.ResetTrigger("goAttack");
                anim.ResetTrigger("goHit");
                anim.SetTrigger(param);
                break;
        }
    }
    
    
    

    /// <summary>
    /// Base state class. Can only be inherited
    /// </summary>
    [Serializable]
    public abstract class CreepStateBase {
        // state name
        protected CreepState state;
        // lane creep instance
        protected LaneCreep laneCreep;
        // when do we start this state?
        protected float startTime;
        
        public CreepState State { get { return state; } }

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

        public CreepStateBase() { startTime = Time.time; }
    }

    [Serializable]
    public class CreepStateIdle : CreepStateBase {
        public CreepStateIdle(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.IDLE;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                //laneCreep.anim.SetTrigger("goIdle");
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goIdle", 0f);
        }

        public override void OnUpdate() {
            if (Vector3.SqrMagnitude(laneCreep.target.position - laneCreep.transform.position) > 5.0) {
				laneCreep.SwitchState(CreepState.MOVING);
                return;
			}
        }

        public override void OnExit() {

        }
    }

    [Serializable]
    public class CreepStateMove : CreepStateBase {
        public CreepStateMove(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.MOVING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                //laneCreep.anim.SetTrigger("goWalk");
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goWalk", 0f);
            laneCreep.navAgent.Resume();
            //laneCreep.navAgent.SetPath(laneCreep.mainNavPath);
            laneCreep.navAgent.SetDestination(laneCreep.target.transform.position + laneCreep.spreadDir * 10.0f);
            Vector3 target = laneCreep.m_navAgent.steeringTarget;
            target.y = 0;
            laneCreep.transform.LookAt(target);
        }

        public override void OnUpdate() {
            Vector3 target = laneCreep.m_navAgent.steeringTarget;
            target.y = 0;
            laneCreep.transform.LookAt(target);

            Collider[] nearByColliders = Physics.OverlapSphere(laneCreep.transform.position, laneCreep.detectRadius);
            int index = -1;
            float minDistance = 0;
            for (int i = 0; i < nearByColliders.Length; i++) {
                if (nearByColliders[i].tag == "Player") {
                    if (nearByColliders[i].GetComponent<Player>().team == laneCreep.owner || nearByColliders[i].GetComponent<Player>().GetState() == Player.PlayerState.Dead)
                        continue;
                }
                else if (nearByColliders[i].tag == "LaneCreep") {
                    if (nearByColliders[i].GetComponent<LaneCreep>().owner == laneCreep.owner || nearByColliders[i].GetComponent<LaneCreep>().curState == LaneCreep.CreepState.DEAD)
                        continue;
                }
				else if (nearByColliders[i].tag == "Boss") {
					if (nearByColliders[i].GetComponent<Boss>().GetBossState() == Boss.BossState.DEAD)
                        continue;
				}
                else
                    continue;
                float currentDistance = Vector3.SqrMagnitude(laneCreep.transform.position - laneCreep.transform.position);
                if (index == -1 || currentDistance < minDistance) {
                    minDistance = currentDistance;
                    index = i;
                }

                GameObject tmp_enemy = null;
                if (index != -1) {
                    tmp_enemy = nearByColliders[index].gameObject;
                }

                if (tmp_enemy != null) {
                    laneCreep.lockOnEnemy = tmp_enemy;
                    if (laneCreep.m_creepType == CreepType.Melee)
                    {
                        laneCreep.SwitchState(CreepState.CHASING);
                        return;
                    }
                    else if (laneCreep.m_creepType == CreepType.Ranged)
                    {
                        laneCreep.SwitchState(CreepState.DISTANCE_ATTACKING);
                        return;
                    }
                }
            }

            //foreach (var enemy in laneCreep.enemyPlayers) {
            //    // if enemy in sight, start chasing it
            //    if (Vector3.SqrMagnitude(laneCreep.transform.position - enemy.transform.position) < laneCreep.detectRadius * laneCreep.detectRadius &&
            //        enemy.gameObject.GetComponent<Player>().GetState() != Player.PlayerState.Dead) {
            //        laneCreep.lockOnEnemy = enemy.gameObject;
            //        if (laneCreep.m_creepType == CreepType.Melee) {                   
            //            laneCreep.SwitchState(CreepState.CHASING);
            //            return;
            //        }
            //        else if (laneCreep.m_creepType == CreepType.Ranged) {
            //            laneCreep.SwitchState(CreepState.DISTANCE_ATTACKING);
            //            return;
            //        }
            //    }
            //}


            if (laneCreep.target != null) {
                // target not reached, continue approaching
                Vector3 distVector = laneCreep.target.position + laneCreep.spreadDir * 5.0f - laneCreep.transform.position;
                distVector.y = 0;
                if (Vector3.SqrMagnitude(distVector) > 11 * 11) {
                    /*
                    Debug.Log(laneCreep.target.position + laneCreep.spreadDir * 2.0f - laneCreep.transform.position);
                    Vector3 targetPos = laneCreep.target.position + laneCreep.spreadDir * 2.0f;
                	Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
                    direction.y = 0;

                	laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
					laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
                    */
					
					//laneCreep.navAgent.SetDestination(laneCreep.target.transform.position);
					
				}
				else {
                    // start capturing sparkpoint
                    laneCreep.SwitchState(CreepState.CAPTURING);
                    return;
				}
			}
            else {
                // what if the target has been destroyed?
                laneCreep.SwitchState(CreepState.IDLE);
                return;
            }
        }

        public override void OnExit() {
            laneCreep.navAgent.Stop();
        }
    }

    [Serializable]
    public class CreepStateAttack : CreepStateBase {
        public float attackIntervial = 2;
        private float lastAttackTime = -2;

        public CreepStateAttack(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.ATTACKING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            
        }

        public override void OnUpdate() {
			if (laneCreep.lockOnEnemy == null || 
			    (laneCreep.lockOnEnemy.GetComponent<UnitObject>() != null && 
			 		laneCreep.lockOnEnemy.GetComponent<UnitObject>().m_isDead)) {
				laneCreep.SwitchState(CreepState.IDLE);
				return;
			}


            float sqrDistance = Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position);
            // if enemy not in attack range but in chasing range, return to tracing state
            if ( sqrDistance > laneCreep.attackRadius * laneCreep.attackRadius ) {
                laneCreep.SwitchState(CreepState.CHASING);
                return;
            }
            // attack enemy
            if (Time.time - lastAttackTime > attackIntervial) {
                if (laneCreep.anim)
                    //laneCreep.anim.SetTrigger("goAttack");
                    laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goAttack", 0f);
                lastAttackTime = Time.time;
            }
        }

        public override void OnExit() {

        }
    }


    [Serializable]
    public class CreepStateDistanceAttack : CreepStateBase {
        public float attackIntervial = 4;
        private float lastAttackTime = -4;

        public CreepStateDistanceAttack(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.DISTANCE_ATTACKING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
        }

        public override void OnUpdate() {
            if (laneCreep.lockOnEnemy == null || 
			    (laneCreep.lockOnEnemy.GetComponent<UnitObject>() != null && 
			 		laneCreep.lockOnEnemy.GetComponent<UnitObject>().m_isDead)) {
                laneCreep.SwitchState(CreepState.IDLE);
                return;
            }

            laneCreep.transform.LookAt(laneCreep.lockOnEnemy.transform.position - new Vector3(0, laneCreep.lockOnEnemy.transform.position.y, 0));
            float sqrDistance = Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position);
            // if enemy not in attack range but in chasing range, return to tracing state
            if (sqrDistance > laneCreep.detectRadius * laneCreep.detectRadius || (laneCreep.lockOnEnemy.tag == "Player" && laneCreep.lockOnEnemy.GetComponent<Player>().GetState() == Player.PlayerState.Dead) || (laneCreep.lockOnEnemy.tag == "LaneCreep" && laneCreep.lockOnEnemy.GetComponent<LaneCreep>().curState == LaneCreep.CreepState.DEAD)) {
                laneCreep.lockOnEnemy = null;
                laneCreep.SwitchState(CreepState.IDLE);
                return;
            }
            // attack enemy
            if (Time.time - lastAttackTime > attackIntervial) {
                if (laneCreep.anim)
                    //laneCreep.anim.SetTrigger("goAttack");
                    laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goAttack", 0f);
                PhotonNetwork.InstantiateSceneObject(laneCreep.m_fireBallName, laneCreep.transform.position + laneCreep.transform.forward * 4 + laneCreep.transform.transform.up * 4, Quaternion.identity, 0, new object[] { laneCreep.transform.forward } );
                lastAttackTime = Time.time;
            }
        }

        public override void OnExit() {

        }
    }

    [Serializable]
    public class CreepStateAttacked : CreepStateBase {

        private bool hasHit;

        public CreepStateAttacked(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.ATTACKED;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            laneCreep.navAgent.Stop();
            hasHit = false;
        }

        public override void OnUpdate() {
            if (Time.time - startTime > 0.5f && !hasHit) {
                hasHit = true;
                if (laneCreep.anim)
                    //jungleMonster.anim.SetTrigger("goWalk");
                    laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.All, (int)JungleMonster.AnimatorType.TRIGGER, "goHit", 0f);
            }

            if (Time.time - startTime > 1.5f) {
                laneCreep.SwitchState(CreepState.IDLE);
                return;
            }
        }

        public override void OnExit() {

        }
    }

    [Serializable]
    public class CreepStateChase : CreepStateBase {
        // maximum distance creep can chase before it gives up
        public float chasingDistance = 10;

        // position where creep starts chasing enemy
        private Vector3 deviatePosition;
        
        [SerializeField] float flockingRadius = 10; 

        public CreepStateChase(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.CHASING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                //laneCreep.anim.SetTrigger("goWalk");
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goWalk", 0f);
            deviatePosition = laneCreep.transform.position;
            laneCreep.navAgent.Resume();
            laneCreep.navAgent.SetDestination(laneCreep.lockOnEnemy.transform.position);
            Vector3 target = laneCreep.m_navAgent.steeringTarget;
            target.y = 0;
            laneCreep.transform.LookAt(target);
        }

        public override void OnUpdate() {
            Vector3 target = laneCreep.m_navAgent.steeringTarget;
            target.y = 0;
            laneCreep.transform.LookAt(target);
            float sqrDistance = Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position);
            float sqrChasingDist = Vector3.SqrMagnitude(laneCreep.transform.position - deviatePosition);
            // if has found enemy && in chasing distance && in chasing radius
            if (laneCreep.lockOnEnemy && sqrChasingDist < chasingDistance * chasingDistance && sqrDistance < laneCreep.detectRadius * laneCreep.detectRadius) {
                // still too far away to attack
                if (Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position) > laneCreep.attackRadius * laneCreep.attackRadius) {

//                  Vector3 targetPos = laneCreep.lockOnEnemy.transform.position;
//                  Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
//                  direction.y = 0;
// 
//                  laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
//                  laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
					
					
					// Move via flocking algorithm (if flock leader) or navmesh pathfinding (if follower)
#if false // currently not seems to work
					GameObject[] laneCreeps = GameObject.FindGameObjectsWithTag("LaneCreep");
					List<FlockingUnitObject> eligibleFlockMates = new List<FlockingUnitObject>();
					for (int i = 0; i < laneCreeps.Length; i++) {
						if (laneCreeps[i].GetComponent<LaneCreep>().GetCreepState().State == CreepState.CHASING && laneCreeps[i].GetComponent<LaneCreep>() != this.laneCreep) {
							eligibleFlockMates.Add((FlockingUnitObject)laneCreeps[i].GetComponent<LaneCreep>());
						}
					}
					if (!laneCreep.MoveWithFlockUnlessLeader(eligibleFlockMates, laneCreep.maxSpeed)){
						laneCreep.navAgent.SetDestination(laneCreep.lockOnEnemy.transform.position);
					}
#else
					laneCreep.navAgent.SetDestination(laneCreep.lockOnEnemy.transform.position);
#endif
					
                    
                }	
                else {
                    laneCreep.SwitchState(CreepState.ATTACKING);
                    return;
                }
            }
            else {
                laneCreep.SwitchState(CreepState.RETURNING);
                return;
            }
        }

        public override void OnExit() {
            laneCreep.navAgent.Stop();
        }
    }

    [Serializable]
    public class CreepStateReturn : CreepStateBase {

        public Vector3 returnTarget = new Vector3(0, -100, 0);

        public CreepStateReturn(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.RETURNING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goWalk", 0f);

            float nearestSqrtDist = float.MaxValue;
            Vector3[] corners = laneCreep.mainNavPath.corners;
            for (int i = 0; i < corners.Length - 1; i++) {
                Vector3 laneDir = (corners[i + 1] - corners[i]).normalized;
                Vector3 creepPos = laneCreep.transform.position - corners[i];
                Vector3 creepPosRev = laneCreep.transform.position - corners[i + 1];
                float dot1 = Vector3.Dot(creepPos, laneDir);
                float dot2 = Vector3.Dot(creepPosRev, -laneDir);
                if (Vector3.Dot(creepPos, laneDir) > 0 && Vector3.Dot(creepPosRev, -laneDir) > 0) {
                    Vector3 pos = Vector3.Dot(creepPos, laneDir) * laneDir + corners[i];
                    float sqrtDist = Vector3.SqrMagnitude(laneCreep.transform.position - pos);
                    if (sqrtDist < nearestSqrtDist) {
                        nearestSqrtDist = sqrtDist;
                        returnTarget = pos;
                    }
                }
            }

            if (returnTarget == new Vector3(0, -100, 0))
                returnTarget = corners[corners.Length - 1];
            laneCreep.navAgent.Resume();
            laneCreep.navAgent.SetDestination(returnTarget);
            Vector3 target = laneCreep.m_navAgent.steeringTarget;
            target.y = 0;
            laneCreep.transform.LookAt(target);
        }

        public override void OnUpdate() {
            Vector3 target = laneCreep.m_navAgent.steeringTarget;
            target.y = 0;
            laneCreep.transform.LookAt(target);
            /*
            foreach (var enemy in laneCreep.enemyPlayers) {
                // if enemy in sight, start chasing it
                if (Vector3.SqrMagnitude(laneCreep.transform.position - enemy.transform.position) < laneCreep.detectRadius * laneCreep.detectRadius) {
                    laneCreep.lockOnEnemy = enemy.gameObject;
                    laneCreep.SwitchState(CreepState.CHASING);
                    return;
                }
            }
            */

            if (true) {
                if (Vector3.SqrMagnitude(returnTarget - laneCreep.transform.position) > 0.01) {
//                     Vector3 targetPos = returnTarget;
//                     Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
//                     direction.y = 0;
// 
//                     laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
//                     laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
                }
                else {
                    laneCreep.SwitchState(CreepState.MOVING);
                    return;
                }
            }
            else {
                // what if the target has been destroyed?
                laneCreep.SwitchState(CreepState.IDLE);
                return;
            }
        }

        public override void OnExit() {
            laneCreep.navAgent.Stop();
        }
    }

    [Serializable]
    public class CreepStateCapture : CreepStateBase {
		private bool midCheck = false;

        public float attackIntervial = 2;
        private float lastAttackTime = -2;

        public CreepStateCapture(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.CAPTURING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            lastAttackTime = -2;
            startTime = Time.time;

            Vector3 lookDir = laneCreep.target.position - laneCreep.transform.position;
            lookDir.y = 0;
            laneCreep.transform.rotation = Quaternion.LookRotation(lookDir); // maybe use a slerp to limit angular speed

            if (laneCreep.targetSparkPoint.owner != laneCreep.owner) {
                if (laneCreep.targetSparkPoint.owner == -1) {
                    PlayerManager.Instance.photonView.RPC("RPC_setSparkPointCapture", PhotonTargets.All, laneCreep.target.name, laneCreep.playerName, laneCreep.owner, true, 1.0f / (laneCreep.captureTimePerWave * CreepManager.Instance.m_waveCount + 0.0001f));
                    laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goCapture", 0f);
                }
            }
        }

        public override void OnUpdate() {

			if (laneCreep.targetSparkPoint.owner == laneCreep.owner || laneCreep.targetSparkPoint.sparkPointState == SparkPoint.SparkPointState.DESTROYED) {
                if (laneCreep.targetSparkPoint.m_defaultNextSparkPoint == null) {
                    CreepManager.Instance.creepDict[laneCreep.source.gameObject].Remove(laneCreep); // should sync on server
                    PhotonNetwork.Destroy(laneCreep.gameObject);
                }
                else if (!midCheck) {
					midCheck = true;
					if (laneCreep.onAlternatePath && (laneCreep.NotLastUsed(PrevPath.ALTERNATE))) {
						//Currently in the process of following an alternate path, so bias towards that (if we didn't use it when we last visited this sparkpoint).
						laneCreep.target = laneCreep.targetSparkPoint.m_alternateNextSparkPoint.transform;
						laneCreep.m_previousPaths[laneCreep.targetSparkPoint.name] = PrevPath.ALTERNATE;
						laneCreep.onAlternatePath = true;
					} else if (laneCreep.NotLastUsed(PrevPath.SELECTED) && laneCreep.targetSparkPoint.m_selectedNextSparkPoint != null) {
						//Lane creep either has not visited the sparkpoint or did not use the selected path the last time it visited,
						//and a selected path exists.
						//Therefore, use the selected path (as determined by the team's dispatch).
						laneCreep.target = laneCreep.targetSparkPoint.m_selectedNextSparkPoint.transform;
						laneCreep.m_previousPaths[laneCreep.targetSparkPoint.name] = PrevPath.SELECTED;
						laneCreep.onAlternatePath = false;
					} else if (laneCreep.NotLastUsed(PrevPath.DEFAULT) && (laneCreep.DefaultNotSelected())) {
						//Lane creep either has not visited the sparkpoint or did not use the default path the last time it visited.
						//Additionally, the default path is not equivalent to the selected path.
						//Therefore, use the default path.
						laneCreep.target = laneCreep.targetSparkPoint.m_defaultNextSparkPoint.transform;
						laneCreep.m_previousPaths[laneCreep.targetSparkPoint.name] = PrevPath.DEFAULT;
						laneCreep.onAlternatePath = false;
					} else {
						//Lane creep must use the alternate path.
						laneCreep.target = laneCreep.targetSparkPoint.m_alternateNextSparkPoint.transform;
						laneCreep.m_previousPaths[laneCreep.targetSparkPoint.name] = PrevPath.ALTERNATE;
						laneCreep.onAlternatePath = true;
					}
					laneCreep.targetSparkPoint = laneCreep.target.GetComponent<SparkPoint>();
					midCheck = false;
                    laneCreep.SwitchState(CreepState.MOVING);
                    return;
                }
            }
            else {
                if (laneCreep.targetSparkPoint.sparkPointState == SparkPoint.SparkPointState.CAPTURED && Time.time - lastAttackTime > attackIntervial) {
                    laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goAttack", 0f);
                    lastAttackTime = Time.time;
                    laneCreep.targetSparkPoint.receiveAttack(laneCreep.m_basicHit, laneCreep.transform);
                }
            }
        }

        public override void OnExit() {
            // TODO: remove capturing speed from current sparkpoint target
        }
    }

    [Serializable]
    public class CreepStateDead : CreepStateBase {
        public float corpseRemainTime = 3f;

        public CreepStateDead(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.DEAD;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBuffered, (int)LaneCreep.AnimatorType.TRIGGER, "goDead", 0f);
			//Give XP to the Killer and the teammates of the Killer
			Transform lastHit = laneCreep.lastAttacker;
			if (lastHit != null && lastHit.GetComponent<Player>() != null) 
			{
				Player attacker = lastHit.GetComponent<Player>();
				int teamXP = attacker.team;	
				
				Vector3 position = laneCreep.transform.position;
				Collider[] objectsAroundMe = Physics.OverlapSphere(position, laneCreep.m_XPradius);
				Collider temp;
				for (int i = 0; i < objectsAroundMe.Length; i++)
				{
					temp = objectsAroundMe[i];
					if (temp.CompareTag("Player"))
					{	//Only give XP to players on the same team as the killing player
						if (temp.GetComponent<Player>().team == teamXP)
							temp.GetComponent<Player>().GetXP(laneCreep.m_XP);
					}
					
				}

			}
            if (laneCreep.targetSparkPoint.owner == -1) {
                // TODO: if it is killed before capturing, do not do the following line
                //laneCreep.targetSparkPoint.capturersInfo[laneCreep.owner].energyInjectionSpeed -= laneCreep.captureSpeed;
            }
			laneCreep.m_isDead = true;
        }

        public override void OnUpdate() {
            if (Time.time - startTime >= corpseRemainTime) {
                PhotonNetwork.Destroy(laneCreep.gameObject);
            }
        }

        public override void OnExit() {
			laneCreep.m_isDead = false;
        }
    }
}

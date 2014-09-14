using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LaneCreep : UnitObject {
    public enum CreepState {
        IDLE,
        MOVING,
        CHASING,
        RETURNING,
        ATTACKING,
        ATTACKED,
        CAPTURING,
        DEAD,
        DEFAULT
    }
    public enum AnimatorType {
        BOOL = 1,
        INT = 2,
        FLOAT = 3,
        TRIGGER = 4
    }

    #region CREEP_ATTRIBUTE
    public int owner;
    public float maxSpeed = 5.0f;
	public string playerName;
    public float detectRadius = 10;
    public float attackRadius = 2;
    public float captureSpeed = 0.05f;
    public Transform target;
    public SparkPoint targetSparkPoint;
    public Transform source;
    public GameObject lockOnEnemy;
    public CreepState curState;
    public Vector3 spreadDir;
    #endregion

    #region CREEP_STATE
    public CreepStateIdle creepStateIdle;
    public CreepStateMove creepStateMove;
    public CreepStateAttack creepStateAttack;
    public CreepStateChase creepStateChase;
    public CreepStateReturn creepStateReturn;
    public CreepStateCapture creepStateCapture;
    public CreepStateDead creepStateDead;

    private CreepStateBase creepState;
    #endregion
    
    private GameObject sparkPointGroup;
    private Vector3 correctCreepPos;
    private Quaternion correctCreepRot;
    private Animator anim;
    

    private bool appliedInitialUpdate = false;

    // store the enemy so we do not search them during game
    // in case of a player die or drop, could just move it out of vision and not destroy it in case of null reference
    public List<Player> enemyPlayers = new List<Player>();

	// Use this for initialization
	void Awake () {
        // initialize all states
        creepStateIdle = new CreepStateIdle(this);
        creepStateMove = new CreepStateMove(this);
        creepStateAttack = new CreepStateAttack(this);
        creepStateChase = new CreepStateChase(this);
        creepStateReturn = new CreepStateReturn(this);
        creepStateCapture = new CreepStateCapture(this);
        creepStateDead = new CreepStateDead(this);

        // initialize Findable stuff
		sparkPointGroup = GameObject.Find("SparkPoints");
        SwitchState(CreepState.IDLE);
        target = GameObject.Find((string)photonView.instantiationData[0]).transform;
        targetSparkPoint = target.GetComponent<SparkPoint>();
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        // initialize creep property passed in by photon network instantiation 
        owner = (int)photonView.instantiationData[1];
        playerName = (string)photonView.instantiationData[2];

        anim = GetComponent<Animator>();
        GetComponentInChildren<SkinnedMeshRenderer>().material.color 
            = new Color((float)photonView.instantiationData[3],
                        (float)photonView.instantiationData[4],
                        (float)photonView.instantiationData[5],
                        (float)photonView.instantiationData[6]);
        source = GameObject.Find((string)photonView.instantiationData[7]).transform;
        spreadDir = (Vector3)photonView.instantiationData[8];
        // initialize enemy list
        foreach (var p in allPlayers) {
            Player playerScript = p.GetComponent<Player>();
            if (playerScript.team != owner) {
                enemyPlayers.Add(playerScript);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        // if I'm not in control, sync position from network
        if (!PhotonNetwork.isMasterClient) {
            transform.position = Vector3.Lerp(transform.position, this.correctCreepPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctCreepRot, Time.deltaTime * 5);
        }
        // otherwise process current state's update
        else {
            if (target.GetComponent<SparkPoint>().owner == owner && curState != CreepState.DEAD) {
                SwitchState(CreepState.DEAD);
                return;
            }
            if (creepState != null)
                creepState.OnUpdate();
        }
	}

    /// <summary>
    /// switch to another state
    /// </summary>
    /// <param name="toState">destination state</param>
    private void SwitchState(CreepState toState) {
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
        }
        creepState.OnEnter();
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
            stream.SendNext(rigidbody.velocity);
        }
        else {
            correctCreepPos = (Vector3)stream.ReceiveNext();
            correctCreepRot = (Quaternion)stream.ReceiveNext();
            rigidbody.velocity = (Vector3)stream.ReceiveNext();

            if (!appliedInitialUpdate) {
                appliedInitialUpdate = true;
                transform.position = correctCreepPos;
                transform.rotation = correctCreepRot;
                rigidbody.velocity = Vector3.zero;
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
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)LaneCreep.AnimatorType.TRIGGER, "goIdle", 0f);
        }

        public override void OnUpdate() {
            if (Vector3.SqrMagnitude(laneCreep.target.position - laneCreep.transform.position) > 2.0) {
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
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)LaneCreep.AnimatorType.TRIGGER, "goWalk", 0f);
        }

        public override void OnUpdate() {
            foreach (var enemy in laneCreep.enemyPlayers) {
                // if enemy in sight, start chasing it
                if (Vector3.SqrMagnitude(laneCreep.transform.position - enemy.transform.position) < laneCreep.detectRadius * laneCreep.detectRadius) {
                    laneCreep.lockOnEnemy = enemy.gameObject;
                    laneCreep.SwitchState(CreepState.CHASING);
                    return;
                }
            }

            if (laneCreep.target != null) {
                // target not reached, continue approaching
                Vector3 distVector = laneCreep.target.position + laneCreep.spreadDir * 2.0f - laneCreep.transform.position;
                distVector.y = 0;
                if (Vector3.SqrMagnitude(distVector) > 0.01) {
                    Debug.Log(laneCreep.target.position + laneCreep.spreadDir * 2.0f - laneCreep.transform.position);
                    Vector3 targetPos = laneCreep.target.position + laneCreep.spreadDir * 2.0f;
                	Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
                    direction.y = 0;

                	laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
					laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
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
                    laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)LaneCreep.AnimatorType.TRIGGER, "goAttack", 0f);
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

        public CreepStateChase(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.CHASING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                //laneCreep.anim.SetTrigger("goWalk");
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)LaneCreep.AnimatorType.TRIGGER, "goWalk", 0f);
            deviatePosition = laneCreep.transform.position;
        }

        public override void OnUpdate() {
            float sqrDistance = Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position);
            float sqrChasingDist = Vector3.SqrMagnitude(laneCreep.transform.position - deviatePosition);
            // if has found enemy && in chasing distance && in chasing radius
            if (laneCreep.lockOnEnemy && sqrChasingDist < chasingDistance * chasingDistance && sqrDistance < laneCreep.detectRadius * laneCreep.detectRadius) {
                // still too far away to attack
                if (Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position) > laneCreep.attackRadius * laneCreep.attackRadius) {

                    Vector3 targetPos = laneCreep.lockOnEnemy.transform.position;
                    Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
                    direction.y = 0;

                    laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
                    laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
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

        }
    }

    [Serializable]
    public class CreepStateReturn : CreepStateBase {

        Vector3 returnTarget;

        public CreepStateReturn(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.RETURNING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                //laneCreep.anim.SetTrigger("goWalk");
                laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)LaneCreep.AnimatorType.TRIGGER, "goWalk", 0f);

            Vector3 laneDir = (laneCreep.target.position - laneCreep.source.position).normalized;
            Vector3 creepPos = laneCreep.transform.position - laneCreep.source.position;
            returnTarget = Vector3.Dot(creepPos, laneDir) * laneDir + laneCreep.source.position;
        }

        public override void OnUpdate() {
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
                if (Vector3.SqrMagnitude(returnTarget - laneCreep.transform.position) > 2) {
                    Vector3 targetPos = returnTarget;
                    Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
                    direction.y = 0;

                    laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
                    laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
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

        }
    }

    [Serializable]
    public class CreepStateCapture : CreepStateBase {
        public float attackIntervial = 2;
        private float lastAttackTime = -2;

        public CreepStateCapture(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.CAPTURING;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)LaneCreep.AnimatorType.TRIGGER, "goCapture", 0f);

            Vector3 lookDir = laneCreep.target.position - laneCreep.transform.position;
            lookDir.y = 0;
            laneCreep.transform.rotation = Quaternion.LookRotation(lookDir); // maybe use a slerp to limit angular speed

            if (laneCreep.targetSparkPoint.owner != laneCreep.owner) {
                if (laneCreep.targetSparkPoint.owner == -1)
                    PlayerManager.Instance.photonView.RPC("RPC_setSparkPointCapture", PhotonTargets.All, laneCreep.target.name, laneCreep.playerName, laneCreep.owner, true, laneCreep.captureSpeed);
                else
                    PlayerManager.Instance.photonView.RPC("RPC_setSparkPointDestroy", PhotonTargets.All, laneCreep.target.name, laneCreep.playerName, laneCreep.owner);
            }
        }

        public override void OnUpdate() {
            if (laneCreep.targetSparkPoint.owner == laneCreep.owner) {
                CreepManager.Instance.creepDict[laneCreep.source.gameObject].Remove(laneCreep); // should sync on server
                PhotonNetwork.Destroy(laneCreep.gameObject);
            }      
        }

        public override void OnExit() {

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
            laneCreep.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)LaneCreep.AnimatorType.TRIGGER, "goDead", 0f);

            if (laneCreep.targetSparkPoint.owner == -1) {
                laneCreep.targetSparkPoint.capturersInfo[laneCreep.owner].energyInjectionSpeed -= laneCreep.captureSpeed;
            }
        }

        public override void OnUpdate() {
            if (Time.time - startTime >= corpseRemainTime) {
                CreepManager.Instance.creepDict[laneCreep.source.gameObject].Remove(laneCreep);
                PhotonNetwork.Destroy(laneCreep.gameObject);
            }
        }

        public override void OnExit() {

        }
    }
}

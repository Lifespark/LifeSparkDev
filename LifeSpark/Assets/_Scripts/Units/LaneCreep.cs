using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LaneCreep : UnitObject {

    public enum CreepState {
        Idle,
        Moving,
        Chasing,
        Attacking,
        Attacked,
        Dead,
        Default
    }

    public int owner;

    public float maxSpeed = 5.0f;
	public string playerName;
    public float detectRadius = 10;
    public float attackRadius = 2;
    public Transform target;
    public CreepManager creepManager;
    public PlayerManager playerManager;
    public GameObject lockOnEnemy;
    public CreepState curState;

    public CreepStateIdle creepStateIdle;
    public CreepStateMove creepStateMove;
    public CreepStateAttack creepStateAttack;
    public CreepStateChase creepStateChase;

    private CreepStateBase creepState;
    
	private GameObject sparkPointGroup;
    private Vector3 correctCreepPos;
    private Quaternion correctCreepRot;
    private Animator anim;

    private bool appliedInitialUpdate = false;
    private bool syncedInitialState = false;

    // store the enemy so we do not search them during game
    public List<Player> enemyPlayers = new List<Player>();

	// Use this for initialization
	void Awake () {
        creepStateIdle = new CreepStateIdle(this);
        creepStateMove = new CreepStateMove(this);
        creepStateAttack = new CreepStateAttack(this);
        creepStateChase = new CreepStateChase(this);

        playerManager = GameObject.FindWithTag("Ground").GetComponent<PlayerManager>();

		sparkPointGroup = GameObject.Find("SparkPoints");
        creepState = creepStateIdle;

        target = GameObject.Find((string)photonView.instantiationData[0]).transform;

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        owner = (int)photonView.instantiationData[1];
        playerName = (string)photonView.instantiationData[2];
        /*
        renderer.material.color = new Color ((float)photonView.instantiationData[3], 
                                             (float)photonView.instantiationData[4], 
                                             (float)photonView.instantiationData[5], 
                                             (float)photonView.instantiationData[6]);
        */
        anim = GetComponent<Animator>();
        GetComponentInChildren<SkinnedMeshRenderer>().material.color 
            = new Color((float)photonView.instantiationData[3],
                        (float)photonView.instantiationData[4],
                        (float)photonView.instantiationData[5],
                        (float)photonView.instantiationData[6]);


        foreach (var p in allPlayers) {
            Player playerScript = p.GetComponent<Player>();
            if (playerScript.team != owner) {
                enemyPlayers.Add(playerScript);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine) {
            transform.position = Vector3.Lerp(transform.position, this.correctCreepPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctCreepRot, Time.deltaTime * 5);
        }
        else {
            if (creepState != null)
                creepState.OnUpdate();
        }
	}

    private void SwitchState(CreepState toState) {
        if (creepState.State == toState)
            return;
        curState = toState;
        if (creepState != null)
            creepState.OnExit();

        switch (toState) {
            case CreepState.Idle:
                creepState = creepStateIdle;
                break;
            case CreepState.Moving:
                creepState = creepStateMove;
                break;
            case CreepState.Attacking:
                creepState = creepStateAttack;
                break;
            case CreepState.Chasing:
                creepState = creepStateChase;
                break;
        }

        creepState.OnEnter();
    }

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

    [Serializable]
    public abstract class CreepStateBase {
        protected CreepState state;
        protected LaneCreep laneCreep;
        protected float startTime;
        
        public CreepState State { get { return state; } }

        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();

        public CreepStateBase() { startTime = Time.time; }
    }

    [Serializable]
    public class CreepStateIdle : CreepStateBase {
        public CreepStateIdle(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.Idle;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                laneCreep.anim.SetTrigger("goIdle");
        }

        public override void OnUpdate() {
            if (Vector3.SqrMagnitude(laneCreep.target.position - laneCreep.transform.position) > 2.0) {
				laneCreep.SwitchState(CreepState.Moving);
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
            state = CreepState.Moving;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                laneCreep.anim.SetTrigger("goWalk");
        }

        public override void OnUpdate() {

            foreach (var enemy in laneCreep.enemyPlayers) {
                if (Vector3.SqrMagnitude(laneCreep.transform.position - enemy.transform.position) < laneCreep.detectRadius * laneCreep.detectRadius) {
                    Debug.Log("found enemy!");
                    laneCreep.lockOnEnemy = enemy.gameObject;
                    laneCreep.SwitchState(CreepState.Chasing);
                    return;
                }
            }

            if (laneCreep.target != null) {
				if (Vector3.SqrMagnitude(laneCreep.target.position - laneCreep.transform.position) > 2.0) {
					//laneCreep.SwitchState(CreepState.Moving);
                    //return;
				
                	Vector3 targetPos = laneCreep.target.position;
                	Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
                    direction.y = 0;

                	laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
					laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
				}
				else {
					if (Vector3.SqrMagnitude(laneCreep.target.position - laneCreep.transform.position) <= 2.0) {
						//laneCreep.target.GetComponent<SparkPoint>().SetSparkPointCapture(laneCreep.playerName, laneCreep.owner, true);
                        laneCreep.playerManager.photonView.RPC("RPC_setSparkPointCapture", PhotonTargets.All, laneCreep.target.name, laneCreep.playerName, laneCreep.owner, true);
                        //laneCreep.creepManager.creepDict[laneCreep.target.gameObject].Remove(laneCreep); // should sync on server
						//Destroy(laneCreep.gameObject);
                        PhotonNetwork.Destroy(laneCreep.photonView);
					}
				}
			}
            else {
                // what if the target has been destroyed?
                laneCreep.SwitchState(CreepState.Idle);
                return;
            }
        }

        public override void OnExit() {

        }

        [RPC]
        void RPC_setSparkPointCapture(string sparkPointName, string playerName, int team, bool b) {
            GameObject tempSparkPoint = GameObject.Find("SparkPoints/" + sparkPointName);
            tempSparkPoint.GetComponent<SparkPoint>().SetSparkPointCapture(playerName, team, b);
        }
    }

    [Serializable]
    public class CreepStateAttack : CreepStateBase {
        public float attackIntervial = 2;

        private float lastAttackTime = -2;

        public CreepStateAttack(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.Attacking;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            
        }

        public override void OnUpdate() {
            float sqrDistance = Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position);

            if ( sqrDistance > laneCreep.attackRadius * laneCreep.attackRadius ) {
                laneCreep.SwitchState(CreepState.Chasing);
                return;
            }

            if (Time.time - lastAttackTime > attackIntervial) {
                if (laneCreep.anim)
                    laneCreep.anim.SetTrigger("goAttack");
            }
        }

        public override void OnExit() {

        }
    }

    [Serializable]
    public class CreepStateChase : CreepStateBase {
        public float chasingDistance = 10;

        private Vector3 deviatePosition;

        public CreepStateChase(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.Chasing;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
            if (laneCreep.anim)
                laneCreep.anim.SetTrigger("goWalk");
            deviatePosition = laneCreep.transform.position;
        }

        public override void OnUpdate() {
            float sqrDistance = Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position);
            if (laneCreep.lockOnEnemy && sqrDistance < chasingDistance * chasingDistance && sqrDistance < laneCreep.detectRadius * laneCreep.detectRadius) {
                if (Vector3.SqrMagnitude(laneCreep.lockOnEnemy.transform.position - laneCreep.transform.position) > laneCreep.attackRadius * laneCreep.attackRadius) {

                    Vector3 targetPos = laneCreep.lockOnEnemy.transform.position;
                    Vector3 direction = (targetPos - laneCreep.transform.position).normalized;
                    direction.y = 0;

                    laneCreep.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
                    laneCreep.transform.position += direction * laneCreep.maxSpeed * Time.deltaTime;
                }
                else {
                    laneCreep.SwitchState(CreepState.Attacking);
                    return;
                }
            }
            else {
                // no!!! should return to deviate position then resume moving? or not?
                laneCreep.SwitchState(CreepState.Moving);
                return;
            }
        }

        public override void OnExit() {

        }
    }
}

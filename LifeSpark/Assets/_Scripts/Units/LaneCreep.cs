using UnityEngine;
using System.Collections;

public class LaneCreep : UnitObject {

    public enum CreepState {
        Idle,
        Moving,
        Attacking,
        Attacked,
        Dead,
        Default
    }

    public int owner;

    public float maxSpeed = 5.0f;
	public string playerName;
    public Transform target;
    public CreepManager creepManager;
    public PlayerManager playerManager;

    private CreepStateBase creepState;
    private CreepStateIdle creepStateIdle;
    private CreepStateMove creepStateMove;

	private GameObject sparkPointGroup;
    private Vector3 correctCreepPos;
    private Quaternion correctCreepRot;

    private bool appliedInitialUpdate = false;
    private bool syncedInitialState = false;

	// Use this for initialization
	void Awake () {
        creepStateIdle = new CreepStateIdle(this);
        creepStateMove = new CreepStateMove(this);

        playerManager = GameObject.FindWithTag("Ground").GetComponent<PlayerManager>();

		sparkPointGroup = GameObject.Find("SparkPoints");
        creepState = creepStateIdle;

        target = GameObject.Find((string)photonView.instantiationData[0]).transform;
        owner = (int)photonView.instantiationData[1];
        playerName = (string)photonView.instantiationData[2];
        renderer.material.color = new Color ((float)photonView.instantiationData[3], 
                                             (float)photonView.instantiationData[4], 
                                             (float)photonView.instantiationData[5], 
                                             (float)photonView.instantiationData[6]);

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

        if (creepState != null)
            creepState.OnExit();

        switch (toState) {
            case CreepState.Idle:
                creepState = creepStateIdle;
                break;
            case CreepState.Moving:
                creepState = creepStateMove;
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

    abstract class CreepStateBase {
        protected CreepState state;
        protected LaneCreep laneCreep;
        protected float startTime;
        
        public CreepState State { get { return state; } }

        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();

        public CreepStateBase() { startTime = Time.time; }
    }

    class CreepStateIdle : CreepStateBase {
        public CreepStateIdle(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.Idle;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
        }

        public override void OnUpdate() {
            if (Vector3.SqrMagnitude(laneCreep.target.position - laneCreep.transform.position) > 2.0) {
				laneCreep.SwitchState(CreepState.Moving);
			}
        }

        public override void OnExit() {

        }
    }

    class CreepStateMove : CreepStateBase {
        public CreepStateMove(LaneCreep pLaneCreep) {
            startTime = Time.time;
            state = CreepState.Moving;
            laneCreep = pLaneCreep;
        }

        public override void OnEnter() {
            startTime = Time.time;
        }

        public override void OnUpdate() {
            if (laneCreep.target != null) {
				if (Vector3.SqrMagnitude(laneCreep.target.position - laneCreep.transform.position) > 2.0) {
					laneCreep.SwitchState(CreepState.Moving);
				
                	Vector3 targetPos = laneCreep.target.position;
                	Vector3 direction = (targetPos - laneCreep.transform.position).normalized;

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
}

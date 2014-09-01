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

    public float maxSpeed = 5.0f;
	public string playerName;
    public Transform target;
    public CreepManager creepManager;

    private CreepStateBase creepState;
    private CreepStateIdle creepStateIdle;
    private CreepStateMove creepStateMove;

	private GameObject sparkPointGroup;

    Vector3 correctPlayerPos;
    Quaternion correctPlayerRot;

    bool appliedInitialUpdate = false;
    bool syncedInitialState = false;

    public int owner;

	// Use this for initialization
	void Awake () {
        creepStateIdle = new CreepStateIdle(this);
        creepStateMove = new CreepStateMove(this);

		sparkPointGroup = GameObject.Find("SparkPoints");
        creepState = creepStateIdle;
	}
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine) {
            transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
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
            //We own this player: send the others our data
            // stream.SendNext((int)controllerScript._characterState);
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rigidbody.velocity);
            if (!syncedInitialState) {
                //stream.SendNext(target);
                stream.SendNext(owner);
                stream.SendNext(playerName);
                stream.SendNext(renderer.material.color.r);
                stream.SendNext(renderer.material.color.g);
                stream.SendNext(renderer.material.color.b);
                stream.SendNext(renderer.material.color.a);
                syncedInitialState = true;
            }
        }
        else {
            //Network player, receive data
            //controllerScript._characterState = (CharacterState)(int)stream.ReceiveNext();
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
            rigidbody.velocity = (Vector3)stream.ReceiveNext();

            if (!appliedInitialUpdate) {
                appliedInitialUpdate = true;
                transform.position = correctPlayerPos;
                transform.rotation = correctPlayerRot;
                rigidbody.velocity = Vector3.zero;
            }

            if (!syncedInitialState) {
                syncedInitialState = true;
                //target = (Transform)stream.ReceiveNext();
                owner = (int)stream.ReceiveNext();
                playerName = (string)stream.ReceiveNext();
                float r = (float)stream.ReceiveNext();
                float g = (float)stream.ReceiveNext();
                float b = (float)stream.ReceiveNext();
                float a = (float)stream.ReceiveNext();
                renderer.material.color = new Color(r, g, b, a);
                syncedInitialState = true;
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
						laneCreep.target.GetComponent<SparkPoint>().SetSparkPointCapture(laneCreep.playerName, laneCreep.owner, true);
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
    }
}

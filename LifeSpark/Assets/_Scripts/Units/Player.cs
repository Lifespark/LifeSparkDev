using UnityEngine;
using System.Collections;

public class Player : UnitObject {
	
	public int team;
	public int playerID;
	public string playerName;
	////
	public Vector3 target;
	public string targetName;
	public float speed;
	
	Vector3 tempPosition;
	Vector3 tempValue;
	float totalSqrLength;
	
	enum PlayerState {
		Idle,
		Moving,
		Capturing,
		Attacking
	};
	PlayerState playerState;
	
	// Use this for initialization
	void Start () {
		speed = 5;
		target = this.transform.position;
		target.y = 0;
		playerState = PlayerState.Idle;
		this.unitHealth = 50;
		this.baseAttack = 5;

        // initialize line renderer for drawing path to false
        GetComponent<LineRenderer>().enabled = false;

	}
	
	// Update is called once per frame
	void Update () {
		movePlayer ();
        // Draw Path
        if (GetComponent<NavMeshAgent>().hasPath)
        {
            DrawPath(GetComponent<NavMeshAgent>().path);
            GetComponent<LineRenderer>().enabled = true;
        }
        else 
        {
            GetComponent<LineRenderer>().enabled = false;
        }
	}
	
	void OnGUI () {
		if (this.GetComponent<PlayerInput> ().isMine) {
			GUI.TextArea (new Rect (10, 10, 200, 20), "Position:" + this.transform.position.x + ":" + this.transform.position.y + ":" + this.transform.position.z);
			GUI.TextArea (new Rect (10, 30, 200, 20), "Target:" + target.x + ":0:" + target.z);
			//
		}
	}
	
	void movePlayer () {
		switch (playerState) {
		case PlayerState.Idle:
			break;
		case PlayerState.Moving:
			tempPosition = this.transform.position;
			tempPosition.y = 0;
			if (!tempPosition.Equals(target)) {
				tempValue = target - tempPosition;
				totalSqrLength = tempValue.sqrMagnitude;
				tempValue = Vector3.Normalize(tempValue) * speed * Time.deltaTime;
				if (totalSqrLength <= 10.0f /*&& targetName.Contains("SparkPoint")*/) {
					
					if (targetName.Contains("SparkPoint"))
					{
						GameObject.Find("Ground").GetPhotonView().RPC("RPC_setSparkPointCapture",
						                                              PhotonTargets.All,
						                                              targetName,
						                                              this.name,
						                                              team,
						                                              true);
						playerState = PlayerState.Capturing;
					}
					else if (targetName.Contains("Player"))
					{
						GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerAttack",
						                                              PhotonTargets.All,
						                                              targetName,
						                                              this.name,
						                                              true
						                                              );
						playerState = PlayerState.Attacking;
						
					}
				}
				else if(tempValue.sqrMagnitude > totalSqrLength) {
					this.transform.position.Set(target.x, this.transform.position.y, target.z);
					playerState = PlayerState.Idle;
				}
				else {
					//this.transform.position = this.transform.position + tempValue;
				}
			}
			break;
		case PlayerState.Capturing:
			break;
		case PlayerState.Attacking:
			tempPosition = this.transform.position;
			tempPosition.y = 0;
			if (!tempPosition.Equals(target)) {
				tempValue = target - tempPosition;
				totalSqrLength = tempValue.sqrMagnitude;
				if (totalSqrLength > 10.0f ) {//Our target ran away/became distant,we can no longer deal DPS
					GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
					                                                 PhotonTargets.All,
					                                                 this.targetName,
					                                                 this.name,
					                                                 false);
					playerState = PlayerState.Moving;//Chase the target
					
				}
				
				
				
			}
			break;
		}
	}
	
	public void UpdateTarget(Vector3 target, string targetName) {
		if (playerState == PlayerState.Capturing) {
			GameObject.Find("Ground").GetPhotonView().RPC("RPC_setSparkPointCapture",
			                                              PhotonTargets.All,
			                                              this.targetName,
			                                              this.name,
			                                              team,
			                                              false);
		}
		else if (playerState == PlayerState.Attacking) {
			GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
			                                                 PhotonTargets.All,
			                                                 this.targetName,
			                                                 this.name,
			                                                 false);
		}
		this.target = target;
		this.targetName = targetName;
		playerState = PlayerState.Moving;
	}
	
	public void CapturedObjective() {
		playerState = PlayerState.Idle;
	}
	
	public int GetTeam() {
		return team;
	}


    public void DrawPath(NavMeshPath path)
    {
        LineRenderer LR = GetComponent<LineRenderer>();
        

        LR.SetVertexCount(path.corners.Length);

        int i = 0;
        foreach (Vector3 v in path.corners)
        {
            LR.SetPosition(i, v);
            i++;
        }
     

    }
}

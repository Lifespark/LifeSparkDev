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
		Capturing
	};
	PlayerState playerState;
	
	// Use this for initialization
	void Start () {
		speed = 5;
		target = this.transform.position;
		target.y = 0;
		playerState = PlayerState.Idle;
	}
	
	// Update is called once per frame
	void Update () {
		movePlayer ();
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
				if (totalSqrLength <= 10.0f && targetName.Contains("SparkPoint")) {
					GameObject.Find("Ground").GetPhotonView().RPC("RPC_setSparkPointCapture",
					                                              PhotonTargets.All,
					                                              targetName,
					                                              this.name,
					                                              team,
					                                              true);
					playerState = PlayerState.Capturing;
				}
				else if(tempValue.sqrMagnitude > totalSqrLength) {
					this.transform.position.Set(target.x, this.transform.position.y, target.z);
					playerState = PlayerState.Idle;
				}
				else {
					this.transform.position = this.transform.position + tempValue;
				}
			}
			break;
		case PlayerState.Capturing:
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
}

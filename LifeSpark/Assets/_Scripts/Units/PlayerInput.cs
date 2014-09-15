using UnityEngine;
using System.Collections;

public class PlayerInput : UnitMovement {
	
	Vector3 tempHit;
	public bool isMine;

	public enum TargetType {
		Position,
		LineAttack,
        TargetAreaAttack,
		SelfAreaAttack
	};

	public TargetType targetType;

	// Use this for initialization
	void Start () {
		tempHit = Vector3.zero;
		isMine = false;
		targetType = TargetType.Position;
	}
	
	// Update is called once per frame
	void Update () {
		if (isMine && this.GetComponent<Player>().GetState() != Player.PlayerState.Dead) {
			KeyBoardMouseInput ();
		}
	}
	
	
	
	// PC input
	//Uses 'S' for line attack and 'A' for area attack
	//Defaults to normal movement after every click
	void KeyBoardMouseInput () {
		if (Input.GetKeyDown ("s")) {
			targetType = TargetType.LineAttack;
		}
		if (Input.GetKeyDown ("a")) {
			targetType = TargetType.TargetAreaAttack;
		}
        if (Input.GetKeyDown("d")) {
            targetType = TargetType.SelfAreaAttack;
            // Making the aoe around player a quick cast (no mouse involved.)
            GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerTarget",
                                                             PhotonTargets.All,
                                                             this.name,
                                                             this.transform.position,
                                                             this.name,
                                                             (int)targetType);
            targetType = TargetType.Position;
            return;
        }
        if (Input.GetKeyDown("f")) {
            this.GetComponent<Player>().KillPlayer();
        }
		// mouse left button down
		if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl==0) {
			Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			// check if hit, the length(1000.0f) can set to other value
			if (Physics.Raycast(cameraRay, out hit, 1000.0f)) {
				Debug.Log(hit.collider.name);

				if (targetType == TargetType.Position) {
					if (hit.collider.name.Equals("Ground")) {
						// hit.point.y = 0;
						tempHit = hit.point;
						tempHit.y = 0;
						//
						//
						//
						GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerTarget",
						                                              PhotonTargets.All,
						                                              this.name,
						                                              tempHit,
						                                              hit.collider.name,
						                                              (int)targetType);
					}
					else if (hit.collider.name.Contains("SparkPoint")) {
						if (GameObject.Find(hit.collider.name).GetComponent<SparkPoint>().GetOwner()
						    != this.GetComponent<Player>().GetTeam()) {
							tempHit = hit.point;
							tempHit.y = 0;
							GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerTarget",
							                                              PhotonTargets.All,
							                                              this.name,
							                                              tempHit,
							                                              hit.collider.name,
							                                              (int)targetType);
						}
					}
					else if (hit.collider.name.Contains ("Player")) {//Going to attack some other dude
						if (GameObject.Find (hit.collider.name).GetComponent<Player>().GetTeam()
						    != this.GetComponent<Player>().GetTeam ()){
							tempHit = hit.point;
							tempHit.y = 0;
							GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerTarget",
							                                                 PhotonTargets.All,
							                                                 this.name,
							                                                 tempHit,
							                                                 hit.collider.name,
							                                                 (int)targetType);
						}
					}
				} else {
					tempHit = hit.point;
					tempHit.y = 0;
					GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerTarget",
					                                                 PhotonTargets.All,
					                                                 this.name,
					                                                 tempHit,
					                                                 hit.collider.name,
					                                                 (int)targetType);
				}
				targetType = TargetType.Position;
			}
		}
	}
	
}

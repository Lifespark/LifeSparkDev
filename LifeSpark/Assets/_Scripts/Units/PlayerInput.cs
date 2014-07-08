using UnityEngine;
using System.Collections;

public class PlayerInput : UnitMovement {
	
	Vector3 tempHit;
	public bool isMine;
	
	// Use this for initialization
	void Start () {
		tempHit = Vector3.zero;
		isMine = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (isMine) {
			KeyBoardMouseInput ();
		}
	}
	
	
	
	// PC input
	void KeyBoardMouseInput () {
		// mouse left button down
		if (Input.GetMouseButtonDown(0)) {
			Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			// check if hit, the length(1000.0f) can set to other value
			if (Physics.Raycast(cameraRay, out hit, 1000.0f)) {
				Debug.Log(hit.collider.name);
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
					                                              hit.collider.name);
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
						                                              hit.collider.name);
					}
				}
			}
		}
	}
	
}

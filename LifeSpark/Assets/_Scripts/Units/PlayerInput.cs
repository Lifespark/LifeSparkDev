using UnityEngine;
using System.Collections;

public class PlayerInput : UnitMovement {
	public bool isMine;

	// Use this for initialization
	void Start () {
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
					Vector3 tempHit = hit.point;
					tempHit.y = 0;
					//

					//
					GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerTarget", PhotonTargets.All, this.name, tempHit);
				}
			}
		}
	}

}

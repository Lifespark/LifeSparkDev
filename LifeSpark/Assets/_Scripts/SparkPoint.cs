using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SparkPoint : LSMonoBehaviour {
	
	public SparkPoint[] _connections;

	int owner;
	enum SparkPointState {
		Free,
		Freeing,
		Capturing,
		Captured,
		Destroying,
		Destroyed
	};
	SparkPointState sparkPointState;
	List<string> capturers;
	int capturingTeam;
	int captureTimer;
	int captureTime;
	Color sparkPointColor;
	
	// Use this for initialization
	void Start () {
		owner = -1;
		sparkPointState = SparkPointState.Free;
		capturers = new List<string>();
		capturingTeam = -1;
		captureTimer = 0;
		captureTime = 200;
		sparkPointColor = new Color(0.5f,0.5f,0.5f);
		renderer.material.color = sparkPointColor;
	}
	
	// Update is called once per frame
	void Update () {
		/*foreach(SparkPoint sp in _connections) {
			Debug.DrawLine(this.transform.position, sp.transform.position, Color.red);
		}*/
		switch (sparkPointState) {
		case SparkPointState.Capturing:
			if (captureTimer == captureTime) {
				captureTimer = 0;
				sparkPointState = SparkPointState.Captured;
				//-----------------------------------------------------------------------------------------
				//// captured, check connection sparkpoint owner to build new line (MasterClient only)
				//Debug.Log(PhotonNetwork.isMasterClient);
				if(PhotonNetwork.isMasterClient){
					int tempSize = _connections.Length;
					for (int i=0; i<tempSize; i++) {
						if(_connections[i].owner == capturingTeam) {
							//// if this connection has same owner, initial new lane
							GameObject tempObject = PhotonNetwork.InstantiateSceneObject("Lane", _connections[i].transform.position, new Quaternion(), 0, null);
							Lane tempLane = tempObject.GetComponent<Lane>();
							//// set lane name
							tempObject.name = "Lane" + this.name + _connections[i].name;
							//// set lane material
							tempLane.photonView.RPC("RPC_setLaneMaterial", PhotonTargets.All, capturingTeam);
							//// set line position, location and scale
							tempLane.photonView.RPC("RPC_setInitialTransform", PhotonTargets.All, this.transform.position, _connections[i].transform.position);
						}
					}
				}
				//-----------------------------------------------------------------------------------------
				owner = capturingTeam;
				capturingTeam = -1;
				for (int i = 0; i < capturers.Count; i++) {
					GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerSparkPointCaptured",
					                                              PhotonTargets.All,
					                                              capturers[i]);
				}
				capturers.Clear();
				//Debug.Log ("Team "+owner+" has successfully captured: "+this.name);
			}
			else {
				captureTimer++;
				if (capturingTeam == 1) {
					sparkPointColor.r = 0.5f + (float)captureTimer/400;
					sparkPointColor.g = 0.5f - (float)captureTimer/400;
					sparkPointColor.b = 0.5f - (float)captureTimer/400;
				}
				else if (capturingTeam == 2) {
					sparkPointColor.r = 0.5f - (float)captureTimer/400;
					sparkPointColor.g = 0.5f - (float)captureTimer/400;
					sparkPointColor.b = 0.5f + (float)captureTimer/400;
				}
				renderer.material.color = sparkPointColor;
			}
			break;
		}
	}
	
	public int GetOwner() {
		return owner;
	}
	
	public void SetSparkPointCapture (string playerName, int team, bool b) {
		if (b) {
			if (owner == -1) {
				capturers.Add(playerName);
				capturingTeam = team;
				sparkPointState = SparkPointState.Capturing;
				//Debug.Log ("Team"+team+" "+playerName+" attempting to capture: "+this.name);
			}
		}
		else {
			//Debug.Log ("Broke the capture for "+this.name);
			capturers.Clear();
			capturingTeam = -1;
			sparkPointState = SparkPointState.Free;
			captureTimer = 0;
			sparkPointColor.r = 0.5f;
			sparkPointColor.g = 0.5f;
			sparkPointColor.b = 0.5f;
			renderer.material.color = sparkPointColor;
		}
	}
	
}
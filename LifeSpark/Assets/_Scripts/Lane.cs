using UnityEngine;
using System.Collections;

public class Lane : LSMonoBehaviour {
	public SparkPoint[] edgePoints;	// should just have 2 spark points
	public int state; // may not be used // now -1 = broken, 0 = unoccupied, 1 = team 1, 2 = team 2

	// Use this for initialization
	void Start () {
		state = 0;
	}
	
	// Update is called once per frame
	void Update () {
		//RPC_setLaneMaterial (state);
	}

	//--------------------------------------------------------------------------
	// Set color functions
	//---------------------------
	// Set Lane Color By State
	[RPC]
	void RPC_setLaneColorByState (int state) {
		this.state = state;
		// just change material's color
		if (state == -1) {
			this.renderer.material.color = Color.black;
		} else if (state == 0) {
			this.renderer.material.color = Color.white;
		} else if (state == 1) {
			this.renderer.material.color = Color.red;
		} else if (state == 2) {
			this.renderer.material.color = Color.blue;
		} else {
			this.renderer.material.color = Color.yellow;
		}
	}

	// Set Lane Material
	[RPC]
	void RPC_setLaneMaterial (int state) {
		this.state = state;
		GameObject temp = GameObject.Find ("Manager"); 
		if (state == -1) {
			this.renderer.material = temp.GetComponent<TerritoryManager>().BrokenLane;
		} else if (state == 0) {
			this.renderer.material = temp.GetComponent<TerritoryManager>().OriginalLane;
		} else if (state == 1) {
			this.renderer.material = temp.GetComponent<TerritoryManager>().Team1Lane;
		} else if (state == 2) {
			this.renderer.material = temp.GetComponent<TerritoryManager>().Team2Lane;
		} else {
			this.renderer.material = temp.GetComponent<TerritoryManager>().OriginalLane;
		}
	}
	//--------------------------------------------------------------------------
	// Set transform
	//---------------------------
	// Set all
	[RPC]
	void RPC_setInitialTransform (Vector3 edge1, Vector3 edge2) {
		this.transform.position = (edge1 + edge2 - new Vector3(0, edge1.y, 0)) / 2;
		this.transform.localScale = new Vector3(Mathf.Sqrt((edge1 - edge2).sqrMagnitude) - 1, 0.1f, 1);
		Vector3 direction = edge1 - edge2;
		float angle = Vector3.Angle (direction, this.transform.right);
		if (angle > 90) {angle = angle - (2*(angle - 90));}
		this.transform.Rotate (new Vector3 (0, angle, 0));
	}
}

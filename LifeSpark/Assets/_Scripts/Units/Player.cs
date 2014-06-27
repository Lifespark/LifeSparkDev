﻿using UnityEngine;
using System.Collections;

public class Player : UnitObject {

	public int team;
	public int playerID;
	public string playerName;
	////
	public Vector3 target; 
	public float speed;

	// Use this for initialization
	void Start () {
		speed = 5;
		target = this.transform.position;
		target.y = 0;
	}
	
	// Update is called once per frame
	void Update () {
		movePlayer ();
	}

	void OnGUI () {
		if (this.GetComponent<PlayerInput> ().isMine) {
			GUI.TextArea (new Rect (10, 10, 200, 20), "Position:" + this.transform.position.x + ":" + this.transform.position.y + ":" + this.transform.position.z);
			GUI.TextArea (new Rect (10, 30, 200, 20), "Target:" + target.x + ":0:" + target.z);
		}
	}

	void movePlayer () {
		Vector3 tempPosition = this.transform.position;
		tempPosition.y = 0;
		if (!tempPosition.Equals(target)) {
			Vector3 tempValue = target - tempPosition;
			float totalSqrLength = tempValue.sqrMagnitude;
			tempValue = Vector3.Normalize(tempValue) * speed * Time.deltaTime;
			if(tempValue.sqrMagnitude > totalSqrLength) {
				this.transform.position.Set(target.x, this.transform.position.y, target.z);
			} else {
				this.transform.position = this.transform.position + tempValue;
			}
		}
	}
}

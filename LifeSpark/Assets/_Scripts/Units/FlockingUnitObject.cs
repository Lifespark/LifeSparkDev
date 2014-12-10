using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** Wrapper class for Unit Object. Units that require flocking behavior should inherit from this */
public class FlockingUnitObject : UnitObject {

	[SerializeField] float flockingRadius = 100;
	[SerializeField] bool isLeader = false; 	// only serialized for debugging
	
	[SerializeField] float alignmentWeight = 1.0f;
	[SerializeField] float cohesionWeight = 1.0f;
	[SerializeField] float separationWeight = 1.0f;

    public override void Awake() {
        base.Awake();
    }

	// Update is called once per frame
	new protected void Update () {
		base.Update();
	}

    new void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
    }

	// Returns true if not flock leader and moved successfully. Returns false if unit was determined to be leader of the flock
	// @arg1 eligableFlockMates - array of other units that this unit can flock with (regardless of distance)
	// @arg2 maxSpeed - maximum speed (magnitude) that this unit can move
	protected bool MoveWithFlockUnlessLeader(List<FlockingUnitObject> eligibleFlockMates, float maxSpeed) {
	
		//	First, establish what flock is and determine if leader or follower
		isLeader = true;
		List<FlockingUnitObject> myFlock = new List<FlockingUnitObject>();
		for (int i = 0; i < eligibleFlockMates.Count; i++) {
			float distanceToCreep = Vector3.Distance(transform.position, eligibleFlockMates[i].transform.position);
			if (distanceToCreep < flockingRadius) {
				myFlock.Add(eligibleFlockMates[i]);
				if (eligibleFlockMates[i].IsFlockLeader()) {
					isLeader = false;
				}
			}
		}
		
		// If follower, do Flocking calculations
		if (!isLeader) {
			Vector3 alignment = ComputeAlignment(myFlock);
			Vector3 cohesion = ComputeCohesion(myFlock, transform.position);
			Vector3 separation = ComputeSeparation(myFlock, transform.position);
			
			Vector3 finalVelocity = rigidbody.velocity;
			finalVelocity.x += alignment.x * alignmentWeight + cohesion.x * cohesionWeight + separation.x * separationWeight;
			finalVelocity.z += alignment.z * alignmentWeight + cohesion.z  * cohesionWeight + separation.z * separationWeight;
			
			finalVelocity.Normalize();
			finalVelocity *= maxSpeed;
			rigidbody.velocity = finalVelocity;
			return true;
		}
		else {
			return false;
		}
	}
	
	// Returns the Alignment component used in flocking algorithm
	Vector3 ComputeAlignment(List<FlockingUnitObject> flock) {
		Vector3 v = new Vector3();
		for (int i = 0; i < flock.Count; i++) {
			v.x += flock[i].rigidbody.velocity.x;
			v.z += flock[i].rigidbody.velocity.z;
		}
		v.x /= flock.Count;
		v.z /= flock.Count;
		v.Normalize();
		return v;
	}
	
	
	// Returns the Cohesion component used in flocking algorithm
	Vector3 ComputeCohesion(List<FlockingUnitObject> flock, Vector3 myPos) {
		Vector3 v = new Vector3();
		for (int i = 0; i < flock.Count; i++) {
			v.x += flock[i].transform.position.x;
			v.z += flock[i].transform.position.z;
		}
		v.x /= flock.Count;
		v.z /= flock.Count;
		v = new Vector3(v.x - myPos.x, v.z - myPos.z);
		v.Normalize();
		return v;
	}
	
	
	// Returns the Separation component used in flocking algorithm
	Vector3 ComputeSeparation(List<FlockingUnitObject> flock, Vector3 myPos) {
		Vector3 v = new Vector3();
		for (int i = 0; i < flock.Count; i++) {
			v.x += flock[i].transform.position.x - myPos.x;
			v.z += flock[i].transform.position.z - myPos.z;
		}
		v.x /= flock.Count;
		v.z /= flock.Count;
		v.x *= -1;
		v.z *= -1;
		v.Normalize();
		return v;
	}
	
	
	// GETTERS AND SETTERS
	public bool IsFlockLeader() { return isLeader; }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Detour : LSMonoBehaviour {
	#region PUBLIC_ADJUST_PARAMS
	public float m_rotationAdjustParam = 0.25f;
	public float m_velocityAdjustParam = 0.25f;
	public float m_ignoreAngleDegree = 75f;
	public float m_turnAngleDegree = 90f;
	public float m_smoothEndureParam = 1.0f;
	public float m_sqrNavAgentVelocityToleration = 0.05f;
	#endregion

	GameObject m_parentObject;
	public GameObject ParentObject{
		get {
			return m_parentObject;
		}
	}
	
	List<Vector3> m_directionList;
	
	bool m_optSet = false;
	Vector3 m_optDirection = Vector3.zero;

	bool m_smoothSet = false;
	Quaternion m_lastRotationAdjust;
	
	#region TEMP_PARAMS
	Vector3 t_vector3;
	Vector3 t_curNavMoveDirection;
	Vector3 t_stackDirection;
	GameObject t_gameObject;
	#endregion

	#region UNITY_FUNCS
	// Use this for initialization
	void Start () {
		if(this.transform.parent.gameObject != null) {
			m_parentObject = this.transform.parent.gameObject;
		}
		m_directionList = new List<Vector3>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerStay(Collider col) {
		if(NeedDetour()) {
			AddDirectionToList(col);
		}
	}

	void LateUpdate(){
		UpdateNavmeshAgentVelocity();
		SmoothBack();
	}
	#endregion

	#region PRIVATE_FUNCS
	private bool NeedDetour(){
		t_gameObject = this.transform.parent.gameObject;
		if(t_gameObject.tag == "Player") {
			if(t_gameObject.GetComponent<Player>().getPlayerState() == Player.PlayerState.Moving) {
				return true;
			} else {
				return false;
			}
		} else if(t_gameObject.tag == "LaneCreep") {
			if(t_gameObject.GetComponent<LaneCreep>().GetCreepState().State == LaneCreep.CreepState.MOVING ||
			   t_gameObject.GetComponent<LaneCreep>().GetCreepState().State == LaneCreep.CreepState.CHASING ||
			   t_gameObject.GetComponent<LaneCreep>().GetCreepState().State == LaneCreep.CreepState.RETURNING) {
				// Debug.Log("In Detour:LaneCreep:Need detour.");
				return true;
			} else {
				return false;
			}
		}
		return false;
	}

	private void AddDirectionToList(Collider col) {
		// Add this is for detecting if im really moving, if really moving need detour
		if(m_parentObject.GetComponent<NavMeshAgent>().velocity.sqrMagnitude > m_sqrNavAgentVelocityToleration) {
			if(col.gameObject.tag == ("Detour")) {
				m_optSet = true;

				// Get relative vector.
				t_vector3 = (col.gameObject.GetComponent<Detour>().ParentObject.transform.position - this.ParentObject.transform.position);

				// Dont do y-axis movement for now.
				t_vector3.y = 0;

				// Get direction.
				t_vector3.Normalize();

				// Add to list.
				m_directionList.Add(t_vector3);
			}
		}
	}

	private void CalculateFinalDirection() {
		// Get NavMeshAgent velocity for direction.
		t_curNavMoveDirection = m_parentObject.GetComponent<NavMeshAgent>().velocity;
		t_curNavMoveDirection.Normalize();
		t_stackDirection = Vector3.zero;

		for(int n=0; n<m_directionList.Count; n++) {
			t_vector3 = m_directionList[n];

			// Use direction in certain angle range.
			if(Vector3.Angle(t_curNavMoveDirection, t_vector3) < m_ignoreAngleDegree) {
				// t_vector3 = Quaternion.AngleAxis(m_turnAngleDegree, Vector3.up) * t_vector3;
				t_stackDirection += t_vector3;
			}
		}

		// Normalize to get simple rotation for final stack direction. 
		t_stackDirection.Normalize();

		// Check turn right or left.
		if(Vector3.Angle(t_curNavMoveDirection, Quaternion.AngleAxis(m_turnAngleDegree, Vector3.up) * t_stackDirection) <
		   Vector3.Angle(t_curNavMoveDirection, Quaternion.AngleAxis(-m_turnAngleDegree, Vector3.up) * t_stackDirection)) {
			m_optDirection = Quaternion.AngleAxis(m_turnAngleDegree, Vector3.up) * t_stackDirection;
		} else {
			m_optDirection = Quaternion.AngleAxis(-m_turnAngleDegree, Vector3.up) * t_stackDirection;
		}
		

	}

	/// <summary>
	/// Updates final navmesh agent velocity by using m_optDirection.
	/// </summary>
	private void UpdateNavmeshAgentVelocity() {
		if(m_optSet){
			Debug.Log("In Detour:Calculate Final Velocity.");
			CalculateFinalDirection();

			if(m_optDirection != Vector3.zero){
				// Set smooth flag.
				m_smoothSet = true;

				// Add scale by get NavMeshAgent velocity.
				m_optDirection *= (Mathf.Sqrt(m_parentObject.GetComponent<NavMeshAgent>().velocity.sqrMagnitude));

				// Set rotation and velocity
				m_lastRotationAdjust = Quaternion.Lerp(m_parentObject.transform.rotation, Quaternion.LookRotation(m_optDirection), m_rotationAdjustParam);
				m_parentObject.transform.rotation = m_lastRotationAdjust;
				m_parentObject.GetComponent<NavMeshAgent>().velocity = Vector3.Lerp(m_parentObject.GetComponent<NavMeshAgent>().velocity, m_optDirection, m_velocityAdjustParam);

				// Reset params.
				m_optSet = false;
				m_optDirection = Vector3.zero;
				m_directionList.Clear();
			} else {
				m_optSet = false;
				m_directionList.Clear();
			}
		}
	}

	/// <summary>
	/// After change velocity, Smooth back.
	/// </summary>
	private void SmoothBack() {
		if((m_smoothSet == true) && (m_optSet == false)) {
			if(Quaternion.Angle(m_lastRotationAdjust, m_parentObject.transform.rotation) < m_smoothEndureParam) {
				m_smoothSet = false;
			}
		}
	}
	#endregion

}

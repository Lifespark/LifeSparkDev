using UnityEngine;
using System.Collections;

public class Projectile : LSMonoBehaviour {

	private CombatManager combatManager;
	public GameObject m_owner;
	public GameObject m_target;

	public float m_projectileSpeed = 300.0f;
	public bool m_isHoming = true;
	public float m_lifeTime = -1; 				//-1 for forever until impact

	// Use this for initialization
	void Start () {
		GameObject manager = GameObject.Find ("Manager");
		combatManager = (CombatManager) manager.GetComponent ("CombatManager");

		rigidbody.AddForce(Vector3.up * 100.0f);

		//Destroy(this, 5.0f);
	}
	
	// Update is called once per frame
	void Update () {
		if(m_target.GetComponent<Player>().playerState == Player.PlayerState.Dead)
			Destroy(this.gameObject);

		rigidbody.velocity = (m_target.transform.position - m_owner.transform.position).normalized * 
			m_projectileSpeed;
	}

	void OnTriggerEnter(Collider other) {
		if(other.gameObject.name != m_owner.name) {
			combatManager.MissileHit(m_owner.name, m_target.name);
			Destroy(this.gameObject);
		}
	}

}

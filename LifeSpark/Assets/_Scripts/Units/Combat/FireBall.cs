using UnityEngine;
using System.Collections;

public class FireBall : LSMonoBehaviour {

    private Vector3 m_dir;
    private float m_initTime;
    private Vector3 _pos;
    private Vector3 m_velocity;

    public float m_lifeSpan = 3;
    public float m_speed = 10;
    public Hit m_hit;

	// Use this for initialization
	void Awake () {
        m_dir = (Vector3)photonView.instantiationData[0];
        m_initTime = Time.time;
        m_velocity = m_dir * m_speed;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time - m_initTime < 0.8) return;
        if (Time.time - m_initTime < m_lifeSpan) {
            _pos = transform.position;
            _pos += m_velocity * Time.deltaTime;
            transform.position = _pos;
        }
        else {
            if (PhotonNetwork.isMasterClient) {
                PhotonNetwork.Destroy(gameObject);
            }
        }
	}

    void OnTriggerEnter(Collider col) {
        if ((col.tag == "Player") || (col.tag == "LaneCreep")) {
            Debug.Log("Hit Unit");
            if (PhotonNetwork.isMasterClient) {
                col.GetComponent<UnitObject>().receiveAttack(m_hit, transform);
                PhotonNetwork.Destroy(gameObject);
            }
        } 
    }
}

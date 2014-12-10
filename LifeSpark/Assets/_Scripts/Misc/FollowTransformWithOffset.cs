using UnityEngine;
using System.Collections;

public class FollowTransformWithOffset : MonoBehaviour {

    public Transform m_follow;

    private Vector3 m_offset;

	// Use this for initialization
	void Start () {
        m_offset = transform.position - m_follow.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = m_follow.position + m_offset;
        transform.rotation = m_follow.rotation;
	}
}

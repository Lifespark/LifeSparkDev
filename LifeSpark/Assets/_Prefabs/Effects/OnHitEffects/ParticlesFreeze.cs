using UnityEngine;
using System.Collections;

public class ParticlesFreeze : MonoBehaviour {

	//freezes the particle system after a certain period of time in seconds

	private ParticleSystem m_system;

	public float m_freezeAfterTime = 1.0f;
	private float m_aliveTime = 0.0f;

	// Use this for initialization
	void Start () {
		m_system = this.particleSystem;
	}
	
	// Update is called once per frame
	void Update () {

		if(m_system) {
			if(m_aliveTime > m_freezeAfterTime) m_system.Pause();

			if(m_system.isPlaying) 
				m_aliveTime += Time.deltaTime;
		}
	}
}

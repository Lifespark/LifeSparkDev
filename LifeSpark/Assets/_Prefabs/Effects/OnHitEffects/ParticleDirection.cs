using UnityEngine;
using System.Collections;

public class ParticleDirection : MonoBehaviour {

	//directs the particles of a particle system to a target position

	private ParticleSystem m_system;
	private ParticleSystem.Particle[] m_particles;

	public Vector3 m_target;
	private Transform m_targetObj;

	private float m_systemSpeed = 50.0f;

	Vector3 t_particleToTarget;
	float t_particleToTargetDistance;
	Vector3 t_arcDirection;

	public bool m_start = false;

	private PhotonView m_callerPhotonView;

	// Use this for initialization
	void Start () {
		m_system = this.particleSystem;
		m_particles = new ParticleSystem.Particle[m_system.maxParticles];
	}

	public void SetTarget(Transform target) {
		m_targetObj = target;
		m_target = target.transform.position;
	}

	public void SetTarget(Vector3 target) {
		m_target = target;
	}

	public void setCallerPhotonView(PhotonView pv) {
		m_callerPhotonView = pv;
	}

	public void StartMovement() {
		m_start = true;
		m_system.simulationSpace = ParticleSystemSimulationSpace.World;
		m_system.loop = false;
		m_system.emissionRate = 5;
		//m_system.startLifetime = 3.0f;
		//m_systemSpeed = 2.0f;

		m_system.GetParticles(m_particles);
		for(int i = 0; i < m_system.particleCount; i++) {
			//m_particles[i].position = gameObject.transform.wo
		}
	}

	// Update is called once per frame
	void Update () {

		if(m_system && m_start) {

			if(m_targetObj) m_target = m_targetObj.position;

			m_system.GetParticles(m_particles);
			//m_system.emissionRate = (m_target-m_system.transform.position).magnitude;
			//m_system.emissionRate = 0;
			m_system.startLifetime = ((m_target-m_system.transform.position).magnitude)/m_systemSpeed;
			//m_system.startLifetime = 2.0f;

			for(int i = 0; i < m_system.particleCount; i++) {
				t_particleToTarget = (m_target-m_particles[i].position);
				t_particleToTargetDistance = t_particleToTarget.magnitude;

				if(t_particleToTargetDistance < 5) m_particles[i].lifetime = m_system.startLifetime;

//				if(i < m_system.particleCount/3) {
//					t_arcDirection = Vector3.up;
//				}
//				else if(i < m_system.particleCount*2/3) {
//					t_arcDirection = Vector3.forward;
//				}
//				else {
//					t_arcDirection = Vector3.back;
//				}

				t_arcDirection = Vector3.up;

				//Debug.Log(m_particles[0].lifetime);
				m_systemSpeed = (m_particles[i].startLifetime - m_particles[i].lifetime)*20;


				//m_particles[i].size = m_system.startSize;

				m_particles[i].velocity = (t_particleToTarget.normalized)*(m_systemSpeed);
				m_particles[i].velocity += (t_arcDirection*(t_particleToTargetDistance*0.05f));//Vector3.up*Mathf.Sin(m_particles[i].lifetime)*20;
			}
			m_system.SetParticles(m_particles, m_system.particleCount);


		}
		if(!m_system.IsAlive() && m_callerPhotonView) {
			m_callerPhotonView.RPC("RPC_DestroyElement", PhotonTargets.MasterClient);
		}
	}
}

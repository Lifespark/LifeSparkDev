using UnityEngine;
using System.Collections;

public class SinglePlayEffects : MonoBehaviour {

	private ParticleSystem m_system;
	//public float m_duration = 0.0f;
	public bool isPhotonNetworked = false;
	public bool hasParent = false;

	// Use this for initialization
	void Start () {
		m_system = this.particleSystem;
		//m_system.Play();
	}
	
	// Update is called once per frame
	void Update () {
		//if(m_duration > 0) 
	}

	void LateUpdate () 
	{
		if (!particleSystem.IsAlive()) {
// 			if(hasParent)
// 				PhotonNetwork.Destroy(this.transform.parent.GetComponent<PhotonView>());
// 			else if(isPhotonNetworked)
// 				PhotonNetwork.Destroy(this.GetComponent<PhotonView>());
// 			else 
				GameObject.Destroy(this.gameObject);
		}
			//Object.Destroy (this.gameObject);	
	}
}

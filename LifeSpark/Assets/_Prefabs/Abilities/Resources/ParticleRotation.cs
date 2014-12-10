using UnityEngine;
using System.Collections;

public class ParticleRotation : MonoBehaviour {

	//sets start rotation to parent rotation
	
	private ParticleSystem m_system;

	public int m_matchXYZ = 0;	//0 = x, 1 = y, 2 = z

	
	// Use this for initialization
	void Start () {
		m_system = this.particleSystem;
		if(m_system) m_system.startRotation = this.transform.rotation.y;
	}
	
	// Update is called once per frame
	void Update () {
		

	}
}

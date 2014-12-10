using UnityEngine;
using System.Collections;

public class SnakeHook : MonoBehaviour {
    bool isStarted = false;

	public Transform chainRoot;
	public Transform levantisHand;

	public int length = 10;

	private LineRenderer m_chain;

	// Use this for initialization
	void Start () {
		m_chain = this.GetComponent<LineRenderer>();
		//m_chain.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		//m_chain.enabled = true;

		float len = (chainRoot.position-levantisHand.position).magnitude;

		m_chain.SetPosition(0, levantisHand.position);
		m_chain.SetPosition(1, chainRoot.position);

		m_chain.material.mainTextureScale = new Vector2(len,1);
	}

    public void StartSkill() {
        isStarted = true;
    }

    public void StopSkill() {
        isStarted = false;
    }

	public void StartSnakeHook(bool on) {

	}
}

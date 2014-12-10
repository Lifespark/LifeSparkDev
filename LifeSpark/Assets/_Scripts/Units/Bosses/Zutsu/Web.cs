using UnityEngine;
using System.Collections;

public class Web : LSMonoBehaviour {
    //The starting "Health of the Web"
	public int m_MaxHealth;
    //The slow% that is applied to the player's normal speed range: [0,100]
    public float m_Slow;
    //Duration of the Slow Effect in ms
	public int m_SlowDuration;
    //Interval that the timer waits before checking if a player is on the web in ms
    private int m_Interval = 10;
    //The current Health of the web
    private int m_Health;

    public int m_tier;

    private int tick;
    private float m_radius;
	private Vector3 m_pos;
    private bool m_enabled;
    public NavMeshObstacle[] m_obstacles;

	// Use this for initialization
	void Start () {

        m_Health = m_MaxHealth;
        //Assumes the prefab hax x and z bounds that are equal
        m_radius = this.renderer.bounds.extents.x;
        tick = 0;
		m_pos = this.transform.position;
        m_enabled = false;
        m_obstacles = GetComponentsInChildren<NavMeshObstacle>();
        
	}
	
	// Update is called once per frame
	void Update () {

        if (tick == m_Interval)
        {   
            if (m_enabled) {
                CheckPlayer(m_pos);
            }
            tick = 0;
        }
        else
            tick++;

	}

	/// <summary>Checks to see if a player is on the Web Object</summary>
	/// <param name="position">The position of the web object</param>
	private void CheckPlayer(Vector3 position) {

		Collider[] objectsAroundMe = Physics.OverlapSphere(position, m_radius);
        Collider temp;
        for (int i = 0; i < objectsAroundMe.Length; i++)
        {
            temp = objectsAroundMe[i];
            if (temp.CompareTag("Player"))
            {
                temp.GetComponent<Player>().SlowEffect(m_Slow, m_SlowDuration);
            }

        }
	}

    public void SetEnabled(bool enabled) {
        photonView.RPC("RPC_SetEnabled", PhotonTargets.AllBufferedViaServer, enabled);
    }


    [RPC]
    public void RPC_SetEnabled(bool enabled) {
        this.renderer.enabled = enabled;
        m_enabled = enabled;
        foreach(NavMeshObstacle obstacle in m_obstacles) {
            obstacle.gameObject.SetActive(!enabled);
        }
		GetComponentInChildren<ParticleSystem>().Play();
    }
}

using UnityEngine;
using System.Collections;

public class LoreObject : MonoBehaviour {

    public LoreManager.LoreItem m_loreItem;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision c) 
    {
        if (c.gameObject.tag == "Player") {
            PlayerManager.Instance.photonView.RPC("RPC_pickupLore",
                                                   PhotonTargets.All,
                                                   c.gameObject.name,
                                                   this);
        }
    }
}

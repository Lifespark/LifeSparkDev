using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Photon;
using System;

public class MonsterClient : LSMonoBehaviour
{
    private static int[,] CAMPPOSITION = { { 33, 35 }, { 29, 33 }, { 32, 39 }, { -19, -39 }, { -13, 33 }, { 33, -32 }, { 28, -32 } };

	// Use this for initialization
	void Start () {
        System.Random ra = new System.Random();
		NetworkManager tempNM = GameObject.Find("Manager").GetComponent<NetworkManager>();
		if (tempNM.IsMasterClient()) {
	        for (int i = 0; i < 7; i++) {
	            int rand = ra.Next(0, 2);
	            Vector3 position = new Vector3(CAMPPOSITION[i, 0], 0, CAMPPOSITION[i, 1]);
	            GameObject monster;

                object[] instantiateData = { position };

	            if (rand == 1)
                    monster = PhotonNetwork.InstantiateSceneObject("JungleMonsterProto1", position, new Quaternion(), 0, instantiateData) as GameObject;
	            else
                    monster = PhotonNetwork.InstantiateSceneObject("JungleMonsterProto2", position, new Quaternion(), 0, instantiateData) as GameObject;
	        }
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

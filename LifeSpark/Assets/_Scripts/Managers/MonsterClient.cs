using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Photon;
using System;

public class MonsterClient : LSMonoBehaviour
{
    static private MonsterClient _instance;
    static public MonsterClient Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(MonsterClient)) as MonsterClient;
            return _instance;
        }
    }

    public List<JungleMonster> allMonsters = new List<JungleMonster>();
    public Dictionary<string, JungleMonster> MonsterLookUp = new Dictionary<string,JungleMonster>();

    public Transform[] jungleCamp;
    private Vector3[] jungleCampPos;

    private int m_jungleCreepCount = 7;

    public int[] target;

	// Use this for initialization
	void Awake () {
        //Invoke("InitMonster", 1.0f);
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void InitMonster() {
		if (jungleCamp.Length == 0) {
			LevelManager.Instance.m_jungleCreepInited = true;
			Debug.Log("alljungleMonsterInited");
			return;
		}

        jungleCampPos = new Vector3[jungleCamp.Length];
        for (int i = 0; i < jungleCamp.Length; i++)
            jungleCampPos[i] = jungleCamp[i].position;

        System.Random ra = new System.Random();
        int monsterCount = 0;
        if (PhotonNetwork.isMasterClient) {
			for (int i = 0; i < jungleCamp.Length; i++) {
				//TODO: This will probably need to be expanded to allow for differing numbers & varieties of creeps in each camp later.
				//		In that case, prefab will need to have different formations depending on the number of creeps.
				//		--Kaleb
                JungleCamp jc = jungleCamp[i].GetComponent<JungleCamp>();
                for (int j = 0; j < jc.m_CreepTypes.Length; j++) {
					Vector3 position = jungleCamp[i].FindChild("CreepPos" + (j + 1).ToString()).position;
					GameObject monster;

                    string monsterName = "JungleCreep" + monsterCount.ToString();
                    object[] instantiateData = { position, i, jungleCampPos, target, monsterName };

                    string name = "Orc";
                    if (jc.m_CreepTypes[j] == JungleMonster.CreepType.ORC)
                        name = "Orc";
                    else if (jc.m_CreepTypes[j] == JungleMonster.CreepType.KOBOLD)
                        name = "Kobold";
                    else if (jc.m_CreepTypes[j] == JungleMonster.CreepType.GOBLIN)
                        name = "Goblin";                    

                    monster = PhotonNetwork.InstantiateSceneObject(name, position, Quaternion.Euler(0, ra.Next(0, 360), 0), 0, instantiateData) as GameObject;
                    jc.m_myJungleCreeps.Add(monster.GetComponent<JungleMonster>());
                    monsterCount++;
				}
                jc.AssignCamp();
			}
        }

        // add callback for all jungle monster, if only all of them has really been created we call this
       
    }

    public void OnJungleMonsterInstantiated() {
        m_jungleCreepCount--;
        //Debug.Log("jungleMonsterInited");

        if (m_jungleCreepCount == 0) {
            LevelManager.Instance.m_jungleCreepInited = true;
            //Debug.Log("alljungleMonsterInited");
        }

    }
}

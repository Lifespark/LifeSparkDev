using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class SparkPoint : UnitObject {
    public class CapturerData {
        public float energyInjectionSpeed = 0;
        public float totalEnergyInjected = 0;
    }

    public Texture2D m_magiTexture;
    public Texture2D m_synthTexture;

    public MeshRenderer[] m_bodyParts;
    private List<Material> m_bodyMaterial = new List<Material>();

    public ParticleSystem m_magiParticle;
    public ParticleSystem m_sythnParticle;
    public ParticleSystem m_activationParticle;
    public ParticleSystem m_activatedParticle;
    public ParticleSystem m_destructionParticle01;
    public ParticleSystem m_destrcutionParticle02;

    public Material m_lazerAttackMat;
    public MeshRenderer m_sparkMesh;
    private Material m_sparkMaterial;

    public float m_attackRadius = 20;

    public MeshRenderer m_rangeMesh;

    public SparkPoint m_defaultNextSparkPoint;
	public SparkPoint m_alternateNextSparkPoint;
	public SparkPoint m_selectedNextSparkPoint;

	public SparkPoint[] _connections;
	public SparkPoint[] m_bossAreaConnections;

    private List<string> initConnected = new List<string>();

    public List<GameObject> associatedLanes = new List<GameObject>();
	
	public bool isStartingSparkpoint;
    private Animation m_anim;
    private float m_lastAnimSpeedLerpTime = -1;
    private float m_lastAnimSpeedLerpTarget = -1;

    public int owner;
	public enum SparkPointState {

		FREE,
		FREEING,
		CAPTURING,
		CAPTURED,
		DESTROYING,
		DESTROYED,
	};

	public SparkPointState sparkPointState;
	List<string> capturers;

    public Dictionary<int, CapturerData> capturersInfo = new Dictionary<int, CapturerData>();

	public int capturingTeam;
	int captureTimer;
	int captureTime;
    public float attackingRange = 20;
    private GameObject m_Lazer;
    private GameObject attackingTarget;
    public float attackingInterval = 0.5f;
    public float restInterval = 1;
    public float activeTimer;
    public Hit m_sparkPointHit;
	public static float m_XPradius = 35;
	public static float m_XP = 35;

    private bool m_hasAttack = false;
    LineRenderer m_LazerRender;

	public Color sparkPointColor;
    LS.Region[] regions;
    ParticleSystem m_particle;

	//SparkPointConnection m_sparkPointConnection;
	public Transform m_sparkPointTip;
	public Material m_sparkPointConnectionMaterial;
	public GameObject m_sparkPointActivation;
	public GameObject m_sparkPointDestruction;

    public Transform m_bossCageConnectionPoint;
    [HideInInspector]
    public GameObject m_bossCageConnectionLane;

	// Use this for initialization
	void Awake () {

        m_unitType = UnitObjectType.STATIC;
        m_anim = GetComponentInChildren<Animation>();
        m_sparkMaterial = m_sparkMesh.material;

		owner = -1;

		sparkPointState = SparkPointState.FREE;
		capturers = new List<string>();
		capturingTeam = -1;
		captureTimer = 0;
		captureTime = 200;

		sparkPointColor = new Color(0.5f, 0.5f, 0.5f);
		m_sparkMaterial.color = sparkPointColor;
        if (photonView != null && photonView.instantiationData != null) {
            gameObject.name = (string)photonView.instantiationData[0] + "_net";
            gameObject.tag = (string)photonView.instantiationData[1];

            for (int i = 2; i < photonView.instantiationData.Length; i++) {
                initConnected.Add((string)photonView.instantiationData[i]);
            }

            SparkPointManager.Instance.OnSparkPointInstantiated();
        }
        regions = GameObject.FindGameObjectWithTag("Ground").GetComponents<LS.Region>();

        //init Lazer
        m_Lazer = new GameObject();
        m_Lazer.transform.parent = gameObject.transform.root;
        m_LazerRender = m_Lazer.AddComponent<LineRenderer>();
        m_LazerRender.SetWidth(5, 5);
        m_LazerRender.SetVertexCount(2);
        m_LazerRender.SetColors(Color.yellow, Color.yellow);
        m_LazerRender.material = m_lazerAttackMat;
        m_LazerRender.renderer.enabled = false;

        activeTimer = Time.time;
        //m_particle = GetComponent<ParticleSystem>();
        //m_particle.Stop();
		GameObject go = UIManager.mgr.getHpBar(this,new Vector3(0,4f,0));
		//go.transform.localScale = (Vector3.one * (1/go.transform.lossyScale.x));
        m_LazerRender = m_Lazer.GetComponent<LineRenderer>();

		//m_sparkPointConnection = this.GetComponent<SparkPointConnection>();

		/* TODO: DESIGNERS
		 * right now this is temp code for loading in temp arrays
		 * must load in own config from outside file down below. -jk */
        //for (int i = 0; i < 3; i++) {
        //    for (int j = 0; j < 4; j++) {
        //        bossAreaConnections[i,j] = new SparkPoint();
        //    }
        //}

        m_anim["Capturing"].speed = 0;
        for (int i = 0; i < m_bodyParts.Length; i++) {
            m_bodyMaterial.Add(m_bodyParts[i].material);
        }
	}

    public void InitNetworkPassedData() {
        
        List<SparkPoint> spconnect = new List<SparkPoint>();
        foreach (var spName in initConnected) {
            spconnect.Add(GameObject.Find(spName).GetComponent<SparkPoint>());
        }
        _connections = spconnect.ToArray();
        Debug.Log("initSparkPoint");
    }

    void SetAnimParam(int animType, string param, float fadeTime = 0.2f) {
        m_lastAnimSpeedLerpTarget = 1.0f;
        m_lastAnimSpeedLerpTime = Time.time;
//         switch ((AnimatorType)animType) {
//             case AnimatorType.BOOL:
//                 m_anim.SetBool(param, value == 1);
//                 break;
//             case AnimatorType.INT:
//                 m_anim.SetInteger(param, (int)value);
//                 break;
//             case AnimatorType.FLOAT:
//                 m_anim.SetFloat(param, value);
//                 break;
//             case AnimatorType.TRIGGER:
//                 m_anim.ResetTrigger("goCapturing");
//                 m_anim.ResetTrigger("goCaptured");
//                 m_anim.ResetTrigger("goUncapturing");
//                 m_anim.SetTrigger(param);
//                 break;
//         }
        m_anim.CrossFade(param, fadeTime);
    }

	// Update is called once per frame
	new void Update () {
        if (Time.time - m_lastAnimSpeedLerpTime < 0.1) {
            m_anim["Capturing"].speed = Mathf.Lerp(m_anim["Capturing"].speed / 0.8569f, m_lastAnimSpeedLerpTarget, (Time.time - m_lastAnimSpeedLerpTime) * 5f) * 0.8569f;
        }

        // play audio of sparkpoint captured
        if (!audio.isPlaying && sparkPointState == SparkPointState.CAPTURED) {
            audio.Play();
        }


		/*foreach(SparkPoint sp in _connections) {
			Debug.DrawLine(this.transform.position, sp.transform.position, Color.red);
		}*/
        Vector2 _offset = new Vector2(0, 0.5f * Time.time % 1.0f); /*= m_rangeMesh.material.GetTextureOffset("_MainTex")*/;

        m_rangeMesh.material.SetTextureOffset("_MainTex", _offset);

        if (PhotonNetwork.isMasterClient) {
            if (unitHealth <= 0 && sparkPointState != SparkPointState.DESTROYED) {
                photonView.RPC("RPC_setSparkPointDestroy", PhotonTargets.AllBuffered);
            }
        }

        switch (sparkPointState) {
            case SparkPointState.CAPTURING:
                int maxEnergyTeam = -1;
                int fasterEnergyTeam = -1;
                float fasterSpeed = 0;
                float fasterEnergy = -1;
                sparkPointColor = Color.black;
                for (int i = 0; i < capturersInfo.Count; i++) {
                    var info = capturersInfo.ElementAt(i);
                   
                    info.Value.totalEnergyInjected += info.Value.energyInjectionSpeed * Time.deltaTime;

                    if (info.Value.totalEnergyInjected > fasterEnergy) {
                        fasterEnergy = info.Value.totalEnergyInjected;
                        fasterEnergyTeam = info.Key;
                    }

//                  if (info.Key == 1) {
//                      sparkPointColor.r += (0.5f + info.Value.totalEnergyInjected / 2);
//                      sparkPointColor.g += (0.5f - info.Value.totalEnergyInjected / 2);
//                      sparkPointColor.b += (0.5f - info.Value.totalEnergyInjected / 2);
//                  }
//                  else if (info.Key == 2) {
//                      sparkPointColor.r += (0.5f - info.Value.totalEnergyInjected / 2);
//                      sparkPointColor.g += (0.5f - info.Value.totalEnergyInjected / 2);
//                      sparkPointColor.b += (0.5f + info.Value.totalEnergyInjected / 2);
//                  }

                    if (info.Value.energyInjectionSpeed > fasterSpeed) {
                        fasterSpeed = info.Value.energyInjectionSpeed;
                    }

                    if (info.Value.totalEnergyInjected >= 1) {
                        if (maxEnergyTeam == -1 || info.Value.totalEnergyInjected > capturersInfo[maxEnergyTeam].totalEnergyInjected) {
                            maxEnergyTeam = info.Key;
                        }
                    }
                }

                if (fasterEnergyTeam == 1) {
                    for (int i = 0; i < m_bodyMaterial.Count; i++) {
                        m_bodyMaterial[i].SetTexture("_Texture2", m_magiTexture);
                        m_bodyMaterial[i].SetFloat("_Blend", capturersInfo[fasterEnergyTeam].totalEnergyInjected);
                    }    
                }
                else if (fasterEnergyTeam == 2) {
                    for (int i = 0; i < m_bodyMaterial.Count; i++) {
                        m_bodyMaterial[i].SetTexture("_Texture2", m_synthTexture);
                        m_bodyMaterial[i].SetFloat("_Blend", capturersInfo[fasterEnergyTeam].totalEnergyInjected);
                    }
                }

                m_activationParticle.renderer.material.SetColor("_TintColor", fasterEnergyTeam == 1 ? new Vector4(66 / 255.0f, 177 / 255.0f, 187 / 255.0f, 1) : new Vector4(165 / 255.0f, 49 / 255.0f, 225 / 255.0f, 1));

                //float remainEnergy = 1 - capturersInfo[maxEnergyTeam].totalEnergyInjected;
                //float remainTime = remainEnergy / capturersInfo[maxEnergyTeam].energyInjectionSpeed;

                m_lastAnimSpeedLerpTarget = fasterSpeed;
                m_lastAnimSpeedLerpTime = Time.time;

                //sparkPointColor /= capturersInfo.Count;
                //m_sparkMaterial.color = sparkPointColor;
                if (maxEnergyTeam != -1) {
                    sparkPointState = SparkPointState.CAPTURED;

//                  if (maxEnergyTeam == 1) m_sparkMaterial.color = Color.red;
//                  else if (maxEnergyTeam == 2) m_sparkMaterial.color = Color.blue;
                    //m_particle.startColor = m_sparkMaterial.color;
                    //m_particle.Play();

                    if (PhotonNetwork.isMasterClient) {
                        int tempSize = _connections.Length;
                        for (int i = 0; i < tempSize; i++) {
                            if (_connections[i].owner == maxEnergyTeam) {
                                //// if this connection has same owner, initial new lane
//                                GameObject tempObject = PhotonNetwork.InstantiateSceneObject("Lane", _connections[i].transform.position, new Quaternion(), 0, null);
//                                Lane tempLane = tempObject.GetComponent<Lane>();
//                                //// set lane name
//                                tempObject.name = "Lane" + this.name + _connections[i].name;
//                                //// set lane material
//                                tempLane.photonView.RPC("RPC_setLaneMaterial", PhotonTargets.All, maxEnergyTeam);
//                                //// set line position, location and scale
//                                tempLane.photonView.RPC("RPC_setInitialTransform", PhotonTargets.All, this.transform.position + new Vector3(0, 25, 0), _connections[i].transform.position + new Vector3(0, 25, 0));
//
//                                associatedLanes.Add(tempObject);
//                                _connections[i].associatedLanes.Add(tempObject);
                                object[] initData = { maxEnergyTeam };
								GameObject tempLine = PhotonNetwork.InstantiateSceneObject("SparkPointLink", m_sparkPointTip.transform.position, Quaternion.identity, 0, initData);
								//tempLine.AddComponent<LineRenderer>();
								//tempLine.GetComponent<LineRenderer>().material = m_sparkPointConnectionMaterial;
								//tempLine.AddComponent<SparkPointConnection>();
								tempLine.transform.parent = m_sparkPointTip.transform;
								//tempLine.GetComponent<SparkPointConnection>().startConnection(m_sparkPointTip.transform, 
							                                                              //_connections[i].m_sparkPointTip.transform);
								tempLine.GetPhotonView().RPC("RPC_startSparkPointConnection", PhotonTargets.All, m_sparkPointTip.transform.position, _connections[i].m_sparkPointTip.transform.position);
								associatedLanes.Add(tempLine);
								_connections[i].associatedLanes.Add(tempLine);
                            }
                        }
                    }
                    owner = maxEnergyTeam;
					for (int i = 0; i < regions.Length; i++) {
						regions[i].CheckActivation();
					}
                    for (int i = 0; i < capturers.Count; i++) {
                        PlayerManager.Instance.photonView.RPC("RPC_setPlayerSparkPointCaptured",
                                                                      PhotonTargets.All,
                                                                      capturers[i],
                                                                      this.name, owner);
                    }
                    capturers.Clear();
                    capturersInfo.Clear();

                }
                break;

            case SparkPointState.CAPTURED:

                if ((Time.time - activeTimer) >= attackingInterval + restInterval) {
                    m_LazerRender.enabled = false;
                    if (attackingTarget != null) {
                        if (attackingTarget.tag == "Player" && attackingTarget.GetComponent<Player>().GetState() == Player.PlayerState.Dead ||
                            attackingTarget.tag == "LaneCreep" && attackingTarget.GetComponent<LaneCreep>().GetCreepState().State == LaneCreep.CreepState.DEAD)
                            attackingTarget = null;
                    }

                    if (attackingTarget == null) {
                        //GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

//                         int index = -1;
//                         float minDistance = 0;
// 
//                         for (int i = 0; i < PlayerManager.Instance.allPlayers.Count; i++) {
//                             Player player = PlayerManager.Instance.allPlayers[i];
//                             if (player.team == owner || player.GetState() == Player.PlayerState.Dead)
//                                 continue;
//                             float currentDistance = Vector3.SqrMagnitude(gameObject.transform.position - PlayerManager.Instance.allPlayers[i].transform.position);
//                             if (index == -1 || currentDistance < minDistance) {
//                                 minDistance = currentDistance;
//                                 index = i;
//                             }
//                         }
//                         if (index != -1)
//                             attackingTarget = PlayerManager.Instance.allPlayers[index].gameObject;
                        Collider[] nearByColliders = Physics.OverlapSphere(transform.position, 20);
                        int index = -1;
                        float minDistance = 0;
                        for (int i = 0; i < nearByColliders.Length; i++) {
                            if (nearByColliders[i].tag == "Player") {
                                if (nearByColliders[i].GetComponent<Player>().team == owner || nearByColliders[i].GetComponent<Player>().GetState() == Player.PlayerState.Dead)
                                    continue;
                            }
                            else if (nearByColliders[i].tag == "LaneCreep") {
                                if (nearByColliders[i].GetComponent<LaneCreep>().owner == owner || nearByColliders[i].GetComponent<LaneCreep>().curState == LaneCreep.CreepState.DEAD)
                                    continue;
                            }
                            else
                                continue;
                            float currentDistance = Vector3.SqrMagnitude(gameObject.transform.position - collider.transform.position);
                            if (index == -1 || currentDistance < minDistance) {
                                minDistance = currentDistance;
                                index = i;
                            }
                        }
                        if (index != -1)
                            attackingTarget = nearByColliders[index].gameObject;
                    }

                    if (attackingTarget != null && Vector3.SqrMagnitude(gameObject.transform.position - attackingTarget.transform.position) > m_attackRadius * m_attackRadius)
                        attackingTarget = null;

                    if (attackingTarget != null) {
 //                     m_LazerRender.enabled = true;
 //                     m_LazerRender.SetPosition(0, gameObject.transform.position);
 //                     m_LazerRender.SetPosition(1, attackingTarget.transform.position);
 // 
 //                     if ((Time.time - activeTimer) < attackingInterval) {
 //                         m_LazerRender.enabled = true;
 //                         if (!m_hasAttack) {
 //                             attackingTarget.GetComponent<UnitObject>().receiveAttack(m_sparkPointHit, transform);
 //                             m_hasAttack = true;
 //                         }
 //                     }
 //                     else if ((Time.time - activeTimer) < attackingInterval + restInterval)
 //                         m_LazerRender.enabled = false;
 //                     else {
 //                         activeTimer = Time.time;
 //                         m_hasAttack = false;
 //                     }
                        if (PhotonNetwork.isMasterClient) {
                            photonView.RPC("RPC_setSparkpointAttack", PhotonTargets.All, attackingTarget.name);
                        }
                    }
                }
                break;
        }
	}
	
	public int GetOwner() {
		return owner;
	}
	

	public void SetSparkPointCapture (string playerName, int team, bool b, float rate = 0.2f) {
		if (b) {
            if (owner == -1 && sparkPointState != SparkPointState.DESTROYED) {

				//m_sparkPointActivation.SetActive(true);
                m_activationParticle.Play();

				capturers.Add(playerName);
				capturingTeam = team;
				sparkPointState = SparkPointState.CAPTURING;
                SetAnimParam((int)AnimatorType.TRIGGER, "Capturing");
				//Debug.Log ("Team"+team+" "+playerName+" attempting to capture: "+this.name);
                if (capturersInfo.ContainsKey(team)) {
                    capturersInfo[team].energyInjectionSpeed += rate;
                }
                else {
                    capturersInfo.Add(team, new CapturerData());
                    capturersInfo[team].energyInjectionSpeed += rate;
                }
			}
            else if (owner != team && sparkPointState == SparkPointState.CAPTURED ) {
                //SetSparkPointDestroy(playerName, team);
            }
		}
		else {
            if (sparkPointState != SparkPointState.DESTROYED && sparkPointState != SparkPointState.CAPTURED) {
                
				//m_sparkPointActivation.SetActive(false);
                m_activationParticle.Stop();

				capturers.Remove(playerName);
                if (capturersInfo.ContainsKey(team)) {
                    capturersInfo[team].energyInjectionSpeed -= 0.2f;
                    if (capturersInfo[team].energyInjectionSpeed < 0.001) {
                        capturersInfo.Remove(team);
                    }
                }
                //capturingTeam = -1;
                sparkPointState = SparkPointState.FREE;
                captureTimer = 0;
                sparkPointColor.r = 0.5f;
                sparkPointColor.g = 0.5f;
                sparkPointColor.b = 0.5f;
                m_sparkMaterial.color = sparkPointColor;
                if (capturers.Count == 0) {
                    m_anim["Uncapturing"].normalizedTime = 1 - m_anim["Capturing"].normalizedTime;
                    SetAnimParam((int)AnimatorType.TRIGGER, "Uncapturing");
                }
            }
		}
	}

    [RPC]
    public void RPC_setSparkPointDestroy() {
        //m_particle.Stop();
		//m_sparkPointDestruction.SetActive(true);
        m_destructionParticle01.Play();
        m_destrcutionParticle02.Play();
        m_destructionParticle01.startColor = owner == 1 ? new Color(66 / 255.0f, 177 / 255.0f, 187 / 255.0f, 1) : new Color(165 / 255.0f, 49 / 255.0f, 225 / 255.0f, 1);
        m_destrcutionParticle02.startColor = owner == 1 ? new Color(66 / 255.0f, 177 / 255.0f, 187 / 255.0f, 1) : new Color(165 / 255.0f, 49 / 255.0f, 225 / 255.0f, 1);

        m_activationParticle.Stop();
        m_activatedParticle.Stop();
        m_magiParticle.Stop();
        m_sythnParticle.Stop();


		owner = -1;
        capturers.Clear();
        capturingTeam = -1;
        sparkPointState = SparkPointState.DESTROYED;
        captureTimer = 0;
        sparkPointColor.r = 0.0f;
        sparkPointColor.g = 0.0f;
        sparkPointColor.b = 0.0f;
        m_sparkMaterial.color = sparkPointColor;
        transform.GetChild(1).gameObject.SetActive(false);

        m_anim.Stop();
        m_anim.enabled = false;

		m_sparkPointActivation.SetActive(false);
		

        if (PhotonNetwork.isMasterClient) {
            for (int i = 0; i < associatedLanes.Count; i++) {
                PhotonNetwork.Destroy(associatedLanes[i]);
            }
        }
        for (int i = 0; i < regions.Length; i++) {
            regions[i].CheckActivation();
        }
        associatedLanes.Clear();
        m_LazerRender.enabled = false;
        Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigidBodies.Length; i++) {
            if (rigidbody != rigidBodies[i]) {
                rigidBodies[i].isKinematic = false;
                rigidBodies[i].AddForce(0, 0.01f, 0);
            }
        }
    }

    /// <summary>
    /// synchronize once more to ensure all client knows correct sparkpoint state
    /// </summary>
    /// <param name="team"></param>
    public void SetSparkPointCaptured(int team) {
        SetAnimParam((int)AnimatorType.TRIGGER, "CapturedIdle", 0.5f);
        sparkPointState = SparkPointState.CAPTURED;
        owner = team;
        //if (owner == 1) m_sparkMaterial.color = Color.red;
        //else if (owner == 2) m_sparkMaterial.color = Color.blue;
        m_activationParticle.Stop();

        m_activatedParticle.startColor = owner == 1 ? new Color(66 / 255.0f, 177 / 255.0f, 187 / 255.0f, 1) : new Color(165 / 255.0f, 49 / 255.0f, 225 / 255.0f, 1);
        m_activatedParticle.Play();

        if (owner == 1) {
            for (int i = 0; i < m_bodyMaterial.Count; i++) {
                m_bodyMaterial[i].SetTexture("_Texture2", m_magiTexture);
                m_bodyMaterial[i].SetFloat("_Blend", 1);
            }
            m_magiParticle.Play();
        }
        else if (owner == 2) {
            for (int i = 0; i < m_bodyMaterial.Count; i++) {
                m_bodyMaterial[i].SetTexture("_Texture2", m_synthTexture);
                m_bodyMaterial[i].SetFloat("_Blend", 1);
            }
            m_sythnParticle.Play();
        }

		Vector3 position = this.transform.position;
		Collider[] objectsAroundMe = Physics.OverlapSphere(position, m_XPradius);
		Collider temp;
		for (int i = 0; i < objectsAroundMe.Length; i++)
		{
			temp = objectsAroundMe[i];
			if (temp.CompareTag("Player"))
			{
				if (temp.GetComponent<Player>().team == owner)
					temp.GetComponent<Player>().GetXP(m_XP);
			}
			
		}
        //m_particle.Play();
    }

    [RPC]
    public void RPC_setSparkpointAttack(string targetName) {
        //if (true) {
        attackingTarget = GameObject.Find(targetName);//PlayerManager.Instance.PlayerLookUp[targetName].gameObject;
        StartCoroutine(LazerAttack());
        //}
    }

    IEnumerator LazerAttack() {
        m_LazerRender.enabled = true;
        activeTimer = Time.time;
        m_hasAttack = false;

        while ((Time.time - activeTimer) < attackingInterval) {
            m_LazerRender.SetPosition(0, gameObject.transform.position + new Vector3(0, 15, 0));
            m_LazerRender.SetPosition(1, attackingTarget.transform.position);

            if (!m_hasAttack) {
                attackingTarget.GetComponent<UnitObject>().receiveAttack(m_sparkPointHit, transform);
                m_hasAttack = true;
            }
            yield return null;
        }
        m_LazerRender.enabled = false;
    }

    [RPC]
    public void RPC_deleteSparkPoint() {

    }
}
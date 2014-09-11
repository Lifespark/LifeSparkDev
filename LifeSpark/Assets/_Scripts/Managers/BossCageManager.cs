using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossCageManager : LSMonoBehaviour {

	static private BossCageManager m_instance;
	static public BossCageManager GetInstance() {
		return m_instance;
	}

	private GameObject m_bossCageHolder;

	void Awake(){
		m_instance = this;
	}

	// Use this for initialization
	void Start() {
	
	}
	
	// Update is called once per frame
	void Update() {
	
	}

	void OnLevelWasLoaded(int level) {
		if (level == 1) {
			m_bossCageHolder = GameObject.FindGameObjectWithTag("BossCage");
			if(m_bossCageHolder == null){
				Debug.Log("Warning:BossCageManager:OnLevelWasLoaded:Can't find BossCage.");
				return;
			}
			if(PhotonNetwork.isMasterClient){
				Debug.Log(m_bossCageHolder.name + ":" + m_bossCageHolder.tag);
				List<object> initData = new List<object>{m_bossCageHolder.name, m_bossCageHolder.tag};

				BossCage bossCage = m_bossCageHolder.GetComponent<BossCage>();

				initData.Add(bossCage.m_chargeRate);
				initData.Add(bossCage.m_breakValue);

				List<object> connectedSparkPoints = new List<object>();
				for(int i = 0; i < bossCage.m_connections.GetLength(0); i++){
					connectedSparkPoints.Add(bossCage.m_connectionNames[i]);
				}
				/*for(int i = 0; i < connectedSparkPoints.Count; i++){
					Debug.Log("CheckCheck:" + connectedSparkPoints[i]);
				}*/

				initData.AddRange(connectedSparkPoints);

				Debug.Log("Final count:" + initData.Count);
				/*for(int i = 0; i < initData.Count; i++){
					Debug.Log("Check InitData:" + initData[i]);
				}*/
				Vector3 tempTransform = new Vector3(3, 0, 3);
				tempTransform = tempTransform + m_bossCageHolder.transform.position;
				PhotonNetwork.InstantiateSceneObject("BossCage", 
				                                     //m_bossCageHolder.transform.position, 
				                                     tempTransform,
				                                     m_bossCageHolder.transform.rotation,
				                                     0,
				                                     initData.ToArray());
			}
		}
	}

	public void OnBossCageInstantiated() {
		Debug.Log("BossCageManager:OnBossCageInstantiated");
		// must take parent first before the m_bossCageHolder be destroyed.
		Transform bossCagePerent = m_bossCageHolder.transform.parent;
		DestroyImmediate(m_bossCageHolder);

		GameObject netBossCage = GameObject.FindGameObjectWithTag("BossCage");
		netBossCage.transform.parent = bossCagePerent;

		netBossCage.GetComponent<BossCage>().InitNetworkPassedData();
	}
}

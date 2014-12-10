using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SparkPointManager : LSMonoBehaviour {

    static private SparkPointManager _instance;

    // use this to access SparkPointManager
    static public SparkPointManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(SparkPointManager)) as SparkPointManager;
            return _instance;
        }
    }

    // use this dict to look up sparkpoint by name. DO NOT USE FIND!
    public Dictionary<string, SparkPoint> sparkPointsDict = new Dictionary<string, SparkPoint>();
    public List<GameObject> netSps;

    private int sparkPointCount = 0, teamCount = 2;
    private bool loaded = false;
    public List<GameObject> sparkPointPlaceHolders;
    public List<GameObject> getAllSparkPoint()
    {
        return sparkPointPlaceHolders;
    }

    private List<LS.Region> regions = new List<LS.Region>();

    void Awake() {
        _instance = this;
    }

    public void InitSparkPoint() {
//  *** COMMENTED OUT FOR DEMO 12/4 ***
		//regions = new List<LS.Region>(GameObject.FindWithTag("Ground").GetComponents<LS.Region>());
//
//        for (int i = 0; i < regions.Count; i++) {
//            regions[i].PrepareRegionPolygon();
//        }

        sparkPointPlaceHolders = new List<GameObject>(GameObject.FindGameObjectsWithTag("SparkPoint"));
        sparkPointCount = sparkPointPlaceHolders.Count;
        //if (PhotonNetwork.isMasterClient) {
            for (int i = 0; i < sparkPointPlaceHolders.Count; i++) {

                sparkPointsDict.Add(sparkPointPlaceHolders[i].name, sparkPointPlaceHolders[i].GetComponent<SparkPoint>());

                /*
                List<object> connectedSparkPointNames = new List<object>();
                for (int j = 0; j < sparkPointPlaceHolders[i].GetComponent<SparkPoint>()._connections.Length; j++) {
                    if (sparkPointPlaceHolders[i].GetComponent<SparkPoint>()._connections[j] != null)
                        connectedSparkPointNames.Add(sparkPointPlaceHolders[i].GetComponent<SparkPoint>()._connections[j].gameObject.name);
                }
                List<object> initData = new List<object> { sparkPointPlaceHolders[i].name, sparkPointPlaceHolders[i].tag };
                initData.AddRange(connectedSparkPointNames);
                GameObject netSp = PhotonNetwork.InstantiateSceneObject("SparkPoint", sparkPointPlaceHolders[i].transform.position, sparkPointPlaceHolders[i].transform.rotation, 0, initData.ToArray());
                //sparkPoints.Add(netSp.GetComponent<SparkPoint>());
                */
            }
        //}
        LevelManager.Instance.m_sparkPointInited = true;
        loaded = true;
	}
	

	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// when a networked Sparkpint is fully prepared, it will call this function
    /// </summary>
    public void OnSparkPointInstantiated() {
        sparkPointCount--;

        // if all sparkpoints are replaced, clean up the scene
        if (sparkPointCount == 0) {
            Transform spParent = sparkPointPlaceHolders[0].transform.parent;

            netSps = new List<GameObject>(GameObject.FindGameObjectsWithTag("SparkPoint"));
            netSps.RemoveAll(sparkPointPlaceHolders.Contains);
            for (int i = 0; i < netSps.Count; i++) {
				netSps[i].name = netSps[i].name.Substring(0, netSps[i].name.LastIndexOf("_net"));
				//netSps[i].name = netSps[i].name.Substring(0, 11);
				Debug.Log (netSps[i].name);
                sparkPointsDict.Add(netSps[i].name, netSps[i].GetComponent<SparkPoint>());
                netSps[i].transform.parent = spParent;
                foreach (var region in regions) {
                    for (int j = 0; j < region.regionPoints.Length; j++) {
                        if (region.regionPoints[j] != null && region.regionPoints[j].gameObject.name == netSps[i].name)
                            region.regionPoints[j] = netSps[i].GetComponent<SparkPoint>();
                    }
                }
            }
            for (int i = 0; i < sparkPointPlaceHolders.Count; i++) {
                DestroyImmediate(sparkPointPlaceHolders[i]);
            }
            for (int i = 0; i < netSps.Count; i++) {
                if (netSps[i] != null)
                    netSps[i].GetComponent<SparkPoint>().InitNetworkPassedData();
            }
            for (int i = 0; i < regions.Count; i++ ) {
                regions[i].PrepareRegionPolygon();
            }

			int rand = Random.Range (0, netSps.Count);
			netSps[rand].GetComponent<SparkPoint>().sparkPointState = SparkPoint.SparkPointState.CAPTURED;
			netSps[rand].GetComponent<SparkPoint>().owner = 1;
			netSps[rand].GetComponent<SparkPoint>().sparkPointColor.r  = 1f;
			netSps[rand].GetComponent<SparkPoint>().sparkPointColor.g  = 0f;
			netSps[rand].GetComponent<SparkPoint>().sparkPointColor.b  = 0f;
			netSps[rand].GetComponent<SparkPoint>().renderer.material.color = netSps[rand].GetComponent<SparkPoint>().sparkPointColor;

			int rand2 = 1;
			while(true) {
				rand2 = Random.Range (0, netSps.Count);
				if(rand2 != rand)
					break;
			}
			netSps[rand2].GetComponent<SparkPoint>().sparkPointState = SparkPoint.SparkPointState.CAPTURED;
			netSps[rand2].GetComponent<SparkPoint>().owner = 2;
			netSps[rand2].GetComponent<SparkPoint>().sparkPointColor.r  = 0f;
			netSps[rand2].GetComponent<SparkPoint>().sparkPointColor.g  = 0f;
			netSps[rand2].GetComponent<SparkPoint>().sparkPointColor.b  = 1f;
			netSps[rand2].GetComponent<SparkPoint>().renderer.material.color = netSps[rand2].GetComponent<SparkPoint>().sparkPointColor;
			/*
			GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerStartingSparkPointCaptured",
			                                              PhotonTargets.All,
			                                              capturers[i],
			                                              netSps[rand].GetComponent<SparkPoint>().name);*/

			
            CreepManager.Instance.sparkPointSetUp = true;
        }

        LevelManager.Instance.m_sparkPointInited = true;
    }
}

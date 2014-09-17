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
    public Dictionary<string, GameObject> sparkPointsDict = new Dictionary<string, GameObject>();
    public List<GameObject> netSps;

    private int sparkPointCount = 0;
    private bool loaded = false;
    private List<GameObject> sparkPointPlaceHolders;

    private List<Region> regions = new List<Region>();

    void Awake() {
        _instance = this;
    }

    void OnLevelWasLoaded(int level) {
        if (level == 1) {
            regions = new List<Region>(GameObject.FindWithTag("Ground").GetComponents<Region>());
            sparkPointPlaceHolders = new List<GameObject>(GameObject.FindGameObjectsWithTag("SparkPoint"));
            sparkPointCount = sparkPointPlaceHolders.Count;
            if (PhotonNetwork.isMasterClient) {
                for (int i = 0; i < sparkPointPlaceHolders.Count; i++) {
                    List<object> connectedSparkPointNames = new List<object>();
                    for (int j = 0; j < sparkPointPlaceHolders[i].GetComponent<SparkPoint>()._connections.Length; j++) {
                        if (sparkPointPlaceHolders[i].GetComponent<SparkPoint>()._connections[j] != null)
                            connectedSparkPointNames.Add(sparkPointPlaceHolders[i].GetComponent<SparkPoint>()._connections[j].gameObject.name);
                    }
                    List<object> initData = new List<object> { sparkPointPlaceHolders[i].name, sparkPointPlaceHolders[i].tag };
                    initData.AddRange(connectedSparkPointNames);
                    GameObject netSp = PhotonNetwork.InstantiateSceneObject("SparkPoint", sparkPointPlaceHolders[i].transform.position, sparkPointPlaceHolders[i].transform.rotation, 0, initData.ToArray());
                    //sparkPoints.Add(netSp.GetComponent<SparkPoint>());
                }
            }
        }
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
                netSps[i].name = netSps[i].name.Substring(0, 11);
                sparkPointsDict.Add(netSps[i].name, netSps[i]);
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
            CreepManager.Instance.sparkPointSetUp = true;
        }
    }
}

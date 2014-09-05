using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SparkPointManager : LSMonoBehaviour {

    // should be called after level loaded
    // any callback for this?
    List<SparkPoint> sparkPoints = new List<SparkPoint>();

    int sparkPointCount = 0;
    bool loaded = false;
    List<GameObject> sparkPointPlaceHolders;

    List<Region> regions = new List<Region>();

    void OnLevelWasLoaded(int level) {
        if (level == 1) {
            // never use FindObjectOfType!!
            regions = new List<Region>(FindObjectsOfType<Region>());
            sparkPointPlaceHolders = new List<GameObject>(GameObject.FindGameObjectsWithTag("SparkPoint"));
            sparkPointCount = sparkPointPlaceHolders.Count;
            if (PhotonNetwork.isMasterClient) {
                foreach (var sp in sparkPointPlaceHolders) {
                    List<object> connectedSparkPointNames = new List<object>();
                    foreach (var spsript in sp.GetComponent<SparkPoint>()._connections) {
                        if (spsript != null)
                            connectedSparkPointNames.Add(spsript.gameObject.name);
                    }
                    List<object> initData = new List<object> { sp.name, sp.tag };
                    initData.AddRange(connectedSparkPointNames);
                    GameObject netSp = PhotonNetwork.InstantiateSceneObject("SparkPoint", sp.transform.position, sp.transform.rotation, 0, initData.ToArray());
                    //sparkPoints.Add(netSp.GetComponent<SparkPoint>());
                }
            }
        }
        loaded = true;
	}
	

	// Update is called once per frame
	void Update () {
	
	}

    public void OnSparkPointInstantiated() {
        sparkPointCount--;
        if (sparkPointCount == 0) {
            Transform spParent = sparkPointPlaceHolders[0].transform.parent;

            List<GameObject> netSps = new List<GameObject>(GameObject.FindGameObjectsWithTag("SparkPoint"));
            netSps.RemoveAll(sparkPointPlaceHolders.Contains);
            foreach (var netSp in netSps) {
                netSp.name = netSp.name.Substring(0, 11);
                netSp.transform.parent = spParent;
                foreach (var region in regions) {
                    for (int i = 0; i < region.regionPoints.Length; i++) {
                        if (region.regionPoints[i] != null && region.regionPoints[i].gameObject.name == netSp.name)
                            region.regionPoints[i] = netSp.GetComponent<SparkPoint>();
                    }
                }
            }
            foreach (GameObject sp in sparkPointPlaceHolders) {
                DestroyImmediate(sp);
            }
            foreach (GameObject netSp in netSps) {
                if (netSp != null)
                    netSp.GetComponent<SparkPoint>().InitNetworkPassedData();
            }
            foreach (var region in regions) {
                region.PrepareRegionPolygon();
            }
        }
    }
}

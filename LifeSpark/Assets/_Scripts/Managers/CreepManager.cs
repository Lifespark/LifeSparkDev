using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreepManager : LSMonoBehaviour {
    public GameObject creepPrefab;
    public GameObject highlightPrefab;
    public int maximumCreepNum = 1;

	public Dictionary<GameObject, List<LaneCreep>> creepDict = new Dictionary<GameObject, List<LaneCreep>>();
    //private List<LaneCreep> creepList = new List<LaneCreep>();
	private GameObject source;
    private Transform target;
    public bool menuOn = false;
	public bool selectingTarget = false;

	private SparkPoint sparkPoint;
	private Player player;

    Vector3 originalPos;
    Quaternion originalRot;

    List<GameObject> highLighting = new List<GameObject>();

	// Use this for initialization
	void Awake () {
		sparkPoint = gameObject.GetComponent<SparkPoint> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (player == null) {
			PlayerInput[] playerInputs = FindObjectsOfType<PlayerInput> ();
			foreach(PlayerInput p in playerInputs) {
				if (p.isMine) {
					player = p.gameObject.GetComponent<Player>();
				}
			}
			return;
		}


        if (!selectingTarget) {
            if (GetTarget(1)) {
                menuOn = true;
                StartCoroutine(FlyCamera());
                highLighting.Clear();
                GameObject[] sparkPoints = GameObject.FindGameObjectsWithTag("SparkPoint");
                foreach (var sp in sparkPoints) {
                    if (sp.GetComponent<SparkPoint>().GetOwner() != player.team) {
                        GameObject hl = Instantiate(highlightPrefab, sp.transform.position, Quaternion.identity) as GameObject;
                        highLighting.Add(hl);                    
                    }
                }
            }
        }
        else {
            if (GetTarget(0)) {
                StartCoroutine(DispatchCreep());
                StartCoroutine(FlyCameraBack());
                selectingTarget = false;
                menuOn = false;
                foreach (var hl in highLighting)
                    Destroy(hl);
            }
        }

	}

    void OnGUI() {
        if (menuOn) {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(source.transform.position);
            if (GUI.Button(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 50), "Dispatch")) {
                selectingTarget = true;
            }
            if (GUI.Button(new Rect(screenPos.x, Screen.height - screenPos.y + 50, 100, 50), "Cancel")) {
                menuOn = false;
                StartCoroutine(FlyCameraBack());
            }
        }
    }

    // type: 0: select target for creep, 1: select sparkpoint to operate
    bool GetTarget(int type) {
        // could use a helper manager for all spark point general selection, not one for each like now
        if (Input.GetMouseButtonUp(type)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                if (type == 1) {
					//Debug.Log(hit.collider.GetComponent<SparkPoint>().GetOwner());
					if (hit.collider.GetComponent<SparkPoint>() != null && 
					    hit.collider.GetComponent<SparkPoint>().GetOwner() == player.team) {
						source = hit.collider.gameObject;
                        return true;
                    }
                }
                else if (type == 0) {
					if (hit.collider.GetComponent<SparkPoint>() != null && 
					    hit.collider.GetComponent<SparkPoint>().GetOwner() != player.team) {
                        target = hit.collider.gameObject.transform;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    IEnumerator DispatchCreep() {

		if (!creepDict.ContainsKey (source)) {
			creepDict.Add(source, new List<LaneCreep>());
		}

        for (int i = creepDict[source].Count; i < maximumCreepNum; i++) {
			//photonView.RPC ("RPC_dispatchCreep", PhotonTargets.All, source.name, target.name, player.name, source.GetComponent<SparkPoint>().GetOwner());
            DispatchCreepAlternative(source.name, target.name, player.name, source.GetComponent<SparkPoint>().GetOwner());
			yield return new WaitForSeconds(2.0f);
        }
    }

    void DispatchCreepAlternative(string source, string target, string playerName, int team) {

        Color creepColor = team == 1 ? Color.red : Color.blue;
        object[] instantiateData = { target, team, playerName, creepColor.r, creepColor.g, creepColor.b, creepColor.a };

        GameObject sourceObj = GameObject.Find(source);
        //GameObject creep = Instantiate(creepPrefab, sourceObj.transform.position + Vector3.up * 0.5f, Quaternion.identity) as GameObject;
        //GameObject creep = PhotonNetwork.Instantiate("LaneCreep", sourceObj.transform.position + Vector3.up * 0.5f, Quaternion.identity, 0) as GameObject;
        GameObject creep = PhotonNetwork.Instantiate("LaneCreep", sourceObj.transform.position + Vector3.up * 0.5f, Quaternion.identity, 0, instantiateData) as GameObject;
        
        LaneCreep thisCreep = creep.GetComponent<LaneCreep>();

        if (!creepDict.ContainsKey(sourceObj)) {
            creepDict.Add(sourceObj, new List<LaneCreep>());
        }
        
        creepDict[sourceObj].Add(thisCreep);
    }

	[RPC]
	void RPC_dispatchCreep (string source, string target, string playerName, int team) {
		GameObject sourceObj = GameObject.Find(source);
		GameObject creep = Instantiate(creepPrefab, sourceObj.transform.position + Vector3.up * 0.5f, Quaternion.identity) as GameObject;
		LaneCreep thisCreep = creep.GetComponent<LaneCreep>();
		thisCreep.target = GameObject.Find(target).transform;
		thisCreep.owner = team;
		thisCreep.playerName = playerName;
		if (team == 1)
			thisCreep.renderer.material.color = Color.red;
		else if (team == 2)
			thisCreep.renderer.material.color = Color.blue;
		//thisCreep.renderer.material = source.renderer.material;
		if (!creepDict.ContainsKey (sourceObj)) {
			creepDict.Add(sourceObj, new List<LaneCreep>());
		}
		creepDict[sourceObj].Add(thisCreep);
	}

    IEnumerator FlyCamera() {
        originalPos = Camera.main.transform.position;
        Vector3 targetPos = new Vector3(0, 130, 0);
        originalRot = Camera.main.transform.rotation;
        Quaternion lookDownRot = Quaternion.Euler(90, 0, 0);
        float percent = 0;
        while (percent < 1) {
            percent += Time.deltaTime * 2;
            Camera.main.transform.rotation = Quaternion.Slerp(originalRot, lookDownRot, percent);
            Camera.main.transform.position = Vector3.Lerp(originalPos, targetPos, percent);
            yield return null;
        }
        Camera.main.transform.rotation = lookDownRot;
        Camera.main.transform.position = targetPos;
        yield return null;
    }

    IEnumerator FlyCameraBack() {
        Vector3 targetPos = Camera.main.transform.position;
        Quaternion lookDownRot = Camera.main.transform.rotation;
        float percent = 0;
        while (percent < 1) {
            percent += Time.deltaTime * 2;
            Camera.main.transform.rotation = Quaternion.Slerp(lookDownRot, originalRot, percent);
            Camera.main.transform.position = Vector3.Lerp(targetPos, originalPos, percent);
            yield return null;
        }
        Camera.main.transform.rotation = originalRot;
        Camera.main.transform.position = originalPos;
        yield return null;
    }
}

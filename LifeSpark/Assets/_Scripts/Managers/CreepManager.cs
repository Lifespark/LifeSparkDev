using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreepManager : LSMonoBehaviour {

    static private CreepManager _instance;
    static public CreepManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(CreepManager)) as CreepManager;
            return _instance;
        }
    }

	private enum DispatchType { BEGINSOURCE, TARGET, ENDSOURCE, NONE };

    public float m_perCreepTimer = 2;
    public float m_waveTimer = 10;
    public float m_waveCount = 3;

    public GameObject creepPrefab;
    public GameObject highlightPrefab;
    private int maximumCreepNum = 1;
    public bool sparkPointSetUp = false;

	public Dictionary<GameObject, List<LaneCreep>> creepDict = new Dictionary<GameObject, List<LaneCreep>>();
    public Dictionary<string, LaneCreep> LaneCreepLookUp = new Dictionary<string, LaneCreep>();
	public List<LaneCreep> creepList = new List<LaneCreep>();

	private GameObject source;
    private Transform target;
    public bool menuOn = false;
	public bool selectingTarget = false;

	private SparkPoint[] sparkPoints;
	private Player player;

    [HideInInspector] 
    public int m_creepCount = 0;

	private bool needInitPrefabs = true;
    Vector3 originalPos;
    Quaternion originalRot;

    List<GameObject> highLighting = new List<GameObject>();

	// Use this for initialization
	void Awake () {
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () {
        if (!LevelManager.Instance.m_startedGame)
            return;

		if (player == null) {
            player = PlayerManager.Instance.myPlayer.GetComponent<Player>();
		}

		DispatchType clickType = GetTarget();
        if (!selectingTarget) {
            if (!menuOn) {
                if (clickType == DispatchType.BEGINSOURCE) {
                    menuOn = true;
					selectingTarget = true;
                    StartCoroutine(FlyCamera());
                    highLighting.Clear();
					SparkPoint sourcePoint = source.GetComponent<SparkPoint>();
                    //foreach (var sp in SparkPointManager.Instance.netSps) {
                    //for (int i = 0; i < SparkPointManager.Instance.sparkPointPlaceHolders.Count; i++) {
					SparkPoint iterPoint;
					for (int i = -1; i < sourcePoint._connections.Length; i++) {

						if (i == -1) {
							iterPoint = sourcePoint;
						} else {
							iterPoint = sourcePoint._connections[i];
						}
						if ((iterPoint.GetOwner() != player.team || true) &&
						    iterPoint.sparkPointState != SparkPoint.SparkPointState.DESTROYED) {
                            if(needInitPrefabs){
								needInitPrefabs = false;
								highlightPrefab = Resources.Load<GameObject>("Root/" + "SparkPointCircle");
							}
							GameObject hl = Instantiate(highlightPrefab, iterPoint.transform.position, Quaternion.identity) as GameObject;
							LifeSparkCircle sparkPointCircle = hl.GetComponent<LifeSparkCircle>();
							sparkPointCircle.m_sparkPoint = iterPoint.GetComponent<SparkPoint>();
                            highLighting.Add(hl);

							//change color based on the team
							Color32 c;
							SparkPoint lsp =  iterPoint.GetComponent<SparkPoint>();
							if(lsp.owner == -1){
								//neutral sparkpoint
								 c = new Color32(255,255,255,255); //white
							} else if (sourcePoint.name == lsp.GetComponent<SparkPoint>().name) {
								//source sparkpoint
								c = new Color32(0,255,0,255); //green
								sparkPointCircle.isSource = true;
							} else if(lsp.owner == PlayerManager.Instance.myPlayer.GetComponent<Player>().team){
								//same team
								if (lsp.owner == 1) {
									//Magi
									c = new Color32(54,255,242,255); //cyan
								} else {
									//Synth
									c = new Color32(148,0,153,255); //purple
								}
							} else {
								//enemy team
								 c = new Color32(255,0,0,255); //red
							}
							sparkPointCircle.circle.color = c;
                        }
                    }
                }
            }
        }
        else {
            if (clickType == DispatchType.TARGET) {
               	//As long as the selected target isn't our source, dispatch creeps. (Should just cancel if it's our source.)
				source.GetPhotonView().RPC("RPC_dispatchCreepFromSP", PhotonTargets.MasterClient, target.name, player.name, source.GetComponent<SparkPoint>().GetOwner());
			} 
			if (clickType == DispatchType.TARGET || clickType == DispatchType.ENDSOURCE) {
                StartCoroutine(FlyCameraBack());
                selectingTarget = false;
                menuOn = false;
                foreach (var hl in highLighting) {
                    Destroy(hl);
				}
            } 
        }
	}

//    void OnGUI() {
//        if (menuOn) {
//            Vector2 screenPos = Camera.main.WorldToScreenPoint(source.transform.position);
//            if (GUI.Button(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 50), "Dispatch")) {
//                selectingTarget = true;
//            }
//            if (GUI.Button(new Rect(screenPos.x, Screen.height - screenPos.y + 50, 100, 50), "Cancel")) {
//                menuOn = false;
//                selectingTarget = false;
//                StartCoroutine(FlyCameraBack());
//                foreach (var hl in highLighting)
//                    Destroy(hl);
//            }
//        }
//    }

    // type: 0: select target for creep, 1: select sparkpoint to operate, 2: select current source
    private DispatchType GetTarget() {
        if (Input.GetMouseButtonUp(/*type*/0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
				SparkPoint hitSparkPoint = hit.collider.GetComponent<SparkPoint>();
				LifeSparkCircle sparkPointCircle = hit.collider.GetComponent<LifeSparkCircle>();

				if (hitSparkPoint != null && hitSparkPoint.GetOwner() == player.team) {
					source = hitSparkPoint.gameObject;
					return DispatchType.BEGINSOURCE;
				} else if (sparkPointCircle != null && sparkPointCircle.isSource) {
					target = sparkPointCircle.m_sparkPoint.transform;
					return DispatchType.ENDSOURCE;
				} else if (sparkPointCircle != null) {
					target = sparkPointCircle.m_sparkPoint.transform;
					return DispatchType.TARGET;
				}
			}
		}
		return DispatchType.NONE;
    }
// 
//     IEnumerator DispatchCreep() {
// 
// 		if (!creepDict.ContainsKey (source)) {
// 			creepDict.Add(source, new List<LaneCreep>());
// 		}
// 
//         Vector3 forwardDir = (source.transform.position - target.transform.position).normalized;
// 
//         for (int i = creepDict[source].Count; i < m_waveCount; i++) {
// 			//photonView.RPC ("RPC_dispatchCreep", PhotonTargets.All, source.name, target.name, player.name, source.GetComponent<SparkPoint>().GetOwner());
//             Vector3 spreadVect;
//             Quaternion rot = Quaternion.AngleAxis(i * 360 / maximumCreepNum, Vector3.up);
//             spreadVect = rot * forwardDir;
// 
//             DispatchCreepAlternative(source.name, target.name, player.name, source.GetComponent<SparkPoint>().GetOwner(), spreadVect);
//             yield return new WaitForSeconds(m_perCreepTimer);
//         }
    //     }




    #region NOT_USED_ANY_MORE

    IEnumerator DispatchCreep(string pSource, string pTarget, string pPlayerName, int team) {
        GameObject _source = GameObject.Find(pSource);
        Transform _target = GameObject.Find(pTarget).transform;
        Vector3 forwardDir = (_source.transform.position - _target.transform.position).normalized;

        if (!creepDict.ContainsKey(_source)) {
            creepDict.Add(_source, new List<LaneCreep>());
        }

        for (int i = creepDict[_source].Count; i < m_waveCount; i++) {
            //photonView.RPC ("RPC_dispatchCreep", PhotonTargets.All, source.name, target.name, player.name, source.GetComponent<SparkPoint>().GetOwner());
            Vector3 spreadVect;
            Quaternion rot = Quaternion.AngleAxis(i * 360 / maximumCreepNum, Vector3.up);
            spreadVect = rot * forwardDir;

            DispatchCreepAlternative(_source.name, _target.name, pPlayerName, team, spreadVect);
            yield return new WaitForSeconds(m_perCreepTimer);
        }
    }

    void DispatchCreepAlternative(string pSource, string pTarget, string pPlayerName, int pTeam, Vector3 pSpreadVect) {

        //Color creepColor = team == 1 ? Color.red : Color.blue;
        object[] instantiateData = { pTarget, pTeam, pPlayerName, pSource, pSpreadVect, m_creepCount++, (int)LaneCreep.CreepType.Ranged };

        GameObject sourceObj = GameObject.Find(pSource);
        GameObject creep = PhotonNetwork.InstantiateSceneObject("MageCreep", sourceObj.transform.position + Vector3.up * 0.5f, Quaternion.identity, 0, instantiateData) as GameObject;

        LaneCreep thisCreep = creep.GetComponent<LaneCreep>();
    }

    /// <summary>
    /// if Player is not MasterClient, call this RPC and MasterClient will instantiate the creeps
    /// </summary>
    /// <param name="source">where creep from</param>
    /// <param name="target">where creep to</param>
    /// <param name="playerName">who's creep's owner</param>
    /// <param name="team">what's creep's team</param>
	[RPC]
	void RPC_dispatchCreep (string pSource, string pTarget, string pPlayerName, int pTeam) {
        StartCoroutine(DispatchCreep(pSource, pTarget, pPlayerName, pTeam));
	}

    #endregion

    IEnumerator FlyCamera() {
		PlayerManager.Instance.myPlayer.GetComponent<PlayerInput>().m_isDisabled = true;
        originalPos = Camera.main.transform.position;
        Vector3 targetPos = new Vector3(0, 500, 0);
        originalRot = Camera.main.transform.rotation;
        Quaternion lookDownRot = Quaternion.Euler(90, 0, 0);
        UIManager.mgr.hideByFly = true;
        float percent = 0;
        while (percent < 1) {
            percent += Time.deltaTime * 2;
            Camera.main.transform.rotation = Quaternion.Slerp(originalRot, lookDownRot, percent);
            Camera.main.transform.position = Vector3.Lerp(originalPos, targetPos, percent);
            yield return null;
        }
        Camera.main.transform.rotation = lookDownRot;
        Camera.main.transform.position = targetPos;
        Camera.main.GetComponent<FogOfWarShader>().enabled = false;
        yield return null;
    }

    IEnumerator FlyCameraBack() {
		PlayerManager.Instance.myPlayer.GetComponent<PlayerInput>().m_isDisabled = false;
        Camera.main.GetComponent<FogOfWarShader>().enabled = true;
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
        UIManager.mgr.hideByFly = false;
        yield return null;
    }
}

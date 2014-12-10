using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MiniMap : MonoBehaviour {

    Dictionary<Player, GameObject> playerIcons = new Dictionary<Player, GameObject>();
    Dictionary<SparkPoint, GameObject> sparkPointIcons = new Dictionary<SparkPoint, GameObject>();
    Dictionary<LaneCreep, GameObject> laneCreepIcons = new Dictionary<LaneCreep, GameObject>();

	private Vector3 miniMapSize = new Vector3(257, 0, 257);
	private Vector3 mapPos = new Vector3(261, 0, 282);
	private Vector3 mapSize = new Vector3(610, 0, 616); // 522 / 580  // 440/480
	private Vector3 miniMapZeroPos = new Vector3(-109, 0, -110); // -109/-110
	private float scaleSize = 2.6f; //2.5
	/* maps minimap to regular map. -jk */
	private float xScale = 1.2f;
	private float yScale = 1.1f;
	/* checks if touch in expanded minimap. -jk */
	private float centerX = -10.7f;
	private float centerY = -15.2f;
	private float radiusSquared = 300.0f*300.0f;

	// Camera View Indicator - BR
	public GameObject camIndicatorView;
	// Indicator Boundaries
	public float eastIndicatorBounds = -88;
	public float westIndicatorBounds = -250;
	public float northIndicatorBounds = 0;
	public float southIndicatorBounds = -150;
	// Main Camera Boundaries
	private int eastCamBounds;
	private int westCamBounds;
	private int northCamBounds;
	private int southCamBounds;

    public GameObject miniMapForm;
    public GameObject frameAndBG;
    public GameObject zeroPoint;
    public GameObject center;
    public GameObject topLeft;
    public GameObject settingButton;
    public Camera camera;
    private GameObject mainCamera;
    private CameraManager camManager;
    private TweenPosition tweenPos;
    private TweenScale tweenScale;
    private state currentState = state.RESETED;
    enum state
    {
        RESETED,
        RESETING,
        EXPANDING,
        EXPANDED
    }
	// Use this for initialization
    void Start()
    {
        tweenPos = miniMapForm.GetComponent<TweenPosition>();
        tweenScale = frameAndBG.GetComponent<TweenScale>();
        tweenPos.enabled = false;
        tweenScale.enabled = false;
	}

    void updateCameraManager()
    {
        mainCamera = GameObject.Find("Main Camera");
        //camManager = mainCamera.GetComponent<CameraManager>();

		camManager = mainCamera.GetComponent<CameraManager>();

		eastCamBounds = camManager.m_boundary_East;
		westCamBounds = camManager.m_boundary_West;
		northCamBounds = camManager.m_boundary_North;
		southCamBounds = camManager.m_boundary_South;

		UpdateCamIndicator();
    }

    Vector3 getMiniMapPos(Vector3 pos)
    {
        Vector3 result = new Vector3(miniMapZeroPos.x + pos.x / mapSize.x * miniMapSize.x, miniMapZeroPos.z + pos.z / mapSize.z * miniMapSize.z, 0);
        return result;
    }

	// Update is called once per frame
	void Update () {
        if (mainCamera == null && GameObject.Find("Main Camera"))
        {
            updateCameraManager();
        }
        settingButton.transform.localPosition = topLeft.transform.localPosition * frameAndBG.transform.localScale.x + new Vector3(-27, 44, 0);

        if (LevelManager.Instance.m_startedGame) {
            UpdatePlayer();
            UpdateSparkPoint();
            UpdateCreeps();
			UpdateCamIndicator();

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                OnReset();
            }
        }

        if (Input.GetMouseButtonUp(0) && currentState == state.EXPANDED)
        {
            Vector3 relativePos = Input.mousePosition - camera.WorldToScreenPoint(center.transform.position);
            if (relativePos.magnitude >= 150)
                OnReset();
        }
	}

    void UpdateCreeps()
    {
        Dictionary<LaneCreep, GameObject> tmpLaneCreepIcons = new Dictionary<LaneCreep, GameObject>();
        Dictionary<GameObject, List<LaneCreep>> creeps = CreepManager.Instance.creepDict;
        for (int i = 0; i < creeps.Count; i++)
        {
            List<LaneCreep> laneCreeps = creeps.ElementAt(i).Value;
            for (int j = 0; j < laneCreeps.Count; j++)
            {
                if (!laneCreepIcons.ContainsKey(laneCreeps[j]))
                {
                    GameObject laneCreepIcon = GameObject.Instantiate(Resources.Load("MiniMap/Creep")) as GameObject;
                    UISprite icon = laneCreepIcon.GetComponent<UISprite>();
                    if (laneCreeps[j].owner == 1)
                    {
                        icon.spriteName = "redCreep";
                    }
                    else if (laneCreeps[j].owner == 2)
                    { 
                        icon.spriteName = "blueCreep";
                    }
                    laneCreepIcon.transform.parent = this.gameObject.transform;
                    laneCreepIcon.transform.position = this.gameObject.transform.position;

                    laneCreepIcons.Add(laneCreeps[j],laneCreepIcon);
                }

                Vector3 creepPos = laneCreeps[j].gameObject.transform.position;
                laneCreepIcons[laneCreeps[j]].transform.localPosition = getMiniMapPos(creepPos);
                laneCreepIcons[laneCreeps[j]].SetActive(true);

                tmpLaneCreepIcons.Add(laneCreeps[j], laneCreepIcons[laneCreeps[j]]);
                laneCreepIcons.Remove(laneCreeps[j]);
            }
        }

        for (int i = laneCreepIcons.Count -1 ; i >= 0; i--)
        {
            GameObject tmp = laneCreepIcons.ElementAt(i).Value;
            laneCreepIcons.Remove(laneCreepIcons.ElementAt(i).Key);
            GameObject.Destroy(tmp);
        }

        laneCreepIcons = tmpLaneCreepIcons;

    }

    void UpdateSparkPoint()
    {
        List<GameObject> sparkPoints = SparkPointManager.Instance.getAllSparkPoint();
        for (int i = 0; i < sparkPoints.Count; i++)
        {
            SparkPoint sparkPoint = sparkPoints[i].GetComponent<SparkPoint>();
            if (!sparkPointIcons.ContainsKey(sparkPoint))
            {
                GameObject sparkPointIcon = GameObject.Instantiate(Resources.Load("MiniMap/SparkPoint")) as GameObject;
                sparkPointIcon.transform.parent = this.gameObject.transform;
                sparkPointIcon.transform.position = this.gameObject.transform.position;

                sparkPointIcons.Add(sparkPoint, sparkPointIcon);

                Vector3 sparkPointPos = sparkPoints[i].gameObject.transform.position;
                sparkPointIcon.transform.localPosition = getMiniMapPos(sparkPointPos);
                sparkPointIcon.SetActive(true);
            }

            UISprite icon = sparkPointIcons[sparkPoint].GetComponent<UISprite>();

            if (sparkPoint.owner == 1)
                icon.spriteName = "redSP";
            else if (sparkPoint.owner == 2)
                icon.spriteName = "blueSP";
            else
                icon.spriteName = "darkSP";



        }
    }

    void UpdatePlayer()
    {
        List<Player> players = PlayerManager.Instance.allPlayers;
        for (int i = 0; i < players.Count; i++)
        {
            if (!playerIcons.ContainsKey(players[i]))
            {
                GameObject playerIcon = new GameObject();
                if (players[i].team == 2)
                    playerIcon = GameObject.Instantiate(Resources.Load("MiniMap/BluePlayer")) as GameObject;
                else if (players[i].team == 1)
                    playerIcon = GameObject.Instantiate(Resources.Load("MiniMap/RedPlayer")) as GameObject;
                playerIcon.transform.parent = this.gameObject.transform;
                playerIcon.transform.position = this.gameObject.transform.position;

                playerIcons.Add(players[i], playerIcon);
            }

            Vector3 playerPos = players[i].gameObject.transform.position;
            playerIcons[players[i]].transform.localPosition = getMiniMapPos(playerPos);
            playerIcons[players[i]].SetActive(true);

            if (players[i].getPlayerState() == Player.PlayerState.Dead)
                playerIcons[players[i]].SetActive(false);
        }
    }


	void UpdateCamIndicator()
	{
		float mapXPos = (miniMapSize.x / mapSize.x) * (eastCamBounds - mainCamera.transform.position.x) - eastIndicatorBounds;
		float mapZPos = (miniMapSize.z / mapSize.z) * (northCamBounds - mainCamera.transform.position.z) - northIndicatorBounds;

		camIndicatorView.transform.localPosition = new Vector3(-mapXPos, -mapZPos, 0);
	}

    void OnClick()
    {
        if (currentState == state.RESETED || currentState == state.RESETING)
        {
            tweenPos.from = miniMapForm.transform.position;
            tweenPos.to = new Vector3(-270,-30,0);
            tweenScale.from = frameAndBG.transform.localScale;
            tweenScale.to = new Vector3(scaleSize, scaleSize, 1);

            tweenScale.onFinished = OnFinishExpanding;

            tweenPos.Reset();
            tweenScale.Reset();

            tweenPos.enabled = true;
            tweenScale.enabled = true;

            currentState = state.EXPANDING;
        }
		else if (currentState == state.EXPANDED)
		{
			/* changed to jordan mapping. -jk */
			Vector3 relativePos = Input.mousePosition - camera.WorldToScreenPoint(zeroPoint.transform.position);
			if ((Mathf.Pow(relativePos.x - centerX,2) + Mathf.Pow(relativePos.y - centerY,2)) <= radiusSquared) {
				relativePos = new Vector3(relativePos.x * xScale, mainCamera.transform.position.y, relativePos.y * yScale);
				camManager.FocusOnMiniMap(relativePos);
			}
			OnReset();
		}
		
    }

    void OnFinishExpanding(UITweener tween)
    {
        currentState = state.EXPANDED;
    }

    void OnFinishReset(UITweener tween)
    {
        currentState = state.RESETED;
    }

    void OnReset()
    {
        if (currentState == state.EXPANDING || currentState == state.EXPANDED)
        {
            Vector3 pos = miniMapForm.transform.localPosition;
            Vector3 scale = frameAndBG.transform.localScale;
            tweenPos.Reset();
            tweenScale.Reset();

            tweenPos.from = pos;
            tweenPos.to = new Vector3(0, 0, 0);
            tweenScale.from = scale;
            tweenScale.to = new Vector3(1f, 1f, 1);

            tweenPos.enabled = true;
            tweenScale.enabled = true;
            tweenPos.Play(true);
            tweenScale.Play(true);

            tweenScale.onFinished = OnFinishReset;

            currentState = state.RESETING;
        }
    }
}

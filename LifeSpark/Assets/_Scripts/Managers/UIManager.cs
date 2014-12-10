/// <summary>
/// User interface manager.
/// After it loaded, it will add UI Root (from ngui) in to scene;
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class UIManager : LSMonoBehaviour {

    public UIRoot uiroot;
    public UICamera uiCamera;
    private bool needInit = true;
    public Color32 Team1HpBackground;
    public Color32 Team2HpBackground;
    private GameObject t_loadingForm;
    public bool UseHpCircle;
    public static bool hp_circle;
    private int selectedHero = 1;
    public SkillUIForm mskilluiForm;
	public VictoryUI m_victUI;
	private TargetSprite mTargetSprite;
    public static UIManager mUImger;
    GameObject defeatedMovie;
    GameObject victoryMovie;
    HpParent m_hpParent;
	ScreenUIEffectForm m_screeneffect;

	public ScreenUIEffectForm screenUIEffectForm{
		get{
			if(m_screeneffect == null){
				m_screeneffect = AddGui("ScreenUIEffect").GetComponent<ScreenUIEffectForm>();
			}
			return m_screeneffect;

		}
	}


	public void OnAttack(){
		screenUIEffectForm.OnAttack ();
	}



    public bool endFlag;
    public static UIManager mgr {
        get {
            if(mUImger != null) { return mUImger; }
            GameObject go = GameObject.Find("Manager") as GameObject;
            if (go != null) {
                return go.GetComponent<UIManager>();
            }
            return null;
        }
    }


    public void SetSelectHero(int index) {
        selectedHero = index;
    }

    // Use this for initialization
    void Start() {
        if(needInit)
        init();
    }
    [ExecuteInEditMode]
    // Update is called once per frame
    void Update() {
        hp_circle = UseHpCircle;
    }

    /// <summary>
    /// Init this this class, load prefabs (ui root) in to scene;
    /// 
    /// </summary>
    void init() {

        needInit = false;
        if (!GameObject.Find("UI Root")) // already there;
		{
           GameObject go =  Resources.Load<GameObject>("Root/UIRoot") as GameObject;
           go = Instantiate(go) as GameObject;
            uiroot = go.GetComponent<UIRoot>();
        }else

        uiroot = GameObject.Find("UI Root").gameObject.GetComponent<UIRoot>();
        //Instantiate(uiroot.gameObject).name = "UI Root";
        uiCamera = uiroot.GetComponentInChildren<UICamera>();
        uiCamera.gameObject.SetActive(true);
        Object.DontDestroyOnLoad(uiroot.gameObject);

        defeatedMovie = Resources.Load("DefeatedPlane") as GameObject;
        victoryMovie = Resources.Load("VictoryPlane") as GameObject;
    }

    void displayGuiPrefab(Transform t) {
        if (needInit) {
            init();
        }
        t.gameObject.SetActive(true);
    }

    /// <summary>
    /// Adds the GUI prefab into UIRoot. if it is already, it will display it if it is hiden.
    /// </summary>
    /// <param name="GUIName">the name of UI prefab.</param>
    /// <returns>return the gameobject of the prefab</returns>
    public GameObject AddGui(string GUIName) {
        if (needInit) {
            init();
        }
        // this is for check Weather the ui is alreadly loaded. need to be changed for the low efficiency
        foreach (Transform t in uiCamera.transform) {
            if (t.name == GUIName) // when it alreadly loaded into scene
			{
                displayGuiPrefab(t);
                t.gameObject.SetActive(true);
                t.SendMessage("OnDisplay", SendMessageOptions.DontRequireReceiver);
                return t.gameObject;
            }

        }
        GameObject go = Resources.Load<GameObject>("Root/" + GUIName);
        //go = Instantiate(go) as GameObject;
        go = NGUITools.AddChild(uiCamera.gameObject, go);
        go.name = GUIName;
        go.SendMessage("OnDisplay", SendMessageOptions.DontRequireReceiver);

        return go;
    }

    private SkillUIForm t_skillUIForm;

    /// <summary>
    /// Gets the skill form.
    /// </summary>
    /// <value>Get the KillUIForm class.</value>
    public SkillUIForm T_skillUIForm {
        get {
            if (t_skillUIForm == null) {
				t_skillUIForm =	AddGui("SkillUIForm").GetComponent<SkillUIForm>();
                       }
            return this.t_skillUIForm;
        }
    }

    private VirtualControlForm t_virtualControlForm;

    /// <summary>
    /// Gets the virtual control form.
    /// </summary>
    /// <value>Get the VirtualControlForm class.</value>

    public VirtualControlForm m_virtualControlForm {
        get {
            if (t_virtualControlForm == null) {
                foreach (Transform t in uiCamera.transform) {
                    if (t.name == "VirtualControlForm") // when it alreadly loaded into scene
					{

                        t_virtualControlForm = t.GetComponent<VirtualControlForm>();
                    }

                }
            }
            return this.t_virtualControlForm;
        }
    }

    void OnLevelWasLoaded(int level) {
        if (level != 0) {
			
			AddGui ("VirtualControlForm").SetActive (true);
			AddGui ("MiniMapForm");
			AddGui ("ChatWindow");
			//AddGui ("VictoryWindow");
			GameObject.Find ("Manager").GetComponent<UIManager> ().showLoadForm ();
            endFlag = true;
			PickHero ();
		} 
		else {
			//GameObject.Find("StartAndLobby").GetComponent<StartUpUI>().m_sGui.leavelRoom();
		}
    }

	public void EndGame(bool victory) {
        if(endFlag) {
            endFlag = false;
        } else {
            return;
        }
        Debug.Log("end game");

		m_victUI = AddGui("VictoryWindow").GetComponent<VictoryUI> ();
		if (victory) {
			m_victUI.victoryLabel.text = "Victory!!!";
			m_victUI.victoryLabel.color = Color.blue;
#if UNITY_IPHONE || UNITY_ANDROID
			Handheld.PlayFullScreenMovie("victory.mp4",Color.black,FullScreenMovieControlMode.Hidden);
			//BackToMainMenu.QuitToMainMenu();
			m_victUI.OnBackToMainMenu();
#endif
		} else {
			m_victUI.victoryLabel.text = "Defeat...";
			m_victUI.victoryLabel.color = Color.red;
			//Handheld.PlayFullScreenMovie("defeat.mp4");
#if UNITY_IPHONE || UNITY_ANDROID
			Handheld.PlayFullScreenMovie ("defeat.mp4",Color.black,FullScreenMovieControlMode.Hidden);
			//BackToMainMenu.QuitToMainMenu();
			m_victUI.OnBackToMainMenu();
#endif
		}

	}

    public GameObject getHPCircle(Player pl) {
      //  GameObject go = Resources.Load<GameObject>("HPCircle");
        GameObject go = Resources.Load<GameObject>("PlayerHpUI");
        if (go != null) {
            go = NGUITools.AddChild(hpParent.gameObject , go) as GameObject;
            if (go.GetComponent<HPBar_3>()) {
                //go.GetComponent<UIHPUnit>().SetHpBar(pl, pl.team == 1 ? Team1HpBackground : Team2HpBackground);
                go.GetComponent<HPBar_3>().setObject(pl);
                //go.transform.Rotate(new Vector3(90, 0, 0));
              //  go.transform.localPosition = new Vector3(0, 11, 0) / pl.transform.localScale.x;
             //   go.transform.localScale = new Vector3(0.1f,0.1f,1);
            }
        }
        return go;
    }


    public GameObject getHpBar(UnitObject obj) {
        GameObject go = Resources.Load<GameObject>("HPbar");
        if(go != null) {
            go = NGUITools.AddChild(obj.gameObject, go) as GameObject;
            if(go.GetComponent<HPBar>()) {
                go.GetComponent<HPBar>().SetHpBar(obj);
            }    

        } else {
            Debug.LogError("cannot load the hp bar");
        }


        return go;

    }

    public void showLoadForm() {
        t_loadingForm = AddGui("LoadingForm");
    }

    public void closeLoadForm() {
        if (t_loadingForm) {
            t_loadingForm.SetActive(false);
        }
		T_skillUIForm.SendMessage ("OnDisplay");
    }

    public void setPlayer(Player me) {
        GameObject go = AddGui("Player");
         psu= go.GetComponent<PlayerStateUI>();
        psu.setPlayer(me);


    }
	public PlayerStateUI psu;

    [RPC]
    public void RPC_setPickedHero(int playerID, int index) {
        //Debug.Log("Player with ID" + playerID + " selected hero #" + index);
        LevelManager.Instance.AddPlayerHero(playerID, index);
    }

    public void PickHero() {
        GameObject.Find("Manager").GetPhotonView().RPC("RPC_setPickedHero", PhotonTargets.MasterClient, PhotonNetwork.player.ID, selectedHero);
    }


    FriendListForm m_flf;

    /// <summary>
    /// open friend list form
    /// </summary>
    public void OpenFriendForm() {
        if(m_flf== null){
           m_flf =  UIManager.mgr.AddGui("FriendForm").GetComponent<FriendListForm>();
           m_flf.transform.transform.localPosition = new Vector3(m_flf.transform.transform.localPosition.x, m_flf.transform.transform.localPosition.y, -2);
        } else {
            m_flf.gameObject.SetActive(true);
        }
    }

    public void OnTargetChanged() {
        if(mskilluiForm == null) {
            mskilluiForm = AddGui("SkillUIForm").GetComponent<SkillUIForm>();
        }
        mskilluiForm.refreshTarget();
    }

    public void OnAttackStatusChanged(bool isAttacking) {
        if (mskilluiForm == null) {
            mskilluiForm = AddGui("SkillUIForm").GetComponent<SkillUIForm>();
        }
        mskilluiForm.SetAttackStatus(isAttacking);
    }

    public TargetSprite targetSprite {
        get {
            if(mTargetSprite == null) {
                GameObject go = Resources.Load("TargetSprite") as GameObject;
                go = Instantiate(go) as GameObject;
                mTargetSprite = go.GetComponent<TargetSprite>();
            }

            return mTargetSprite;
        }
    }


    public GameObject getHpBar(UnitObject obj, Vector3 vector3) {
		GameObject go = Resources.Load<GameObject>("HPbar");
		if(go != null) {
            go = NGUITools.AddChild(hpParent.gameObject, go) as GameObject;
			if(go.GetComponent<HPBar>()) {
				go.GetComponent<HPBar>().SetHpBar(obj,vector3);
			}    
			
		} else {
			Debug.LogError("cannot load the hp bar");
		}
        return go;
    }


    public HpParent hpParent {
        get {
            if(m_hpParent == null) {

                m_hpParent = AddGui("HPParent").GetComponent<HpParent>();
            }

            return m_hpParent;
        }
    }


    public void addHp(Player obj) {
        getHPCircle(obj);
    }


    internal void OnGameStarted() {
        mskilluiForm = AddGui("SkillUIForm").GetComponent<SkillUIForm>();
    }

    public bool hideByFly { get; set; }
}

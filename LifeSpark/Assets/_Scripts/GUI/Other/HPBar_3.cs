using UnityEngine;
using System.Collections;

public class HPBar_3 : HPBar {
    public UISprite hp;
    public UISprite mp;
    public UISprite bg;
    public GameObject bar;
    public Player obj;
    bool needInitTeam;
    public UISprite xpSprite;
    public GameObject spriteRoot;
    public UILabel levelLabel;
	public UISprite levelSprite;
    const string mgBGString = "bgm";
    const string syBGString = "bgs";
    const string myspString = "MAGI player HUD health";
    const string eneString = "enemy SYNTH HUD health";
    const string magia = "MAGI hp";
    const string sna = "Synth ally HUD health";
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(obj == null) {
            Destroy(gameObject);
            return;
        }

        if(UIManager.mgr.hideByFly) {
            transform.GetChild(0).gameObject .SetActive(false);
            return;
        } else {
            transform.GetChild(0).gameObject.SetActive(true);
        }
        if(needInitTeam) {
            if(PlayerManager.Instance.myPlayer)
            initTeam();
        }
        Vector3 v2 = Camera.main.WorldToScreenPoint(obj.transform.position);
         float screenH = Camera.main.pixelHeight;
        v2 = new Vector3((v2.x - Camera.main.pixelWidth / 2) / Camera.main.pixelWidth, (v2.y - Camera.main.pixelHeight / 2) / Camera.main.pixelHeight, 0);
        float yfix = (1 - v2.y) * 0.7f + 0.1342812006319115f * screenH;
       

        float ra = 1536f / Camera.main.pixelHeight;
        transform.localPosition = ra * (new Vector3(UIManager.mgr.uiCamera.camera.pixelWidth * v2.x, UIManager.mgr.uiCamera.camera.pixelHeight * v2.y + yfix , 0));
        hp.fillAmount = obj.unitHealth / obj.maxHealth;
        mp.fillAmount = obj.unitMana / obj.maxMana;
		levelLabel.text = obj.CurrLevel.ToString();
		levelSprite.spriteName = obj.CurrLevel.ToString();
		levelSprite.MakePixelPerfect ();
        if(PlayerManager.Instance.myPlayer.name == obj.name) {
            xpSprite.fillAmount = ((float)obj.currentXP) / (obj.m_LvlXP);
        } else {
            xpSprite.fillAmount = 1;
        }

	}

    private void initTeam() {
        needInitTeam = false;
        int myTeam;
        myTeam = PlayerManager.Instance.myPlayer.GetComponent<Player>().team;
        if(obj.team == 1) {
            xpSprite.spriteName = "XPM";
        } else {
            xpSprite.spriteName = "XPA";
        }
        if(PlayerManager.Instance.myPlayer.name == obj.name) {
            hp.spriteName = myspString;
            //1  2
            if(myTeam == 1) {

                bg.spriteName = mgBGString;
            } else {

                bg.spriteName = syBGString;
             
            }
        } else {

            if(obj.team == 1) {

                bg.spriteName = mgBGString;
            } else {

                bg.spriteName = syBGString;

            }
            if(obj.team == myTeam) {

                if(obj.team == 1) {
                    hp.spriteName = magia;
                } else {
                    hp.spriteName = sna;
                }

            } else {
                spriteRoot.transform.localEulerAngles = new Vector3(0, 180, 0);
                levelLabel.transform.localEulerAngles = new Vector3(0, 180, 0);
				levelSprite.transform.localEulerAngles = new Vector3(0, 180, 0);
                hp.spriteName = eneString;
                if(obj.team == 1) {

                } else {

                }

            }
        }
        
    }


    public GameObject setObject(Player obj) {

        this.obj = obj;
        needInitTeam = true;

		levelLabel.text = obj.CurrLevel.ToString();
        UIWidget[] warray = GetComponentsInChildren<UIWidget>();
        foreach(UIWidget uw in warray){
            uw.depth -= 10;
        }

        return this.gameObject;
    }
}

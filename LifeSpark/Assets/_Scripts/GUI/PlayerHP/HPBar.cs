using UnityEngine;
using System.Collections;

public class HPBar : MonoBehaviour {

    public UISprite hp_circle_fg;
    public GameObject bar;
    UnitObject obj;
	Vector3 offset;
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
            transform.GetChild(0).gameObject.SetActive(false);
            return;
        } else {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        float yFix2 = 0f;
        int myTema;
        switch(obj.tag[0]) {
    
            case 'S':
                yFix2 = -65;
                if(obj.GetComponent<SparkPoint>().owner != -1) {
                    transform.GetChild(0).gameObject.SetActive(true);
                    myTema = PlayerManager.Instance.myPlayer.GetComponent<Player>().team;
                    if(obj.GetComponent<SparkPoint>().owner == myTema) {
                        if(myTema == 1) {
                            hp_circle_fg.spriteName = "MAGI hp";
                        } else {
                            hp_circle_fg.spriteName = "Synth ally HUD health";
                        }
                    }
                } else {
                    transform.GetChild(0).gameObject.SetActive(false);
                }
                break;
        }

        float objH = ((CapsuleCollider) obj.collider).height * obj.transform.localScale.y;
       // Debug.Log(obj.name + "  "+objH);
        Vector3 v2 = Camera.main.WorldToScreenPoint(obj.transform.position);
        float screenH = Camera.main.pixelHeight;
        v2 = new Vector3((v2.x - Camera.main.pixelWidth / 2) / Camera.main.pixelWidth, (v2.y - Camera.main.pixelHeight / 2) / Camera.main.pixelHeight, 0);
        float yfix = (1 - v2.y) * 0.7f + objH*6/633 * screenH;
      
        float ra = 1536f / Camera.main.pixelHeight;
        transform.localPosition = ra * (new Vector3(UIManager.mgr.uiCamera.camera.pixelWidth * v2.x, UIManager.mgr.uiCamera.camera.pixelHeight * v2.y + yfix + yFix2, 0));

      

        if(obj) {
            hp_circle_fg.fillAmount = obj.unitHealth / obj.maxHealth; 
        }
	}

    public GameObject SetHpBar(UnitObject obj, Color32 color ) {
        this.obj = obj;
       // hp_circle_fg.color = color;
        hp_circle_fg.fillAmount = 1;
        //transform.parent = obj.transform;
        transform.localPosition = offset;
        UIWidget[] warray = GetComponentsInChildren<UIWidget>();
        foreach(UIWidget uw in warray) {
            uw.depth -= 10;
        }
        int myTema;
        switch(obj.tag[0]){
            case 'L':
                 myTema = PlayerManager.Instance.myPlayer.GetComponent<Player>().team;
                if(obj.GetComponent<LaneCreep>().owner == myTema) {
                    if(myTema == 1) {
                        hp_circle_fg.spriteName = "MAGI hp";
                    } else {
                        hp_circle_fg.spriteName = "Synth ally HUD health";
                    }
                }
                break;
            case 'S':
                if(obj.GetComponent<SparkPoint>().owner != -1) {
                     myTema = PlayerManager.Instance.myPlayer.GetComponent<Player>().team;
                    if(obj.GetComponent<SparkPoint>().owner == myTema) {
                        if(myTema == 1) {
                            hp_circle_fg.spriteName = "MAGI hp";
                        } else {
                            hp_circle_fg.spriteName = "Synth ally HUD health";
                        }
                    }
                }
                break;
        }

        return this.gameObject;
    }

    public GameObject SetHpBar(UnitObject obj) {
		offset = new Vector3 (0, 5, 0);
       return SetHpBar(obj, new Color32(255, 0, 0, 255));
    }
	public GameObject SetHpBar(UnitObject obj, Vector3 v) {
		offset = v;
		return SetHpBar(obj, new Color32(255, 0, 0, 255));
	}

    
}

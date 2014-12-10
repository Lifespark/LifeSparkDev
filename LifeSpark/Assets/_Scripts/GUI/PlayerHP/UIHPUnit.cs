using UnityEngine;
using System.Collections;

public class UIHPUnit : MonoBehaviour {

    public UISprite hp_circle_fg;
    public UISprite hg_circle_bg;
    public UISprite circle_bg;
    private float t_last_hp;
    private Player t_pl;
    private float t_per;

    public UISprite hp_bar_bg;
    public UISprite hp_bar_fg;

    public GameObject bar;
    public GameObject circle;
    private bool t_useCircle;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (t_pl) {
	        bar.SetActive(!t_useCircle);
	        circle.SetActive(t_useCircle);
            if (t_useCircle) { 
            //if (t_last_hp != t_pl.unitHealth) {
                t_per = t_pl.unitHealth / t_pl.maxHealth /3;
                hp_circle_fg.fillAmount = t_per;
            } else {

                t_per = t_pl.unitHealth / t_pl.maxHealth;
                hp_bar_fg.fillAmount = t_per;
            }
        }
        if (t_useCircle) {

            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        } else {
            Vector3 v = transform.position - Camera.main.transform.position;
            v = new Vector3(0,v.y,v.z);
            Quaternion rotation;

            rotation = Quaternion.LookRotation(v);

            bar.transform.rotation = rotation;
        }
	}

    public GameObject SetHpBar(Player pl,Color32 hpBG) {

        t_color = new Color32(255,0,0,255);
        t_bg_color = new Color32(255, 0, 0, 128);
        t_pl = pl;
        if (pl.team == 1) {

        } else if (pl.team == 2) {
            t_color = new Color32(0, 0, 255, 255);
            t_bg_color = new Color32(0,0, 255,  128);
        }
        hp_circle_fg.color = t_color;
        hg_circle_bg.color = hpBG;
        circle_bg.color = t_bg_color;
        t_last_hp = pl.unitHealth;
        hp_circle_fg.fillAmount = 1/3;
        t_useCircle = UIManager.hp_circle;
        return this.gameObject;
    }


    private Color32 t_color;
    private Color32 t_bg_color;
}

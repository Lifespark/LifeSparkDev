using UnityEngine;
using System.Collections;

public class HeroList : MonoBehaviour {
    public HeroCell[] heroLIst;
    int selected = 0;
	public UISprite selection;
	public GameObject playButton;
	public GameObject form;
	public StartUpUI stu;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    internal void select(int index) {
        for(int i = 0; i < heroLIst.Length; i++) {
            heroLIst[i].selectBox.gameObject.SetActive(false);
        }
        heroLIst[index].selectBox.gameObject.SetActive(true);
        selected = index;
        if(UIManager.mgr) {
            UIManager.mgr.SetSelectHero(index+1);
        }
		selection.spriteName = "c" + (index+1);
		playButton.gameObject.SetActive (true);
    }

	void OnPlay(){
		stu.needPick = false;
		form.SetActive (false);
	}

}

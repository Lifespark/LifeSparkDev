using UnityEngine;
using System.Collections;

public class HeroCell : MonoBehaviour {

    public UISprite heroPic;
    public UILabel heroName;
    public UISprite selectBox;
    public int index;
    public HeroList hl;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnClick() {
        hl.select(index);
    }

}

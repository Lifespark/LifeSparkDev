using UnityEngine;
using System.Collections;

public class ScreenUIEffectForm : MonoBehaviour {

    public UITexture screenEffect;

    Player myPlayer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(myPlayer == null) {
            Destroy(gameObject);
        }
	}

    public void setObject(Player obj) {
        myPlayer = obj;

    }


    public void OnAttack() {
		TweenColor ta = TweenAlpha.Begin<TweenColor>(screenEffect.gameObject, 0.5f);
        ta.from = new Color32(255,255,255,255);
		ta.to = new Color32 (255, 255, 255, 0);
    }
}

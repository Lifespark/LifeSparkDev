using UnityEngine;
using System.Collections;

public class HpBar : MonoBehaviour {

    public SpriteRenderer fg;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (hpr == 0) {
            fg.gameObject.SetActive(false);
        } else {

            fg.transform.localScale = new Vector3(fg.transform.localScale.x, 100 * hpr, 1);
        }
	}

    public float hpr= 1;
}

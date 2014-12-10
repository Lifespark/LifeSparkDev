using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class UIDragElement : MonoBehaviour {

    public int r = 148;
    public float maxY = -136;
    UIElementMerger tar;
    bool isPressed;
    float DragScale;
    Vector3 mTrans;
    public float angel;
	public PlayerStateUI psu;
	public UISprite sprite;
    // Use this for initialization
    void Start() {

    }

    void Awake() {
        DragScale = 0.5f;
        mTrans = transform.localPosition;
        // Debug.Log("scale " + DragScale);
    }

    // Update is called once per frame
    void Update() {
        //float cos = transform.localPosition.y / r;
        //float x = r * Mathf.Sqrt(1 - cos * cos);
        //transform.localPosition = new Vector3(x, transform.localPosition.y, 0);
    }

    void OnDrag(Vector2 delta) {
        delta = delta / DragScale;

        if((transform.localPosition[1] + delta[1]) > mTrans.y) {
            transform.localPosition = mTrans;

        } else if((transform.localPosition[1] + delta[1]) < maxY) {
            transform.localPosition = new Vector3(0, maxY, 0);
            float cos = transform.localPosition.y / r;
            float x = r * Mathf.Sqrt(1 - cos * cos);
            transform.localPosition = new Vector3(x, transform.localPosition.y, 0);

        } else {
            transform.localPosition += new Vector3(0, delta.y, 0);
            float cos = transform.localPosition.y / r;
            float x = r * Mathf.Sqrt(1 - cos * cos);
            transform.localPosition = new Vector3(x, transform.localPosition.y, 0);
        }
    }

//	bool enterMerge;
//
//	void OnTriggerEnter(Collider other){
//		if (other.GetComponent<UIElementMerger> ()) {
//			enterMerge = true;
//		}
//	}
//
//	void OnTriggerExit(Collider other) {
//		if (other.GetComponent<UIElementMerger> ()) {
//			enterMerge = false;
//		}
//	}
//

    void OnPress(bool isPressed) {
        this.isPressed = isPressed;
        if(isPressed) {

        } else {
			if( transform.localPosition.y<= maxY+10) {
				psu.OnMerge();
				gameObject.SetActive(false);
            } else {
				transform.localPosition = mTrans;
            }
        }
    }
}

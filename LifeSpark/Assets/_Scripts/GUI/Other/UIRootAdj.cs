using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class UIRootAdj : MonoBehaviour {
	public bool update = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Awake () {
#if UNITY_EDITOR || UNITY_STANDALONE
        gameObject.GetComponent<UIRoot>().scalingStyle = UIRoot.Scaling.FixedSize;
        gameObject.GetComponent<UIRoot>().manualHeight = (int)(Screen.height*3.4f);
		gameObject.GetComponent<UIRoot>().manualHeight = (int)(1536);
#elif UNITY_IPHONE
        gameObject.GetComponent<UIRoot>().scalingStyle = UIRoot.Scaling.PixelPerfect;
		gameObject.GetComponent<UIRoot>().scalingStyle = UIRoot.Scaling.FixedSize;
		gameObject.GetComponent<UIRoot>().manualHeight = (int)(1536);
#endif

	}

	void Update(){
	//	Debug.Log ("s");

		if (update) {
			update = false;
			Awake();
				}
	}
}

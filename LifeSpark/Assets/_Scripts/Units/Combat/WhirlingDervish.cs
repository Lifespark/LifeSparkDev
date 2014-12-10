using UnityEngine;
using System.Collections;

public class WhirlingDervish : MonoBehaviour {
    bool isStarted = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (isStarted) {
            transform.Rotate(0, 1080 * Time.deltaTime, 0);
        }
	}

    public void StartSkill() {
        isStarted = true;
    }

    public void StopSkill() {
        isStarted = false;
    }
}

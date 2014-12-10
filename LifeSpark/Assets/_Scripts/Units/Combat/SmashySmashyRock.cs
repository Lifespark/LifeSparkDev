using UnityEngine;
using System.Collections;

public class SmashySmashyRock : MonoBehaviour {

    float existTime = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        existTime += Time.deltaTime;
        if (existTime > 2.0)
            Destroy(gameObject);
	}
}

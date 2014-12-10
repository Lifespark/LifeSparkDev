using UnityEngine;
using System.Collections;

public class StalagmiteEffects : MonoBehaviour {

    float totalTime = 0.2f;
	// Use this for initialization
	void Awake () {
        StartCoroutine(RemoveStalagmite());
	}
	
	// Update is called once per frame
	void Update () {
        if (totalTime > 0) {
            totalTime -= Time.deltaTime;
            Vector3 pos = transform.position;
            pos.y += 60 * Time.deltaTime;
            transform.position = pos;
        }
	}

    IEnumerator RemoveStalagmite() {
        yield return new WaitForSeconds(1.0f);
        float time = 0.3f;
        while (time > 0) {
            time -= Time.deltaTime;
            Vector3 pos = transform.position;
            pos.y -= 60 * Time.deltaTime;
            transform.position = pos;
            yield return null;
        }
        Destroy(gameObject);
    }
}

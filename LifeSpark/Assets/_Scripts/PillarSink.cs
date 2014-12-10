using UnityEngine;
using System.Collections;

public class PillarSink : MonoBehaviour {

    public float m_sinkSpeed = 10;
    public bool m_isSink = false;
    public ParticleSystem m_pillarDust;

    void Update() {
        if (m_isSink) {
            Vector3 pos = transform.position;
            pos.y -= m_sinkSpeed * Time.deltaTime;
            transform.position = pos;
//             if (m_pillarDust != null && !m_pillarDust.isPlaying)
//                 m_pillarDust.Play();
        }
    }
}

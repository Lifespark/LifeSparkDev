using UnityEngine;
using System.Collections;

public class TombSink : MonoBehaviour {

    public PillarSink[] m_pillars;
    public float m_sinkSpeed = 10;
    public bool m_isSink = false;
    public NavMeshObstacle m_obstacle;

    public void Break() {
        StartCoroutine(StartBreak());
    }

    IEnumerator StartBreak() {
        for (int i = 0; i < 4; i++) {
            m_pillars[i].m_isSink = true;
            m_pillars[i].m_pillarDust.Play();
            yield return new WaitForSeconds(1.0f);
        }
        yield return new WaitForSeconds(1.0f);

        GetComponent<PillarSink>().m_isSink = true;
        GetComponent<PillarSink>().m_pillarDust.Play();
        m_obstacle.carving = false;
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}

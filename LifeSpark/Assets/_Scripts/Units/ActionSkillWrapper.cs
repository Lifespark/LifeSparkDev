using UnityEngine;
using System.Collections;

public class ActionSkillWrapper : LSMonoBehaviour {

    int m_attackIdx = -1;
    // wrapper of hit object for action editor
    public void receiveActionAttack(GameObject gObj) {
        //if (PhotonNetwork.isMasterClient) {
        Hit hit = gObj.GetComponent<HitObjLookUp>().hits[m_attackIdx];
        if (gObj.tag == "Player") {
            if (gameObject.tag == "Player") {
                if (gObj.GetComponent<Player>().team == GetComponent<Player>().team) return;
            }
            else if (gameObject.tag == "LaneCreep") {
                if (gObj.GetComponent<Player>().team == GetComponent<LaneCreep>().owner) return;
            }
            else if (gameObject.tag == "SparkPoint") {
                if (GetComponent<SparkPoint>().sparkPointState != SparkPoint.SparkPointState.CAPTURED ||
                    gObj.GetComponent<Player>().team == GetComponent<SparkPoint>().owner) return;
            }
        }
        else if (gObj.tag == "Boss") {

        }
        GetComponent<UnitObject>().receiveAttack(hit, gObj.transform);
        //}
    }

    public void SetAttackIndex(int idx) {
        m_attackIdx = idx;
    }
}

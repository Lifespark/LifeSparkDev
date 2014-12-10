using UnityEngine;
using System.Collections;

public class SparkPointCaptureTrigger : MonoBehaviour {

    void OnTriggerEnter(Collider col) {
        if (PhotonNetwork.isMasterClient && col.tag == "Player") {
            PlayerManager.Instance.photonView.RPC("RPC_setSparkPointCapture",
                                              PhotonTargets.All,
                                              transform.parent.name,
                                              col.name,
                                              col.GetComponent<Player>().GetTeam(),
                                              true,
                                              1.0f / (col.GetComponent<Player>().m_sparkPointCapturingTime + 0.00001f));
            //m_playerState = PlayerState.Capturing;
            //SwitchState(PlayerState.Capturing);
        }
    }

    void OnTriggerExit(Collider col) {
        if (PhotonNetwork.isMasterClient && col.tag == "Player") {
            PlayerManager.Instance.photonView.RPC("RPC_setSparkPointCapture", PhotonTargets.All,
                                                          transform.parent.name,
                                                          col.name,
                                                          col.GetComponent<Player>().GetTeam(),
                                                          false,
                                                          0f);
        }
    }
}

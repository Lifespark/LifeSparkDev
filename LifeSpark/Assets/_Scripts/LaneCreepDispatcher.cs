using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaneCreepDispatcher : LSMonoBehaviour {

    public Transform m_target;

    public bool m_hasStartedSendingCreeps = false;

    private SparkPoint m_sparkPoint;

	// Use this for initialization
	void Awake () {
        m_sparkPoint = GetComponent<SparkPoint>();
	}
	
    [RPC]
    void RPC_dispatchCreepFromSP(string pTarget, string pPlayerName, int team) {
		m_sparkPoint.m_selectedNextSparkPoint = SparkPointManager.Instance.sparkPointsDict[pTarget];
		m_target = m_sparkPoint.m_selectedNextSparkPoint.transform;
        if (!m_hasStartedSendingCreeps)
            StartCoroutine(DispatchCreep(pPlayerName, team, true));
    }

    IEnumerator DispatchCreep(string pPlayerName, int team, bool selectedPath) {

        m_hasStartedSendingCreeps = true;
        Vector3 forwardDir = (transform.position - m_target.transform.position).normalized;

        if (!CreepManager.Instance.creepDict.ContainsKey(gameObject)) {
            CreepManager.Instance.creepDict.Add(gameObject, new List<LaneCreep>());
        }

        while (m_sparkPoint.sparkPointState != SparkPoint.SparkPointState.DESTROYED) {
            for (int i = 0; i < CreepManager.Instance.m_waveCount; i++) {
                //photonView.RPC ("RPC_dispatchCreep", PhotonTargets.All, source.name, target.name, player.name, source.GetComponent<SparkPoint>().GetOwner());
                Vector3 spreadVect;
                Quaternion rot = Quaternion.AngleAxis(i * 360 / CreepManager.Instance.m_waveCount, Vector3.up);
                spreadVect = rot * forwardDir;

                DispatchCreepAlternative(pPlayerName, team, spreadVect, selectedPath);
                yield return new WaitForSeconds(CreepManager.Instance.m_perCreepTimer);
            }
            yield return new WaitForSeconds(CreepManager.Instance.m_waveTimer);
        }
    }

    void DispatchCreepAlternative(string pPlayerName, int pTeam, Vector3 pSpreadVect, bool selectedPath) {

        //Color creepColor = team == 1 ? Color.red : Color.blue;
        object[] instantiateData = { m_target.name, pTeam, pPlayerName, gameObject.name, pSpreadVect, CreepManager.Instance.m_creepCount++, (int)LaneCreep.CreepType.Ranged };

        GameObject creep = PhotonNetwork.InstantiateSceneObject("MageCreep", gameObject.transform.position + Vector3.up * 0.5f, Quaternion.identity, 0, instantiateData) as GameObject;

        LaneCreep thisCreep = creep.GetComponent<LaneCreep>();
		CreepManager.Instance.creepList.Add(thisCreep);

		if (selectedPath) {
			thisCreep.m_previousPaths[m_sparkPoint.name] = LaneCreep.PrevPath.SELECTED;
		} else {
			thisCreep.m_previousPaths[m_sparkPoint.name] = LaneCreep.PrevPath.DEFAULT;
		}
    }
}

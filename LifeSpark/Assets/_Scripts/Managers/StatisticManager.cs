using UnityEngine;
using System.Collections;

public class StatisticManager : LSMonoBehaviour {
	static private StatisticManager m_instance;
	public StatisticManager GetInstance() {
		return m_instance;
	}

	#region ENUM
	public enum TeamIdentity {
		Team1 = 0,
		Team2 = 1,
	}

	public enum PlayerIdentity {
		Player1 = 0,
		Player2 = 1,
		Player3 = 2,
		Plyaer4 = 3,
	}

	public enum KillType {
		Player = 0,
		Minion = 1,
		Jungle = 2,
		Boss = 3,
	}
	#endregion

	#region STATISTIC_STRUCT
	[System.Serializable]
	public struct PlayerStatistic{
		public bool m_using;
		//
		public int m_kill;
		public int m_death;
		public int m_minionKill;
		public int m_jungleKill;
		public int m_totalSmallKill;
		//
		public int m_SPCapture;
		public int m_SPDestroy;
		//
		public void reset() {
			m_using = false;
			//
			m_kill = 0;
			m_death = 0;
			m_minionKill = 0;
			m_jungleKill = 0;
			m_totalSmallKill = 0;
			//
			m_SPCapture = 0;
			m_SPDestroy = 0;
		}
	}

	[System.Serializable]
	public struct TeamStatistic{
		public bool m_using;
		//
		public int m_totalKill;
		public int m_totalDeath;
		public int m_bossKill;
		public int m_totalSmallKill;
		//
		public int m_totalSPCapture;
		public int m_totalSPDestroy;
		//
		public void reset() {
			m_using = false;
			//
			m_totalKill = 0;
			m_totalDeath = 0;
			m_bossKill = 0;
			m_totalSmallKill = 0;
			//
			m_totalSPCapture = 0;
			m_totalSPDestroy = 0;
		}
	}
	#endregion

	#region STATISTIC_PARAMS
	public TeamStatistic[] m_teamDatas;
	public PlayerStatistic[] m_playerDatas;
	#endregion

	public int m_teamNumber = 2;
	public int m_playerNumber = 4;
	private bool m_isInitialized = false;

	void Awake() {
		m_instance = this;
	}

	#region PUBLIC_FUNCS
	public void Initialize() {
		if(!m_isInitialized) {
			m_isInitialized = true;
			////
			m_teamDatas = new TeamStatistic[2];
			m_playerDatas = new PlayerStatistic[4];
			////
			reset();
		}
	}


	/// <summary>
	/// Reset everything.
	/// </summary>
	public void reset() {
		for(int n = 0; n < m_teamDatas.GetLength(0); n++) {
			m_teamDatas[n].reset();
		}
		for(int n = 0; n < m_playerDatas.GetLength(0); n++) {
			m_playerDatas[n].reset();
		}
	}

	public void AddKill(TeamIdentity killingTeam, PlayerIdentity killingPlayer, TeamIdentity killedTeam, PlayerIdentity killedPlayer, KillType type){
		photonView.RPC("RPC_AddKill", PhotonTargets.All, killingTeam, killingPlayer, killedTeam, killedPlayer, type);
	}

	public void CaptureSparkPoint(TeamIdentity team, PlayerIdentity player) {
		photonView.RPC("RPC_CaptureSparkPoint", PhotonTargets.All, team, player);
	}

	public void DestroySparkPoint(TeamIdentity team, PlayerIdentity player) {
		photonView.RPC("RPC_DestroySparkPoint", PhotonTargets.All, team, player);
	}
	#endregion


	#region RPC_FUNCS
	[RPC]
	void RPC_AddKill(TeamIdentity killingTeam, PlayerIdentity killingPlayer, TeamIdentity killedTeam, PlayerIdentity killedPlayer, KillType type) {
		if(type == KillType.Player) {
			m_teamDatas[(int)killingTeam].m_totalKill++;
			m_playerDatas[(int)killingPlayer].m_kill++;
			//
			m_teamDatas[(int)killedTeam].m_totalDeath++;
			m_playerDatas[(int)killedPlayer].m_death++;
		} else if(type == KillType.Boss) {
			m_teamDatas[(int)killingTeam].m_bossKill++;
		} else if(type == KillType.Minion ||
		          type == KillType.Jungle) {
			m_teamDatas[(int)killingTeam].m_totalSmallKill++;
			m_playerDatas[(int)killingPlayer].m_totalSmallKill++;
			if(type == KillType.Minion) {
				m_playerDatas[(int)killingPlayer].m_minionKill++;
			}
			if(type == KillType.Jungle) {
				m_playerDatas[(int)killingPlayer].m_jungleKill++;
			}
		}
	}

	[RPC]
	void RPC_CaptureSparkPoint(TeamIdentity team, PlayerIdentity player) {
		m_teamDatas[(int)team].m_totalSPCapture++;
		m_playerDatas[(int)player].m_SPCapture++;
	}

	[RPC]
	void RPC_DestroySparkPoint(TeamIdentity team, PlayerIdentity player) {
		m_teamDatas[(int)team].m_totalSPDestroy++;
		m_playerDatas[(int)player].m_SPDestroy++;
	}
	#endregion

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JungleCamp : MonoBehaviour {

    public List<JungleMonster> m_myJungleCreeps = new List<JungleMonster>();

    public JungleMonster.CreepType[] m_CreepTypes = 
    {
        JungleMonster.CreepType.ORC, 
        JungleMonster.CreepType.KOBOLD, 
        JungleMonster.CreepType.ORC, 
    };

    public void AssignCamp() {
        for (int i = 0; i < m_myJungleCreeps.Count; i++) {
            m_myJungleCreeps[i].m_myCamp = this;
        }
    }

    public void AggroAll() {
        for (int i = 0; i < m_myJungleCreeps.Count; i++) {
            m_myJungleCreeps[i].isOffensive = true;
        }
    }
}

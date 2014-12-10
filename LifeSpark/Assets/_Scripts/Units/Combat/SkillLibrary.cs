using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillLibrary: MonoBehaviour {

    static private SkillLibrary _instance;
    static public SkillLibrary Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(SkillLibrary)) as SkillLibrary;
            return _instance;
        }
    }

    public Skill[] m_skillSet;

	public Dictionary<string, Skill> m_skills = new Dictionary<string,Skill>();

    void Awake() {
        DontDestroyOnLoad(transform.gameObject);
        _instance = this;
        for (int i = 0; i < m_skillSet.Length; i++) {
            m_skills.Add(m_skillSet[i].m_actionName, m_skillSet[i]);
        }
    }
}

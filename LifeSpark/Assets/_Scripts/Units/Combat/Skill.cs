using UnityEngine;
using System.Collections;
using AGE;

[System.Serializable]
public class Skill {

    public string m_actionName;
    public string[] m_parameterNames;

    //wzh add some attributes
    //public int maxLevel;
    //public float[] coolDownTime;
    public float manaCost;
    public float coolDownTime;

    public void Cast(ref Action action, ref ActionHelper actionHelper, object[] parameters = null) {
        if (action != null)
            action.Stop();
        action = actionHelper.PlayAction(m_actionName);

        if (parameters != null) {
            for (int i = 0; i < m_parameterNames.Length; i++) {
                action.SetRefParam(m_parameterNames[i], parameters[i]);
            }
        }
    }

    public Skill(string actionName, string[] parameterNames) {
        m_actionName = actionName;
        m_parameterNames = parameterNames;
    }
}

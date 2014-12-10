using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class Hit {



    public ArrayList m_hitPlayerList;

    public float m_damageDuration;
    public float m_remainingDamageDuration;
    public float m_totalDamage;
    public float m_effectDuration;
    public float m_remainingEffectDuration;
    public float m_slowAmount;
    public bool m_isDiminishing;
    public int tempInt;
    public string tempString;

    public Player m_source;
    public Hashtable m_effectList;

	public bool m_isMagicDamage;
	public float m_damageBonus;//taken from the heroes' 'stat' values
	public float m_critChance;

	float m_criticalMultipier;

    public enum DamageType {
        Instant,
        OverTime
    };

    public DamageType m_damageType;
    public enum EffectType {
        None,
        Burn,
        Freeze,
        Slow,
        Bounce,
        Regen,
        Leech
    };

    public EffectType m_effectType;
	public EffectType m_primaryEffectType, m_secondaryEffectType;
    public Effect m_primaryEffect, m_secondaryEffect;

    // Use this for initialization
    public Hit() {
        m_hitPlayerList = new ArrayList();
        m_damageDuration = 5;
        m_remainingDamageDuration = 5;
        m_totalDamage = 5;
        m_effectDuration = 5;
        m_remainingEffectDuration = 5;
        m_slowAmount = 99.0f;
        m_isDiminishing = true;
        m_damageType = DamageType.OverTime;
        m_effectType = EffectType.None;
        m_primaryEffect = new Effect(m_effectType, m_effectDuration);
        m_secondaryEffect = new Effect(EffectType.None, 0);
        //m_primaryEffect = new Effect(m_effectType, m_effectDuration, m_totalDamage);
        //m_primaryEffect = new Effect(m_effectType, m_effectDuration, m_isDiminishing, m_slowAmount);
        m_effectList = new Hashtable();
        m_effectList[m_primaryEffect.m_effectType] = m_primaryEffect;

		m_isMagicDamage = false;
		m_damageBonus = 0;
		m_critChance = 0.0f;

		//this.m_criticalMultipier = CombatManager.Instance.m_criticalHitMultiplier;
    }

	public float GetTotalDamage(){
		float critRoll = UnityEngine.Random.Range (0.0f, 1.0f);

		float effectiveDamage = this.m_totalDamage + this.m_damageBonus;

		if (critRoll <= this.m_critChance)
            effectiveDamage *= CombatManager.Instance.m_criticalHitMultiplier;

		return effectiveDamage;
	}

	public void SetDamageBonusCritChance(float damageBonus, float critChance){
		this.m_damageBonus = damageBonus;
		this.m_critChance = critChance;
	}

    void ApplyDamage() {

    }

    /*void ApplyEffect() {
        if (m_effectType != EffectType.None) {
            for (tempInt = 0; tempInt < m_hitPlayerList.Count; tempInt++) {
                tempString = ((Player)m_hitPlayerList[tempInt]).playerName;
				GameObject.Find("Manager").GetPhotonView().RPC("RPC_addPlayerEffect",
				                                               PhotonTargets.All,
				                                               tempString,
				                                               m_effectList
				                                               );

            }
        }
    }*/


    public void SetEffect(Effect e) {

        if (m_primaryEffect.m_effectType == EffectType.None) {
            m_primaryEffect = e;
            //m_effectList[m_primaryEffect.m_effectType] = e;
        }
        else {
            //m_effectList[m_secondaryEffect.m_effectType] = null;
            m_secondaryEffect = e;
            //m_effectList[m_secondaryEffect.m_effectType] = e;
        }

		//m_effectType
    }

    public void AddEffect(Effect e) {
        m_effectList[e.m_effectType] = e;
    }
}

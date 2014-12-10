using UnityEngine;
using System.Collections;


///*Summary
/// Stub
/// */Summary
public class Effect {

    public float m_duration;
    public float m_remainingDuration;
    float m_slowAmount;
    float m_remainingSlowAmount;
    bool m_isDiminishing;
    float m_burnDamage;
    float m_remainingBurnDamage;
    float m_regenAmount;
    float m_remainingRegenAmount;
    float tick;

    public Hit.EffectType m_effectType;

    public Effect(Hit.EffectType effectType, float duration) {
        m_effectType = effectType;
        m_duration = duration;
        m_remainingDuration = duration;
    }
    public Effect(Hit.EffectType effectType, float duration, float burnDamage) {
        m_effectType = effectType;
        m_duration = duration;
        m_remainingDuration = duration;
        m_burnDamage = burnDamage;
        m_remainingBurnDamage = burnDamage;
    }
    public Effect(Hit.EffectType effectType, float duration, bool isDiminishing, float slowAmount) {
        m_effectType = effectType;
        m_duration = duration;
        m_remainingDuration = duration;
        m_isDiminishing = isDiminishing;
        m_slowAmount = slowAmount;
    }

    public void setEffect(Hit.EffectType effectType, float duration, bool isDiminishing, float slowAmount) {
        m_effectType = effectType;
        m_duration = duration;
        m_remainingDuration = duration;
        m_isDiminishing = isDiminishing;
        m_slowAmount = slowAmount;
    }

    public bool isOver() {
        if (m_remainingDuration <= 0) {
            return true;
        }
        else {
            return false;
        }
    }

    public float applyBurn(float deltaTime) {
        tick = m_burnDamage * deltaTime;
        if (tick > m_remainingBurnDamage) {
            m_remainingDuration = 0;
            return m_remainingBurnDamage;
        }
        else {
            m_remainingBurnDamage -= tick;
            m_remainingDuration -= deltaTime;
            return tick;
        }
    }

    public void applyFreeze(float deltaTime) {
        Debug.Log("Remaining freeze time: " + m_remainingDuration);
        m_remainingDuration -= deltaTime;
    }

    public float applySlow(float deltaTime) {
        if (m_isDiminishing) {
            m_remainingSlowAmount = m_slowAmount * m_remainingDuration / m_duration;
            m_remainingDuration -= deltaTime;
            if (m_remainingDuration < 0) {
                m_remainingDuration = 0;
            }
            Debug.Log("slow time remaining is " + m_remainingDuration);
            return m_remainingSlowAmount;
        }
        else {
            m_remainingDuration -= deltaTime;
            if (m_remainingDuration < 0) {
                m_remainingDuration = 0;
            }
            return m_slowAmount;
        }
    }

    public void applyBounce(float deltaTime) {
        m_remainingDuration -= deltaTime;
    }

    public float applyRegen(float deltaTime) {
        tick = m_regenAmount * deltaTime;
        if (tick > m_remainingRegenAmount) {
            m_remainingDuration = 0;
            return m_remainingRegenAmount;
        }
        else {
            m_remainingRegenAmount -= tick;
            m_remainingDuration -= deltaTime;
            return tick;
        }
    }


}

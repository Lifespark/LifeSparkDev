using UnityEngine;
using System.Collections;

public class Hit : MonoBehaviour {

    public float damageDuration;
    public float remainingDuration;
    public float totalDamage;

    public enum DamageType {
        Instant,
        OverTime
    }

    public DamageType damageType;

    public enum EffectType {
        Stun,
        Slow,
        Freeze,
        Burn,
        Snare
    }

    public EffectType effectType;

	// Use this for initialization
	void Start () {
        damageDuration = 5;
        remainingDuration = 5;
        totalDamage = 5;
        damageType = DamageType.OverTime;
        effectType = EffectType.Slow;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void ApplyDamage() {
        
    }
}

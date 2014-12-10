using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnitObject : LSMonoBehaviour {

    [Flags]
    public enum PacketMask {
        NONE = 0,
        POSITION = 1,
        ROTATION = 2,
        VELOCITY = 4,
    }

	public enum AnimatorType {
		BOOL = 1,
		INT = 2,
		FLOAT = 3,
		TRIGGER = 4
	}

    public enum UnitObjectType {
        STATIC = 0,
        DYNAMIC = 1,
    }

	#region UNIT_STATS
	public int m_physicalAttack;
	public int m_physicalDefense;
	public int m_magicAttack;
	public int m_magicDefense;
	public float m_critChance;
	#endregion

    protected Vector3 correctUnitPos;
    protected Quaternion correctUnitRot;

    protected Vector3 lastCorrectUnitPos;
    protected Quaternion lastCorrectUnitRot;

    protected Vector3 onUpdateUnitPos;
    protected Quaternion onUpdateUnitRot;

    protected float lastUpdateTime;
    protected Vector3 lastSentPosition;
    protected Quaternion lastSentOrientation;
    protected Vector3 lastSentVelocity;
	protected Transform lastAttacker;
	private int Team1DMG;
	private int Team2DMG;

    protected bool appliedInitialUpdate = false;
    protected float fraction = 0;

	protected CombatManager combatManager;
    protected NavMeshAgent m_navAgent;

    public UnitObjectType m_unitType = UnitObjectType.STATIC;
    public float maxHealth;
	public float unitHealth;
	public int baseAttack;

    public SkinnedMeshRenderer m_silhouetteRenderer;

	public Attack m_basicAttack;

    public Dictionary<Hit.EffectType, object> effectList;
	public float slowAmount;
	public bool isFrozen;
	public bool isKnockedUp;

	public bool m_isDead;

    float tempSlowAmount;
	protected int tempInt;//Protected to be used in derived classes too
    protected ArrayList tempArrayList;
    protected List<Effect> tempEffectList;
    protected Dictionary<Hit.EffectType, object> tempHash;
    bool atkrecd = false;

	public Transform m_effectContainer;


	// Use this for initialization
	public virtual void Awake () {
        combatManager = CombatManager.Instance;
        m_navAgent = GetComponent<NavMeshAgent>();
        if (m_navAgent) {
            m_navAgent.enabled = true;
			m_navAgent.ResetPath();
		}

        effectList = new Dictionary<Hit.EffectType, object>();
        effectList.Add(Hit.EffectType.Burn, new ArrayList());
        effectList.Add(Hit.EffectType.Slow, new ArrayList());
        effectList.Add(Hit.EffectType.Regen, new ArrayList());
        effectList.Add(Hit.EffectType.Freeze, null);
        effectList.Add(Hit.EffectType.Bounce, null);
        effectList.Add(Hit.EffectType.Leech, null);
        slowAmount = 0;
		isFrozen = false;
		isKnockedUp = false;
		Team1DMG = 0;
		Team2DMG = 0;
        tempSlowAmount = 0;
        tempInt = 0;
        tempArrayList = new ArrayList();
        tempEffectList = new List<Effect>();

		// Init correct params.
		correctUnitPos = this.transform.position;
		correctUnitRot = this.transform.rotation;

		m_isDead = false;

		/*m_physicalAttack = 0;
		m_physicalDefense = 0;
		m_magicAttack = 0;
		m_magicDefense = 0;
		m_critChance = 0;*/

	}
	
	// Update is called once per frame

	protected void Update () {
        if (!LevelManager.Instance.m_startedGame) return;
        if (!PhotonNetwork.isMasterClient && m_unitType == UnitObjectType.DYNAMIC) {
            //fraction = fraction + Time.deltaTime * 14;
            //
            //transform.position = Vector3.Lerp(onUpdateUnitPos, this.correctUnitPos, fraction);
            //transform.rotation = Quaternion.Lerp(onUpdateUnitRot, this.correctUnitRot, fraction);

            transform.position = Vector3.Lerp(transform.position, this.correctUnitPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctUnitRot, Time.deltaTime * 5);


            //transform.position = Vector3.Lerp(lastCorrectUnitPos, this.correctUnitPos, (Time.time - lastUpdateTime) / (1.0f / 10));
            //transform.rotation = Quaternion.Lerp(lastCorrectUnitRot, this.correctUnitRot, (Time.time - lastUpdateTime) / (1.0f / 10));
        }

        // Add effects from temp list into actual list
        foreach (Effect e in tempEffectList) {
            Debug.Log("adding from temp effect list");
            AddEffect(e);
        }
        tempEffectList.Clear();
		UpdateEffects();

        // fix object on ground. otherwise would hover for unknown reason when attack
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;
	}
	
	public void UnitUpdate () {
		if (unitHealth <= 0) {
            // Commented this out so that the respawn timer on Player.cs will work
			//this.enabled = false;
			

		}
	}

	//Distributes damage evenly over a duration of time. It keeps looping untill there is no time left.
	//Input: The amount of damage to distribute and how long it should run
	IEnumerator DamageOverTime(int damageRemaining, int timeRemaining, int team) {
		if (timeRemaining>0) {
			float damageToInflict = damageRemaining / timeRemaining;
			unitHealth -= damageToInflict;

			if (team == 1)
				Team1DMG += (int)damageToInflict;
			else if (team == 2)
				Team2DMG += (int) damageToInflict;

            if (PhotonNetwork.isMasterClient)
                photonView.RPC("UpdateAllClientHealth", PhotonTargets.AllBuffered, unitHealth);
			yield return new WaitForSeconds (1);
			StartCoroutine (DamageOverTime((int)(damageRemaining-damageToInflict),(int)(timeRemaining-1), team));
		} else {
			unitHealth -= damageRemaining;
			if (team == 1)
				Team1DMG += (int) damageRemaining;
			else if (team == 2)
				Team2DMG += (int) damageRemaining;
            if (PhotonNetwork.isMasterClient)
                photonView.RPC("UpdateAllClientHealth", PhotonTargets.OthersBuffered, unitHealth);
		}
	}

	private int setEffect(int effectType) {
		int fx = 0;
		switch(effectType) {
			case (int)Hit.EffectType.None: fx = (int)CombatManager.StatusEffects.BLOOD; break;
			case (int)Hit.EffectType.Burn: fx = (int)CombatManager.StatusEffects.FIRE; break;
			case (int)Hit.EffectType.Freeze: fx = (int)CombatManager.StatusEffects.ICE; break;
			case (int)Hit.EffectType.Slow: fx = (int)CombatManager.StatusEffects.EARTH; break;
			case (int)Hit.EffectType.Bounce: fx = (int)CombatManager.StatusEffects.AIR; break;
			case (int)Hit.EffectType.Regen: fx = (int)CombatManager.StatusEffects.LIGHT; break;
			case (int)Hit.EffectType.Leech: fx = (int)CombatManager.StatusEffects.DARK; break;
		}
		return fx;
	}


	//Starts distributing damage over time
	//Input: Origional amount of damage to be done and origional amount of time to deal it in
	public void receiveAttack(Hit hit, Transform attacker) {
//         if (m_unitType == UnitObjectType.DYNAMIC)
//             transform.LookAt(attacker.transform.position - new Vector3(0, attacker.transform.position.y - transform.position.y, 0));

		if (this.tag != "SparkPoint") {

            Debug.Log("HIT EFFECT TYPE: " + hit.m_primaryEffectType);
            Debug.Log("SECOND EFFECT TYPE: " + hit.m_secondaryEffectType);
            int primaryfx = setEffect((int)hit.m_primaryEffectType);
            int secondaryfx = setEffect((int)hit.m_secondaryEffectType);


//             GameObject effect1 = PhotonNetwork.InstantiateSceneObject(combatManager.m_statusFXNames[primaryfx],
//                                                             m_effectContainer.position,
//                                                             Quaternion.identity,
//                                                             0,
//                                                             null);
             GameObject effect1 = GameObject.Instantiate(Resources.Load(combatManager.m_statusFXNames[primaryfx]),
                                                             m_effectContainer.position,
                                                             Quaternion.identity) as GameObject;


            if (combatManager.m_statusPositions[primaryfx] == 0) {
                effect1.transform.parent = m_effectContainer;
            }
            else if (combatManager.m_statusPositions[primaryfx] == 1) {
                effect1.transform.parent = this.transform;
            }

            effect1.transform.localPosition = Vector3.zero;

//             GameObject effect2 = PhotonNetwork.InstantiateSceneObject(combatManager.m_statusFXNames[secondaryfx],
//                                                             m_effectContainer.position,
//                                                             Quaternion.identity,
//                                                             0,
//                                                             null);
            GameObject effect2 = GameObject.Instantiate(Resources.Load(combatManager.m_statusFXNames[secondaryfx]),
                                                m_effectContainer.position,
                                                Quaternion.identity) as GameObject;

            if (combatManager.m_statusPositions[secondaryfx] == 0) {
                effect2.transform.parent = m_effectContainer;
            }
            else if (combatManager.m_statusPositions[secondaryfx] == 1) {
                effect2.transform.parent = this.transform;
            }

            effect2.transform.localPosition = Vector3.zero;
			if(PlayerManager.Instance.myPlayer.GetComponent<Player>() == this){
				UIManager.mgr.OnAttack();
			}
		}

//		GameObject effect2 = GameObject.Instantiate(combatManager.m_statusFX[fx]) as GameObject;
//		//effect.transform.position = m_effectContainer.position;
//		effect2.transform.parent = m_effectContainer;
//		effect2.transform.localPosition = Vector3.zero;

        if (PhotonNetwork.isMasterClient) {
            lastAttacker = attacker;
            if (gameObject.tag == "JungleCreep") {
                JungleMonster monster = this as JungleMonster;
                monster.m_myCamp.AggroAll();
            }


            int tempDuration = (int)hit.m_damageDuration;
            int tempDamage = (int)hit.GetTotalDamage();

			//apply damage reduction
			int reductionAmount = hit.m_isMagicDamage ? this.m_magicDefense : this.m_physicalDefense;
			tempDamage -= reductionAmount;

            if (hit.m_damageType == Hit.DamageType.OverTime) {
                StartCoroutine(DamageOverTime(tempDamage, tempDuration, attacker.GetComponent<Player>().GetTeam()));
            }
            else {
				// Debug.Log(this.gameObject.tag + ":Decrease unit health.");
                unitHealth -= tempDamage;

                if (attacker.tag == "Player") {
                    if (attacker.GetComponent<Player>().GetTeam() == 1)
                        Team1DMG += tempDamage;
                    else
                        Team2DMG += tempDamage;
                }
                if (PhotonNetwork.isMasterClient)
                    photonView.RPC("UpdateAllClientHealth", PhotonTargets.AllBuffered, unitHealth);
            }
            if (hit.m_primaryEffect.m_effectType != Hit.EffectType.None) {
                tempEffectList.Add(hit.m_primaryEffect);
                if (hit.m_secondaryEffect.m_effectType != Hit.EffectType.None) {
                    tempEffectList.Add(hit.m_secondaryEffect);
                }
            }

            //Debug.Log("receiving attack");
            // Add effects to temp list to avoid concurrency issues
            //         foreach (DictionaryEntry e in hit.m_effectList) {
            //             tempEffectList.Add((Effect)e.Value);
            //             Debug.Log("Adding effect " + ((Effect)e.Value).m_effectType + ((Effect)e.Value).m_duration + " " + ((Effect)e.Value));
            // 			//AddEffect((Effect)e.Value);
            //         }
            atkrecd = true;
        }
	}
	/// <summary>
	/// Gets the % of Damage done by the team specified
	/// </summary>
	/// <param name="team">Team.</param>
	public float GetTeamDMG(int team)
	{
		float result = 100;
		if (team == 1)
			result = Team1DMG / unitHealth;
		else if (team == 2)
			result = Team2DMG / unitHealth;

		return result;

	}

    /// <summary>
    /// photon syncing
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (!LevelManager.Instance.m_startedGame) return;

        if (m_unitType == UnitObjectType.DYNAMIC) {
            if (stream.isWriting) {

                // reduce network traffic
                PacketMask mask = PacketMask.NONE;

                if (transform.position != lastSentPosition) {
                    lastSentPosition = transform.position;
                    mask |= PacketMask.POSITION;
                }

                if (transform.rotation != lastSentOrientation) {
                    lastSentOrientation = transform.rotation;
                    mask |= PacketMask.ROTATION;
                }

                //             if (rigidbody != null && rigidbody.velocity != lastSentVelocity) {
                //                 lastSentVelocity = rigidbody.velocity;
                //                 mask |= PacketMask.VELOCITY;
                //             }

                stream.SendNext(mask);

                // only send changed attributes
                if ((mask & PacketMask.POSITION) != PacketMask.NONE) stream.SendNext(transform.position);
                if ((mask & PacketMask.ROTATION) != PacketMask.NONE) stream.SendNext(transform.rotation);
                //            if ((mask & PacketMask.VELOCITY) != PacketMask.NONE) stream.SendNext(rigidbody.velocity);
            }
            else {
                lastUpdateTime = Time.time;
                PacketMask mask = (PacketMask)stream.ReceiveNext();

                if ((mask & PacketMask.POSITION) != PacketMask.NONE) {
                    onUpdateUnitPos = transform.position;
                    lastCorrectUnitPos = correctUnitPos;
                    correctUnitPos = (Vector3)stream.ReceiveNext();
                }
                if ((mask & PacketMask.ROTATION) != PacketMask.NONE) {
                    onUpdateUnitRot = transform.rotation;
                    lastCorrectUnitRot = correctUnitRot;
                    correctUnitRot = (Quaternion)stream.ReceiveNext();
                }
                //             if (rigidbody != null && (mask & PacketMask.VELOCITY) != PacketMask.NONE) {
                //                 rigidbody.velocity = (Vector3)stream.ReceiveNext();
                //             }

                if (!appliedInitialUpdate) {
                    appliedInitialUpdate = true;
                    transform.position = correctUnitPos;
                    transform.rotation = correctUnitRot;
                    lastCorrectUnitPos = correctUnitPos;
                    lastCorrectUnitRot = correctUnitRot;
                    //                rigidbody.velocity = Vector3.zero;
                }
                fraction = 0;
            }
        }
    }

	public void AddEffect(Effect effect) {
        // Check for stackable effects first
        if (effect.m_effectType == Hit.EffectType.Burn || 
            effect.m_effectType == Hit.EffectType.Slow || 
            effect.m_effectType == Hit.EffectType.Regen) {
            ((ArrayList)effectList[effect.m_effectType]).Add(effect);
            Debug.Log("new array size is " + ((ArrayList)effectList[effect.m_effectType]).Count);
        }
        else if (effectList[effect.m_effectType] == null || effect.m_duration >= ((Effect)effectList[effect.m_effectType]).m_remainingDuration) {
            effectList[effect.m_effectType] = effect;
            //Debug.Log("adding effect to " + this.name);
        }
		// else effects duration is less than the current duration and would be counterproductive to apply.
	}
	
	public void UpdateEffects() {

        // so laggy don't do it!
        return;

        tempHash = new Dictionary<Hit.EffectType, object>(effectList);
        int i = 0;
        foreach (KeyValuePair<Hit.EffectType, object> entry in effectList) {
            i++;
            if (entry.Key == Hit.EffectType.Burn && ((ArrayList)entry.Value).Count != 0) {
                tempArrayList = (ArrayList)effectList[Hit.EffectType.Burn];
                for (tempInt = 0; tempInt < tempArrayList.Count; tempInt++) {
                    unitHealth -= ((Effect)tempArrayList[tempInt]).applyBurn(Time.deltaTime);
                    if (((Effect)tempArrayList[tempInt]).isOver()) {
                        tempArrayList.RemoveAt(tempInt);
                        tempInt--;
                    }
                }
                tempHash[Hit.EffectType.Burn] = (ArrayList)tempArrayList.Clone();
                tempArrayList.Clear();
            }
            if (entry.Key == Hit.EffectType.Freeze && entry.Value != null) {
                isFrozen = true;
                ((Effect)effectList[Hit.EffectType.Freeze]).applyFreeze(Time.deltaTime);
                if (((Effect)effectList[Hit.EffectType.Freeze]).isOver()) {
                    isFrozen = false;
                    tempHash[Hit.EffectType.Freeze] = null;
                }
            }
            if (entry.Key == Hit.EffectType.Slow && ((ArrayList)entry.Value).Count != 0) {
                //Debug.Log("Slow found");
                tempArrayList = (ArrayList)effectList[Hit.EffectType.Slow];
                slowAmount = 0;
                for (tempInt = 0; tempInt < tempArrayList.Count; tempInt++) {
                    if (tempInt >= 1) {
                        Debug.Log("omg yay " + ((Effect)tempArrayList[tempInt]).m_remainingDuration);
                    }
                    tempSlowAmount = ((Effect)tempArrayList[tempInt]).applySlow(Time.deltaTime);
                    // Only apply the strongest slow
                    //Debug.Log("Checking current slow of " + slowAmount + " against new slow of " + tempSlowAmount);
                    if (slowAmount < tempSlowAmount) {
                        slowAmount = tempSlowAmount;
                        //Debug.Log("new slow amount: " + slowAmount);
                    }
                    if (((Effect)tempArrayList[tempInt]).isOver()) {
                        tempArrayList.RemoveAt(tempInt);
                        tempInt--;
                    }
                }
                tempHash[Hit.EffectType.Slow] = (ArrayList)tempArrayList.Clone();
                if (tempArrayList.Count == 0) {
                    Debug.Log("no more slows");
                    slowAmount = 0;
                }
                tempArrayList.Clear();
            }
            if (entry.Key == Hit.EffectType.Bounce && entry.Value != null) {
                isKnockedUp = true;
                this.transform.Translate(0, 3.0f, 0, gameObject.transform);
				((Effect)effectList[Hit.EffectType.Bounce]).applyBounce(Time.deltaTime);
                if (((Effect)effectList[Hit.EffectType.Bounce]).isOver()) {
                    isKnockedUp = false;
                    this.transform.Translate(0, -3.0f, 0, gameObject.transform);
                    tempHash[Hit.EffectType.Bounce] = null;
                }
            }
            if (entry.Key == Hit.EffectType.Regen && ((ArrayList)entry.Value).Count != 0) {
                tempArrayList = (ArrayList)effectList[Hit.EffectType.Regen];
                for (tempInt = 0; tempInt < tempArrayList.Count; tempInt++) {
                    unitHealth += ((Effect)tempArrayList[tempInt]).applyRegen(Time.deltaTime);
                    if (unitHealth > maxHealth) {
                        unitHealth = maxHealth;
                    }
                    if (((Effect)tempArrayList[tempInt]).isOver()) {
                        tempArrayList.RemoveAt(tempInt);
                        tempInt--;
                    }
                }
                tempHash[Hit.EffectType.Regen] = (ArrayList)tempArrayList.Clone();
                tempArrayList.Clear();
            }
        }
        effectList[Hit.EffectType.Burn] = tempHash[Hit.EffectType.Burn];
        effectList[Hit.EffectType.Freeze] = tempHash[Hit.EffectType.Freeze];
        effectList[Hit.EffectType.Slow] = tempHash[Hit.EffectType.Slow];
        effectList[Hit.EffectType.Bounce] = tempHash[Hit.EffectType.Bounce];
        effectList[Hit.EffectType.Regen] = tempHash[Hit.EffectType.Regen];
	}

    /// <summary>
    /// synchronize health between all client from master client. No non-master client should manage health on its own
    /// </summary>
    /// <param name="health"></param>
    [RPC]
    public void UpdateAllClientHealth(float health) {
        unitHealth = health;
    }

    /// <summary>
    /// Used for knock back skills
    /// </summary>
    /// <param name="attacker"></param>
    public void NavMeshMove(GameObject attacker) {
        if (m_navAgent) {
            if (gameObject.tag == "Player") {
                if (attacker.GetComponent<Player>().team == GetComponent<Player>().team) return;
            }
            else if (gameObject.tag == "LaneCreep") {
                if (attacker.GetComponent<Player>().team == GetComponent<LaneCreep>().owner) return;
            }
            StartCoroutine(MoveOnNavMesh(attacker.transform.position));
        }
    }

    IEnumerator MoveOnNavMesh(Vector3 srcPos) {
        Vector3 dir = transform.position - srcPos;
        dir.Normalize();
       
        float movedDistance = 0;

        while (movedDistance < 10) {
            float deltaMove = 10.0f * Time.deltaTime;
            movedDistance += deltaMove;
            m_navAgent.Move(dir * deltaMove);
            yield return null;
        }
    }
}

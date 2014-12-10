//------------------------------------------------------------------------------
//BossType Class which contains all sub Boss Classes
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Bosstype : LSMonoBehaviour
{
    public enum Name
    {
        ZUTSU
    }

	//List Bosses here
	public Name m_name;
    protected int m_tier;
	protected bool m_alive;
    public bool m_hasPeriodicEvent;

	public Bosstype(Bosstype.Name name, bool hasPeriodicEvent)
	{
        m_name = name;
        m_hasPeriodicEvent = hasPeriodicEvent;
	}

    public virtual void Initialize() {
    }

    public void OnTierChange(int tier)
    {
        m_tier = tier;
        TierBehavior(tier);
    } 
   
    public virtual void OnPeriodicEvent() {
    }

	public virtual bool TierBehavior(int tier){
		return true;
	}

}







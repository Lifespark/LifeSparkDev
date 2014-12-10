using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Zutsu : Bosstype
	{
		//private int m_Tier = 0;
		
		//Web Locations
		public List<Web>[] m_webTiers = new List<Web>[3];
        public List<Web> m_webs = new List<Web>();

		public Zutsu () : base(Bosstype.Name.ZUTSU, true/*hasPeriodicEvent*/)
		{
			m_alive = true;
            UnityEngine.Random.seed = (int)Time.time;
		}
        
		private bool WebBehavior(int tierIndex)
		{

            Debug.Log("WebBehavior!");

            int webChoice = UnityEngine.Random.Range(0, m_webs.Count-1);
		    Web web = m_webs[webChoice];
            m_webs.RemoveAt(webChoice);
            web.SetEnabled(true);
			
			return true;
		}

        public override void Initialize() {
            InitWebs();
        }

        public void InitWebs() {
            Debug.Log("InitWebs");
            GameObject[] webObjects = GameObject.FindGameObjectsWithTag("ZutsuWeb");

            foreach (GameObject webObject in webObjects) {
                Web web = webObject.GetComponent<Web>();

                int tierIndex = web.m_tier - 1;
                if (m_webTiers[tierIndex] == null) {
                    m_webTiers[tierIndex] = new List<Web>();
                }
                m_webTiers[tierIndex].Add(web);
            }
        }

        public override void OnPeriodicEvent() {
            Debug.Log("Zutsu Periodic Action: tier " + m_tier);
            if (m_tier != 0) {
                Debug.Log(m_tier);
                WebBehavior(m_tier-1);
            }
        }

		public override bool TierBehavior (int tier) {
            m_webs.AddRange(m_webTiers[tier - 1]);
            return true;
		}
	}



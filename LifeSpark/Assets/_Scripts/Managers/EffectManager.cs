using UnityEngine;
using System.Collections;

public class EffectManager : LSMonoBehaviour {

	#region EFFECT PREFABS
	public GameObject m_slowEffect;
	#endregion

	static private EffectManager _instance;
	static public EffectManager Instance {
		get {
			if (_instance == null)
				_instance = FindObjectOfType(typeof(EffectManager)) as EffectManager;
			return _instance;
		}
	}




	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

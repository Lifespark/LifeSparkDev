using UnityEngine;
using System.Collections;

public class CombatManager : LSMonoBehaviour {



	public enum StatusEffects {
		BLOOD = 0,
		FIRE = 1,
		FIREBURN = 2,
		DARK = 3,
		DARKLEECH = 4,
		LIGHT = 5,
		LIGHTHEAL = 6,
		EARTH = 7,
		AIR = 8,
		ICE = 9,
		ZUTSUGOOP = 10
	}

	public Object[] m_statusFX;			//blood, fire, fire burn, dark, dark leech, light, light heal, earth, air, zutsu
	public string[] m_statusFXNames;

	public float m_criticalHitMultiplier;
	//0 for use ElementContainer, 1 for use unit base, 2 requires a ParticleDirection Target to be set
	public int[] m_statusPositions = { 0, 0, 0, 0,
										2, 0, 0, 1,
										1, 0, 1 };			

    static private CombatManager _instance;
    static public CombatManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(CombatManager)) as CombatManager;
            return _instance;
        }
    }

    private GameObject tempPlayer;
    private GameObject tempTarget;

	private NetworkManager m_networkManager;
	private bool m_handledGameStartup = false;

	// Use this for initialization
	void Awake () {
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public enum AttackIndex {
		BASIC = -1,
		LINE = 0,
		AREATARGETED = 2,
		AREANOVA = 3
	};

	/// <summary>
	/// Create missile targeted at player with targetName shot by attacker Name
	/// </summary>
	[RPC]
	void RPC_ShootMissile(string attackerName, string targetName){
		
		//Create missile targeted at player with targetName, store attacking player name in the missile
		GameObject attacker = GameObject.Find("Players/" + attackerName);
		GameObject target = GameObject.Find("Players/" + targetName);

		Player attackerPlayer = attacker.GetComponent<Player>();
		
		//missile prefab stored in player or missile itself?
		GameObject missile = (GameObject)Instantiate(attackerPlayer.missilePrefab, attackerPlayer.transform.position, 
		                                            Quaternion.LookRotation(target.transform.position - attackerPlayer.transform.position));
		//missile.GetComponent<Renderer>().material.color = (attackerPlayer.team == 1) ? Color.red : Color.blue;
		missile.renderer.material.color = Element.setElementVisualEffect(attackerPlayer.m_elementalEnchantment);

		Projectile missileProjectile = missile.GetComponent<Projectile>();
		missileProjectile.m_owner = attacker;
		missileProjectile.m_target = target;
		//other properties set according to attacker properties
	}

	/// <summary>
	/// projectile hits, target of the projectile takes damage based on the attack
	/// </summary>
	public void MissileHit(string attackerName, string targetName){

		Debug.Log(attackerName + " hit " + targetName);

		GameObject tempPlayer;
		tempPlayer = GameObject.Find ("Players/" + attackerName);
		



		GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
		                                                 PhotonTargets.All,
		                                                 targetName,
		                                                 attackerName,
		                                                 (int)AttackIndex.BASIC);
		
	}

	/// <summary>
	/// Visualization of line attack
	/// </summary>
	/// <param name="attackerName">Attacker, where we retrieve position, prefab to use, and attack distance .</param>
	/// <param name="targetDir">Target direction to cast attack.</param>
	/// <param name="targetSrc">Target source - for now the attack originates from the attacker, but this may change in the future.</param>
	[RPC]
	public void RPC_lineAttackVisualization (string attackerName, Vector3 targetSrc, Vector3 targetDir) {

		//now uses prefabs for visual effect
		//consider using projections to handle irregular terrain?
		GameObject attacker = GameObject.Find ("Players/" + attackerName);
		Player attackerPlayer = attacker.GetComponent<Player>();

		//player pos is 1.2 units above ground, reset prefab y value to zero
		Vector3 realPos = targetSrc; 
		realPos.y = 0.0f;	

		//instantiate, scales, and kills the effect - scaling needs to be moved to update for animated effects (eg. growing)
		//would not be necessary if prefab itself is an animated object
		//GameObject lineAttack = (GameObject)PhotonNetwork.Instantiate(attackerPlayer.lineAttackPrefab.name, realPos, Quaternion.LookRotation(targetDir), 0);
		GameObject lineAttack = (GameObject)Instantiate(attackerPlayer.lineAttackPrefab, realPos, Quaternion.LookRotation(targetDir));
		lineAttack.transform.localScale = (new Vector3(1, 1, attackerPlayer.GetLineAttack().m_range));//attackObject.m_range));

		//temporary coloring mechanic
		lineAttack.GetComponentInChildren<Renderer>().material.color = (attackerPlayer.team == 1) ? Color.red : Color.blue;

		//lifetime is temporary set to 1 second - should set a value either in Player component or an attackType Object
		Destroy (lineAttack, attackerPlayer.GetLineAttack().m_duration);//attackObject.m_duration);
		//PhotonNetwork.Destroy(lineAttack.GetPhotonView());

	}

	/// <summary>
	/// Visualization of Area Attacks
	/// </summary>
	/// <param name="attackerName">Attacker - radius of AOE is retrieved.</param>
	/// <param name="targetPt">Target location.</param>
	[RPC]
	public void RPC_areaAttackVisualization(string attackerName, Vector3 targetPt) {

		GameObject attacker = GameObject.Find ("Players/" + attackerName);
		Player attackerPlayer = attacker.GetComponent<Player>();

		Vector3 realPos = targetPt; 
		realPos.y = 0.0f;	

		//GameObject areaAttack = (GameObject)PhotonNetwork.Instantiate(attackerPlayer.areaAttackPrefab.name, realPos, Quaternion.identity, 0);
		GameObject areaAttack = (GameObject)Instantiate(attackerPlayer.areaAttackPrefab, realPos, Quaternion.identity);
		areaAttack.transform.localScale = (new Vector3(attackerPlayer.GetAreaAttack().m_radius, 0.01f, attackerPlayer.GetAreaAttack().m_radius));

		//temporary coloring mechanic
		areaAttack.renderer.material.color = (attackerPlayer.team == 1) ? Color.red : Color.blue;

		Destroy(areaAttack, attackerPlayer.GetAreaAttack().m_duration);

	}

	public void LineAttack(string attackerName, Vector3 endLocation, LineAttack attackObject) {

		tempPlayer = GameObject.Find ("Players/" + attackerName);
		//Makes sure the ray is to the middle of the capsule
		Vector3 startLocation = new Vector3(tempPlayer.transform.position.x,tempPlayer.GetComponent<CapsuleCollider>().bounds.size.y/2,tempPlayer.transform.position.z);
		endLocation.y = tempPlayer.GetComponent<CapsuleCollider>().bounds.size.y/2;
		
		int attackerTeam = tempPlayer.GetComponent<Player>().team;

		RaycastHit[] hits;
		
		Vector3 heading = endLocation - startLocation;
		Vector3 direction = (heading / heading.magnitude);//*tempPlayer.GetComponent<Player>().lineAttackDist;
		//Just to show how far the attack reaches for now
		//Debug.DrawRay (startLocation, direction *attackObject.m_range, Color.white);
		photonView.RPC ("RPC_lineAttackVisualization",
		                PhotonTargets.All,
		                attackerName,
		                startLocation,
		                direction);
		
		hits = Physics.RaycastAll(startLocation, direction, attackObject.m_range);
		for (int n=0; n <hits.Length; n++) {
			if (hits[n].collider.tag=="Player" && (hits[n].collider.GetComponentInParent<Player>().team != attackerTeam)) {
				
				Debug.Log("Enemy Hit");
				GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
				                                                 PhotonTargets.All,
				                                                 hits[n].collider.name,
				                                                 attackerName,
				                                                 (int) AttackIndex.LINE);
			}
		}

		StartCoroutine(tempPlayer.GetComponent<Player>().coolLineAttack());

	}

	public void AreaAttack(string attackerName, Vector3 location, AreaAttack attackObject) {
	
		tempPlayer = GameObject.Find ("Players/" + attackerName);
		//Makes sure the ray doesnt hit the ground
		location.y = tempPlayer.transform.position.y;

		int attackerTeam = tempPlayer.GetComponent<Player>().team;

		
		GameObject[] entities = GameObject.FindGameObjectsWithTag("Player");
		Debug.DrawRay (location, Vector3.forward * attackObject.m_radius, Color.black);
		Debug.DrawRay (location, Vector3.left * attackObject.m_radius, Color.red);
		Debug.DrawRay (location, Vector3.back * attackObject.m_radius, Color.red);
		Debug.DrawRay (location, Vector3.right * attackObject.m_radius, Color.black);
		
		photonView.RPC ("RPC_areaAttackVisualization",
		                PhotonTargets.All,
		                attackerName,
		                location);
		
		for (int x=0; x<entities.Length; x++) {
			if(entities[x].GetComponent<Player>().team == attackerTeam) continue;
			float dist = Vector3.Distance(location,entities[x].transform.position);
			if (dist < attackObject.m_radius) {
				Debug.Log("Enemy Hit");
				GameObject.Find ("Ground").GetPhotonView ().RPC ("RPC_setPlayerAttack",
				                                                 PhotonTargets.All,
				                                                 entities[x].collider.name,
				                                                 attackerName,
				                                                 (int) (attackObject.m_isPlayerOrigin ? AttackIndex.AREANOVA : AttackIndex.AREATARGETED));
			}
		}

		StartCoroutine(tempPlayer.GetComponent<Player>().coolAreaAttack());

	}


	
}

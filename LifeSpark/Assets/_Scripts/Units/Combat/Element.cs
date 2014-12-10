using UnityEngine;
using System.Collections;

public class Element : LSMonoBehaviour {
	
	public enum ElementType {
		
        Fire = 0,
		Ice = 1,
        Air = 2,
        Earth = 3,
        Light = 4,
        Dark = 5,
		Water,
		Lightning,
        None,
		Replen,
		
		
		
		Diamond,
		DarkMatter,
		HellFire,
		Lava,
		Demigod,
		Burn,
		Steam,
		Angelfire,
		Freeze,
		Acid,
		Holywater,
		Maelstrom,
		Plasma,
		Bounce,
		Tornado,
		Life,Gravity,
		Antimatter,
		Leech,
        Slow,
	};

	public const int ElementTypeCount = 6;
	//public Material[] m_materials;

	public ElementType m_elementType;

	void Awake () {
		if(photonView != null && photonView.instantiationData != null) {
			m_elementType = (ElementType)photonView.instantiationData[0];
		}
		//this.renderer.material.color = setElementVisualEffect(m_elementType);
		//this.renderer.material = m_materials[(int)m_elementType];
		DroppableManager dm = GameObject.Find("Manager").GetComponent<DroppableManager>();

//		this.renderer.material = 
//			dm.m_ElementMaterials[(int)m_elementType];
	
		this.transform.position += (Vector3.up*5);

		GameObject fx = (GameObject)GameObject.Instantiate(dm.m_ElementFX[(int)m_elementType]);
		//fx.GetComponent<ParticleSystem>().startLifetime = 0.5f;
		if(fx) {
			fx.transform.parent = this.transform;
			fx.transform.localPosition = Vector3.zero;
		}
	}

	// Use this for initialization
	void Start () {

	}

	public static string getSpriteName(ElementType elementType){
		switch (elementType) {
		case ElementType.None:
			return "none";
		case ElementType.Fire:
			return "fire";
		case ElementType.Air:
			return "air";
		case ElementType.Dark:
			return "dark";
		case ElementType.Earth:
			return "earth";
		case ElementType.Light:
			return "light";
		case ElementType.Water:
			return "water";
		case ElementType.DarkMatter:
			return "darkMatter";
		case ElementType.Demigod:
			return "demigod";
		case ElementType.Diamond:
			return "diamond";
		case ElementType.HellFire:
			return "hellfire";
		case ElementType.Lava:
			return "lava";
		case ElementType.Lightning:
			return "lightning";

		
		default:
			return null;
		}

	}

	public static Color setElementVisualEffect(ElementType elementType) {
		//can set prefabs or textures in the future instead of color
		switch(elementType) {
		case ElementType.Fire: 
			return Color.red;
			break;
		case ElementType.Ice:
			return Color.blue;
			break;
		case ElementType.Lightning:
			return Color.magenta;
			break;
		case ElementType.Light:
			return Color.white;
			break;
		case ElementType.Dark:
			return Color.black;
			break;
		}
		return Color.gray;
	}

	// Update is called once per frame
	void Update () {
		//this.transform.Rotate(0, 25*Time.deltaTime, 0);
	}

	//when picked up
	void OnTriggerEnter(Collider other) {
		if(other.gameObject.name.Contains("Player")) {
			Debug.Log(other.gameObject.name + " has picked up the element " + m_elementType);

			other.GetComponent<Player>().setElementalEnchantment(m_elementType);

			photonView.RPC("RPC_DestroyElement", PhotonTargets.MasterClient);

//			ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
//			for(int i = 0; i < particles.Length; i++) {
//				ParticleDirection director = particles[i].GetComponent<ParticleDirection>();
//				director.SetTarget(other.GetComponent<Player>().m_effectContainer.transform);
//				director.setCallerPhotonView(photonView);
//				director.StartMovement();
//			}
		}
	}

	[RPC]
	void RPC_DestroyElement() {
			PhotonNetwork.Destroy (this.gameObject);
	}
}

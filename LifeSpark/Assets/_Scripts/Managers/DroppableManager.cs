using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroppableManager : LSMonoBehaviour {

	//manager for objects that can be dropped / picked up
	//ie. elemental powerups

	#region ELEMENT PREFABS
	public Material[] m_ElementMaterials;
	public Object[] m_ElementFX;
	public string[] m_ElementNames;
	#endregion

	// Use this for initialization
	void Start () {
	
	}

	void OnLevelWasLoaded(int level) {
		//Debug.Log("DroppableManager: " + PhotonNetwork.isMasterClient);
		if(PhotonNetwork.isMasterClient) {
			//Debug.Log("i'm at droppablemanager");
			
			//temporary elements randomly on the scene
//			for(int i = 0; i < 3; i++) {
//				List<object> initData = new List<object>{(Element.ElementType.Fire+i)};
//				PhotonNetwork.InstantiateSceneObject("Element", 
//				                                     //m_bossCageHolder.transform.position, 
//				                                     new Vector3(i*10, 1, 20),
//				                                     Quaternion.identity,
//				                                     0,
//				                                     initData.ToArray());
//			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}

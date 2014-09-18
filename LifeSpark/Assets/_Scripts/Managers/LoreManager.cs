using UnityEngine;
using System.Collections;

public class LoreManager : MonoBehaviour {

	// Class for storing Lore Items
	[System.Serializable]
	public class LoreItem {
		public string name;
		public string text;
	}
	
	
	// DATA
	public LoreItem[] loreItems;


}

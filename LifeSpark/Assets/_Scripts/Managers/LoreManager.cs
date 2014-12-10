using UnityEngine;
using System.Collections;

public class LoreManager : MonoBehaviour {

    static private LoreManager _instance;
    static public LoreManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(LoreManager)) as LoreManager;
            return _instance;
        }
    }


	// Class for storing Lore Items
	[System.Serializable]
	public class LoreItem {
		public string name;
		public string text;
	}
	
	
	// DATA
	public ArrayList loreItems;




    public LoreItem DropLore() {
        int random = Random.Range(0, loreItems.Count);
        return (LoreItem)loreItems[random];
    }


}

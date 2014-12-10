using UnityEngine;
using System.Collections;

public class ElementManager : LSMonoBehaviour {

    static ElementManager mElement;

    public static ElementManager mgr {
        get {
            if(mElement != null) { return mElement; }
            GameObject go = GameObject.Find("Manager") as GameObject;
            if(go != null) {
                if(go.GetComponent<ElementManager>() != null) {
                    mElement = go.GetComponent<ElementManager>();
                } else {
                    mElement = go.AddComponent<ElementManager>();
                }
                return mElement;
            }
            return null;
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    //public Element.ElementType[][] fusionTabled;

    public Element.ElementType[][] fusionTable = new Element.ElementType[][] 
{
    new Element.ElementType[] {
        Element.ElementType.Burn,        Element.ElementType.Steam,      Element.ElementType.Plasma,     Element.ElementType.Lava,       Element.ElementType.Angelfire,  Element.ElementType.HellFire},
    new Element.ElementType[] {
        Element.ElementType.Steam,       Element.ElementType.Freeze,     Element.ElementType.Lightning,  Element.ElementType.Acid,       Element.ElementType.Holywater,  Element.ElementType.Maelstrom},
    new Element.ElementType[] {
        Element.ElementType.Plasma,      Element.ElementType.Lightning,  Element.ElementType.Bounce,     Element.ElementType.Tornado,    Element.ElementType.Life,       Element.ElementType.Gravity},
    new Element.ElementType[] {
        Element.ElementType.Lava,        Element.ElementType.Acid,       Element.ElementType.Tornado,    Element.ElementType.Slow,       Element.ElementType.Diamond,    Element.ElementType.Antimatter},
    new Element.ElementType[] {
        Element.ElementType.Angelfire,   Element.ElementType.Holywater,  Element.ElementType.Life,       Element.ElementType.Diamond,    Element.ElementType.Replen,     Element.ElementType.Demigod},
    new Element.ElementType[] {
        Element.ElementType.HellFire,    Element.ElementType.Maelstrom,  Element.ElementType.Gravity,    Element.ElementType.Antimatter, Element.ElementType.Demigod,    Element.ElementType.Leech}
};

}

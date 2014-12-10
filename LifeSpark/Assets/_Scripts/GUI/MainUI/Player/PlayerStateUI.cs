using UnityEngine;
using System.Collections;

public class PlayerStateUI : MonoBehaviour {
	public ParticleSystem elementPa;
    public UISprite hpSprite,mpSprite;
    public UISprite portrait;
    Player myPlayer;
    public UIElementMerger m_merger;
    public UIDragElement m_drageElement;
	Element.ElementType e1,e2;
	public UILabel label;
    public UISprite testsp;
	// Use this for initialization
	void Start () {
	
	}

    public void setPlayer(Player me) {
       
        myPlayer = me;

	
		// e1 = (Element.ElementType)r;


		m_merger.sprite.spriteName = Element.getSpriteName (me.m_primaryElementalEnchantment);
        
		m_drageElement.sprite.spriteName = Element.getSpriteName (me.m_elementalEnchantment);
       
		portrait.spriteName = "" + ((int)me.m_heroType);
   
		if (me.m_elementalEnchantment == Element.ElementType.None) {
			m_drageElement.GetComponentInChildren<UIButton> ().collider.enabled = false;
		} else {
			m_drageElement.GetComponentInChildren<UIButton> ().collider.enabled = true;
            m_drageElement.sprite.color = new Color32(255, 255, 255, 255);
		}
		label.text = me.CurrLevel+"";
    }

	public void refresh ()
	{
		setPlayer (myPlayer);
	}

	// Update is called once per frame
	void Update () {
      //  Debug.Log(Screen.height);
        if(myPlayer) {
            hpSprite.fillAmount = myPlayer.unitHealth/ myPlayer.maxHealth;
			label.text = myPlayer.CurrLevel.ToString();

            //Camera.main.wi
        } else {
//            Debug.LogError("cannot find the player");
        }
	}

	Color getPaColor (Element.ElementType et)
	{
		switch (et) {
		case Element.ElementType.Air:
		case Element.ElementType.Water:
		case Element.ElementType.Diamond:

			return Color.blue;
			break;
		case Element.ElementType.HellFire:
		case Element.ElementType.Fire:
		case Element.ElementType.Lava:

			return Color.red;
			break;

		case Element.ElementType.Light:
		case Element.ElementType.Lightning:
			return Color.yellow;
			break;

		case Element.ElementType.Earth:
			return Color.green;
			break;

		case Element.ElementType.Dark:
		case Element.ElementType.Demigod:
			return Color.magenta;
			break;

		case Element.ElementType.DarkMatter:
			return new Color(0,30,60);

		default:
			return Color.yellow;
		}
	}

	public void OnMerge ()
	{
        //int r = Random.Range (11, 16);
        //m_merger.sprite.spriteName = r + "";
		//throw new System.NotImplementedException ();
        if(myPlayer.m_elementalEnchantment == Element.ElementType.None) {
            return;
        }else{
            Element.ElementType et = DoFusion(myPlayer.m_elementalEnchantment,myPlayer.m_primaryElementalEnchantment);
            m_merger.sprite.spriteName = Element.getSpriteName(et);
           // m_drageElement.GetComponentInChildren<UIButton>().collider.enabled = true;
			elementPa.startColor = getPaColor(et);
			elementPa.Play();
			Debug.Log("Element    " + et);
        }

	}

    public Element.ElementType DoFusion(Element.ElementType e1, Element.ElementType e2) {
        //switch(e1) {
        //    case Element.ElementType.Fire:
        //        switch(e2) {
        //            case Element.ElementType.Fire:
        //                return Element.ElementType.Burn;
                        
        //            case Element.ElementType.Water:
        //                return Element.ElementType.Steam;
                

        //            case Element.ElementType.Air:
        //                return Element.ElementType.Plasma;

        //            case Element.ElementType.Earth:

        //                return Element.ElementType.Lava;

        //            case Element.ElementType.Light:

        //                return Element.ElementType.Angelfire;

        //            case Element.ElementType.Dark:

        //                return Element.ElementType.HellFire;
        //        }
        //        break;
        //    case Element.ElementType.Water:

        //        break;

        //    case Element.ElementType.Air:
        //        break;

        //    case Element.ElementType.Earth:

        //        break;

        //    case Element.ElementType.Light:

        //        break;

        //    case Element.ElementType.Dark:

        //        break;
        //}

        int i = (int)e1;
        int j = (int)e2;
        Element.ElementType result = ElementManager.mgr.fusionTable[j][i];


        return result;
    }
}

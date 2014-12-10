using UnityEngine;
using System.Collections;

public class TargetSprite : MonoBehaviour {

    const string AllaySpriteName = "ally target reticle";
    const string EnemySpriteName = "enemy target reticle";
    const string SmartSpriteName = "Vector Smart Object";

    public UISprite sprite;
    public GameObject target;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(target == null) {
            if(gameObject.activeSelf)
              gameObject. SetActive(false);
        } else {
         

            transform.position = target.transform.position;
            //Vector3 v = transform.position - Camera.main.transform.position;
            //v = new Vector3(0, v.y, v.z);
            //Quaternion rotation = Quaternion.LookRotation(v);

            //transform.rotation = rotation;


        }
	}

    public void setObject(UnitObject obj) {
        if(obj == null){
            target = null;
            return;
        }
        if(sprite == null) {
            sprite = GetComponent<UISprite>();
        }
     //   if(!gameObject.activeSelf) {
            gameObject.SetActive(true);
        
        int myteam;
         int targetteam;
        switch(obj.gameObject.tag) {
            case "LaneCreep":
               myteam= PlayerManager.Instance.myPlayer.GetComponent<Player>().team;
               targetteam = obj.GetComponent<LaneCreep>().owner;
               if(targetteam == myteam) {
                   sprite.spriteName = AllaySpriteName;
               } else {
                   sprite.spriteName = EnemySpriteName;
               }
                break;
            case "Player":
                   myteam= PlayerManager.Instance.myPlayer.GetComponent<Player>().team;
               targetteam = obj.GetComponent<Player>().team;
               if(targetteam == myteam) {
                   sprite.spriteName = AllaySpriteName;
               } else {
                   sprite.spriteName = EnemySpriteName;
               }
                break;
            default:
                sprite.spriteName = SmartSpriteName;
                break;
        }

        target = obj.gameObject;
    }
}

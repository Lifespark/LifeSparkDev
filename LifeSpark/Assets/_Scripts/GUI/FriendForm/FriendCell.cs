using UnityEngine;
using System.Collections;

public class FriendCell : MonoBehaviour {
    FriendData m_fdata;
    public GameObject icon;
    public UILabel nameLabel;
    public Transform fgTrans;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void setData(FriendData fd) {
        m_fdata = fd;
        nameLabel.text = fd.name;
        setPlayerIcon(fd);
    }

    /// <summary>
    /// set the icon of player using friend data;
    /// now icons are fiex sprites, in the future, they may changed into dynamic downloaded.
    /// </summary>
    /// <param name="fd">friend data</param>
    private void setPlayerIcon(FriendData fd) {
        for(int i = 0; i < 16; i++) {
            //GameObject go = Instantiate(icon) as GameObject;
            GameObject go =  NGUITools.AddChild(fgTrans.gameObject, icon);
            go.transform.localScale = new Vector3(16, 16, 1);
            int x = Random.Range(-3, 3);
            int y = Random.Range(-3, 3);
            go.transform.localPosition = new Vector3(16*x -8 ,16*y -8 ,0);
            go.GetComponent<UISprite>().color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
            go.GetComponent<UISprite>().depth = 10;

        }
    }
}

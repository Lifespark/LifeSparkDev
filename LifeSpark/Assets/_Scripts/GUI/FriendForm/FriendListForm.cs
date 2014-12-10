using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FriendListForm : MonoBehaviour {

    public GameObject fc;
    public UIGrid grid;
    public FriendCell[] friendCellArray;
	// Use this for initialization
	void Start () {
	    //make fake list
        List<FriendData> list = new List<FriendData>();
        for(int i = 0; i < 20; i++) {
            FriendData fd = new FriendData();
            fd.id = i;
            fd.name = "Fighter" + i;
            fd.spritePath = "";
            list.Add(fd);

        }

        setData(list);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void setData(List<FriendData> fdList) {
        for(int i = 0; i < fdList.Count; i++) {
            GameObject go = NGUITools.AddChild(grid.gameObject, fc) as GameObject;
            
        }
       // grid.enabled = true;
        grid.repositionNow = true;
        friendCellArray = grid.GetComponentsInChildren<FriendCell>();
        for(int i = 0; i < friendCellArray.Length; i++) {
            friendCellArray[i].setData(fdList[i]);
        }
    }

    void CloseForm() {
        gameObject.SetActive(false);

    }
    
}

public class FriendData {
    public string name;
    public int id;
    public string spritePath;

}

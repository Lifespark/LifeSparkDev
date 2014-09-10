/// <summary>
/// User interface manager.
/// After it loaded, it will add UI Root (from ngui) in to scene;
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UIManager : MonoBehaviour {

	public UIRoot uiroot;
	public UICamera uiCamera;
	private bool needInit = true;
	// Use this for initialization
	void Start () {
		init();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/// <summary>
	/// Init this this class, load prefabs (ui root) in to scene;
	/// 
	/// </summary>
	void init()
	{
	
		needInit = false;
		if(!GameObject.Find("UI Root")) // already there;
		{
			uiroot =  Resources.Load<GameObject>("Root/UI Root").GetComponent<UIRoot>();
			uiroot = (Instantiate(uiroot) as GameObject).GetComponent<UIRoot>();
		}

		uiroot = GameObject.Find("UI Root").gameObject.GetComponent<UIRoot>();
		//Instantiate(uiroot.gameObject).name = "UI Root";
		uiCamera = uiroot.GetComponentInChildren<UICamera>();
		uiCamera.gameObject.SetActive(true);
		Object.DontDestroyOnLoad (uiroot.gameObject);
	}

	void displayGuiPrefab (Transform t)
	{
		if(needInit)
		{
			init();
		}
		t.gameObject.SetActive (true);
	}

	/// <summary>
	/// Adds the GUI prefab into UIRoot. if it is already, it will display it if it is hiden.
	/// </summary>
	/// <param name="GUIName">the name of UI prefab.</param>
	/// <returns>return the gameobject of the prefab</returns>
	public GameObject AddGui(string GUIName)
	{
		if(needInit)
		{
			init();
		}
		// this is for check Weather the ui is alreadly loaded. need to be changed for the low efficiency
		foreach(Transform t in uiCamera.transform)
		{
			if(t.name == GUIName) // when it alreadly loaded into scene
			{
				displayGuiPrefab (t);
				t.gameObject.SetActive(true);
				t.SendMessage("OnDisplay",SendMessageOptions.DontRequireReceiver);
				return t.gameObject;
			}

		}
		GameObject go = Resources.Load<GameObject>("Root/" + GUIName);
		go = Instantiate(go) as GameObject;
		go.name = GUIName;
		go.SendMessage("OnDisplay",SendMessageOptions.DontRequireReceiver);
		return go;
	}
}

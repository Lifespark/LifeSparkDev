/// <summary>
/// User interface manager.
/// After it loaded, it will add UI Root (from ngui) in to scene;
/// </summary>
using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {

	public UIRoot uiroot;
	public UICamera uiCamera;
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
	public void init()
	{
	

		if(GameObject.Find("UI Root")) // already there;
		{
			return;
		}

		uiroot =  Resources.Load<GameObject>("Root/UI Root").GetComponent<UIRoot>();
		Instantiate(uiroot.gameObject).name = "UI Root";
		uiCamera = uiroot.GetComponentInChildren<UICamera>();
	}

	static void displayGuiPrefab (Transform t)
	{
		t.gameObject.SetActive (true);
	}

	/// <summary>
	/// Adds the GUI prefab into UIRoot. if it is already, it will display it if it is hiden.
	/// </summary>
	/// <param name="GUIName">the name of UI prefab.</param>
	/// <returns>return the gameobject of the prefab</returns>
	public GameObject AddGui(string GUIName)
	{
		foreach(Transform t in uiCamera.transform)
		{
			if(t.name == GUIName) // when it alreadly loaded into scene
			{
				displayGuiPrefab (t);

				t.SendMessage("OnDisplay",SendMessageOptions.DontRequireReceiver);
				return t.gameObject;
			}

		}
		GameObject go = Resources.Load<GameObject>("Root/" + GUIName);
		go = Instantiate(go) as GameObject;
		go.name = GUIName;
		return go;
	}
}

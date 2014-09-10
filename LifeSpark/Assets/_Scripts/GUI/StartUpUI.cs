using UnityEngine;
using System.Collections;

public class StartUpUI : MonoBehaviour {

	public GameObject start, lobby;
	public UILabel pingLabel;
	private SimpleGUI m_sGui;

	public void SetSimpleGuiObejct(SimpleGUI sgui){
		m_sGui = sgui;

	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void OnMulti()
	{

		m_sGui.StartMultiPlayer();
		start.SetActive(false);
	}

	void OnQuickTest()
	{
		m_sGui.SendMessage("StartQuickTest");
		this.gameObject.SetActive(false);
	}
	/// <summary>
	/// this will be called by UIManager after use the func AddGUI
	/// this method served as reset method
	/// </summary>
	void OnDisplay()
	{
		start.SetActive(true);
		lobby.SetActive(false);
	}

	public void MultiMenu()
	{
		start.SetActive(false);
		lobby.SetActive(true);
	}


	public void DisplayPing(string ping)
	{
		pingLabel.text = ping;
	}


	public void reset ()
	{
		OnDisplay();
	}
}

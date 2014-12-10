using UnityEngine;
using System.Collections;

public class ChatWindow : MonoBehaviour {
	
	public bool isChatButtonPressed;
	public GameObject wheel;
	
	/// <summary>
	/// is the chat window activated from holding on screen center?
	/// </summary>
	public bool m_activatedFromHeld = false;
	//position of the click (set from Hover)
	public Vector2 m_clickPosFromHover;

	private Rect screenCenterBounds;
	private int radius = 100;
	
	// Use this for initialization
	void Start () {
		isChatButtonPressed = false;
		screenCenterBounds = new Rect((Screen.width/2)-radius, (Screen.height/2)-radius, 2*radius, 2*radius);
		//screenCenterBounds = new Rect(0, 0, 2*radius, 2*radius);
	}
	
	// Update is called once per frame
	void Update () {
		
		//if touch is held for over 1 second
		// ***  COMMENTED OUT FOR DEMO 12/4 ***
//		if(UICamera.m_secondsTouchHeld > 1 &&
//		   screenCenterBounds.Contains(m_clickPosFromHover)) {
//			if(!wheel.GetActive()) {
//				m_activatedFromHeld = true;
//				wheel.SetActive(true);
//			}
//		}
//		else {
//			m_activatedFromHeld = false;
//		}
//		
//		if(isChatButtonPressed != wheel.GetActive()) {
//			isChatButtonPressed = wheel.GetActive();
//		}
	}
	
	public void ToggleWheelDisplay(bool active) {
		wheel.SetActive (active);
	}
}

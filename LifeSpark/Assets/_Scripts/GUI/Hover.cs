using UnityEngine;
using System.Collections;

/// <summary>
/// This Hover is mostly base on NGUI button.
/// All type of input event will go to call send() function.
/// For now, set functionName and target(HoverForm), when go send, will active that function by SendMessage().
/// </summary>
public class Hover : MonoBehaviour {
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		OnDoubleClick,
	}
	
	// These parameters special for hover use. 
	public GameObject target;
	public string clickFunctionName;
	public string dragFunctionName;
	public string releaseFunctionName;
	
	public ChatCenter chatCenter;
	
	public Trigger trigger = Trigger.OnClick;
	public bool includeChildren = false;
	
	bool mStarted = false;
	bool mHighlighted = false;
	
	void Start () { mStarted = true; }
	
	void OnEnable () { if (mStarted && mHighlighted) OnHover(UICamera.IsHighlighted(gameObject)); }
	
	void OnHover (bool isOver)
	{
		if (enabled)
		{
			if (((isOver && trigger == Trigger.OnMouseOver) ||
			     (!isOver && trigger == Trigger.OnMouseOut))) Send();
			mHighlighted = isOver;
		}
	}
	
	void OnPress (bool isPressed)
	{	
		//Debug.Log("----->" + UICamera.currentTouch.pos);
		chatCenter.window.m_clickPosFromHover = UICamera.currentTouch.pos;
		chatCenter.ResetPos();
		if (enabled)
		{	
			//Call the chatcenter on Press event
			//Note that this is a custom Onpress event (Not the Default NGUI OnPress)
			chatCenter.OnPressHover(isPressed);

			// Drop
			if(!isPressed) {
				//Debug.Log("In Hover:Drop, The linked func=" + releaseFunctionName);
				target.SendMessage(releaseFunctionName, gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	void OnClick () { 
		// Debug.Log("In Hover:OnClick.");
		// if (enabled && trigger == Trigger.OnClick) Send(); 
		Send(); 
	}
	
	void OnDoubleClick () { if (enabled && trigger == Trigger.OnDoubleClick) Send(); }
	
	void OnDrag (Vector2 delta) {
		//Custom OnDrag Event
		//Debug.Log("In Hover:OnDrag, func=" + dragFunctionName);
		target.SendMessage(dragFunctionName, gameObject, SendMessageOptions.DontRequireReceiver);
		// chatCenter.OnDragFromHover(delta);
	}
	
	/// <summary>
	/// Send input event to this and childen.
	/// Now add SendMessage to specific function in HoverForm.
	/// </summary>
	void Send ()
	{
		if (string.IsNullOrEmpty(clickFunctionName)) return;
		if (target == null) target = gameObject;
		
		if (includeChildren)
		{
			Transform[] transforms = target.GetComponentsInChildren<Transform>();
			
			for (int i = 0, imax = transforms.Length; i < imax; ++i)
			{
				Transform t = transforms[i];
				t.gameObject.SendMessage(clickFunctionName, gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			target.SendMessage(clickFunctionName, gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}

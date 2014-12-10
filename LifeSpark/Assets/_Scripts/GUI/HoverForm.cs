using UnityEngine;
using System.Collections;

public class HoverForm : MonoBehaviour {
	private InputManager t_inputManager;
	void Start(){
		t_inputManager = GameObject.FindGameObjectWithTag ("Manager").GetComponent<InputManager>();
	}

	/// <summary>
	/// Presses the on hover.
	/// Special be called if hover box collider was pressed.
	/// </summary>
	void PressOnHover() {
		//Debug.Log("In HoverForm:Click.");
		t_inputManager.SinglePress();
	}

	void DragOnHover() {
		//Debug.Log("In HoverForm:Drag.");
		t_inputManager.Drag();
	}

	void ReleaseAfterHover() {
		//Debug.Log("In HoverForm:Release.");
		t_inputManager.Release();
	}
}

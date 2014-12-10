using UnityEngine;
using System.Collections;

public class InputManager : LSMonoBehaviour {

    static private InputManager _instance;
    static public InputManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(InputManager)) as InputManager;
            return _instance;
        }
    }

	/// <summary>
	/// Event in this region for game to hook on.
	/// Warning: never call these event outside of this class.
	/// </summary>
	#region DELEGATE_INTERFACE
	public delegate void OnClick(Vector3 mousePos);
	public event OnClick OnClicked;
	public delegate void OnDrag(Vector3 mousePos);
	public event OnDrag OnDragged;
	public delegate void OnRelease(Vector3 mousePos);
	public event OnRelease OnReleased;

	public delegate void OnJoyStickDragStart();
	public event OnJoyStickDragStart OnJoyStickDragStarting;
	public delegate void OnJoyStickDrag(Vector3 delta);
	public event OnJoyStickDrag OnJoyStickDragging;
	public delegate void OnJoyStickDragEnd();
	public event OnJoyStickDragEnd OnJoyStickDragEnding;
	#endregion

	#region INPUT_PARAMS
	public Vector3 m_jotstickDelta;
	#endregion

	// Use this for initialization
	void Awake () {
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		//if(Input.GetMouseButton(0) || Input.GetTouch(0).phase == TouchPhase.Stationary) Debug.Log("holding.....");
	}

	/// <summary>
	/// Functions in this region all called by GUI, and will active the event.
	/// Warning: never call these functions in game, these are only for GUI.
	/// </summary>
	#region GUI_OUTER_INTERFACE
	/// <summary>
	/// Single Press now just from HoverForm, if need multiple touch, need change.
	/// </summary>
	public void SinglePress() {
		//Debug.Log("In InputManager:SinglePress.");
		if(OnClicked != null) {
			OnClicked(Input.mousePosition);
		}
	}

	public void Drag() {
		//Debug.Log("In InputManager:Drag.");
		if(OnDragged != null) {
			OnDragged(Input.mousePosition);
		}
	}

	public void Release() {
		//Debug.Log("In InputManager:Release.");
		if(OnReleased != null) {
			OnReleased(Input.mousePosition);
		}
	}

	#region JOYSTICK
	public void JoyStickDragStarting() {
		if(OnJoyStickDragStarting != null) {
			OnJoyStickDragStarting();
		}
	}
	public void JoyStickDragging(Vector3 delta) {
		if(OnJoyStickDragging != null) {
			OnJoyStickDragging(delta);
		}
	}
	public void JoyStickDragEnding() {
		if(OnJoyStickDragEnding != null) {
			OnJoyStickDragEnding();
		}
	}
	#endregion
	#endregion
}

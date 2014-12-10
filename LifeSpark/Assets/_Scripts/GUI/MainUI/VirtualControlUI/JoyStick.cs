using UnityEngine;

/// <summary>
/// This class is mostly based on Drag and Drop example in NGUI.
/// </summary>
public class JoyStick : MonoBehaviour
{
	private InputManager t_inputManager;

	#region JOYSTICK_PARAMS
	public Vector3 m_baseLocalPosition;
	public Vector3 m_delta;
	public bool m_isDragging{
		get {return mIsDragging;}
	}
	public float m_radius = 30;

	private Vector3 t_deltaHandle;
	#endregion

	public Transform tweenTarget;
	public Vector3 pressed = new Vector3(1.05f, 1.05f, 1.05f);
	public float duration = 0.2f;

	Vector3 mScale;
	bool mStarted = false;
	bool mIsDragging = false;
	bool mSticky = false;
	
	void Start ()
	{
		if (!mStarted)
		{
			t_inputManager = GameObject.FindGameObjectWithTag ("Manager").GetComponent<InputManager>();
			//
			mStarted = true;
			if (tweenTarget == null) tweenTarget = transform;
			mScale = tweenTarget.localScale;
			//
			m_baseLocalPosition = this.transform.localPosition;
			m_delta = new Vector3(0, 0, 0);
		}
	}
	
	void Update(){
		if(mIsDragging && (Input.touchCount < 2)) {
			// When joystick is dragging, set m_delta and call InputManager.
			Vector3 calculateVector = t_deltaHandle;
			if(t_deltaHandle.sqrMagnitude > (m_radius * m_radius)){
				calculateVector.Normalize();
				this.transform.localPosition = m_baseLocalPosition + (calculateVector * m_radius);
				m_delta = (calculateVector * m_radius);
			} else {
				this.transform.localPosition = m_baseLocalPosition + t_deltaHandle;
				m_delta = t_deltaHandle;
			}
			t_inputManager.JoyStickDragging(m_delta);
		}
	}

	void OnDisable ()
	{
		if (mStarted && tweenTarget != null)
		{
			TweenScale tc = tweenTarget.GetComponent<TweenScale>();
			
			if (tc != null)
			{
				tc.scale = mScale;
				tc.enabled = false;
			}
		}
	}
	
	void OnPress (bool isPressed)
	{
		if (enabled)
		{
			// Change pressed joystick scale.
			if (!mStarted) Start();
			TweenScale.Begin(tweenTarget.gameObject, duration, isPressed ? Vector3.Scale(mScale, pressed) : mScale).method = UITweener.Method.EaseInOut;

			//
			if (isPressed)
			{
				if (!UICamera.current.stickyPress)
				{
					mSticky = true;
					UICamera.current.stickyPress = true;
				}
			}
			else if (mSticky)
			{
				mSticky = false;
				UICamera.current.stickyPress = false;
			}
			
			mIsDragging = false;
			Collider col = collider;
			if (col != null) col.enabled = !isPressed;
			//
			if (!isPressed) Drop();
		}
	}

	void OnDrag (Vector2 delta)
	{
		if (enabled && UICamera.currentTouchID > -2)
		{
			if (!mIsDragging)
			{
				// Initial Dragging.
				mIsDragging = true;
				t_inputManager.JoyStickDragStarting();
				t_deltaHandle = (Vector3)UICamera.currentTouch.totalDelta;
			}
			else
			{
				t_deltaHandle = (Vector3)UICamera.currentTouch.totalDelta;
				/*// When joystick is dragging, set m_delta and call InputManager.
				Vector3 calculateVector = (Vector3)UICamera.currentTouch.totalDelta;
				if(UICamera.currentTouch.totalDelta.sqrMagnitude > (m_radius * m_radius)){
					calculateVector.Normalize();
					this.transform.localPosition = m_baseLocalPosition + (calculateVector * m_radius);
					m_delta = (calculateVector * m_radius);
				} else {
					this.transform.localPosition = m_baseLocalPosition + (Vector3)UICamera.currentTouch.totalDelta;
					m_delta = (Vector3)UICamera.currentTouch.totalDelta;
				}
				t_inputManager.JoyStickDragging(m_delta);*/
			}
		}
	}

	/// <summary>
	/// Called if OnDropped, set joystick to base and set delta become zero.
	/// </summary>
	void Drop ()
	{
		this.transform.localPosition = m_baseLocalPosition;
		m_delta = Vector3.zero;
		t_inputManager.JoyStickDragEnding();
	}
}

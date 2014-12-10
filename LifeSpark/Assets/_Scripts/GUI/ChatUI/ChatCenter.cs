using UnityEngine;
using System.Collections;

public class ChatCenter : MonoBehaviour {


	public GameObject prefab;
	public ChatWindow window;

	Transform mTrans;
	bool mIsDragging = false;
	bool mSticky = false;
	Transform mParent;

	/// <summary>
	/// Update the table, if there is one.
	/// </summary>

	void Awake () { mTrans = transform; }

	void UpdateTable ()
	{
		UITable table = NGUITools.FindInParents<UITable>(gameObject);
		if (table != null) table.repositionNow = true;
	}

	void Update () {

	}

	/// <summary>
	/// Custom OnDrag function notified by Hover when chat is activated by holding on screen center
	/// </summary>
	/// <param name="delta">Delta.</param>
	public void OnDragFromHover (Vector2 delta) {
		if(window.m_activatedFromHeld) {
			mTrans.localPosition += (Vector3) delta;
		}
	}

	public void ResetPos () {
		if(mTrans != null)
			mTrans.localPosition = Vector3.zero;
	}

	/// <summary>
	/// Drop the dragged object.
	/// </summary>
	
	public void Drop ()
	{
		// Is there a droppable container?
		Collider col = UICamera.lastHit.collider;
		DragDropContainer container = (col != null) ? col.gameObject.GetComponent<DragDropContainer>() : null;
		//Debug.Log ("Dropped");
		if (container != null)
		{
			// Container found -- parent this object to the container
			PlayerManager.Instance.myPlayer.GetComponent<Player>().SendMessage(container.gameObject.GetComponent<ChatItem>().chatItemMsg);
			transform.localPosition = Vector3.zero;	
		}

		window.ToggleWheelDisplay(false);
		transform.localPosition = Vector3.zero;	
	}
	
	
	/// <summary>
	/// Start the drag event and perform the dragging.
	/// </summary>
	
	void OnDrag (Vector2 delta)
	{
		if (enabled && UICamera.currentTouchID > -2)
		{
			if (!mIsDragging)
			{
				mIsDragging = true;
				mParent = mTrans.parent;
				mTrans.parent = DragDropRoot.root;
				
				Vector3 pos = mTrans.localPosition;
				pos.z = 0f;
				mTrans.localPosition = pos;
				
				NGUITools.MarkParentAsChanged(gameObject);
			}
			else
			{
				mTrans.localPosition += (Vector3)delta;
			}
		}
	}
	
	/// <summary>
	///	Custom event to  start or stop the drag operation.
	/// Note the code for this event is the same as the NGUI OnPress Event
	/// </summary>
	
	public void OnPressHover (bool isPressed)
	{
		if (enabled)
		{
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
			if (!isPressed) Drop();
		}
	}
}

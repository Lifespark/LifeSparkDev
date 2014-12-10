using UnityEngine;
using System.Collections;

public class VirtualControlForm : MonoBehaviour {
	public GameObject m_joyStickObject;

	public JoyStick m_joyStick {
		get {
			if(m_joyStickObject==null)
			{
				return null;
			}
			return this.m_joyStickObject.GetComponent<JoyStick>();
		}
	}
}

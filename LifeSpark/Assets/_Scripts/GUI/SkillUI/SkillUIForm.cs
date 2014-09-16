using UnityEngine;
using System.Collections;

public class SkillUIForm : MonoBehaviour {

	public SkillSlot[] skillSlotList;
	private byte m_clickedSkillIndex;
	private bool m_isValidAction;
	private bool t_validAction;
	/// <summary>
	/// For TargetType has not default inValidAction, this var is to check if the action is 
	/// valid or not;
	/// After get this bool, it will set to false whatever the former value to ensure the skill
	/// will only cast once;
	/// </summary>
	/// <value><c>true</c> if this instance is valid action; otherwise, <c>false</c>.</value>
	public bool IsValidAction{
		get{
			t_validAction = m_isValidAction;
			m_isValidAction = false;
			return t_validAction;
		}
	}


	/// <summary>
	/// Gets the player ClickedSkill.
	/// Check IsValidAction FIRST
	/// </summary>
	/// <value>The ClickedSkill, line,area for now.</value>
	public byte ClickedSkill
	{
		get{

			return m_clickedSkillIndex;

		}
	}


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    
	}


    void init()
    {

    }

    void OnDisplay()
    {


    }

	/// <summary>
	/// Raises the skill event.
	/// </summary>
	/// <param name="index">the number of skill clicked</param>
	void OnSkill(byte index)
	{
		m_isValidAction = true;
		m_clickedSkillIndex = index;
		 
	}

	/// <summary>
	/// Reset All except the image
	/// </summary>
    public void OnReset()
    {

    }
}

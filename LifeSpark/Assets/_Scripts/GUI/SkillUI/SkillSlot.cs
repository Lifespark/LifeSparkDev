using UnityEngine;
using System.Collections;

public class SkillSlot : MonoBehaviour {
    
	public UISprite skillSprite;
	private string m_SkillId;
	private bool m_isSelected;
	private byte m_skillInedx;


	void Awake()
	{
		if(skillSprite == null)
		{
			skillSprite = GetComponentInChildren<UIButton>().GetComponentInChildren<UISprite>();
			if(skillSprite == null)
			{
				Debug.LogError("Err: cannot find the button for the skill, please check the prefab");
			}
		}
	}

	public void SetSkillSlot(string name, byte skillIndex, UIAtlas atlas)
	{
		skillSprite.atlas = atlas;
		skillSprite.spriteName = name;
		skillSprite.MakePixelPerfect();
		m_skillInedx = skillIndex;
	}

}



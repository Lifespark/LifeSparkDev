using UnityEngine;
using System.Collections;

public class SkillSlot : MonoBehaviour {
    
	public UISprite skillSprite;
	private string m_SkillId;
	private bool m_isSelected;
	private byte m_skillInedx;
    public UISprite m_SkillCover;
    public float coldTime;
    public float timer;
	void Awake()
	{
		if(skillSprite == null)
		{
			skillSprite = GetComponentInChildren<UIButton>().GetComponentInChildren<UISprite>();
			if(skillSprite == null)
			{
				Debug.LogError("Err: cannot find the button for the skill, please check the prefab");
            } else {
               // skillSprite.color = new Color32(255, 255, 255, 255);
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


    void OnClick() {
       // skillSprite.alpha = 1;
        timer = coldTime;
        m_SkillCover.gameObject.SetActive(true);
        return;
        TweenAlpha ta = TweenAlpha.Begin(skillSprite.gameObject, 0.6f, 0);
        ta.from = 0;
        ta.to = 1;
        ta.method = UITweener.Method.EaseIn;
        ta.style = UITweener.Style.Once;
        ta.Play(true);
        ta.onFinished += OnFinished;
    }

    void OnFinished(UITweener tween) {
        skillSprite.alpha = 0;
    }

    void Update() {
        if(timer > 0) {
            m_SkillCover.fillAmount = timer / coldTime;
            timer -= Time.deltaTime;
        } else {
            m_SkillCover.gameObject.SetActive(false);
        }
    }
}



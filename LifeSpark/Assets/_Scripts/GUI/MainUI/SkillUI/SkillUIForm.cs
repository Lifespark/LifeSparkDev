using UnityEngine;
using System.Collections;

public class SkillUIForm : MonoBehaviour {

    //public SkillSlot[] skillSlotList;
    private byte m_clickedSkillIndex;
    private bool m_isValidAction;
    private bool t_validAction;

    Color32 r = new Color32(255, 0, 0, 255);
    Color32 w = new Color32(255, 255, 255, 255);
	public SkillSlot[] skillSlotArray;
    public UISprite enHp, enMp;

    private Player myPlayer;
	//public UIAtlas  newAtas;

    public UISprite playerIcon, autoAtk;

    public void setTargetUIVisiable(bool visibale) {
        enHp.gameObject.SetActive(visibale);
        enMp.gameObject.SetActive(visibale);
        //        autoAttackSprite.gameObject.SetActive(visibale);
        byte c = visibale ? (byte)255 : (byte)0;
        playerIcon.GetComponent<UISprite>().color = new Color32(c, c, c, 255);
    }


    void tagglAutoAttack() {
        myPlayer.isAutoAttacking = !myPlayer.isAutoAttacking;
        SetAttackStatus(myPlayer.isAutoAttacking);
        //Debug.Log(PlayerManager.Instance.myPlayer.GetComponent<Player>().isAutoAttacking);
    }

    public void SetAttackStatus(bool isAttacking) {
        autoAtk.color = isAttacking ? r : w;
        //Debug.Log(PlayerManager.Instance.myPlayer.GetComponent<Player>().isAutoAttacking);
    }

    UnitObject mTarget;

    public void setTartget(UnitObject tar) {
        //
        mTarget = tar;
        enHp.fillAmount = tar.unitHealth / tar.maxHealth;
        UIManager.mgr.targetSprite.setObject(tar);
        //TODO: Change pro
        //     }
    }

    void setToZero() {
        setTargetUIVisiable(false);
        UIManager.mgr.targetSprite.setObject(null);
    }

    public void refreshTarget() {

        if (myPlayer.targetTransform != null)
        {
			setTargetUIVisiable(true);
            switch(
            myPlayer.targetTransform.tag[0]

                ) {
                case 'P':

                        setTartget(getPlayerByName(myPlayer.targetTransform.name));
                    //Player pl = PlayerManager.Instance.myPlayer.GetComponent<Player>().targetTransform.GetComponent<Player>();
                    if (myPlayer == null)
                    {
                        Debug.LogError("cannot get the player");
                        return;
                    }
                    switch (myPlayer.m_heroType)
                    {
                        case Player.PlayerHero.LEVANTIS:
                            playerIcon.spriteName = "1";
                            break;
                        case Player.PlayerHero.DELIA:
                            playerIcon.spriteName = "2";
                            break;
                    }
                    break;
                case 'J':
                    setTartget(getCreepByName(myPlayer.targetTransform.name));
                    switch (myPlayer.targetTransform.GetComponent<JungleMonster>().m_creepType)
                    {
                        case JungleMonster.CreepType.GOBLIN:
                            playerIcon.spriteName = "GoblinPortrait";
                            break;
                        case JungleMonster.CreepType.KOBOLD:
                            playerIcon.spriteName = "KoboldPortrait";
                            break;
                        case JungleMonster.CreepType.ORC:
                            playerIcon.spriteName = "OrcPortrait";
                            break;
                    }
                  //  playerIcon.spriteName = "GoblinPortrait".ToString();
                    break;
                case 'L':
                   // Debug.LogError("target is land");
                    // setTartget(getLaneCreepByName(PlayerManager.Instance.myPlayer.GetComponent<Player>().targetTransform.name));
                    setTartget(myPlayer.targetTransform.GetComponent<UnitObject>());
                    playerIcon.spriteName = "CreepPortrait";
                    break;
                case 'S':
                    setTartget(myPlayer.targetTransform.GetComponent<UnitObject>());
                    switch (myPlayer.targetTransform.GetComponent<SparkPoint>().capturingTeam)
                    {
                        case -1:
                            playerIcon.spriteName = "s0";
                            break;
                        case 1:
                            playerIcon.spriteName = "s1";
                            break;

                        case 2:
                            playerIcon.spriteName = "s2";
                            break;
                    }
                    break;
                case 'B':
                    setTartget(myPlayer.targetTransform.GetComponent<UnitObject>());
                    break;
                default:
                    setToZero();
                    break;

            }
        }
    }

    UnitObject getPlayerByName(string name) {
        return PlayerManager.Instance.PlayerLookUp[name];
    }

    UnitObject getCreepByName(string name) {
        return MonsterClient.Instance.MonsterLookUp[name];
    }

    UnitObject getLaneCreepByName(string name) {
        return (GameObject.Find(name) as GameObject).GetComponent<UnitObject>();
    }

    void targetLeft() {
        PlayerManager.Instance.myPlayer.GetComponent<PlayerInput>().SwitchTargetByButton(-1);
    }

    void targetRight() {
        PlayerManager.Instance.myPlayer.GetComponent<PlayerInput>().SwitchTargetByButton(1);
    }

    /// <summary>
    /// For TargetType has not default inValidAction, this var is to check if the action is 
    /// valid or not;
    /// After get this bool, it will set to false whatever the former value to ensure the skill
    /// will only cast once;
    /// </summary>
    /// <value><c>true</c> if this instance is valid action; otherwise, <c>false</c>.</value>
    public bool IsValidAction {
        get {
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
    public byte ClickedSkill {
        get {

            return m_clickedSkillIndex;

        }
    }


    // Use this for initialization
    void Start() {
        autoAtk.color = w;
        myPlayer = PlayerManager.Instance.myPlayer.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update() {
        if(mTarget) {
            enHp.fillAmount = mTarget.unitHealth / mTarget.maxHealth;
        }

    }

    string[][] skillName = new string[][] 
{
    new string[] {""},
    new string[] {"Stalagmite","SnakeHook","WhirlingDervish","SmashySmashy"},
   new string[] {"AngelFlight","Smite","HollyFire","DivineFury"}
};
    void init() {

    }

    void OnDisplay() {
      

        myPlayer = PlayerManager.Instance.myPlayer.GetComponent<Player>();
        if(myPlayer == null) {
            Debug.LogError("Cannot find the my player");
        } else {

            for(int i = 0; i < 4; i++) {
                skillSlotArray[i].coldTime = SkillLibrary.Instance.m_skills[skillName[(int)myPlayer.m_heroType][i]].coolDownTime;
                skillSlotArray[i].timer = 0;
                skillSlotArray[i].m_SkillCover.gameObject.SetActive(false);
            }


        }
        return;

		
        // autoAtk.color = PlayerManager.Instance.myPlayer.GetComponent<Player>().isAutoAttacking ? Color.red : Color.white;
		if (PlayerManager.Instance.myPlayer != null) {
			 switch(PlayerManager.Instance.myPlayer.GetComponent<Player>().m_heroType){
			case Player.PlayerHero.LEVANTIS:
				for(int i =0;i<skillSlotArray.Length;i++){
					//skillSlotArray[i].GetComponentInChildren<UIButton>().GetComponentInChildren<UISprite>().atlas = newAtas;
					skillSlotArray[i].GetComponentInChildren<UIButton>().GetComponentInChildren<UISprite>().spriteName = "1-"+(i+1);
				}
				break;

			default:
				for(int i =0;i<skillSlotArray.Length;i++){
					//skillSlotArray[i].GetComponentInChildren<UIButton>().GetComponentInChildren<UISprite>().atlas = newAtas;
					skillSlotArray[i].GetComponentInChildren<UIButton>().GetComponentInChildren<UISprite>().spriteName = "0-"+(i+1);
				}
				break;
			}
				}
    }

    /// <summary>
    /// Raises the skill event.
    /// </summary>
    /// <param name="index">the number of skill clicked</param>
    void OnSkill(byte index) {
        m_isValidAction = true;
        m_clickedSkillIndex = index;

    }


    void CameraCentre() {
        Camera.main.gameObject.GetComponent<CameraManager>().FocusOnPlayer();
    }

    /// <summary>
    /// Reset All except the image
    /// </summary>
    public void OnReset() {

    }
}

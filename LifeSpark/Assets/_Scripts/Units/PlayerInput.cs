using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInput : UnitMovement
{

    Vector3 tempHit;
    public bool isMine = false;
    private UIManager uiManager;
    private Vector3 t_vector3;

    #region CAMERA_PARAMS
    private int m_pressCount = 0;
    private bool m_dragFlag = false;
    private Vector3 m_previousPressPos;
    public float m_iPadScrollSpeed = 1.0f;
    #endregion

    private AGE.ActionHelper m_actionHelper;

    private Player m_player;
    private GameObject m_camera;
    public GameObject m_cursor;
    public GameObject m_cursorGround;
    public GameObject m_cursorAir;
    private Collider[] nearbyColliders;
    private List<Collider> nearbyTargets = new List<Collider>();
    private List<Collider> nearbyPlayers = new List<Collider>();
    private List<Collider> nearbyLaneCreeps = new List<Collider>();
    private List<Collider> nearbyJungleCreeps = new List<Collider>();
    private List<Collider> nearbySparkPoints = new List<Collider>();

    public float m_targetingRange = 20;
    public static int m_touchCountThisSession = 0;

    private float m_touchStartTime;
    private float m_dragTimer = 0.5f; // modify this for end of drag touches. -jk

    public bool m_isDisabled = false;

    public enum TargetType
    {
        Position,
        LineAttack,
        TargetAttack,
        TargetAreaAttack,
        SelfAreaAttack,
        //PositionFromJoystick,
        None
    };

    public TargetType targetType;

    // Use this for initialization
    void Awake()
    {
        tempHit = Vector3.zero;
        targetType = TargetType.Position;
        m_actionHelper = GetComponent<AGE.ActionHelper>();
        m_player = GetComponent<Player>();
        m_camera = GameObject.Find("Main Camera");
        m_cursor = GameObject.Find("Cursor");
        m_cursorGround = GameObject.Find("CursorGround");
        m_cursorAir = GameObject.Find("CursorAir");
        // InputManager hookings
        InputManager.Instance.OnClicked += MousePressedIntoHover;
        InputManager.Instance.OnDragged += MouseDraggedOnHover;
        InputManager.Instance.OnReleased += MouseReleaseAfterHover;
        InputManager.Instance.OnJoyStickDragging += JoyStickDeltaAction;
        InputManager.Instance.OnJoyStickDragStarting += JoyStickStartDraggingHandler;
        InputManager.Instance.OnJoyStickDragEnding += JoyStickEndDraggingHandler;
    }

    // Update is called once per frame
    void Update()
    {

        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }

        //tracks the amount of touches used in this session (any # of touch begins till all touches let go)
        //used to differentiate camera panning and regular use
        if (Input.touchCount == 0)
        {
            m_touchCountThisSession = 0;
        }
        else if (Input.touchCount > m_touchCountThisSession)
        {
            m_touchCountThisSession = Input.touchCount;
        }

        //tracks the duration of a touch
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TouchPhase touchPhase = Input.GetTouch(0).phase;
            if (touchPhase == TouchPhase.Began)
            {
                // Touch began, so store time
                m_touchStartTime = Time.time;
            }
        }

        if (isMine && m_player.GetState() != Player.PlayerState.Dead)
        {
            KeyBoardMouseInput();
        }
        //         if (m_player.playerState == Player.PlayerState.Attacking && m_action == null) {
        //             //m_player.playerState = Player.PlayerState.Idle;
        //             m_player.SwitchState(Player.PlayerState.Idle);
        //         }

        //        if (Input.GetKeyUp(KeyCode.N)) SwitchTargetByButton(-1);
        //        if (Input.GetKeyUp(KeyCode.M)) SwitchTargetByButton(1);
    }



    // PC input
    //Uses 'S' for line attack and 'A' for area attack
    //Defaults to normal movement after every click
    void KeyBoardMouseInput()
    {
        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }

        if (Input.GetKeyDown("s") && m_player.isLineAttackCool)
        {
            targetType = TargetType.LineAttack;
        }
        if (Input.GetKeyDown("a") && m_player.isAreaAttackCool)
        {
            targetType = TargetType.TargetAreaAttack;
        }
        if (Input.GetKeyDown("d") && m_player.isAreaAttackCool)
        {
            targetType = TargetType.SelfAreaAttack;
            //m_action = m_actionHelper.PlayAction("WhirlingDervish");
            //SkillLibrary.Instance.m_skills["WhirlingDervish"].Cast(out m_action, ref m_actionHelper);

            if (m_player.m_heroType == Player.PlayerHero.LEVANTIS)
            {
                photonView.RPC("RPC_castSkill", PhotonTargets.All, "WhirlingDervish", new object[] { });
            }
            else if (m_player.m_heroType == Player.PlayerHero.CAPSULE)
            {
                // Making the aoe around player a quick cast (no mouse involved.)
                PlayerManager.Instance.photonView.RPC("RPC_setPlayerTarget",
                                                                 PhotonTargets.All,
                                                                 this.name,
                                                                 this.transform.position,
                                                                 this.name,
                                                                 (int)targetType);
            }
            else if (m_player.m_heroType == Player.PlayerHero.DELIA)
            {
                photonView.RPC("RPC_castSkill", PhotonTargets.All, "HollyFire", new object[] { });
            }
            targetType = TargetType.Position;
            return;
        }
        if (Input.GetKeyDown("f"))
        {
            if (m_player.m_heroType == Player.PlayerHero.CAPSULE)
                m_player.KillPlayer();
            else
                targetType = TargetType.TargetAttack;
        }
        if (Input.GetKeyUp("l"))
        {
            m_player.isAutoAttacking = !m_player.isAutoAttacking;
        }
        /// Get the UI action
        /// This is a temp method
        if (uiManager == null)
        {
            uiManager = GameObject.Find("Manager").GetComponent<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("Cannot find the UI Manager");
            }
            else
            {

            }
        }
        if (uiManager.T_skillUIForm.IsValidAction)
        {
            Player me = this.GetComponentInParent<Player>();

            Vector3 target;
            if (me.targetUnit != null) {
                target = me.targetUnit.transform.position;
            } else {
                target = me.transform.forward + me.transform.position;
            }

            switch (uiManager.T_skillUIForm.ClickedSkill)
            {
                case 0:
                    if (me.isAreaAttackCool)
                    {
                        targetType = TargetType.TargetAreaAttack;
                        if (m_player.m_heroType == Player.PlayerHero.LEVANTIS)
                        {

                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "Stalagmite", target, new object[] { me.target });
                        }
                        else if (m_player.m_heroType == Player.PlayerHero.DELIA)
                        {
                            Vector3 dir = (me.target + new Vector3(0, 1, 0)) - gameObject.transform.localPosition;
                            Vector3 endPos = 10 * dir.normalized + gameObject.transform.localPosition;
                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "AngelFlight", target, new object[] { endPos });
                        }

                    }
                    targetType = TargetType.Position;
                    return;
                case 1:
                    if (me.isLineAttackCool)
                    {
                        targetType = TargetType.LineAttack;

                        if (m_player.m_heroType == Player.PlayerHero.LEVANTIS)
                        {
                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "SnakeHook", target, new object[] { me.target });
                        }
                        else if (m_player.m_heroType == Player.PlayerHero.DELIA)
                        {
                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "Smite", target, new object[] { me.target });
                        }
                    }
                    targetType = TargetType.Position;
                    return;
                case 2:
                    if (me.isAreaAttackCool)
                    {
                        targetType = TargetType.SelfAreaAttack;
                        if (m_player.m_heroType == Player.PlayerHero.LEVANTIS)
                        {
                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "WhirlingDervish", target, new object[] { });
                        }
                        else if (m_player.m_heroType == Player.PlayerHero.CAPSULE)
                        {
                            // Making the aoe around player a quick cast (no mouse involved.)
                            GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerTarget",
                                                                          PhotonTargets.All,
                                                                          this.name,
                                                                          this.transform.position,
                                                                          this.name,
                                                                          (int)targetType);
                        }
                        else if (m_player.m_heroType == Player.PlayerHero.DELIA)
                        {
                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "HollyFire", target, new object[] { });
                        }

                    }
                    targetType = TargetType.Position;
                    return;
                case 3:
                    if (m_player.m_heroType == Player.PlayerHero.CAPSULE)
                        this.GetComponent<Player>().KillPlayer();
                    else
                    {
                        targetType = TargetType.TargetAttack;

                        if (m_player.m_heroType == Player.PlayerHero.LEVANTIS)
                        {
                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "SmashySmashy", target, new object[] { me.target });
                        }
                        else if (m_player.m_heroType == Player.PlayerHero.DELIA)
                        {
                            photonView.RPC("RPC_castSkill", PhotonTargets.All, "DivineFury", target, new object[] { me.target });
                        }

                    }
                    targetType = TargetType.Position;
                    return;
            }
        }
    }

    #region HOOKED_FUNCS
    /// <summary>
    /// Mouse pressed from InputManager, need hook to InputManager when start. 
    /// </summary>
    /// <param name="mousePos">Mouse position.</param>
    public void MousePressedIntoHover(Vector3 mousePos)
    {

        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            float touchDuration = Time.time - m_touchStartTime;
            TouchPhase touchPhase = Input.GetTouch(0).phase;
            if (touchPhase == TouchPhase.Ended && touchDuration > m_dragTimer)
            {
                return;
            }
        }

        if (isMine && m_player.GetState() != Player.PlayerState.Dead)
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            // check if hit, the length(1000.0f) can set to other value
            if (Physics.Raycast(cameraRay, out hit, 1000.0f))
            {

                // figure out if a ping key was pressed. -jk 
                int key = (Input.GetKey(KeyCode.Alpha1) ? 1 : 0) +
                    2 * (Input.GetKey(KeyCode.Alpha2) ? 1 : 0) +
                        4 * (Input.GetKey(KeyCode.Alpha3) ? 1 : 0) +
                        8 * (Input.GetKey(KeyCode.Alpha4) ? 1 : 0);
                // User wants to ping not update commands. -jk 
                if (key > 0)
                {
                    tempHit = hit.point;
                    tempHit.y = 0.01f;
                    PlayerManager.Instance.photonView.RPC("RPC_setPlayerPing",
                                                          PhotonTargets.All,
                                                          this.name,
                                                          tempHit,
                                                          hit.collider.name,
                                                          key);
                    return;
                }

                // Remove outline from last target for setting new target outline.
                if (m_player.targetName.StartsWith("Player"))
                    PlayerManager.Instance.PlayerLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_Outline", 0.0f);
                else if (m_player.targetName.StartsWith("JungleCreep"))
                {
                    if (MonsterClient.Instance.MonsterLookUp.ContainsKey(m_player.targetName) && MonsterClient.Instance.MonsterLookUp[m_player.targetName].m_silhouetteRenderer != null)
                        MonsterClient.Instance.MonsterLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_Outline", 0.0f);
                }
                else if (m_player.targetName.StartsWith("LaneCreep"))
                {
                    if (CreepManager.Instance.LaneCreepLookUp.ContainsKey(m_player.targetName) && CreepManager.Instance.LaneCreepLookUp[m_player.targetName].m_silhouetteRenderer != null)
                        CreepManager.Instance.LaneCreepLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_Outline", 0.0f);
                }
                else if (m_player.targetName.StartsWith("Spark"))
                {
                    if (SparkPointManager.Instance.sparkPointsDict.ContainsKey(hit.collider.name))
                    {
                        // TODO: cancel outline for sparkpoint
                    }
                }


                //tempHit = hit.point;
                //tempHit.y = 0;
                // no matter what, clean first.
                m_cursorGround.GetComponent<Animator>().SetBool("pingKeep", false);
                m_cursorAir.GetComponent<Animator>().SetBool("cursorKeep", false);

                // Analyze hit case.
                if (hit.collider.name.Equals("Ground"))
                {
                    // Destination move.
                    tempHit = hit.point;
                    tempHit.y = 1;
                    m_player.SetPlayerMoveTarget(Player.TargetType.DESTINATION, hit.point, hit.collider.name, false);
                    // find cursor and set color. -jk 
                    m_cursorGround.GetComponent<Animator>().SetTrigger("pingGreen");
                    m_cursorGround.GetComponent<Animator>().SetBool("pingKeep", true);
                    // make arrow point towards camera. -jk 
                    Vector3 camPos = m_camera.transform.position;
                    camPos.y = 0;
                    m_cursorAir.transform.rotation = Quaternion.LookRotation(camPos - tempHit);
                    m_cursorAir.GetComponent<Animator>().SetTrigger("cursorClick");
                    m_cursorAir.GetComponent<Animator>().SetBool("cursorKeep", true);
                    // reposition cursor. -jk 
                    m_cursor.transform.position = tempHit;
                }
                else if (hit.collider.tag == "SparkPoint")
                {
                    // SparkPoint, if SparkPoint not at same team and captured.
                    if (SparkPointManager.Instance.sparkPointsDict[hit.collider.name].GetComponent<SparkPoint>().GetOwner() == -1)
                    {
                        m_player.SetPlayerMoveTarget(Player.TargetType.DESTINATION, hit.point, hit.collider.name, false);
                    }
                    else if (SparkPointManager.Instance.sparkPointsDict[hit.collider.name].GetComponent<SparkPoint>().GetOwner() != m_player.GetTeam() &&
                        SparkPointManager.Instance.sparkPointsDict[hit.collider.name].GetComponent<SparkPoint>().sparkPointState == SparkPoint.SparkPointState.CAPTURED)
                    {
                        m_player.SetPlayerMoveTarget(Player.TargetType.SPARKPOINT, hit.point, hit.collider.name, true);
                    }
                }
                else if (hit.collider.tag == "Player")
                {
                    // Player, if not in same team
                    if (PlayerManager.Instance.PlayerLookUp[hit.collider.name].GetTeam() != m_player.GetTeam())
                    {
                        // Set outline.
                        PlayerManager.Instance.PlayerLookUp[hit.collider.name].m_silhouetteRenderer.material.SetFloat("_Outline", 0.005f);

                        m_player.SetPlayerMoveTarget(Player.TargetType.PLAYER, hit.point, hit.collider.name, true);
                    }
                }
                else if (hit.collider.tag == "JungleCreep")
                {
                    // Set outline.
                    MonsterClient.Instance.MonsterLookUp[hit.collider.name].m_silhouetteRenderer.material.SetFloat("_Outline", 0.005f);
                    MonsterClient.Instance.MonsterLookUp[hit.collider.name].m_silhouetteRenderer.material.SetFloat("_HideOutline", 0.005f);
                    ;
                    m_player.SetPlayerMoveTarget(Player.TargetType.MONSTER, hit.point, hit.collider.name, true);
                }
                else if (hit.collider.tag == "LaneCreep")
                {
                    // Creep, if now at same team.
                    if (CreepManager.Instance.LaneCreepLookUp[hit.collider.name].owner != m_player.team)
                    {
                        // Set outline.
                        CreepManager.Instance.LaneCreepLookUp[hit.collider.name].m_silhouetteRenderer.material.SetFloat("_Outline", 0.005f);
                        CreepManager.Instance.LaneCreepLookUp[hit.collider.name].m_silhouetteRenderer.material.SetFloat("_HideOutline", 0.005f);

                        m_player.SetPlayerMoveTarget(Player.TargetType.CREEP, hit.point, hit.collider.name, true);
                    }
                }
                else if (hit.collider.tag == "Boss")
                {
                    //TODO: Add silhouette to boss?
                    m_player.SetPlayerMoveTarget(Player.TargetType.CREEP, hit.point, hit.collider.name, true);
                }
            }
        }
    }

    public void MouseDraggedOnHover(Vector3 mousePos)
    {
        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }
        // Camera functionality.
        //Debug.Log("In PlayerInput:Dragged on hover.");


        // Camera movement, call func in camera manager.
        Vector3 translation = Vector3.zero;
        if (!m_dragFlag)
        {
            m_previousPressPos = mousePos;
            m_dragFlag = true;
        }
        else
        {
            Vector3 mouseDelta = mousePos - m_previousPressPos;
            m_previousPressPos = mousePos;
            translation.x = -mouseDelta.x;
            translation.z = -mouseDelta.y;
            CameraManager.Instance.ScrollCamera(translation);
        }
    }

    public void MouseReleaseAfterHover(Vector3 mousePos)
    {
        //Debug.Log("In PlayerInput:Mouse released after do hover funcs.");
        m_dragFlag = false;
    }

    /// <summary>
    /// JoyStick delta from InputManager, need hook to InputManager when start.
    /// </summary>
    /// <param name="delta">JoyStick delta.</param>
    public void JoyStickDeltaAction(Vector3 delta)
    {
        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }
        if (isMine && m_player.GetState() != Player.PlayerState.Dead)
        {
            t_vector3 = (new Vector3(delta.x, 0, delta.y)) * Time.deltaTime;
            m_player.MovePlayerByDelta(t_vector3);
        }
    }

    public void JoyStickStartDraggingHandler()
    {
        // Remove cursor effect.
        m_cursorGround.GetComponent<Animator>().SetBool("pingKeep", false);
        m_cursorAir.GetComponent<Animator>().SetBool("cursorKeep", false);

        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }
        if (isMine && m_player.GetState() != Player.PlayerState.Dead)
        {
            m_player.SetPlayerMoveTarget(Player.TargetType.DIRECTION, Vector3.zero, "", false);
        }
    }

    public void JoyStickEndDraggingHandler()
    {
        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }
        if (isMine && m_player.GetState() != Player.PlayerState.Dead)
        {
            m_player.SetPlayerMoveTarget(Player.TargetType.NONE, Vector3.zero, "", false);
        }
    }
    #endregion

    [RPC]
    void RPC_castSkill(string actionName, Vector3 turnTo, object[] parameters)
    {
        bool success = m_player.castSkill(actionName);

        m_player.transform.LookAt(turnTo);

        if (!success)
            return;
        // Remove cursor effect.
        if (isMine)
        {
            m_cursorGround.GetComponent<Animator>().SetBool("pingKeep", false);
            m_cursorAir.GetComponent<Animator>().SetBool("cursorKeep", false);
        }


        //m_player.playerState = Player.PlayerState.Attacking;
        GetComponent<NavMeshAgent>().ResetPath();
        m_player.RPC_resetAnimTriggers();
        ////
        // Shoudl change to attack target type.
        if (m_player.m_targetType != Player.TargetType.NONE &&
           m_player.m_targetType != Player.TargetType.DESTINATION &&
           m_player.m_targetType != Player.TargetType.DIRECTION)
        {
            if (parameters.Length >= 1)
            {
                Vector3 dir = (Vector3)(parameters[0]);
                dir.y = 0;
                transform.LookAt(dir);
            }
        }
        ////
        m_player.SwitchState(Player.PlayerState.ActionAttacking);
        SkillLibrary.Instance.m_skills[actionName].Cast(ref m_player.m_action, ref m_actionHelper, parameters);
    }

    public void SwitchTargetByButton(int button)
    {
        if (m_isDisabled)
        {
            //Used when in dispatch mode. Could conceivably be used for stuns, etc.
            return;
        }
        if (isMine)
        {
            Color _outLineColor;
            nearbyTargets.Clear();
            nearbyPlayers.Clear();
            nearbyLaneCreeps.Clear();
            nearbyJungleCreeps.Clear();
            //nearbySparkPoints.Clear();

            nearbyColliders = Physics.OverlapSphere(transform.position, m_targetingRange);

            for (int i = 0; i < nearbyColliders.Length; i++)
            {
                if (nearbyColliders[i].tag == "Player")
                {
                    if (PlayerManager.Instance.PlayerLookUp[nearbyColliders[i].name].GetTeam() != m_player.GetTeam())
                    {
                        nearbyPlayers.Add(nearbyColliders[i]);
                        //Debug.Log("Add Player into target");
                    }
                }
                else if (nearbyColliders[i].tag == "JungleCreep")
                    nearbyJungleCreeps.Add(nearbyColliders[i]);
                else if (nearbyColliders[i].tag == "LaneCreep")
                    nearbyLaneCreeps.Add(nearbyColliders[i]);
                //                 else if (nearbyColliders[i].tag == "SparkPoint") {
                //                     if (SparkPointManager.Instance.sparkPointsDict[nearbyColliders[i].name].GetOwner() != m_player.team &&
                //                         SparkPointManager.Instance.sparkPointsDict[nearbyColliders[i].name].sparkPointState == SparkPoint.SparkPointState.CAPTURED) {
                //                         nearbySparkPoints.Add(nearbyColliders[i]);
                //                     }
                //                 }
            }

            if (nearbyPlayers.Count > 0 || nearbyLaneCreeps.Count > 0 || nearbyJungleCreeps.Count > 0)
            {
                nearbyPlayers.Sort((Collider x, Collider y) => x.transform.position.x.CompareTo(y.transform.position.x));
                nearbyLaneCreeps.Sort((Collider x, Collider y) => x.transform.position.x.CompareTo(y.transform.position.x));
                nearbyJungleCreeps.Sort((Collider x, Collider y) => x.transform.position.x.CompareTo(y.transform.position.x));
                //nearbySparkPoints.Sort((Collider x, Collider y) => x.transform.position.x.CompareTo(y.transform.position.x));

                nearbyTargets.AddRange(nearbyPlayers);
                //nearbyTargets.AddRange(nearbySparkPoints);
                nearbyTargets.AddRange(nearbyLaneCreeps);
                nearbyTargets.AddRange(nearbyJungleCreeps);

                int ndx = nearbyTargets.FindIndex((Collider col) => col.name == m_player.targetName);

                // turn off previous target outline
                if (m_player.targetName.StartsWith("Player"))
                    PlayerManager.Instance.PlayerLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_Outline", 0.0f);
                else if (m_player.targetName.StartsWith("JungleCreep"))
                {
                    if (MonsterClient.Instance.MonsterLookUp.ContainsKey(m_player.targetName) && MonsterClient.Instance.MonsterLookUp[m_player.targetName].m_silhouetteRenderer != null)
                    {
                        MonsterClient.Instance.MonsterLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_Outline", 0.0f);
                        MonsterClient.Instance.MonsterLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_HideOutline", 0.0f);
                    }
                }
                else if (m_player.targetName.StartsWith("LaneCreep"))
                {
                    if (CreepManager.Instance.LaneCreepLookUp.ContainsKey(m_player.targetName) && CreepManager.Instance.LaneCreepLookUp[m_player.targetName].m_silhouetteRenderer != null)
                    {
                        CreepManager.Instance.LaneCreepLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_Outline", 0.0f);
                        CreepManager.Instance.LaneCreepLookUp[m_player.targetName].m_silhouetteRenderer.material.SetFloat("_HideOutline", 0.0f);
                    }
                }

                if (ndx == -1) ndx = 0;
                else ndx += button;

                if (ndx < 0) ndx += nearbyTargets.Count;
                else if (ndx > nearbyTargets.Count - 1) ndx -= (nearbyTargets.Count);

                nearbyTargets[ndx].GetComponent<UnitObject>().m_silhouetteRenderer.material.SetFloat("_Outline", 0.005f);
                PlayerManager.Instance.photonView.RPC("RPC_setPlayerTarget",
                                                       PhotonTargets.All,
                                                       this.name,
                                                       tempHit,
                                                       nearbyTargets[ndx].name,
                                                       (int)targetType);
            }
        }
    }
}
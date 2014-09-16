using UnityEngine;
using System.Collections;

public class Player : UnitObject {
	
	public int team;
	public int playerID;
    int spawnPoint;
	public string playerName;
	////
	public Vector3 target;
	public string targetName;
	public float speed;
    public float totalRespawnTime;
    public float remainingRespawnTime;
	
	Vector3 tempPosition;
	Vector3 tempValue;
	float totalSqrLength;
    ArrayList teamSparkPoints;
    SparkPoint[] sparkPoints;
	GameObject[] otherPlayers;
	
	public float lineAttackDist;
	public float areaAttackRadius;
	public Object lineAttackPrefab;
	public Object areaAttackPrefab;

	public enum PlayerState {
		Idle,
		Moving,
		Capturing,
		Attacking,
        Dead
	};
	public PlayerState playerState;
	
	public enum GameState {
		InGame,
		Victory,
		Defeat
	};
	public GameState gameState;
	
	/* Calculating Player and Region Variables. -jk */
	Region[] region;
	float regionArea;
	float regionSign;
	float regionBarS;
	float regionBarT;
	float regionPointOffset;
	Vector3 regionPoint0;
	Vector3 regionPoint1;
	Vector3 regionPoint2;
	Vector3 regionTempPos;
	Vector3 tempVector;
	
	// Use this for initialization
	void Start () {
		speed = 5;
		target = this.transform.position;
		target.y = 0;
		playerState = PlayerState.Idle;
		gameState = GameState.InGame;
        this.maxHealth = 50;
		this.unitHealth = 50;
		this.baseAttack = 5;
        totalRespawnTime = 5;
        sparkPoints = FindObjectsOfType<SparkPoint>();
        teamSparkPoints = new ArrayList();

		lineAttackDist = 30.0f;
		areaAttackRadius = 10.0f;

        // initialize line renderer for drawing path to false
        GetComponent<LineRenderer>().enabled = false;

		/* initialize region variables. -jk */
		region = GameObject.FindGameObjectWithTag("Ground").GetComponents<Region>();
		regionArea = 0;
		regionSign = 0;
		regionBarS = 0;
		regionBarT = 0;
		regionPointOffset = 100.0f;
		regionPoint0 = Vector3.zero;
		regionPoint1 = Vector3.zero;
		regionPoint2 = Vector3.zero;
		regionTempPos = Vector3.zero;
		tempVector = new Vector3(100.0f,0.0f,100.0f);
	}
	
	// Update is called once per frame
	void Update () {
		UnitUpdate ();
        // Health calculation is currently inside UnitUpdate() so I'll be using a second check for now
        if (unitHealth <= 0 && playerState != PlayerState.Dead) {

            GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerDeath",
                                                           PhotonTargets.All,
                                                           this.name, 
                                                           team);
            playerState = PlayerState.Dead;
        }

		movePlayer ();
        // Draw Path
        /*if (GetComponent<NavMeshAgent>().hasPath && playerState != PlayerState.Dead)
        {
            DrawPath(GetComponent<NavMeshAgent>().path);
            GetComponent<LineRenderer>().enabled = true;
        }
        else 
        {
            GetComponent<LineRenderer>().enabled = false;
        }*/
	}
	
	void OnGUI () {
		if (this.GetComponent<PlayerInput> ().isMine) {
			GUI.TextArea (new Rect (10, 10, 200, 20), "Position:" + this.transform.position.x + ":" + this.transform.position.y + ":" + this.transform.position.z);
			GUI.TextArea (new Rect (10, 30, 200, 20), "Target:" + target.x + ":0:" + target.z);
			GUI.TextArea (new Rect (10, 50, 220, 20), "A for area attack, S for line attack, D for self area attack");
            GUI.TextArea(new Rect(10, 70, 200, 20), "F to kill yourself");
			
			//Gui debug message for state of player in-game/post-game - BR
			switch(gameState) {
			case GameState.InGame:
				GUI.TextArea(new Rect(10, 90, 200, 20), "Game in Progress."); 
				break;
			case GameState.Victory:
				GUI.TextArea(new Rect(10, 90, 200, 20), "Victory!");
				break;
			case GameState.Defeat:
				GUI.TextArea(new Rect(10, 90, 200, 20), "Defeat...");
				break;
			}
		}
	}
	
	void movePlayer () {

		//Stop NavMeshAgent, if Player should actually move will be enables in the Moving PlayerState
		GetComponent<NavMeshAgent>().Stop();
		GetComponent<LineRenderer>().enabled = false;


		switch (playerState) {
		case PlayerState.Idle:
			break;
		case PlayerState.Moving:
			tempPosition = this.transform.position;
			tempPosition.y = 0;
			if (!tempPosition.Equals(target)) {
				tempValue = target - tempPosition;
				totalSqrLength = tempValue.sqrMagnitude;
				tempValue = Vector3.Normalize(tempValue) * speed * Time.deltaTime;
				if (totalSqrLength <= 10.0f) {
					
					if (targetName.Contains("SparkPoint"))
					{
						GameObject.Find("Ground").GetPhotonView().RPC("RPC_setSparkPointCapture",
						                                              PhotonTargets.All,
						                                              targetName,
						                                              this.name,
						                                              team,
						                                              true);
						playerState = PlayerState.Capturing;
					}
					else if (targetName.Contains("Player"))
					{
						GameObject.Find("Manager").GetPhotonView().RPC("RPC_ShootMissile",
						                                              PhotonTargets.All,
						                                              this.playerName,
						                                              targetName,
						                                              this.baseAttack);
						playerState = PlayerState.Attacking;
						
					}
				}
				else if(tempValue.sqrMagnitude > totalSqrLength) {
					this.transform.position.Set(target.x, this.transform.position.y, target.z);
					playerState = PlayerState.Idle;
				}
				else {
					//this.transform.position = this.transform.position + tempValue;
					// Draw Path
					GetComponent<NavMeshAgent>().Move(tempValue);
					if (GetComponent<NavMeshAgent>().hasPath){
						DrawPath(GetComponent<NavMeshAgent>().path);
						GetComponent<LineRenderer>().enabled = true;
					}
				}
			}
			break;
		case PlayerState.Capturing:
			break;
		case PlayerState.Attacking:
			tempPosition = this.transform.position;
			tempPosition.y = 0;
			if (!tempPosition.Equals(target)) {
				tempValue = target - tempPosition;
				totalSqrLength = tempValue.sqrMagnitude;
				if (totalSqrLength > 10.0f ) {//Our target ran away/became distant,we can no longer deal DPS
					playerState = PlayerState.Moving;//Chase the target
					
				}
				
			}
			break;
        case PlayerState.Dead:
            if (remainingRespawnTime <= 0.0f) {
                // Grab all sparkpoints on your team
                for (int i = 0; i < sparkPoints.Length; i++) {
                    if (sparkPoints[i].GetOwner() == team) {
                        teamSparkPoints.Add(sparkPoints[i]);
                    }
                }
                // Select random spawn point from list of team spark points
                if (teamSparkPoints.Count > 0) {
                    spawnPoint = Random.Range(0, teamSparkPoints.Count);
                    tempPosition = ((SparkPoint)teamSparkPoints[spawnPoint]).transform.position;
                    tempPosition.y = 1.2f;
                    tempPosition.z -= 2.0f;
                }
				// Dying with no sparkpoints currently causes a loss - Should change in the future
                else {
					gameState = GameState.Defeat;
					otherPlayers = GameObject.FindGameObjectsWithTag("player");
					
					for(int a = 0; a < otherPlayers.Length; a++) {
						if(!otherPlayers[a].GetComponent<Player>().playerName.Equals (playerName))	{
							if(otherPlayers[a].GetComponent<Player>().team == team)
								otherPlayers[a].GetComponent<Player>().SetVictorious(false);
							else
								otherPlayers[a].GetComponent<Player>().SetVictorious(true);
						}
					}
					
                    tempPosition = new Vector3(32.0f, 1.2f, 0.0f); // arbitrary value
                }
                
                GameObject.Find("Ground").GetPhotonView().RPC("RPC_setPlayerRespawn",
                                                              PhotonTargets.All,
                                                              this.name,
                                                              tempPosition);
                playerState = PlayerState.Idle;
            }
            else {
                remainingRespawnTime -= Time.deltaTime;
            }
            break;
		}

	}

	//Sends the call for combat off to the combat manager
	//Input: Type of attack (area or line) and where the combat is located
	public void EngageCombat(PlayerInput.TargetType combatType, Vector3 location) {
		//Combat manager was null so I get it here for now
		GameObject manager = GameObject.Find ("Manager");
		combatManager = (CombatManager) manager.GetComponent ("CombatManager");
		combatManager.startCombat (this.name, combatType, location);
	}

	public void UpdateTarget(Vector3 target, string targetName) {
		if (playerState == PlayerState.Capturing) {
			GameObject.Find("Ground").GetPhotonView().RPC("RPC_setSparkPointCapture", PhotonTargets.All,
			                                              this.targetName,
			                                              this.name,
			                                              team,
			                                              false);
		}
		this.target = target;
		this.targetName = targetName;
		playerState = PlayerState.Moving;
	}

    // Turn player invisible and disable their input and start their respawn timer 
    public void KillPlayer() {
        playerState = PlayerState.Dead;
        target = this.transform.position;
        renderer.enabled = false;
        unitHealth = 0;
        remainingRespawnTime = totalRespawnTime;
    }

    // Turn player visible again, heal them, spawn them at a sparkpoint, and reenable their input.
    public void RespawnPlayer(Vector3 location) {
        playerState = PlayerState.Idle;
        this.transform.position = location;
        target = location;
        this.enabled = true;
        renderer.enabled = true;
        unitHealth = maxHealth;
    }
	
	public void CapturedObjective(string sparkPointName) {
		playerState = PlayerState.Idle;
		TriangulatePosition(sparkPointName,null);
	}
	
	public int GetTeam() {
		return team;
	}

    public PlayerState GetState() {
        return playerState;
    }
	
	/* Called when game is over and Victory gameState is determined*/
	public void SetVictorious(bool isVictorious) {
		if(isVictorious) {
			gameState = GameState.Victory;
		}
		else {
			gameState = GameState.Defeat;	
		}
	}
	
    public void DrawPath(NavMeshPath path)
    {
        LineRenderer LR = GetComponent<LineRenderer>();
        

        LR.SetVertexCount(path.corners.Length);

        int i = 0;
        foreach (Vector3 v in path.corners)
        {
            LR.SetPosition(i, v);
            i++;
        }
     

    }

	/* Fired when stepping into or out of a region. -jk */
	private void OnTriggerExit(Collider collider) {
		if (collider.name.Contains("Lane")) {
			TriangulatePosition(null,collider);
		}
	}
	
	/* Turns on player's health regen when in own region. -jk */
	private void TriangulatePosition(string sparkPointName, Collider collider) {
		/* called when stepping across lane into region. -jk */
		if (sparkPointName == null) {
			for (int i = 0; i < region.Length; i++) {
				if (region[i].GetActivated() && region[i].GetTeam() == team) {
					for (int j = 0; j < region[i].regionPoints.Length; j++) {
						if (!collider.name.Contains(region[i].regionPoints[j].name)) {
							continue;
						}
						else {
							regionPoint0 = region[i].regionPoints[0].transform.position;
							regionPoint0 += tempVector;
							regionPoint1 = region[i].regionPoints[1].transform.position;
							regionPoint1 += tempVector;
							regionPoint2 = region[i].regionPoints[2].transform.position;
							regionPoint2 += tempVector;
							regionTempPos = transform.position + tempVector;
							regionArea = 0.5f * (-regionPoint1.z * regionPoint2.x + regionPoint0.z * 
							                     (-regionPoint1.x + regionPoint2.x) + regionPoint0.x *
							                     (regionPoint1.z - regionPoint2.z) + regionPoint1.x * 
							                     regionPoint2.z);
							regionSign = regionArea < 0 ? -1 : 1;
							regionBarS = (regionPoint0.z * regionPoint2.x - regionPoint0.x * 
							              regionPoint2.z + (regionPoint2.z - regionPoint0.z) *
							              regionTempPos.x + (regionPoint0.x - regionPoint2.x) * 
							              regionTempPos.z) * regionSign;
							regionBarT = (regionPoint0.x * regionPoint1.z - regionPoint0.z * 
							              regionPoint1.x + (regionPoint0.z - regionPoint1.z) * 
							              regionTempPos.x + (regionPoint1.x - regionPoint0.x) * 
							              regionTempPos.z) * regionSign;
							if (regionBarS > 0 && regionBarT > 0 && 
							    (regionBarS + regionBarT) < 2 * regionArea * regionSign) {
								particleSystem.Play();
							}
							else {
								particleSystem.Stop();
							}
							return;
						}
					}
				}
			}
		}
		/* called when in region and capturing. -jk */
		else {
			for (int i = 0; i < region.Length; i++) {
				if (region[i].GetActivated() && region[i].GetTeam() == team) {
					for (int j = 0; j < region[i].regionPoints.Length; j++) {
						if (!region[i].regionPoints[j].name.Contains(sparkPointName)) {
							Debug.Log (region[i].regionPoints[j].name + " " +sparkPointName);
							continue;
						}
						else {
							regionPoint0 = region[i].regionPoints[0].transform.position;
							regionPoint0 += tempVector;
							regionPoint1 = region[i].regionPoints[1].transform.position;
							regionPoint1 += tempVector;
							regionPoint2 = region[i].regionPoints[2].transform.position;
							regionPoint2 += tempVector;
							regionTempPos = transform.position + tempVector;
							regionArea = 0.5f * (-regionPoint1.z * regionPoint2.x + regionPoint0.z * 
							                     (-regionPoint1.x + regionPoint2.x) + regionPoint0.x *
							                     (regionPoint1.z - regionPoint2.z) + regionPoint1.x * 
							                     regionPoint2.z);
							regionSign = regionArea < 0 ? -1 : 1;
							regionBarS = (regionPoint0.z * regionPoint2.x - regionPoint0.x * 
							              regionPoint2.z + (regionPoint2.z - regionPoint0.z) *
							              regionTempPos.x + (regionPoint0.x - regionPoint2.x) * 
							              regionTempPos.z) * regionSign;
							regionBarT = (regionPoint0.x * regionPoint1.z - regionPoint0.z * 
							              regionPoint1.x + (regionPoint0.z - regionPoint1.z) * 
							              regionTempPos.x + (regionPoint1.x - regionPoint0.x) * 
							              regionTempPos.z) * regionSign;
							if (regionBarS > 0 && regionBarT > 0 && 
							    (regionBarS + regionBarT) < 2 * regionArea * regionSign) {
								particleSystem.Play();
							}
							else {
								particleSystem.Stop();
							}
							return;
						}
					}
				}
			}
		}
	}
}

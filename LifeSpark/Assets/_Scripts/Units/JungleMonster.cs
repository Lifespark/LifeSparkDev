using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class JungleMonster : UnitObject
{
    #region ENUM_FIELD
    public enum JungleMonsterState
    {
        IDLE,
        MOVING,
        CHASING,
        RETURNING,
        ATTACKING,
        ATTACKED,
        DEAD,
        DEFAULT
    }

    public enum AnimatorType
    {
        BOOL = 1,
        INT = 2,
        FLOAT = 3,
        TRIGGER = 4
    }

    [Flags]
    public enum PacketMask
    {
        NONE = 0,
        POSITION = 1,
        ROTATION = 2,
        VELOCITY = 4,
    }
    #endregion

    #region JungleMonster_ATTRIBUTE
    public float maxSpeed = 5.0f;
    public float detectRadius = 20;
    public float attackRadius = 2;
    public Transform target;
    public Vector3 source;
    public GameObject lockOnEnemy;
    public JungleMonsterState curState;
    public Vector3 spreadDir;
    #endregion

    #region JungleMonster_STATE
    public JungleMonsterStateIdle jungleMonsterStateIdle;
    public JungleMonsterStateMove jungleMonsterStateMove;
    public JungleMonsterStateAttack jungleMonsterStateAttack;
    public JungleMonsterStateChase jungleMonsterStateChase;
    public JungleMonsterStateReturn jungleMonsterStateReturn;
    public JungleMonsterStateDead jungleMonsterStateDead;

    private JungleMonsterStateBase jungleMonsterState;
    #endregion

    private Vector3 correctjungleMonsterPos;
    private Quaternion correctjungleMonsterRot;
    private Animator anim;
    private NavMeshAgent navAgent;
    private NavMeshPath mainNavPath = new NavMeshPath();

    private Vector3 lastSentPosition;
    private Quaternion lastSentOrientation;
    private Vector3 lastSentVelocity;

    private bool appliedInitialUpdate = false;

    // store the enemy so we do not search them during game
    // in case of a player die or drop, could just move it out of vision and not destroy it in case of null reference
    public List<Player> enemyPlayers = new List<Player>();

    // Use this for initialization
    void Awake()
    {
        // initialize all states
        jungleMonsterStateIdle = new JungleMonsterStateIdle(this);
        jungleMonsterStateMove = new JungleMonsterStateMove(this);
        jungleMonsterStateAttack = new JungleMonsterStateAttack(this);
        jungleMonsterStateChase = new JungleMonsterStateChase(this);
        jungleMonsterStateReturn = new JungleMonsterStateReturn(this);
        jungleMonsterStateDead = new JungleMonsterStateDead(this);

        // initialize Findable stuff
        SwitchState(JungleMonsterState.IDLE);
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        anim = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        source = gameObject.transform.position;

        //GetComponentInChildren<SkinnedMeshRenderer>().material.color
        //    = new Color((float)photonView.instantiationData[3],
        //                (float)photonView.instantiationData[4],
        //                (float)photonView.instantiationData[5],
        //                (float)photonView.instantiationData[6]);
        //source = SparkPointManager.Instance.sparkPointsDict[(string)photonView.instantiationData[7]].transform;

        // calculate and store the nav path from source to target
        //navAgent.CalculatePath(target.position + spreadDir * 2.0f, mainNavPath);
    }

    // Update is called once per frame
    void Update()
    {
        // if I'm not in control, sync position from network
        if (!PhotonNetwork.isMasterClient)
        {
            transform.position = Vector3.Lerp(transform.position, this.correctjungleMonsterPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, this.correctjungleMonsterRot, Time.deltaTime * 5);
        }
        // otherwise process current state's update
        else
        {
            //if (targetSparkPoint.owner == owner && curState != jungleMonsterState.DEAD
            //    || targetSparkPoint.sparkPointState == SparkPoint.SparkPointState.DESTROYED)
            //{
            //    SwitchState(jungleMonsterState.DEAD);
            //    return;
            //}
            if (jungleMonsterState != null)
                jungleMonsterState.OnUpdate();
        }
    }

    /// <summary>
    /// switch to another state
    /// </summary>
    /// <param name="toState">destination state</param>
    private void SwitchState(JungleMonsterState toState)
    {
        if (jungleMonsterState != null && jungleMonsterState.State == toState)
            return;
        curState = toState;
        if (jungleMonsterState != null)
            jungleMonsterState.OnExit();

        switch (toState)
        {
            case JungleMonsterState.IDLE:
                jungleMonsterState = jungleMonsterStateIdle;
                break;
            case JungleMonsterState.MOVING:
                jungleMonsterState = jungleMonsterStateMove;
                break;
            case JungleMonsterState.ATTACKING:
                jungleMonsterState = jungleMonsterStateAttack;
                break;
            case JungleMonsterState.CHASING:
                jungleMonsterState = jungleMonsterStateChase;
                break;
            case JungleMonsterState.RETURNING:
                jungleMonsterState = jungleMonsterStateReturn;
                break;
            case JungleMonsterState.DEAD:
                jungleMonsterState = jungleMonsterStateDead;
                break;
        }
        jungleMonsterState.OnEnter();
    }

    /// <summary>
    /// photon syncing
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {

            // reduce network traffic
            PacketMask mask = PacketMask.NONE;

            if (transform.position != lastSentPosition)
            {
                lastSentPosition = transform.position;
                mask |= PacketMask.POSITION;
            }

            if (transform.rotation != lastSentOrientation)
            {
                lastSentOrientation = transform.rotation;
                mask |= PacketMask.ROTATION;
            }

            if (rigidbody.velocity != lastSentVelocity)
            {
                lastSentVelocity = rigidbody.velocity;
                mask |= PacketMask.VELOCITY;
            }

            stream.SendNext(mask);

            // only send changed attributes
            if ((mask & PacketMask.POSITION) != PacketMask.NONE) stream.SendNext(transform.position);
            if ((mask & PacketMask.ROTATION) != PacketMask.NONE) stream.SendNext(transform.rotation);
            if ((mask & PacketMask.VELOCITY) != PacketMask.NONE) stream.SendNext(rigidbody.velocity);
        }
        else
        {
            PacketMask mask = (PacketMask)stream.ReceiveNext();

            if ((mask & PacketMask.POSITION) != PacketMask.NONE) correctjungleMonsterPos = (Vector3)stream.ReceiveNext();
            if ((mask & PacketMask.ROTATION) != PacketMask.NONE) correctjungleMonsterRot = (Quaternion)stream.ReceiveNext();
            if ((mask & PacketMask.VELOCITY) != PacketMask.NONE) rigidbody.velocity = (Vector3)stream.ReceiveNext();

            if (!appliedInitialUpdate)
            {
                appliedInitialUpdate = true;
                transform.position = correctjungleMonsterPos;
                transform.rotation = correctjungleMonsterRot;
                rigidbody.velocity = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// change animator parameter over network. must cast animType to int and value to float when calling
    /// </summary>
    /// <param name="animType"></param>
    /// <param name="param"></param>
    /// <param name="value"></param>
    [RPC]
    void RPC_setAnimParam(int animType, string param, float value = 0)
    {
        switch ((AnimatorType)animType)
        {
            case AnimatorType.BOOL:
                anim.SetBool(param, value == 1);
                break;
            case AnimatorType.INT:
                anim.SetInteger(param, (int)value);
                break;
            case AnimatorType.FLOAT:
                anim.SetFloat(param, value);
                break;
            case AnimatorType.TRIGGER:
                anim.SetTrigger(param);
                break;
        }
    }

    /// <summary>
    /// Base state class. Can only be inherited
    /// </summary>
    [Serializable]
    public abstract class JungleMonsterStateBase
    {
        // state name
        protected JungleMonsterState state;
        //  jungleMonster instance
        protected JungleMonster jungleMonster;
        // when do we start this state?
        protected float startTime;

        public JungleMonsterState State { get { return state; } }

        /// <summary>
        /// This func will be called once when state starts
        /// <para>Put all initialization code here</para> 
        /// </summary>
        public abstract void OnEnter();

        /// <summary>
        /// This func will be called every frame when state is activated
        /// <para>Put all Update code here</para> 
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
        /// This func will be called once when state ends
        /// <para>Put all cleanup code here</para> 
        /// </summary>
        public abstract void OnExit();

        public JungleMonsterStateBase() { startTime = Time.time; }
    }

    [Serializable]
    public class JungleMonsterStateIdle : JungleMonsterStateBase
    {
        public JungleMonsterStateIdle(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.IDLE;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Idle");
            startTime = Time.time;
            if (jungleMonster.anim)
                //jungleMonster.anim.SetTrigger("goIdle");
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)JungleMonster.AnimatorType.TRIGGER, "goIdle", 0f);
        }

        public override void OnUpdate()
        {
            if (Vector3.SqrMagnitude(jungleMonster.source - jungleMonster.transform.position) > 1.0)
            {
                jungleMonster.SwitchState(JungleMonsterState.ATTACKING);
                return;
            }
        }

        public override void OnExit()
        {

        }
    }

    [Serializable]
    public class JungleMonsterStateMove : JungleMonsterStateBase
    {
        public JungleMonsterStateMove(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.MOVING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Move");
            startTime = Time.time;
            if (jungleMonster.anim)
                //jungleMonster.anim.SetTrigger("goWalk");
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)JungleMonster.AnimatorType.TRIGGER, "goWalk", 0f);
            jungleMonster.navAgent.Resume();
            jungleMonster.navAgent.SetPath(jungleMonster.mainNavPath);
        }

        public override void OnUpdate()
        {
            //foreach (var enemy in jungleMonster.enemyPlayers)
            //{
            //    // if enemy in sight, start chasing it
            //    if (Vector3.SqrMagnitude(jungleMonster.transform.position - enemy.transform.position) < jungleMonster.detectRadius * jungleMonster.detectRadius)
            //    {
            //        jungleMonster.lockOnEnemy = enemy.gameObject;
            //        jungleMonster.SwitchState(JungleMonsterState.CHASING);
            //        return;
            //    }
            //}

            //if (jungleMonster.target != null)
            //{
            //    // target not reached, continue approaching
            //    Vector3 distVector = jungleMonster.target.position + jungleMonster.spreadDir * 2.0f - jungleMonster.transform.position;
            //    distVector.y = 0;
            //    if (Vector3.SqrMagnitude(distVector) > 0.01)
            //    {
            //        /*
            //        Debug.Log(jungleMonster.target.position + jungleMonster.spreadDir * 2.0f - jungleMonster.transform.position);
            //        Vector3 targetPos = jungleMonster.target.position + jungleMonster.spreadDir * 2.0f;
            //        Vector3 direction = (targetPos - jungleMonster.transform.position).normalized;
            //        direction.y = 0;

            //        jungleMonster.transform.rotation = Quaternion.LookRotation(direction); // maybe use a slerp to limit angular speed
            //        jungleMonster.transform.position += direction * jungleMonster.maxSpeed * Time.deltaTime;
            //        */
            //    }
            //    else
            //    {
            //        // start capturing sparkpoint
            //        jungleMonster.SwitchState(JungleMonsterState.CAPTURING);
            //        return;
            //    }
            //}
            //else
            //{
            //    // what if the target has been destroyed?
            //    jungleMonster.SwitchState(jungleMonsterState.IDLE);
            //    return;
            //}
        }

        public override void OnExit()
        {
            jungleMonster.navAgent.Stop();
        }
    }

    [Serializable]
    public class JungleMonsterStateAttack : JungleMonsterStateBase
    {
        public float attackIntervial = 2;
        private float lastAttackTime = -2;

        public JungleMonsterStateAttack(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.ATTACKING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Attack");
            startTime = Time.time;
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            int index = -1;

            for(int i = 0; i < allPlayers.Length;i++)
            {
                float minDistance = 0;
                float currentDistance = Vector3.SqrMagnitude(jungleMonster.transform.position - allPlayers[i].transform.position);
                if (index == -1 ||
                    currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    index = i;
                }
            }
            jungleMonster.lockOnEnemy = allPlayers[index].gameObject;
                
            if (Vector3.SqrMagnitude(jungleMonster.transform.position - allPlayers[index].transform.position) > jungleMonster.attackRadius * jungleMonster.attackRadius)
            {
                jungleMonster.SwitchState(JungleMonsterState.CHASING);
                return;
            }

        }

        public override void OnUpdate()
        {
            float sqrDistance = Vector3.SqrMagnitude(jungleMonster.lockOnEnemy.transform.position - jungleMonster.transform.position);
            // if enemy not in attack range but in chasing range, return to tracing state
            if (sqrDistance > jungleMonster.attackRadius * jungleMonster.attackRadius)
            {
                jungleMonster.SwitchState(JungleMonsterState.CHASING);
                return;
            }
            // attack enemy
            if (Time.time - lastAttackTime > attackIntervial)
            {
                if (jungleMonster.anim)
                    //jungleMonster.anim.SetTrigger("goAttack");
                    jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)JungleMonster.AnimatorType.TRIGGER, "goAttack", 0f);
            }
        }

        public override void OnExit()
        {

        }
    }

    [Serializable]
    public class JungleMonsterStateChase : JungleMonsterStateBase
    {
        public JungleMonsterStateChase(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.CHASING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Chase");
            startTime = Time.time;
            if (jungleMonster.anim)
                //jungleMonster.anim.SetTrigger("goWalk");
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)JungleMonster.AnimatorType.TRIGGER, "goWalk", 0f);
            jungleMonster.navAgent.Resume();
            jungleMonster.navAgent.SetDestination(jungleMonster.lockOnEnemy.transform.position);
        }

        public override void OnUpdate()
        {
            float sqrDistance = Vector3.SqrMagnitude(jungleMonster.transform.position - jungleMonster.source);

            if (sqrDistance < jungleMonster.detectRadius *jungleMonster.detectRadius)
            {
                // still too far away to attack
                if (Vector3.SqrMagnitude(jungleMonster.lockOnEnemy.transform.position - jungleMonster.transform.position) > jungleMonster.attackRadius * jungleMonster.attackRadius)
                {
                    jungleMonster.navAgent.SetDestination(jungleMonster.lockOnEnemy.transform.position);
                }
                else
                {
                    jungleMonster.SwitchState(JungleMonsterState.ATTACKING);
                    return;
                }
            }
            else
            {
                jungleMonster.SwitchState(JungleMonsterState.RETURNING);
                return;
            }
        }

        public override void OnExit()
        {
            jungleMonster.navAgent.Stop();
        }
    }

    [Serializable]
    public class JungleMonsterStateReturn : JungleMonsterStateBase
    {
        public JungleMonsterStateReturn(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.RETURNING;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            Debug.Log("Return");
            startTime = Time.time;
            if (jungleMonster.anim)
                jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)JungleMonster.AnimatorType.TRIGGER, "goWalk", 0f);

            jungleMonster.navAgent.Resume();
            jungleMonster.navAgent.SetDestination(jungleMonster.source);
        }

        public override void OnUpdate()
        {
            if (true)
            {
                if (Vector3.SqrMagnitude(jungleMonster.source - jungleMonster.transform.position) > 0.3)
                {
                    jungleMonster.navAgent.SetDestination(jungleMonster.source);
                }
                else
                {
                    Debug.Log("LLL");
                    jungleMonster.SwitchState(JungleMonsterState.IDLE);
                    return;
                }
            }
            else
            {
                // what if the target has been destroyed?
                jungleMonster.SwitchState(JungleMonsterState.IDLE);
                return;
            }
        }

        public override void OnExit()
        {
            jungleMonster.navAgent.Stop();
        }
    }

    

    [Serializable]
    public class JungleMonsterStateDead : JungleMonsterStateBase
    {
        public float corpseRemainTime = 3f;

        public JungleMonsterStateDead(JungleMonster pjungleMonster)
        {
            startTime = Time.time;
            state = JungleMonsterState.DEAD;
            jungleMonster = pjungleMonster;
        }

        public override void OnEnter()
        {
            startTime = Time.time;
            jungleMonster.photonView.RPC("RPC_setAnimParam", PhotonTargets.AllBufferedViaServer, (int)JungleMonster.AnimatorType.TRIGGER, "goDead", 0f);
        }

        public override void OnUpdate()
        {
            if (Time.time - startTime >= corpseRemainTime)
            {
               // jungleMonsterManager.Instance.jungleMonsterDict[jungleMonster.source.gameObject].Remove(jungleMonster);
                PhotonNetwork.Destroy(jungleMonster.gameObject);
            }
        }

        public override void OnExit()
        {

        }
    }
}

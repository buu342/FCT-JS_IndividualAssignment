using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RoomDef = ProcGenner.RoomDef;
using RoomDefMulti= ProcGennerMultiplayer.RoomDef;
using Unity.AI.Navigation;
using Photon.Pun;
public class MonsterAI : MonoBehaviour
{
    public enum MonsterState {
        ChasingPlayer,
        Patrolling,
        CheckingSound
    }
    
    public enum MonsterCombatState {
        Idle,
        Attacking,
        Staggared
    }
    
    const float POSITION_THRESHOLD = 2.0f;
    [HideInInspector]
    public MonsterState monsterState;
    [HideInInspector]
    public MonsterCombatState monsterCombatState = MonsterCombatState.Idle;
    public HunterAnimations m_MonsterAnims;
    private bool multiplayer=JoinMultiplayer.Multiplayer;
    private float MonsterSpeed;
    private Vector3 destination;
    private NavMeshAgent agent;
    //need to guarantee that its centered
    private GameObject playerToChase;
    private float m_LastSawPlayerTimer = 0;
    private float m_CombatTimer = 0;
    private AudioManager m_Audio;

    private PhotonView view;

    private bool  multiplayer1= JoinMultiplayer.Multiplayer;
    void Awake() {
        agent = GetComponent<NavMeshAgent>();   
        monsterState = MonsterState.Patrolling;
    }

    void Start()
    {   view = GetComponent<PhotonView>();
        monsterState = MonsterState.Patrolling;
        MonsterSpeed = agent.speed;
        this.m_Audio = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        //if(multiplayer1)
         //Random.InitState(view.ViewID);
    }

    // Update is called once per frame
    void Update()
    {  if(view!=null)
            if(!view.IsMine)
            return;
        if (playerToChase != null)
        {
           if (checkIfCanSeePlayer())
            {
                if (monsterState != MonsterState.ChasingPlayer && monsterCombatState == MonsterCombatState.Idle)
                {
                    this.m_Audio.Stop("Hunter/Damage");
                    this.m_Audio.Stop("Hunter/HearSound");
                    this.m_Audio.Stop("Hunter/Attack");
                    this.m_Audio.Play("Hunter/SpotPlayer", this.transform.gameObject);
                }
                monsterState = MonsterState.ChasingPlayer;
                this.m_LastSawPlayerTimer = 0;
            }
            else if (this.m_LastSawPlayerTimer == 0)
                this.m_LastSawPlayerTimer = Time.time + 5.0f;
        }
        
        if (this.m_LastSawPlayerTimer != 0 && this.m_LastSawPlayerTimer < Time.time)
            monsterState = MonsterState.CheckingSound;

        if (monsterCombatState == MonsterCombatState.Idle)
        {
            agent.speed = MonsterSpeed;
            switch(monsterState) {
                case MonsterState.ChasingPlayer:
                    ChasePlayer();
                    Debug.Log("chasing");
                    break;
                case MonsterState.Patrolling:
                    Patrol();
                    break;
                case MonsterState.CheckingSound:
                    if(hasReachedDestination()) {
                        monsterState = MonsterState.Patrolling;
                    }
                    break;
            }
            
            if (playerToChase != null && monsterState == MonsterState.ChasingPlayer && Vector3.Distance(playerToChase.transform.position, transform.position) < 2.0f && this.monsterCombatState == MonsterCombatState.Idle)
            {
                this.monsterCombatState = MonsterCombatState.Attacking;
                this.m_CombatTimer = Time.time + 4.0f;
                this.m_MonsterAnims.TriggerAttack();
                this.m_Audio.Stop("Hunter/Damage");
                this.m_Audio.Stop("Hunter/HearSound");
                this.m_Audio.Stop("Hunter/SpotPlayer");
                this.m_Audio.Play("Hunter/Attack", this.transform.gameObject);
            }
        }
        else
            agent.speed = 0;
        
        // Go back to idle state if the timer ran out
        if (monsterCombatState != MonsterCombatState.Idle && this.m_CombatTimer < Time.time)
            this.monsterCombatState = MonsterCombatState.Idle;
        
    }

    public bool hasReachedDestination() {
        //Debug.Log(Vector3.Distance(destination, transform.position));
          if(Vector3.Distance(destination, transform.position) < POSITION_THRESHOLD) {
                //Debug.Log("Arrived to destination");
                return true;
            }
        return false;
    }

    public bool checkIfCanSeePlayer() {
        Vector3 playerPos = playerToChase.transform.position - transform.position;
    
        float isInFOV = Vector3.Dot(transform.forward, playerPos);

        if(isInFOV > 0.5f) {
            //means player is in front of the monster in a less than 45º angle
            RaycastHit rayInfo;
            if(Physics.Raycast(transform.position, playerPos, out rayInfo)) {
                 //the ray from the monster to the player hit something
                 if(rayInfo.collider != null && rayInfo.collider.tag == "Player") {
                     //the monster saw the player
                      //Debug.Log("Saw Player");
                    return true;
                 }   
            }
            
        }
        return false;
    }

    public void ChasePlayer() {
        if (playerToChase == null)
            return;
          //TODO: change music for chasing
        monsterState = MonsterState.ChasingPlayer;
        if(playerToChase!=null){
            agent.SetDestination(playerToChase.transform.position);
            destination = playerToChase.transform.position;
            if (Vector3.Distance(destination, transform.position) < 2.0f) {
                    //close to player to attack
                    //TODO: start attacking animation
            }
        }
    }

    public void Patrol() {
        
        if(!agent.hasPath&!multiplayer) {
           
            List<RoomDef> roomsInLevel = GameObject.Find("SceneController").GetComponent<ProcGenner>().GetRoomDefs();
            int roomToCheck = Random.Range(0,roomsInLevel.Count);
            Vector3 roomMidPoint =roomsInLevel[roomToCheck].midpoint; 
            destination = new Vector3(roomMidPoint.x,roomsInLevel[roomToCheck].position.y,roomMidPoint.z);
            agent.SetDestination(destination);
            Debug.Log("Patrolling to: (" + destination.x + "," + destination.y + "," + destination.z + ")");
    }
        else if(!agent.hasPath&multiplayer){
           List<RoomDefMulti> roomsInLevel = GameObject.Find("SceneController").GetComponent<ProcGennerMultiplayer>().GetRoomDefs();
           int roomToCheck = Random.Range(0,roomsInLevel.Count);
            Vector3 roomMidPoint =roomsInLevel[roomToCheck].midpoint; 
            destination = new Vector3(roomMidPoint.x,roomsInLevel[roomToCheck].position.y,roomMidPoint.z);
            agent.SetDestination(destination);
            Debug.Log("Patrolling to: (" + destination.x + "," + destination.y + "," + destination.z + ")");
        }
    }

    public void AlertSound(Vector3 origin, float maxDistancesqr) {
        if(monsterState == MonsterState.Patrolling) {
            if( (transform.position-origin).sqrMagnitude < (maxDistancesqr)) {
                destination = origin;
                agent.SetDestination(origin);
                this.m_Audio.Stop("Hunter/Damage");
                this.m_Audio.Stop("Hunter/SpotPlayer");
                this.m_Audio.Stop("Hunter/Attack");
                this.m_Audio.Play("Hunter/HearSound", this.transform.gameObject);
                monsterState = MonsterState.CheckingSound;
            }
        }
    }

    public Vector3 GetDestination()
    {
        return destination;
    }

    public void SetPlayerTarget(GameObject target) {
        playerToChase = target;
    }
    
    public void TakeDamage()
    {
        this.monsterState = MonsterState.ChasingPlayer;
        this.monsterCombatState = MonsterCombatState.Staggared;
        this.m_CombatTimer = Time.time + 4.5f;
        this.m_MonsterAnims.TriggerStagger();
        this.m_Audio.Stop("Hunter/SpotPlayer");
        this.m_Audio.Stop("Hunter/HearSound");
        this.m_Audio.Stop("Hunter/Attack");
        this.m_Audio.Play("Hunter/Damage", this.transform.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RoomDef = ProcGenner.RoomDef;
using RoomDefMulti= ProcGennerMultiplayer.RoomDef;
using Unity.AI.Navigation;
using Photon.Pun;

public class ConvertedAI : MonoBehaviour
{
    public enum MonsterState {
        Idle,
        ChasingPlayer,
        Dead,
    }
    
    public enum MonsterCombatState {
        Idle,
        Attacking,
        Staggared
    }
    
    const float POSITION_THRESHOLD = 2.0f;
    [HideInInspector]
    public MonsterState monsterState = MonsterState.Idle;
    [HideInInspector]
    public MonsterCombatState monsterCombatState = MonsterCombatState.Idle;
    //public HunterAnimations m_MonsterAnims;
    private bool multiplayer=JoinMultiplayer.Multiplayer;
    private float MonsterSpeed;
    private Vector3 destination;
    private NavMeshAgent agent;
    //need to guarantee that its centered
    public GameObject playerToChase;
    private float m_LastSawPlayerTimer = 0;
    private float m_CombatTimer = 0;
    private AudioManager m_Audio;
    public Animator m_Animator;
    public GameObject m_HurtBoxPlacement;
    public GameObject m_HurtBoxPrefab;

    private PhotonView view;

    private bool  multiplayer1= JoinMultiplayer.Multiplayer;
    void Awake() {
        agent = GetComponent<NavMeshAgent>();   
        monsterState = MonsterState.Idle;
    }

    void Start()
    {   view = GetComponent<PhotonView>();
        monsterState = MonsterState.Idle;
        MonsterSpeed = agent.speed;
        this.m_Audio = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        agent.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {  
        if(view!=null)
            if(!view.IsMine)
                return;
            
        if (this.monsterState == MonsterState.Dead)
            return;
            
        if (playerToChase != null)
        {
            if (monsterCombatState == MonsterCombatState.Idle)
            {
                if (this.monsterState == MonsterState.Idle && Vector3.Distance(playerToChase.transform.position, transform.position) < 10.0f && checkIfCanSeePlayer())
                {
                    this.monsterState = MonsterState.ChasingPlayer;
                    this.m_Audio.Play("Converted/Spot", this.transform.gameObject);
                }
                
                if (this.monsterState == MonsterState.ChasingPlayer)
                {
                    agent.speed = MonsterSpeed;
                    this.m_Animator.SetBool("Walking", true);
                    agent.SetDestination(this.playerToChase.transform.position);
                    if (Vector3.Distance(playerToChase.transform.position, transform.position) > 10.0f)
                    {
                        this.monsterState = MonsterState.Idle;
                        this.m_Animator.SetBool("Walking", false);
                    }
                }
                
                if (playerToChase != null && monsterState == MonsterState.ChasingPlayer && Vector3.Distance(playerToChase.transform.position, transform.position) < 2.0f && this.monsterCombatState == MonsterCombatState.Idle)
                {
                    this.monsterCombatState = MonsterCombatState.Attacking;
                    this.m_CombatTimer = Time.time + 1.8f;
                    this.m_Animator.SetTrigger("Attack");
                    this.m_Audio.Play("Converted/Attack", this.transform.gameObject);
                }
            }
            if (monsterCombatState != MonsterCombatState.Idle && this.m_CombatTimer < Time.time)
                this.monsterCombatState = MonsterCombatState.Idle;
        }
    }

    public bool checkIfCanSeePlayer() {
        Vector3 playerPos = playerToChase.transform.position - transform.position;
        RaycastHit rayInfo;
        if(Physics.Raycast(transform.position, playerPos, out rayInfo)) {
             //the ray from the monster to the player hit something
             if(rayInfo.collider != null && rayInfo.collider.tag == "Player") {
                return true;
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

    public void SetPlayerTarget(GameObject target)
    {
        playerToChase = target;
    }
    
    public void TakeDamage()
    {
        agent.enabled = false;
        agent.speed = 0;
        this.m_Animator.enabled = false;
        this.m_Audio.Play("Converted/Die", this.transform.gameObject);
        
        Rigidbody[] ragdollbodies = this.GetComponentsInChildren<Rigidbody>();
        Collider[] ragdollcolliders = this.GetComponentsInChildren<Collider>();
        
        // Enable all the rigidbodies and box colliders
        foreach (Rigidbody rb in ragdollbodies)
            rb.isKinematic = false;
        foreach (Collider rc in ragdollcolliders)
            rc.enabled = true;
            
        // Disable collisions
        this.GetComponent<CapsuleCollider>().enabled = false;
        this.monsterState = MonsterState.Dead;
    }
    
    public void MakeHurtBox()
    {
        GameObject obj = Instantiate(this.m_HurtBoxPrefab, this.m_HurtBoxPlacement.transform.position, this.m_HurtBoxPlacement.transform.rotation);
        obj.transform.parent = this.m_HurtBoxPlacement.transform;
    }
    
    
    /*==============================
        AnimationEventSound
        Called when an animation event sound happens
        @param The sound to play
    ==============================*/

    void AnimationEventSound(string sound) 
    {
        this.m_Audio.Play(sound, this.transform.gameObject);
    }
}
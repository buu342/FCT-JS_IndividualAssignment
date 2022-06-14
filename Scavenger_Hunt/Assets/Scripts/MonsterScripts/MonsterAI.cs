using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RoomDef = ProcGenner.RoomDef;
using Unity.AI.Navigation;
public class MonsterAI : MonoBehaviour
{
    public enum MonsterState {
        ChasingPlayer,
        Patrolling,
        CheckingSound
    }
    const float POSITION_THRESHOLD = 2.0f;
    MonsterState monsterState;
    public int hearingDistance;
    private Vector3 destination;
    private NavMeshAgent agent;
    //need to guarantee that its centered
    private GameObject playerToChase;
    private MusicManager musicManager;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();   
    }

    void OnEnable() {
       // PlayerController.makeSound += HearsSound;
        musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>(); 
    }

    void OnDisable() {
    //       PlayerController.makeSound -= HearsSound;
    }

    void Start()
    {
        hearingDistance = 20;
        monsterState = MonsterState.Patrolling;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerToChase != null && checkIfCanSeePlayer()) {
            monsterState = MonsterState.ChasingPlayer;
        }

        switch(monsterState) {
            case MonsterState.ChasingPlayer:
            ChasePlayer();
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
    }

    public bool hasReachedDestination() {
        Debug.Log(Vector3.Distance(destination, transform.position));
          if(Vector3.Distance(destination, transform.position) < POSITION_THRESHOLD) {
                Debug.Log("Arrived to destination");
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
                      Debug.Log("Saw Player");
                    return true;
                 }   
            }
            
        }
        return false;
    }

    public void ChasePlayer() {
        
          //TODO: change music for chasing
        monsterState = MonsterState.ChasingPlayer;
        agent.SetDestination(playerToChase.transform.position);
        destination = playerToChase.transform.position;
        if(Vector3.Distance(destination, transform.position) < 2.0f) {
                //close to player to attack
                //TODO: start attacking animation
        }
    }

    public void Patrol() {
        
        if(!agent.hasPath) {
            List<RoomDef> roomsInLevel = GameObject.Find("SceneController").GetComponent<ProcGenner>().GetRoomDefs();
            int roomToCheck = Random.Range(0,roomsInLevel.Count);
            Vector3 roomMidPoint =roomsInLevel[roomToCheck].midpoint; 
            destination = new Vector3(roomMidPoint.x,roomsInLevel[roomToCheck].position.y,roomMidPoint.z);
            agent.SetDestination(destination);
            Debug.Log("Patrolling to: (" + destination.x + "," + destination.y + "," + destination.z + ")");
        }
    }

    public void AlertSound(Vector3 origin, float maxDistancesqr) {
        if(monsterState != MonsterState.CheckingSound) {
            if( (transform.position-origin).sqrMagnitude < (maxDistancesqr)) {
                destination = origin;
                agent.SetDestination(origin);
                monsterState = MonsterState.CheckingSound;
                 //TODO: play tension music
            }
        }
    }

    public void SetPlayerTarget(GameObject target) {
        playerToChase = target;
    }
}

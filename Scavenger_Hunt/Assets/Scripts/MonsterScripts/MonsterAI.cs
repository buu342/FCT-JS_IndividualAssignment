using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RoomDef = ProcGenner.RoomDef;
using RoomDefMulti= ProcGennerMultiplayer.RoomDef;
using Unity.AI.Navigation;
public class MonsterAI : MonoBehaviour
{
    const float POSITION_THRESHOLD = 5.0f;
    bool hearsSound;
    bool startedPatrolling;
    bool startedChasingPlayer;
    public int hearingDistance;
    private Vector3 destination;
    private NavMeshAgent agent;
    //need to guarantee that its centered
    private GameObject playerToChase;
    private MusicManager musicManager;
    private bool multiplayer=JoinMultiplayer.Multiplayer;
    void Awake() {
        agent = GetComponent<NavMeshAgent>();   
    }

    void OnEnable() {
        PlayerController.makeSound += HearsSound;
        musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>(); 
    }

    void OnDisable() {
            PlayerController.makeSound -= HearsSound;
    }
    void Start()
    {
        hearsSound = false;
        hearingDistance = 20;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerToChase != null && checkIfCanSeePlayer()) {
            //chase constantly the player
            startedPatrolling = false;
            hearsSound = false;
            ChasePlayer();
        } else {
            startedChasingPlayer = false;
            
            //patrolling or chasing sound
            if(hearsSound) {
                //check if reached destination
                hearsSound = !hasReachedDestination();
                startedPatrolling = !hearsSound;
            } else {
                //patrolling
                Patrol();
            }
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
        if(!startedChasingPlayer) {
          //TODO: change music for chasing
            musicManager.PlaySong("Music/Tense", true, true, true);
            startedChasingPlayer =true;            
        }
        agent.SetDestination(playerToChase.transform.position); 
        destination = playerToChase.transform.position;
        if(Vector3.Distance(destination, transform.position) < 2.0f) {
                //close to player to attack
                //TODO: start attacking animation

        }
    }

    public void Patrol() {
        
        if(!agent.hasPath&!multiplayer) {
            if(!startedPatrolling) {
                startedPatrolling = true;
                //TODO: start standard music
                musicManager.PlaySong("Music/Calm", true, true, true);
            }
            List<RoomDef> roomsInLevel = GameObject.Find("SceneController").GetComponent<ProcGenner>().GetRoomDefs();
            int roomToCheck = Random.Range(0,roomsInLevel.Count);
            Vector3 roomMidPoint =roomsInLevel[roomToCheck].midpoint; 
            Vector3 destination = new Vector3(roomMidPoint.x,roomsInLevel[roomToCheck].position.y,roomMidPoint.z);
            agent.SetDestination(destination);
            Debug.Log("Patrolling to: (" + destination.x + "," + destination.y + "," + destination.z + ")");
        }
        else if(!agent.hasPath&multiplayer){
            List<RoomDefMulti> roomsInLevel = GameObject.Find("SceneController").GetComponent<ProcGennerMultiplayer>().GetRoomDefs();
            int roomToCheck = Random.Range(0,roomsInLevel.Count);
            Vector3 roomMidPoint =roomsInLevel[roomToCheck].midpoint; 
            Vector3 destination = new Vector3(roomMidPoint.x,roomsInLevel[roomToCheck].position.y,roomMidPoint.z);
            agent.SetDestination(destination);
            Debug.Log("Patrolling to: (" + destination.x + "," + destination.y + "," + destination.z + ")");
        }
    }

    void HearsSound(Vector3 origin, float distance) {
        if(!hearsSound && !startedChasingPlayer) {
            Debug.Log("Was able to heard sound!");
            if(Vector3.Distance(origin, transform.position) < (distance + hearingDistance)) {
                 hearsSound = true;
                 agent.SetDestination(origin);
                 destination = origin;
                 Debug.Log("Amber Heard sound");
                 
                musicManager.PlaySong("Music/LessCalm", true, true, true);
                 //TODO: play tension music
            }
        }

    }

    public void SetPlayerTarget(GameObject target) {
        playerToChase = target;
    }
}

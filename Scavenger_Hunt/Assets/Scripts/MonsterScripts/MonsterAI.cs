using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RoomDef = ProcGenner.RoomDef;
using Unity.AI.Navigation;
public class MonsterAI : MonoBehaviour
{
    bool hearsSound;
    bool chasingPlayer;
    bool goingToRoom;
    public int hearingDistance;
    private NavMeshAgent agent;
    private Vector3 destination;
    //TODO: need to guarantee that its centered
    private GameObject playerToChase;

    void Awake() {
        hearsSound = false;
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable() {
        PlayerController.makeSound += HearsSound;
    }

    void OnDisable() {
            PlayerController.makeSound -= HearsSound;
    }
    void Start()
    {
        hearsSound = false;
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastInfo rayInfo;
        //Physics.RayCast(transform.position, player.position, rayInfo, 0);
        //TODO: check capsule instead of sphere? since fov of eye is not a sphere
        if(playerToChase != null && checkIfCanSeePlayer()) {
            //TODO: change music for chasing
            agent.SetDestination(playerToChase.transform.position);
        } else {
            //patrolling or chasing sound
            if(hearsSound) {
                //TODO: change music for tension
                agent.SetDestination(destination);
                 Debug.Log("Heard sound");
            } else {
                //patrolling
                //TODO: standart music
                Patrol();
            }
        }

    }
    public bool checkIfCanSeePlayer() {
        Vector3 playerPos = playerToChase.transform.position - transform.position;
    
        float isInFOV = Vector3.Dot(transform.forward, playerPos);

        if(isInFOV > 0.5f) {
            //means player is in front of the monster in a less than 45º
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

    public void Patrol() {
        if(!goingToRoom || !agent.hasPath) {
        List<RoomDef> roomsInLevel = GameObject.Find("SceneController").GetComponent<ProcGenner>().GetRoomDefs();
        int roomToCheck = Random.Range(0,roomsInLevel.Count);
        Vector3 roomMidPoint =roomsInLevel[roomToCheck].midpoint; 
        destination = new Vector3(roomMidPoint.x,roomsInLevel[roomToCheck].position.y,roomMidPoint.z);
        agent.SetDestination(destination);
        Debug.Log("Patrolling to: (" + destination.x + "," + destination.y + "," + destination.z + ")");
        goingToRoom = true;
        } else {
            /*if(Vector3.Distance(destination, transform.position) < 5.0f) {
                goingToRoom = false;
                Debug.Log("Arrived to room");
            }*/
        }
    }

    //TODO: rewrite with audio manager
    void HearsSound(Vector3 origin, float distance) {
        if(Vector3.Distance(origin, transform.position) < (distance + hearingDistance)) {
           // hearsSound = true;
        }

    }
    public void SetPlayerTarget(GameObject target) {
        playerToChase = target;
    }

    /**
    Taken from https://answers.unity.com/questions/324589/how-can-i-tell-when-a-navmesh-has-reached-its-dest.html
    
     protected bool pathComplete()
     {
         if ( Vector3.Distance( m_NavAgent.destination, m_NavAgent.transform.position) <= m_NavAgent.stoppingDistance)
         {
             if (!m_NavAgent.hasPath || m_NavAgent.velocity.sqrMagnitude == 0f)
             {
                 return true;
             }
         }
 
         return false;
     }
     **/
}

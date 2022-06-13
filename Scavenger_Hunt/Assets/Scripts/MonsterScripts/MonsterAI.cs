using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    bool hearsPlayer;
    public int hearingDistance;
    NavMeshSurface navigationMesh;
    NavMeshAgent agent;
    private Vector3 destination;
    private GameObject player;

    
    void OnEnable() {
        PlayerController.makeSound += HearsSound;
    }

    void OnDisable() {
            PlayerController.makeSound -= HearsSound;
    }
    void Start()
    {
        hearsPlayer = false;
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastInfo rayInfo;
        //Physics.RayCast(transform.position, player.position, rayInfo, 0);
        //TODO: check capsule instead of sphere? since fov of eye is not a sphere
        if(Physics.CheckSphere())

    }


    //TODO: rewrite with audio manager
    void HearsSound(Vector3 origin, float distance) {
        if(Vector3.Distance(origin, transform.position) < (distance + hearingDistance)) {
            hearsPlayer = true;
            
        }

    }

}

/****************************************************************
                         DoorTrigger.cs
    
This script activates doors when walking over a trigger
****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        OpenClose,
        ForceOpen,
        ForceClose,
    }
    
    public List<DoorLogic> m_TargetDoors = new List<DoorLogic>();
    public float m_TriggerDelay = 0.0f;
    
    
    /*==============================
        Start
        Called when the door is initialized
    ==============================*/
    
    void Start()
    {
        
    }
    

    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/
    
    void FixedUpdate()
    {
        
    }
    
    
    /*==============================
        OnTriggerStay
        Handles trigger collision response when an object is currently touching the trigger
        @param The object we're colliding with
    ==============================*/
    
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
            foreach (DoorLogic door in this.m_TargetDoors)
                door.StartOpen(true);
    }


    /*==============================
        OnTriggerExit
        Handles trigger collision response when an object leaves
        @param The object we're no longer colliding with
    ==============================*/
    
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            foreach (DoorLogic door in this.m_TargetDoors)
                door.CloseDoor();
    }
}
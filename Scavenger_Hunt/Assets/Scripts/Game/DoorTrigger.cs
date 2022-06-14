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
    public TriggerType m_TiggerType = TriggerType.OpenClose;
    
    private float m_TriggerTimer = 0.0f;
    
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
        if (this.m_TriggerTimer != 0.0f && this.m_TriggerTimer < Time.time)
        {
            foreach (DoorLogic door in this.m_TargetDoors)
            {
                switch (this.m_TiggerType)
                {
                    case TriggerType.OpenClose: door.StartOpen(true); break;
                    case TriggerType.ForceOpen: door.StartOpen(false); break;
                    case TriggerType.ForceClose: door.ForceCloseDoor(); break;
                }
            }
        }
    }
    
    
    /*==============================
        OnTriggerStay
        Handles trigger collision response when an object is currently touching the trigger
        @param The object we're colliding with
    ==============================*/
    
    void OnTriggerStay(Collider other)
    {
        Debug.Log(other.tag);
        if (other.tag == "Player" || other.tag == "Monster")
        {
            if (this.m_TriggerDelay == 0)
            {
                foreach (DoorLogic door in this.m_TargetDoors)
                {
                    switch (this.m_TiggerType)
                    {
                        case TriggerType.OpenClose: door.StartOpen(true); break;
                        case TriggerType.ForceOpen: door.StartOpen(false); break;
                        case TriggerType.ForceClose: door.ForceCloseDoor(); break;
                    }
                }
            }
            else if (this.m_TriggerTimer == 0.0f)
                this.m_TriggerTimer = Time.time + this.m_TriggerDelay;
        }
    }


    /*==============================
        OnTriggerExit
        Handles trigger collision response when an object leaves
        @param The object we're no longer colliding with
    ==============================*/
    
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Monster")
            foreach (DoorLogic door in this.m_TargetDoors)
                door.CloseDoor();
    }
}
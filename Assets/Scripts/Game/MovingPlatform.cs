/****************************************************************
                        MovingPlatform.cs
    
This script handles moving platform logic
****************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    // Patrol state
    public enum PatrolState
    {
        Idle,
        Moving,
    }
    
    // Public variables
    public float m_MovementSpeed = 5.0f;
    public List<GameObject> m_PatrolPoints;
    public float m_PatrolWaitTime = 0.0f;
    public bool m_LoopPatrol = true;
    public bool m_Activated = true;
    
    // Private variables
    private PatrolState m_State = PatrolState.Idle;
    private int m_NextPatrolTarget = -1;
    private float m_NextPatrolTime = 0.0f;
    private float m_Acceleration = 0.5f;
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    
    // Components
    private Rigidbody m_rb;
    
    
    /*==============================
        Start
        Called when the enemy is initialized
    ==============================*/
    
    void Start()
    {
        this.m_rb = this.GetComponent<Rigidbody>();
    }


    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        Vector3 distance;
        
        // Don't do anything if we've hit the patrol point limit
        if (!this.m_Activated)
            return;
        
        // Move to the target
        this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity, this.m_Acceleration);
        this.m_rb.velocity = this.m_CurrentVelocity;
        foreach(Transform child in transform)
            if (child.GetComponent<Rigidbody>() != null)
                child.GetComponent<Rigidbody>().velocity = this.m_CurrentVelocity;
        
        // Decide what to do based on the platform state
        switch (this.m_State)
        {
            case PatrolState.Idle:
            
                // If we're done waiting at this patrol point, then move to the next point
                if (this.m_NextPatrolTime < Time.time)
                {
                    this.m_State = PatrolState.Moving;
                    
                    // If we hit the patrol point limit, and we're not meant to loop, stop moving
                    this.m_NextPatrolTarget++;
                    if (!this.m_LoopPatrol && this.m_NextPatrolTarget == this.m_PatrolPoints.Count)
                    {
                        this.m_rb.velocity = Vector3.zero;
                        this.m_Activated = false;
                        foreach(Transform child in transform)
                            if (child.GetComponent<Rigidbody>() != null)
                                child.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        break;
                    }
                    
                    // Go to the next patrol point
                    this.m_NextPatrolTarget %= this.m_PatrolPoints.Count;
                    distance = this.m_PatrolPoints[Mathf.Max(0, this.m_NextPatrolTarget)].transform.position - this.transform.position;
                    distance.Normalize();
                    this.m_TargetVelocity = distance*this.m_MovementSpeed;
                }
                break;
            case PatrolState.Moving:
            
                // If we're touching the patrol point, then stop for a bit
                distance = this.m_PatrolPoints[Mathf.Max(0, this.m_NextPatrolTarget)].transform.position - this.transform.position;
                if (distance.sqrMagnitude < 1.0f)
                {
                    this.m_NextPatrolTime = Time.time + this.m_PatrolWaitTime;
                    this.m_State = PatrolState.Idle;
                    this.m_TargetVelocity = new Vector3(0, 0, 0);
                }
                break;
        }
    }


    /*==============================
        SetActivated
        Enables/Disables the moving platform
        @param Whether or not to activate the platform
    ==============================*/
    
    public void SetActivated(bool active)
    {
        this.m_Activated = active;
    }
}
/****************************************************************
                        MovingPlatform.cs
    
This script handles moving platform logic
****************************************************************/

using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    // Public variables
    public float m_MovementSpeed = 5;
    public List<GameObject> m_PatrolPoints;
    public float m_PatrolWaitTime = 0;
    public bool m_LoopPatrol
    
    // Private variables
    private int m_NextPatrolTarget = -1;
    private float m_NextPatrolTime = 0;
    private float m_Acceleration = 0.5f;
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    
    
    /*==============================
        Start
        Called when the enemy is initialized
    ==============================*/
    
    void Start()
    {
        
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        
    }
}

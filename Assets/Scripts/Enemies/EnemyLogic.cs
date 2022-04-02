/****************************************************************
                          EnemyLogic.cs
    
This script handles base enemy logic.
****************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyLogic : MonoBehaviour
{
    // Constants
    private const float Gravity         = -80.0f;
    private const float TraumaSpeed     = 25.0f;
    private const float MaxTraumaOffset = 2.0f;
    private float NoiseSeed;
    private int NoCollideLayer;
    private int BulletLayer;
    
    // Enemy state
    public enum EnemyState
    {
        Idle,
        Running,
        Dead,
    }
    
    // Enemy attack styles
    public enum CombatState
    {
        Idle,
        TakeAim,
        Aiming,
        RemoveAim,
    }
    
    // Enemy attack styles
    public enum AttackStyle
    {
        Aiming,
        Straight,
    }
    
    // Health
    public int m_Health = 10;
    private float m_Trauma = 0.0f;
    
    // Aim
    public AttackStyle m_AttackStyle = AttackStyle.Aiming;
    private Vector3 m_DamagePos;
    private Vector3 m_OriginalMeshPos;
    private Vector3 m_OriginalAimPos;
    private Quaternion m_OriginalAimAng;
    private float m_NextFire = 0;
    private bool m_TargetNear = false;
    public Vector3 m_AimDir = Vector3.zero;
    
    // Combat
    public float m_ReactionTime = 0.7f;
    public float m_DepthPerception = 18.7f;
    #if UNITY_EDITOR
        [SerializeField]
        private bool DebugDepth = false;
    #endif
    public float m_FireRate = 0.5f;
    
    // Patrolling
    public bool m_IsFlying = false;
    public float m_MovementSpeed = 5;
    public List<GameObject> m_PatrolPoints;
    public float m_PatrolWaitTime = 0;
    public bool m_ShootWhileMoving = false;
    private int m_NextPatrolTarget = -1;
    private float m_NextPatrolTime = 0;
    private float m_Acceleration = 0.5f;
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    
    // States
    private EnemyState m_EnemyState = EnemyState.Idle;
    private CombatState m_CombatState = CombatState.Idle;
    private float m_TimeToIdle = 0;
    
    // Components
    public  GameObject m_bulletprefab;
    private GameObject m_mesh;
    private GameObject m_target;
    private GameObject m_fireattachment;
    private GameObject m_shoulder;
    private Rigidbody m_rb;
    private AudioManager m_audio; 
    private EnemyAnimations m_anims;
    
    
    /*==============================
        Start
        Called when the enemy is initialized
    ==============================*/
    
    void Start()
    {
        NoiseSeed = Random.value;
        NoCollideLayer = LayerMask.NameToLayer("NoCollide");
        BulletLayer = LayerMask.NameToLayer("Bullet");
        this.m_target = GameObject.Find("Player");
        this.m_audio = FindObjectOfType<AudioManager>();
        this.m_mesh = this.transform.Find("Model").gameObject;
        this.m_anims = this.m_mesh.GetComponent<EnemyAnimations>();
        this.m_rb = this.GetComponent<Rigidbody>();
        this.m_shoulder = this.transform.Find("Shoulder").gameObject;
        this.m_fireattachment = this.transform.Find("FireAttachment").gameObject;
        this.m_OriginalAimPos = this.m_fireattachment.transform.localPosition;
        this.m_OriginalAimAng = this.m_fireattachment.transform.localRotation;
        this.m_OriginalMeshPos = this.m_mesh.transform.localPosition;
        this.m_DamagePos = this.transform.position;
        if (this.m_PatrolPoints.Count > 0)
            this.m_NextPatrolTarget = 0;
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        // If we're already dead, don't execute any code below
        if (this.m_EnemyState == EnemyState.Dead)
            return;
        
        // If we ran out of health, then commit sudoku
        if (this.m_Health <= 0)
        {
            Die();
            return;
        }
        
        // Handle targeting
        HandleTargeting();
        
        // Handle patrolling
        HandlePatrolling();
        
        // Calculate shake when hurt
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaoffsetx = EnemyLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(NoiseSeed, Time.time*EnemyLogic.TraumaSpeed)*2 - 1);
        float traumaoffsety = EnemyLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(NoiseSeed + 1, Time.time*EnemyLogic.TraumaSpeed)*2 - 1);
        
        // Calculate the shake position
        this.m_mesh.transform.localPosition = this.m_OriginalMeshPos;
        this.m_mesh.transform.localPosition += new Vector3(traumaoffsetx, traumaoffsety, 0);
        if (this.m_IsFlying)
            this.m_mesh.transform.localPosition += new Vector3(0, Mathf.Sin(Time.time*5)/5, 0); 
        
        // Decrease shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime);
    }

    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        // If we're already dead, don't execute any code below
        if (this.m_EnemyState == EnemyState.Dead)
            return;
        
        // Cache whether the target is nearby
        Vector3 distance = this.m_target.transform.Find("Shoulder").gameObject.transform.position - this.m_fireattachment.transform.position;
        this.m_TargetNear = (distance.sqrMagnitude < this.m_DepthPerception*this.m_DepthPerception);
        
        // Move to the target
        this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity, this.m_Acceleration);
        this.m_rb.velocity = new Vector3(this.m_CurrentVelocity.x, this.m_CurrentVelocity.y, this.m_CurrentVelocity.z);
        
        // Add gravity
        if (!this.m_IsFlying)
            this.m_rb.AddForce(0, EnemyLogic.Gravity, 0);
        
        // Go to idle combat state
        if (this.m_TimeToIdle != 0 && this.m_TimeToIdle < Time.time)
        {
            if (this.m_CombatState == CombatState.RemoveAim)
                this.m_CombatState = CombatState.Idle;
            else if (this.m_CombatState == CombatState.TakeAim)
                this.m_CombatState = CombatState.Aiming;
            this.m_TimeToIdle = 0;
        }
    }
    
    
    /*==============================
        TakeDamage
        Makes the enemy take damage
        @param The amount of damage to take
        @param The coordinate where the damage came from
    ==============================*/
    
    public void TakeDamage(int amount, Vector3 position)
    {
        this.m_Health -= amount;
        this.m_Trauma = Mathf.Min(0.5f, this.m_Trauma + ((float)amount)/30.0f);
        this.m_DamagePos = position;
    }


    /*==============================
        HandleTargeting
        Handles the enemy targeting
    ==============================*/
    
    private void HandleTargeting()
    {
        Vector3 targetpos = Vector3.zero;
        
        // Calculate the direction to face the target
        if (this.m_TargetNear && this.m_AttackStyle == AttackStyle.Aiming)
        {
            targetpos = this.m_target.transform.Find("Shoulder").gameObject.transform.position;
            this.m_AimDir = this.m_fireattachment.transform.position - targetpos;
        }
        this.m_AimDir.Normalize();
        
        // Rotate the firing attachment to point at the player
        if (this.m_TargetNear)
        {
            this.m_fireattachment.transform.localPosition = this.m_OriginalAimPos;
            this.m_fireattachment.transform.localRotation = this.m_OriginalAimAng;
            this.m_fireattachment.transform.RotateAround(this.m_shoulder.transform.position, Vector3.forward, Mathf.Atan2(this.m_AimDir.y, this.m_AimDir.x)*Mathf.Rad2Deg);
        }
        
        // Attack based on the style
        switch (this.m_AttackStyle)
        {
            case AttackStyle.Aiming:
                // If the player is within shooting distance
                if (this.m_TargetNear)
                {
                    // If we're idling, start aiming at the player
                    if (this.m_CombatState == CombatState.Idle || this.m_CombatState == CombatState.RemoveAim)
                    {
                        this.m_CombatState = CombatState.TakeAim;
                        this.m_TimeToIdle = Time.time + this.m_ReactionTime;
                    }
                    
                    // Fire the bullet
                    if (this.m_CombatState == CombatState.Aiming)
                        FireBullet();
                }
                else if (this.m_CombatState != CombatState.Idle && this.m_CombatState != CombatState.RemoveAim)
                {
                    // Face straight
                    if (this.m_AimDir.x >= 0.0f)
                        this.m_AimDir = new Vector3(1.0f, 0.0f, 0.0f);
                    else
                        this.m_AimDir = new Vector3(-1.0f, 0.0f, 0.0f);
                    
                    // Stop aiming
                    this.m_CombatState = CombatState.RemoveAim;
                    this.m_TimeToIdle = Time.time + this.m_ReactionTime;
                }
                break;
            case AttackStyle.Straight:
                // Fire bullets in a straight line if the player is within shooting distance
                this.m_CombatState = CombatState.Aiming;
                if (this.m_TargetNear)
                    FireBullet();
                break;
        }
    }


    /*==============================
        FireBullet
        Makes the enemy fire a bullet
    ==============================*/
    
    private void FireBullet()
    {
        if (this.m_NextFire < Time.time)
        {
            // Create the bullet object
            ProjectileLogic bullet = Instantiate(this.m_bulletprefab, this.m_fireattachment.transform.position, this.m_fireattachment.transform.rotation).GetComponent<ProjectileLogic>();
            bullet.SetOwner(this.gameObject);
            bullet.SetSpeed(15.0f);
            
            // Play the shooting sound and set the next fire time
            this.m_audio.Play("Weapons/Laser_Fire", this.m_shoulder.transform.position);
            this.m_NextFire = Time.time + this.m_FireRate;
            this.m_anims.PlayFireAnimation();
        }
    }


    /*==============================
        HandlePatrolling
        Handles the enemy patrolling
    ==============================*/

    private void HandlePatrolling()
    {
        // If we have no patrol target stop
        if (this.m_NextPatrolTarget == -1)
            return;
        
        // Decide what to do based on the enemy state
        switch (this.m_EnemyState)
        {
            case EnemyState.Idle:
                // If we can't shoot while moving, and our target is near, then don't move
                if (!this.m_ShootWhileMoving && this.m_TargetNear)
                {
                    this.m_NextPatrolTime = Time.time + this.m_PatrolWaitTime;
                }
                else if (this.m_NextPatrolTime < Time.time) // Otherwise, if we're done waiting at this patrol point, then move to the next point
                {
                    this.m_EnemyState = EnemyState.Running;
                    this.m_NextPatrolTarget = (this.m_NextPatrolTarget + 1) % this.m_PatrolPoints.Count;
                }
                break;
            case EnemyState.Running:
                Vector3 distance = this.m_PatrolPoints[this.m_NextPatrolTarget].transform.position - this.transform.position;
                
                // If we're touching the patrol point (or we can't shoot and move), then stop for a bit
                if (distance.sqrMagnitude < 1.0f || (!this.m_ShootWhileMoving && this.m_TargetNear))
                {
                    this.m_NextPatrolTime = Time.time + this.m_PatrolWaitTime;
                    this.m_EnemyState = EnemyState.Idle;
                    this.m_TargetVelocity = new Vector3(0, 0, 0);
                }
                else
                {
                    // Set the speed based on whether the patrol point is to the left or right of us
                    this.m_TargetVelocity = this.m_MovementSpeed*distance.normalized;
                }
                break;
        }
    }


    /*==============================
        GetEnemyAttackStyle
        Returns the enemy's attack style
        @returns The enemy's attack style
    ==============================*/
    
    public AttackStyle GetEnemyAttackStyle()
    {
        return this.m_AttackStyle;
    }


    /*==============================
        GetEnemyState
        Returns the enemy's current state
        @returns The enemy's current state
    ==============================*/
    
    public EnemyState GetEnemyState()
    {
        return this.m_EnemyState;
    }


    /*==============================
        GetEnemyCombatState
        Returns the enemy's current combat state
        @returns The enemy's current combat state
    ==============================*/
    
    public CombatState GetEnemyCombatState()
    {
        return this.m_CombatState;
    }


    /*==============================
        GetAimDirection
        Returns a direction vector pointing where the enemy is aiming at
        @returns The enemy's aim vector
    ==============================*/
    
    public Vector3 GetAimDirection()
    {
        return this.m_AimDir;
    }
    

    /*==============================
        GetTarget
        Returns the enemy's target
        @returns the enemy's target
    ==============================*/
    
    public GameObject GetTarget()
    {
        return this.m_target;
    }
    

    /*==============================
        GetPatrolPoint
        Returns a pointer to the target patrol point
        @returns The enemy's target patrol point
    ==============================*/
    
    public GameObject GetPatrolPoint()
    {
        if (this.m_NextPatrolTarget == -1)
            return null;
        return this.m_PatrolPoints[this.m_NextPatrolTarget];
    }
    
    
    /*==============================
        GetTargetNear
        Returns whether our target is within shooting distance
        @returns Whether the target is nearby
    ==============================*/
    
    public bool GetTargetNear()
    {
        return this.m_TargetNear;
    }
    
    
    /*==============================
        IsFlying
        Returns whether we're flying
        @returns Whether we're flying
    ==============================*/
    
    public bool IsFlying()
    {
        return this.m_IsFlying;
    }
    

    /*==============================
        Die
        Turns the enemy into a ragdoll
    ==============================*/
    
    private void Die()
    {
        Rigidbody[] ragdollbodies = this.GetComponentsInChildren<Rigidbody>();
        Collider[] ragdollcolliders = this.GetComponentsInChildren<Collider>();
        
        // Set us as the dead state
        this.m_EnemyState = EnemyState.Dead;
        
        // Enable all the rigidbodies and box colliders
        foreach (Rigidbody rb in ragdollbodies)
            rb.isKinematic = false;
        foreach (Collider rc in ragdollcolliders)
            rc.enabled = true;
            
        // Disable collisions
        this.GetComponent<BoxCollider>().enabled = false;
        this.gameObject.layer = NoCollideLayer;
        foreach (Transform child in this.GetComponentsInChildren<Transform>(true))  
            child.gameObject.layer = NoCollideLayer;
        
        // Apply physics to the bones based on where the damage came from
        Collider[] colliders = Physics.OverlapSphere(this.m_DamagePos, 3);
        foreach (Collider hit in colliders)
            if (hit.GetComponent<Rigidbody>() && hit.gameObject.layer != BulletLayer)
                hit.GetComponent<Rigidbody>().AddExplosionForce(75, this.m_DamagePos, 3, 0, ForceMode.Impulse);
            
        // Stop animating
        this.m_mesh.GetComponent<Animator>().enabled = false;
            
        // Make this object fade out after some time
        FadeoutDestroy fade = this.gameObject.AddComponent<FadeoutDestroy>();
        fade.m_LifeTime = 10;
        fade.m_FadeTime = 1;
    }


    /*==============================
        OnDrawGizmos
        Draws extra debug stuff in the editor
    ==============================*/
    
    public virtual void OnDrawGizmos()
    {
        if (DebugDepth)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, this.m_DepthPerception);
        }
    }
}
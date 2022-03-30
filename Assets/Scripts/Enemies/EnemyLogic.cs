/****************************************************************
                          EnemyLogic.cs
    
This script handles base enemy logic.
****************************************************************/

using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    // Constants
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
        Flying,
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
    
    // Combat
    public float m_ReactionTime = 0.5f;
    public float m_DepthPerception = 18.7f;
    public float m_FireRate = 0.5f;
    
    // States
    public EnemyState m_EnemyState = EnemyState.Idle;
    private CombatState m_CombatState = CombatState.Idle;
    private float m_TimeToIdle = 0;
    
    // Components
    public  GameObject m_bulletprefab;
    private GameObject m_mesh;
    private GameObject m_target;
    private GameObject m_fireattachment;
    private GameObject m_shoulder;
    private AudioManager m_audio; 
    
    
    /*==============================
        Start
        Called when the enemy is initialized
    ==============================*/
    
    void Start()
    {
        NoiseSeed = Random.value;
        NoCollideLayer = LayerMask.NameToLayer("NoCollide");
        BulletLayer = LayerMask.NameToLayer("Bullets");
        this.m_target = GameObject.Find("Player");
        this.m_audio = FindObjectOfType<AudioManager>();
        this.m_mesh = this.transform.Find("Model").gameObject;
        this.m_shoulder = this.transform.Find("Shoulder").gameObject;
        this.m_fireattachment = this.transform.Find("FireAttachment").gameObject;
        this.m_OriginalAimPos = this.m_fireattachment.transform.localPosition;
        this.m_OriginalAimAng = this.m_fireattachment.transform.localRotation;
        this.m_OriginalMeshPos = this.m_mesh.transform.localPosition;
        this.m_DamagePos = this.transform.position;
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
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
        
        // Go to idle combat state
        if (this.m_TimeToIdle != 0 && this.m_TimeToIdle < Time.time)
        {
            if (this.m_CombatState == CombatState.RemoveAim)
                this.m_CombatState = CombatState.Idle;
            else if (this.m_CombatState == CombatState.TakeAim)
                this.m_CombatState = CombatState.Aiming;
            this.m_TimeToIdle = 0;
        }
        
        // Calculate shake when hurt
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaoffsetx = EnemyLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(NoiseSeed, Time.time*EnemyLogic.TraumaSpeed)*2 - 1);
        float traumaoffsety = EnemyLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(NoiseSeed + 1, Time.time*EnemyLogic.TraumaSpeed)*2 - 1);
        
        // Calculate the shake position
        this.m_mesh.transform.localPosition = this.m_OriginalMeshPos;
        this.m_mesh.transform.localPosition += new Vector3(traumaoffsetx, traumaoffsety, 0);
        
        // Decrease shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime);
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
        Vector3 targetpos = this.m_target.transform.Find("Shoulder").gameObject.transform.position;
        
        // Attack based on the style
        switch (this.m_AttackStyle)
        {
            case AttackStyle.Aiming:
                // If the player is within shooting distance
                if (Vector3.Distance(targetpos, this.m_fireattachment.transform.position) < this.m_DepthPerception)
                {
                    // If we're idling, start aiming at the player
                    if (this.m_CombatState == CombatState.Idle || this.m_CombatState == CombatState.RemoveAim)
                    {
                        this.m_CombatState = CombatState.TakeAim;
                        this.m_TimeToIdle = Time.time + this.m_ReactionTime;
                    }
                    
                    // Calculate the direction to face the player
                    Vector3 direction = this.m_fireattachment.transform.position - targetpos;
                    direction.Normalize();
                    
                    // Rotate the firing attachment to point at the player
                    this.m_fireattachment.transform.localPosition = this.m_OriginalAimPos;
                    this.m_fireattachment.transform.localRotation = this.m_OriginalAimAng;
                    this.m_fireattachment.transform.RotateAround(this.m_shoulder.transform.position, Vector3.forward, Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg);
                    
                    // Fire the bullet
                    if (this.m_CombatState == CombatState.Aiming)
                        FireBullet();
                }
                else if (this.m_CombatState != CombatState.Idle && this.m_CombatState != CombatState.RemoveAim)
                {
                    this.m_CombatState = CombatState.RemoveAim;
                    this.m_TimeToIdle = Time.time + this.m_ReactionTime;
                }
                break;
            case AttackStyle.Straight:
                // If the player is within shooting distance
                if (Vector3.Distance(targetpos, this.m_fireattachment.transform.position) < this.m_DepthPerception)
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
            this.m_audio.Play("Weapons/Laser_Fire");
            this.m_NextFire = Time.time + this.m_FireRate;
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
}

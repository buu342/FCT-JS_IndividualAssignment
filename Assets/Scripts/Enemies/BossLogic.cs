using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLogic : MonoBehaviour
{
    // Constants
    private const int   MaxHealth       = 1000;
    private const float MovementSpeed   = 5.0f;
    private const float Acceleration    = 0.5f;
    private const float JumpPower       = 1300.0f;
    private const float Gravity         = -80.0f;
    private const float TraumaSpeed     = 50.0f;
    private const float MaxTraumaOffset = 0.5f;
    private const float MusicTempo      = 1.55f;
    private int NoCollideLayer;
    private float NoiseSeed;
    
    // Boss state
    public enum BossState
    {
        Idle,
        MovingForwards,
        MovingBackwards,
        Dying,
        Dead,
    }
    
    // Boss attack state
    public enum CombatState
    {
        Idle,
        Jump,
        Attack1,
        Attack2,
        Attack3,
        Rocket,
    }
    
    // Boss jump state
    public enum BossJumpState
    {
        Idle,
        Spring,
        Jump,
        Fall,
        Land,
    }
    
    // Health
    private int m_Health = BossLogic.MaxHealth;
    private float m_Trauma = 0.0f;
    
    // Movement
    private bool  m_Enabled = true;
    private int   m_MovementStep = 0;
    private float m_TimeToIdle = 0;
    private float m_NextMoveAction = 0;
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    private BossState m_BossState = BossState.Idle;
    private BossJumpState m_BossJumpState = BossJumpState.Idle;
    private bool m_OnGround;
    
    // Combat
    private float m_NextAttackTime = 0;
    private float m_NextFire = -1;
    private int   m_AttackState = 0;
    private CombatState m_NextCombatState = CombatState.Idle;
    private CombatState m_CombatState = CombatState.Idle;
    private Vector3 m_OriginalMeshPos;
    private Vector3 m_OriginalAimPos;
    private Quaternion m_OriginalAimAng;
    private Vector3 m_AimDir = Vector3.zero;
    
    // Components
    public  GameObject m_bulletprefab;
    public  GameObject m_rocketprefab;
    public  GameObject m_rocketattachment;
    public  GameObject m_explosionbig;
    public  GameObject m_explosionsmall;
    public  GameObject m_dustjump;
    public  GameObject m_dustland;
    private GameObject m_mesh;
    private GameObject m_target;
    private GameObject m_fireattachment;
    private GameObject m_shoulder;
    private BoxCollider m_col;
    private Rigidbody m_rb;
    private AudioManager m_audio; 
    private BossAnimations m_anims;
    
    
    /*==============================
        Start
        Called when the enemy is initialized
    ==============================*/
    
    void Start()
    {
        NoiseSeed = Random.value;
        NoCollideLayer = LayerMask.NameToLayer("NoCollide");
        this.m_target = GameObject.Find("Player");
        this.m_audio = FindObjectOfType<AudioManager>();
        this.m_mesh = this.transform.Find("Model").gameObject;
        this.m_anims = this.m_mesh.GetComponent<BossAnimations>();
        this.m_rb = this.GetComponent<Rigidbody>();
        this.m_col = this.GetComponent<BoxCollider>();
        this.m_shoulder = this.transform.Find("Shoulder").gameObject;
        this.m_fireattachment = this.transform.Find("FireAttachment").gameObject;
        this.m_OriginalAimPos = this.m_fireattachment.transform.localPosition;
        this.m_OriginalAimAng = this.m_fireattachment.transform.localRotation;
        this.m_OriginalMeshPos = this.m_mesh.transform.localPosition;
        this.m_NextAttackTime = Time.time + 1.0f;            
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (this.m_BossState == BossState.Dead )
            return;
        
        // Calculate shake when hurt
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaoffsetx = BossLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(NoiseSeed, Time.time*BossLogic.TraumaSpeed)*2 - 1);
        float traumaoffsety = BossLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(NoiseSeed + 1, Time.time*BossLogic.TraumaSpeed)*2 - 1);
        
        // Calculate the shake position
        this.m_mesh.transform.localPosition = this.m_OriginalMeshPos;
        this.m_mesh.transform.localPosition += new Vector3(traumaoffsetx, traumaoffsety, 0);
        
        // Decrease shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime);
        
        // Don't continue if we're dead
        if (this.m_BossState == BossState.Dying || this.m_BossState == BossState.Dead)
            return;
        
        // Calculate the direction to face the target
        Vector3 targetpos = targetpos = this.m_target.transform.Find("Shoulder").gameObject.transform.position;
        this.m_AimDir = this.m_fireattachment.transform.position - targetpos;
        this.m_AimDir.Normalize();
        
        // Rotate the firing attachment to point at the player
        this.m_fireattachment.transform.localPosition = this.m_OriginalAimPos;
        this.m_fireattachment.transform.localRotation = this.m_OriginalAimAng;
        this.m_fireattachment.transform.RotateAround(this.m_shoulder.transform.position, Vector3.forward, Mathf.Atan2(this.m_AimDir.y, this.m_AimDir.x)*Mathf.Rad2Deg);
    }

    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/
    
    void FixedUpdate()
    {
        if (this.m_BossState == BossState.Dead)
            return;
        
        // Apply gravity if we're not grounded
        this.m_OnGround = IsGrounded();
        if (!this.m_OnGround)
            this.m_rb.AddForce(0, BossLogic.Gravity, 0, ForceMode.Acceleration);
        
        // If we're not enabled, don't do anything else
        if (!this.m_Enabled)
            return;
        
        // Handle death
        if (HandleDeath())
            return;
        
        // Move to the target
        if (this.m_BossJumpState == BossJumpState.Idle || this.m_BossJumpState == BossJumpState.Land)
        {
            this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity, BossLogic.Acceleration);
            this.m_rb.velocity = new Vector3(this.m_CurrentVelocity.x, this.m_rb.velocity.y, this.m_CurrentVelocity.z);
        }
        
        // Handle the boss movement
        HandleBossMovement();
        
        // Handle the boss attacks
        HandleBossAttacks();
    }
    
    
    /*==============================
        HandleBossMovement
        Handle the boss movement logic
    ==============================*/
    
    private void HandleBossMovement()
    {
        if (this.m_TimeToIdle != 0 && this.m_TimeToIdle < Time.time && this.m_MovementStep == 0)
            this.m_BossState = BossState.Idle;
        
        // Only allow the boss to move at the beat of the music
        if (this.m_NextMoveAction <= Time.unscaledTime)
        {
            this.m_NextMoveAction = Time.unscaledTime + BossLogic.MusicTempo;
            if (this.m_BossState == BossState.Idle && this.m_CombatState != CombatState.Jump && this.m_CombatState != CombatState.Rocket)
            {
                if (this.m_target.transform.position.x < this.transform.position.x)
                    this.m_BossState = BossState.MovingForwards;
                else
                    this.m_BossState = BossState.MovingBackwards;
            }
        }
        
        // Slowly inch the boss towards the player with a staggered movement pattern
        if (this.m_BossState == BossState.MovingForwards || this.m_BossState == BossState.MovingBackwards)
        {
            float speedmult = -1;
            if (this.m_BossState == BossState.MovingBackwards)
                speedmult = 1;
            
            switch (this.m_MovementStep)
            {
                case 0:
                    this.m_MovementStep++;
                    this.m_TimeToIdle = Time.time + 8.0f/60.0f;
                    this.m_TargetVelocity = new Vector3(speedmult*BossLogic.MovementSpeed, 0, 0);
                    this.m_audio.Play("Voice/Boss/Move", this.transform.position);
                    break;
                case 1:
                    if (this.m_TimeToIdle < Time.time)
                    {
                        this.m_MovementStep++;
                        this.m_TimeToIdle = Time.time + 7.0f/60.0f;
                        this.m_TargetVelocity = Vector3.zero;
                        Camera.main.GetComponent<CameraLogic>().AddTrauma(0.3f);
                        this.m_audio.Play("Voice/Boss/Step", this.transform.position);
                    }
                    break;
                case 2:
                    if (this.m_TimeToIdle < Time.time)
                    {
                        this.m_MovementStep++;
                        this.m_TimeToIdle = Time.time + 8.0f/60.0f;
                        this.m_TargetVelocity = new Vector3(speedmult*BossLogic.MovementSpeed, 0, 0);
                        this.m_audio.Play("Voice/Boss/Move", this.transform.position);
                    }
                    break;
                case 3:
                    if (this.m_TimeToIdle < Time.time)
                    {
                        this.m_MovementStep++;
                        this.m_TimeToIdle = Time.time + 7.0f/60.0f;
                        this.m_TargetVelocity = Vector3.zero;
                        Camera.main.GetComponent<CameraLogic>().AddTrauma(0.3f);
                        this.m_audio.Play("Voice/Boss/Step", this.transform.position);
                    }
                    break;
                case 4:
                    this.m_MovementStep = 0;
                    this.m_BossState = BossState.Idle;
                    break;
            }
        }
    }
    
    
    /*==============================
        HandleBossAttacks
        Handle the boss attacks
    ==============================*/
    
    private void HandleBossAttacks()
    {            
        // Pick what next attack we're performing (without repeating the previous attack)
        if (this.m_NextAttackTime != 0 && this.m_NextAttackTime < Time.time)
        {
            CombatState prev = this.m_CombatState;
            
            // This is super messy, but I'm kinda running out of time, otherwise I'd use a weighted shufflebag for this code.
            // From a quick glance, C#/Unity doesn't provide anything of the sort, and I don't have time to implement it myself, so this will have to make do.
            while (this.m_NextCombatState == prev)
            {
                float rand = Random.Range(0.0f, 1.0f);
                this.m_AttackState = 0;
                this.m_NextAttackTime = 0;
                this.m_NextFire = -1;
                if (rand > 0.9f)
                {
                    this.m_NextCombatState = CombatState.Rocket;
                    if (this.m_BossState != BossState.Idle) // Finish moving first
                        this.m_NextFire = Time.time + 0.5f;
                }
                else if (rand > 0.75f)
                {
                    this.m_NextCombatState = CombatState.Jump;
                    if (this.m_BossState != BossState.Idle) // Finish moving first
                        this.m_NextFire = Time.time + 0.5f;
                }
                else if (rand > 0.5f)
                    this.m_NextCombatState = CombatState.Attack3;
                else if (rand > 0.25f)
                    this.m_NextCombatState = CombatState.Attack2;
                else
                    this.m_NextCombatState = CombatState.Attack1;
            }
        }
        
        // If we're allowed to, go to our next combat state
        if (this.m_CombatState != this.m_NextCombatState && this.m_NextFire < Time.time)
            this.m_CombatState = this.m_NextCombatState;
        
        // Handle jumping
        if (this.m_CombatState == CombatState.Jump)
        {
            if (this.m_BossJumpState == BossJumpState.Idle)
            {
                this.m_rb.velocity = Vector3.zero;
                this.m_BossJumpState = BossJumpState.Spring;
                this.m_audio.Play("Voice/Boss/Jump", this.transform.position);
                this.m_NextFire = Time.time + 0.56f;
            }
            else if (this.m_NextFire < Time.time && this.m_BossJumpState == BossJumpState.Spring)
            {
                float timetoland = ((BossLogic.JumpPower*Time.fixedDeltaTime)/(-BossLogic.Gravity*Time.fixedDeltaTime))*2*Time.fixedDeltaTime;
                float distancetoplayer = this.transform.position.x - this.m_target.transform.position.x;
                this.m_BossJumpState = BossJumpState.Jump;
                Instantiate(this.m_dustjump, this.transform.position, Quaternion.identity);
                this.m_OnGround = false;
                this.m_rb.velocity = Vector3.zero;
                this.m_rb.AddForce(this.transform.up*BossLogic.JumpPower, ForceMode.Acceleration);
                this.m_rb.AddForce(this.transform.forward*(distancetoplayer/(timetoland*Time.fixedDeltaTime)), ForceMode.Acceleration);
            }
            else if (this.m_BossJumpState == BossJumpState.Jump && this.m_rb.velocity.y < 1)
            {
                this.m_BossJumpState = BossJumpState.Fall;
            }
            else if (this.m_BossJumpState == BossJumpState.Fall && this.m_OnGround)
            {
                Camera.main.GetComponent<CameraLogic>().AddTrauma(0.5f);
                Instantiate(this.m_dustland, this.transform.position, Quaternion.identity);
                this.m_audio.Play("Voice/Boss/Land", this.transform.position);
                this.m_BossJumpState = BossJumpState.Land;
                this.m_NextFire = Time.time + 1.0f;
            }
            else if (this.m_BossJumpState == BossJumpState.Land && this.m_NextFire < Time.time)
            {
                this.m_BossJumpState = BossJumpState.Idle;
                this.m_NextAttackTime = Time.time;
            }
        }
        
        // Handle the actual attacks
        if (this.m_CombatState == this.m_NextCombatState && this.m_NextFire != 0 && this.m_NextFire < Time.time)
        {
            switch (this.m_CombatState)
            {
                case CombatState.Attack1:
                
                    // Shoot at a given firerate
                    if (this.m_AttackState%2 == 0)
                        this.m_NextFire = Time.time + 0.2f;
                    else
                        this.m_NextFire = Time.time + 0.8f;
                    FireBullet();
                    this.m_audio.Play("Weapons/Laser_FireHeavy1", this.m_shoulder.transform.position);
                    this.m_AttackState++;
                    
                    // Stop shooting once this pattern is finished
                    if (this.m_AttackState == 6)
                    {
                        this.m_NextFire = 0;
                        this.m_NextAttackTime = Time.time + 0.8f;
                    }
                    break;
                case CombatState.Attack2:
                
                    // Shoot at a given firerate
                    this.m_NextFire = Time.time + 0.7f;
                    for (float i=-20.0f; i<=20.0f; i+= 10.0f)
                        FireBullet(i);
                    this.m_audio.Play("Weapons/Laser_FireHeavy2", this.m_shoulder.transform.position);
                    this.m_AttackState++;
                    
                    // Stop shooting once this pattern is finished
                    if (this.m_AttackState == 4)
                    {
                        this.m_NextFire = 0;
                        this.m_NextAttackTime = Time.time + 0.8f;
                    }
                    break;
                case CombatState.Attack3:
                
                    // Shoot at a given firerate
                    this.m_NextFire = Time.time + 0.1f;
                    FireBullet(Random.Range(-3.0f, 3.0f));
                    this.m_audio.Play("Weapons/Laser_FireHeavy3", this.m_shoulder.transform.position);
                    this.m_AttackState++;
                    
                    // Stop shooting once this pattern is finished
                    if (this.m_AttackState == 6)
                    {
                        this.m_NextFire = 0;
                        this.m_NextAttackTime = Time.time + 0.8f;
                    }
                    break;
                    
                case CombatState.Rocket:
                    if (this.m_AttackState == 0)
                    {
                        this.m_NextFire = Time.time + 0.45f;
                        this.m_NextAttackTime = Time.time + 1.2f;
                        this.m_audio.Play("Voice/Boss/StartRocket", this.transform.position);
                    }
                    else if (this.m_AttackState == 1)
                    {
                        FireRocket();
                        this.m_audio.Play("Voice/Boss/EndRocket", this.transform.position);
                        this.m_NextFire = 0;
                    }
                    this.m_AttackState++;
                    break;
            }
        }
    }
    
    
    /*==============================
        IsGrounded
        Checks whether the boss is grounded.
        Since the boss collider is a cube, this
        is accomplished using 4 raycasts, one for
        each corner of the cube.
        @returns true if the boss is touching  
                 the ground, false if otherwise
    ==============================*/
    
    public bool IsGrounded()
    {
        float xsize = this.m_col.bounds.size.x/2.0f-0.01f;
        float ysize = 0.01f;
        float zsize = this.m_col.bounds.size.z/2.0f-0.01f;
        float raylen = 0.2f;
        bool cast1 = Physics.Raycast(this.transform.position + (new Vector3( xsize, ysize, 0)), Vector3.down, raylen);
        bool cast2 = Physics.Raycast(this.transform.position + (new Vector3(-xsize, ysize, 0)), Vector3.down, raylen);
        bool cast3 = Physics.Raycast(this.transform.position + (new Vector3(0, ysize,  zsize)), Vector3.down, raylen); 
        bool cast4 = Physics.Raycast(this.transform.position + (new Vector3(0, ysize, -zsize)), Vector3.down, raylen);
        return cast1 || cast2 || cast3 || cast4;
    }


    /*==============================
        FireBullet
        Makes the boss fire a bullet
    ==============================*/
    
    private void FireBullet(float extrangle = 0.0f)
    {
        Quaternion shootangle = this.m_fireattachment.transform.rotation;
        if (extrangle != 0.0f)
            shootangle *= Quaternion.Euler(extrangle, 0, 0);
        
        // Create the bullet object
        ProjectileLogic bullet = Instantiate(this.m_bulletprefab, this.m_fireattachment.transform.position, shootangle).GetComponent<ProjectileLogic>();
        bullet.SetSpeed(8.0f);
        bullet.SetOwner(this.gameObject);
        
        // Play the shooting sound and set the next fire time
        this.m_anims.PlayFireAnimation();
    }
    

    /*==============================
        FireRocket
        Makes the boss fire a rocket
    ==============================*/
    
    private void FireRocket()
    {
        GameObject r = this.m_rocketattachment;
        
        // Create the rocket object
        RocketLogic rocket = Instantiate(this.m_rocketprefab, r.transform.position + r.transform.forward*0.1f, r.transform.rotation).GetComponent<RocketLogic>();
        rocket.SetOwner(this.gameObject);
        rocket.SetTarget(this.m_target.transform.Find("Shoulder").gameObject);
        
        // Play the shooting sound and set the next fire time
        this.m_audio.Play("Weapons/Rocket_Fire", this.m_shoulder.transform.position);
    }
    
    
    /*==============================
        HandleDeath
        Performs boss death logic
        @param Whether we're dead or not
    ==============================*/
    
    private bool HandleDeath()
    {
        bool isdead = (this.m_BossState == BossState.Dying || this.m_BossState == BossState.Dead);
        
        // Die if we're out of HP
        if (this.m_Health <= 0 && !isdead)
        {
            this.m_explosionsmall.SetActive(true);
            this.m_audio.Play("Voice/Boss/Dying", this.m_shoulder.transform.position);
            this.m_target.GetComponent<PlayerCombat>().SayLine("Voice/Shell/BossKill", true);
            this.m_TargetVelocity = Vector3.zero;
            this.m_target.GetComponent<PlayerCombat>().SetPlayerInvulTime(15.0f);
            this.m_rb.velocity = Vector3.zero;
            this.m_BossState = BossState.Dying;
            this.m_TimeToIdle = Time.time + 3.0f;
            this.m_BossJumpState = BossJumpState.Idle;
            isdead = true;
            
            // Disable the body box collider so the death animation doesn't look wonky
            GameObject lb = this.transform.Find("Model").gameObject;
            lb = lb.transform.Find("Armature").gameObject;
            lb = lb.transform.Find("LowerBody").gameObject;
            lb.GetComponent<BoxCollider>().enabled = false;
            
            // Disable collisions
            this.gameObject.layer = NoCollideLayer;
            foreach (Transform child in this.GetComponentsInChildren<Transform>(true))  
                child.gameObject.layer = NoCollideLayer;
        }
        
        // Actually become dead once the timer has run out
        if (this.m_BossState == BossState.Dying && this.m_TimeToIdle != 0 && this.m_TimeToIdle < Time.time)
        {
            this.m_BossState = BossState.Dead;
            this.m_audio.Play("Effects/Explosion_Huge", this.m_shoulder.transform.position);
            this.m_explosionsmall.SetActive(false);
            this.m_explosionbig.SetActive(true);
            FindObjectOfType<MusicManager>().StopMusic();
            FindObjectOfType<Level_FinishAnim>().SetLevelFinished();
        }
        else if (this.m_BossState == BossState.Dying)
            this.m_Trauma = 0.3f;
        return isdead;
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
        GetBossState
        Returns the boss' current state
        @returns The boss' current state
    ==============================*/
    
    public BossState GetBossState()
    {
        return this.m_BossState;
    }


    /*==============================
        GetBossCombatState
        Returns the boss' current combat state
        @returns The boss' current combat state
    ==============================*/
    
    public CombatState GetBossCombatState()
    {
        return this.m_CombatState;
    }


    /*==============================
        GetBossJumpState
        Returns the boss' current jump state
        @returns The boss' current jump state
    ==============================*/
    
    public BossJumpState GetBossJumpState()
    {
        return this.m_BossJumpState;
    }
    
    
    /*==============================
        GetHealth
        Retrieves the boss' health
        @returns The boss' health
    ==============================*/
    
    public int GetHealth()
    {
        return this.m_Health;
    }
    
    
    /*==============================
        GetHealth
        Retrieves the boss' maximum health
        @returns The boss' maximum health
    ==============================*/
    
    public int GetMaxHealth()
    {
        return BossLogic.MaxHealth;
    }
    
    
    /*==============================
        TakeDamage
        Makes the enemy take damage
        @param The amount of damage to take
    ==============================*/
    
    public void TakeDamage(int amount)
    {
        this.m_Health = Mathf.Max(0, this.m_Health - amount);
        this.m_Trauma = Mathf.Min(0.5f, this.m_Trauma + ((float)amount)/50.0f);
    }


    /*==============================
        SetBossJumpState
        Sets the boss' current jump state
        @param The boss' jump state
    ==============================*/
    
    public void SetBossJumpState(BossJumpState state)
    {
        this.m_BossJumpState = state;
    }
    
    
    /*==============================
        SetEnabled
        Sets the boss' enable state
        @param The enable state to set
    ==============================*/
    
    public void SetEnabled(bool enabled)
    {
        this.m_Enabled = enabled;
        if (enabled)
        {
            this.m_NextAttackTime = Time.time + 1.0f;
            if (FindObjectOfType<SceneController>().IsRespawning())
                this.m_NextMoveAction = Time.unscaledTime + 0.5f;
        }
    }
    
    
    /*==============================
        GetEnabled
        Gets whether the boss is enabled or not
        @returns Whether the boss is enabled
    ==============================*/
    
    public bool GetEnabled()
    {
        return this.m_Enabled;
    }
}
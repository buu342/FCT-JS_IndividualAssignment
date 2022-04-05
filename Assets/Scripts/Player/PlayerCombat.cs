/****************************************************************
                       PlayerController.cs
    
This script handles the player combat logic (health, weapons, 
bullet time, etc...).
****************************************************************/

//#define DEBUG

using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Constants
    private const float BulletTimeRate = 0.5f;
    private const float MeleeHoldTime  = 0.15f;
    private const float PistolFireRate = 0.2f;
    private const float MeleeIdleTime  = 0.4f;
    private const float PainIdleTime   = 0.3f;
    private const float InvulTime      = 0.3f;
    private const float StaminaGain    = 0.8f;
    private const float StaminaLose    = 1.0f;
    private const int   StreakLose     = 1;
    private const float StreakLoseTime = 10;

    // Combat states
    public enum CombatState
    {
        Idle,
        Melee,
        Melee2,
        Shooting,
        Pain,
    }
    
    // States
    private int   m_Health = 100;
    private float m_Stamina = 100.0f;
    private bool  m_StaminaRecovering = false;
    private int   m_Score = 0;
    private int   m_Streak = 0;
    private float m_LastStreakTime = 0.0f;
    private float m_InvulTime = 0.0f;
    
    // Combat
    private Vector3 m_OriginalAimPos;
    private Quaternion m_OriginalAimAng;
    private Vector3 m_AimDir = Vector3.zero;
    private float m_MouseHoldTime = 0;
    private float m_NextFire = 0;
    private float m_TargetTimeScale = 1.0f;
    private float m_TimeToIdle = 0.0f;
    private CombatState m_CombatState = CombatState.Idle;
    
    // Components
    public  GameObject m_bulletprefab;
    public  GameObject m_swordprefab;
    private GameObject m_fireattachment;
    private GameObject m_shoulder;
    private PlayerController m_plycont;
    private AudioManager m_audio;
    
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {
        this.m_audio = FindObjectOfType<AudioManager>();
        this.m_shoulder = this.transform.Find("Shoulder").gameObject;
        this.m_fireattachment = this.transform.Find("FireAttachment").gameObject;
        this.m_OriginalAimPos = this.m_fireattachment.transform.localPosition;
        this.m_OriginalAimAng = this.m_fireattachment.transform.localRotation;
        this.m_plycont = this.GetComponent<PlayerController>();
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {        
        HandleControls();
        
        // Handle going to idle state
        if (this.m_TimeToIdle != 0 && this.m_TimeToIdle < Time.unscaledTime )
        {
            this.m_CombatState = CombatState.Idle;
            this.m_TimeToIdle = 0;
        }
        
        // Handle bullet time
        Time.timeScale = Mathf.Lerp(Time.timeScale, this.m_TargetTimeScale, PlayerCombat.BulletTimeRate);
    }

    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        // Handle streak losing
        if (this.m_Streak > 0 && this.m_LastStreakTime < Time.unscaledTime)
            this.m_Streak = Mathf.Max(0, this.m_Streak - PlayerCombat.StreakLose);
        
        // Handle death
        if (this.m_Health <= 0)
            GameObject.Find("SceneController").GetComponent<SceneController>().RestartCurrentScene();
    }
    
    
    /*==============================
        OnCollisionEnter
        Handles collision response
        @param The collision data
    ==============================*/
    
    void OnCollisionEnter(Collision col)
    {
        Collider other = col.collider;
        switch (other.tag)
        {
            case "Enemies":
            case "Boss":
                this.TakeDamage(10, other.gameObject.transform.position);
                break;
        }
    }
    
    
    /*********************************
             Control Handling
    *********************************/
    
    /*==============================
        HandleControls
        Handles buttons that should be 
        checked every frame.
    ==============================*/
    
    private void HandleControls()
    {
        // Calculate aim based on the mouse position
        float dist = 0; 
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        Ray mouseray = Camera.main.ScreenPointToRay(Input.mousePosition);
        plane.Raycast(mouseray, out dist);
        Vector3 point = mouseray.GetPoint(dist);
        this.m_AimDir = point - this.m_shoulder.transform.position;
        this.m_AimDir.Normalize();
        
        // Now that the direction is calculated, point the weapon origin to face it
        this.m_fireattachment.transform.localPosition = this.m_OriginalAimPos;
        this.m_fireattachment.transform.localRotation = this.m_OriginalAimAng;
        this.m_fireattachment.transform.RotateAround(this.m_shoulder.transform.position, Vector3.forward, Mathf.Atan2(this.m_AimDir.y, this.m_AimDir.x)*Mathf.Rad2Deg);
        
        // Debug the ray projection math
        #if DEBUG 
            Debug.DrawRay(this.m_shoulder.transform.position, this.m_AimDir*10, Color.green, 0, false);
            Debug.DrawRay(mouseray.origin, mouseray.direction*100, Color.blue, 0, false);
        #endif
        
        // Shooting and Melee
        if (this.m_CombatState != CombatState.Pain)
        {
            if (Input.GetButton("Fire"))
            {
                OnFire();
            }
            else
            {
                // Melee if we let go of the mouse quickly
                if (this.m_MouseHoldTime > Time.unscaledTime)
                    OnMelee();
                this.m_MouseHoldTime = 0;
            }
        }
        
        // Bullet time
        if (Input.GetButton("BulletTime") && this.m_Stamina > 0 && !this.m_StaminaRecovering)
        {
            OnBulletTime();
        }
        else
        {
            // Stop bullet time
            if (this.m_TargetTimeScale != 1.0f)
            {
                this.m_audio.Play("Gameplay/Slowmo_Out");
                this.m_audio.Stop("Gameplay/Slowmo_In");
            }
            this.m_TargetTimeScale = 1.0f;
            
            // Recover stamina
            if (this.m_Stamina < 100.0f)
            {
                this.m_Stamina = Mathf.Min(this.m_Stamina + PlayerCombat.StaminaGain*100.0f*Time.deltaTime, 100.0f);
                if (this.m_StaminaRecovering && this.m_Stamina == 100.0f)
                    this.m_StaminaRecovering = false;
            }
        }
    }
    
    
    /*==============================
        TakeDamage
        Makes the player take damage
        @param The amount of damage to take
        @param The coordinate where the damage came from
    ==============================*/
    
    public void TakeDamage(int amount, Vector3 position)
    {
        if (this.m_CombatState == PlayerCombat.CombatState.Pain || this.m_InvulTime > Time.unscaledTime)
            return;
        this.m_Streak = Mathf.Max(0, this.m_Streak - 25);
        this.m_Health -= amount;
        this.m_CombatState = CombatState.Pain;
        this.m_TimeToIdle = Time.unscaledTime + PlayerCombat.PainIdleTime;
        this.m_plycont.OnTakeDamage(position);
        this.m_InvulTime = this.m_TimeToIdle + PlayerCombat.InvulTime;
    }
    
    
    /*********************************
                 Attacks
    *********************************/
    
    /*==============================
        OnFire
        Handle shooting
    ==============================*/
    
    public void OnFire()
    {
        // If the mouse was just pressed, start the check if we're melee attacking
        if (this.m_MouseHoldTime == 0)
            this.m_MouseHoldTime = Time.unscaledTime  + PlayerCombat.MeleeHoldTime;
        
        // If we held the shoot button for too long, then we want to fire.
        if (this.m_MouseHoldTime < Time.unscaledTime && this.m_NextFire < Time.time)
        {
            // Create the bullet object
            ProjectileLogic bullet = Instantiate(this.m_bulletprefab, this.m_fireattachment.transform.position, this.m_fireattachment.transform.rotation).GetComponent<ProjectileLogic>();
            bullet.SetOwner(this.gameObject);
            bullet.SetSpeed(30.0f);
            bullet.SetOrigin(this.m_shoulder.transform.position);
            
            // Play the shooting sound and set the next fire time
            this.m_audio.Play("Weapons/Pistol_Fire", this.m_shoulder.transform.position);
            this.m_NextFire = Time.time + PlayerCombat.PistolFireRate;
            this.m_CombatState = CombatState.Shooting;
            this.m_TimeToIdle = Time.unscaledTime + PlayerCombat.PistolFireRate;
            if (this.m_TargetTimeScale != 1.0f) // Fixes a small animation bug I'm too lazy to fix proper
                this.m_TimeToIdle += 0.2f;
        }
    }
    
    
    /*==============================
        OnMelee
        Handle Melee attacking
    ==============================*/
    
    public void OnMelee()
    {
        SwordLogic sword = Instantiate(this.m_swordprefab, this.m_fireattachment.transform.position, this.m_fireattachment.transform.rotation).GetComponent<SwordLogic>();
        sword.SetOwner(this.gameObject);
        
        // Set the player's combat state depending on the attack combo
        if (this.m_CombatState != CombatState.Melee)
            this.m_CombatState = CombatState.Melee;
        else
            this.m_CombatState = CombatState.Melee2;
        
        // Play the attack sound and set the time to idle
        this.m_TimeToIdle = Time.unscaledTime + PlayerCombat.MeleeIdleTime;
        this.m_audio.Play("Weapons/Sword_Swing", this.m_shoulder.transform.position);
    }
    
    
    /*==============================
        OnBulletTime
        Handle bullet time
    ==============================*/
    
    public void OnBulletTime()
    {
        // Start bullet time
        if (this.m_TargetTimeScale != 0.5f)
        {
            this.m_audio.Play("Gameplay/Slowmo_In");
            this.m_audio.Stop("Gameplay/Slowmo_Out");
        }
        this.m_TargetTimeScale = 0.5f;
        
        // Lose stamina
        this.m_Stamina = Mathf.Max(0.0f, this.m_Stamina - PlayerCombat.StaminaLose*100.0f*Time.deltaTime);
        if (this.m_Stamina == 0.0f)
            this.m_StaminaRecovering = true;
    }
    
    
    /*********************************
             Getters/Setters
    *********************************/

    /*==============================
        GetCombatState
        Returns the player's current combat state
        @returns The player's current combat state
    ==============================*/
    
    public CombatState GetCombatState()
    {
        return this.m_CombatState;
    }


    /*==============================
        GetFireAttachment
        Returns a pointer to the player's fire attachment object
        @returns The player's fire attachment object
    ==============================*/
    
    public GameObject GetFireAttachment()
    {
        return this.m_fireattachment;
    }


    /*==============================
        GetAimDirection
        Returns a direction vector pointing where the player is aiming at
        @returns The player's aim vector
    ==============================*/
    
    public Vector3 GetAimDirection()
    {
        return this.m_AimDir;
    }
    
    
    /*==============================
        GetHealth
        Retrieves the player's health
        @returns The player's health
    ==============================*/
    
    public int GetHealth()
    {
        return this.m_Health;
    }
    
    
    /*==============================
        GetStamina
        Retrieves the player's stamina
        @returns The player's stamina
    ==============================*/
    
    public float GetStamina()
    {
        return this.m_Stamina;
    }
    
    
    /*==============================
        GetStaminaRecovering
        Gets whether the player's stamina is recovering
        @returns If the player's stamina is recovering, or not
    ==============================*/
    
    public bool GetStaminaRecovering()
    {
        return this.m_StaminaRecovering;
    }
    
    
    /*==============================
        GetScore
        Retrieves the player's score
        @returns The player's score
    ==============================*/
    
    public int GetScore()
    {
        return this.m_Score;
    }
    
    
    /*==============================
        GetStreak
        Retrieves the player's streak
        @returns The player's streak
    ==============================*/
    
    public int GetStreak()
    {
        return this.m_Streak;
    }
    
    
    /*==============================
        GiveScore
        Give the player some score
        @param The score to give
    ==============================*/
    
    public void GiveScore(int score)
    {
        this.m_Streak = Mathf.Min(100, this.m_Streak + score/10);
        this.m_LastStreakTime = Time.unscaledTime + PlayerCombat.StreakLoseTime;
        this.m_Score += score*Mathf.Min(1 + this.m_Streak/20, 5);
    }
}
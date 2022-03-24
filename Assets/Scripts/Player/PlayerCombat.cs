/****************************************************************
                       PlayerController.cs
    
This script handles the player combat logic (weapons, bullet 
time, etc...).
****************************************************************/

using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private const bool  DebugOn        = true;
    private const float BulletTimeRate = 0.5f;
    private const float MeleeHoldTime  = 0.15f;
    private const float PistolFireRate = 0.2f;
    private const float MeleeIdleTime  = 0.4f;
    private const float ShootIdleTime  = PistolFireRate + 0.1f;

    // Combat states
    public enum CombatState
    {
        Idle,
        Melee,
        Shooting,
    }
    
    // Combat
    private Vector3 m_OriginalAimPos;
    private Quaternion m_OriginalAimAng;
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
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        HandleControls();
        
        // Handle combat state
        if (this.m_TimeToIdle != 0 && this.m_TimeToIdle < Time.time)
        {
            this.m_CombatState = CombatState.Idle;
            this.m_TimeToIdle = 0;
        }
        
        // Handle bullet time
        Time.timeScale = Mathf.Lerp(Time.timeScale, this.m_TargetTimeScale, PlayerCombat.BulletTimeRate);
    }
    
    public CombatState GetCombatState()
    {
        return this.m_CombatState;
    }
    
    public GameObject GetFireAttachment()
    {
        return this.m_fireattachment;
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
        Vector3 direction = point - this.m_shoulder.transform.position;
        direction.Normalize();
        
        // Now that the direction is calculated, point the weapon origin to face it
        this.m_fireattachment.transform.localPosition = this.m_OriginalAimPos;
        this.m_fireattachment.transform.localRotation = this.m_OriginalAimAng;
        this.m_fireattachment.transform.RotateAround(this.m_shoulder.transform.position, Vector3.forward, Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg);
        
        // Debug the ray projection math
        if (PlayerCombat.DebugOn)
        {
            Debug.DrawRay(this.m_shoulder.transform.position, direction*10, Color.green, 0, false);
            Debug.DrawRay(mouseray.origin, mouseray.direction*100, Color.blue, 0, false);
        }
        
        // Shooting and Melee
        if (Input.GetButton("Fire"))
        {
            OnFire();
        }
        else
        {
            if (this.m_MouseHoldTime > Time.time)
                OnMelee();
            this.m_MouseHoldTime = 0;
        }
        
        // Bullet time
        if (Input.GetButton("BulletTime"))
            OnBulletTime();
        else
        {
            if (this.m_TargetTimeScale != 1.0f)
                this.m_audio.Play("Gameplay/Slowmo_Out");
            this.m_TargetTimeScale = 1.0f;
        }
    }
    
    
    /*********************************
                  Combat
    *********************************/
    
    /*==============================
        OnFire
        Handle shooting
    ==============================*/
    
    public void OnFire()
    {
        // If the mouse was just pressed, start the check if we're melee attacking
        if (this.m_MouseHoldTime == 0)
            this.m_MouseHoldTime = Time.time + PlayerCombat.MeleeHoldTime;
        
        // If we held the shoot button for too long, then we want to fire.
        if (this.m_MouseHoldTime < Time.time && this.m_NextFire < Time.time)
        {
            // Create the bullet object
            ProjectileLogic bullet = Instantiate(this.m_bulletprefab, this.m_fireattachment.transform.position, this.m_fireattachment.transform.rotation).GetComponent<ProjectileLogic>();
            bullet.SetOwner(this.gameObject);
            bullet.SetSpeed(30.0f);
            
            // Play the shooting sound and set the next fire time
            this.m_audio.Play("Weapons/Pistol_Fire");
            this.m_NextFire = Time.time + PlayerCombat.PistolFireRate;
            this.m_CombatState = CombatState.Shooting;
            this.m_TimeToIdle = Time.time + PlayerCombat.ShootIdleTime;
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
        this.m_CombatState = CombatState.Melee;
        this.m_TimeToIdle = Time.time + PlayerCombat.MeleeIdleTime*(1/Time.timeScale);
        this.m_audio.Play("Weapons/Sword_Swing1");
    }
    
    
    /*==============================
        OnBulletTime
        Handle bullet time
    ==============================*/
    
    public void OnBulletTime()
    {
        if (this.m_TargetTimeScale != 0.5f)
            this.m_audio.Play("Gameplay/Slowmo_In");
        this.m_TargetTimeScale = 0.5f;
    }
}
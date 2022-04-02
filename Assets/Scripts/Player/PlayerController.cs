/****************************************************************
                       PlayerController.cs
    
This script handles all of the player movement physics. I went
ahead and pretty much implemented all the physics myself, because
Unity's default stuff is really awful. 
****************************************************************/

//#define DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // Constants
    private const float Gravity     = -80.0f;  // Player gravity
    private const float MoveSpeed   = 10.0f;   // Movement speed
    private const float JumpPower   = 1000.0f; // Jump force
    private const int   MaxJumps    = 2;       // Maximum number of allowed jumps
    private const float CoyoteTime  = 0.1f;    // Coyote time (in seconds)
    private const float DamageForce = 4.0f;    // Force to apply when taking damage
    
    // Player state
    public enum PlayerState
    {
        Idle,
        Forward,
        Backward,
        Pain,
    }
    
    // Jump state
    public enum PlayerJumpState
    {
        Idle,
        Jump,
        Jump2,
        Fall,
        Land,
        Flying,
    }
    
    // Movement
    private float m_Acceleration = 0.5f;
    private int m_JumpCount = 0;
    private bool m_JustJumped = false; // Buffer for ground checking after jumping
    private bool m_OnGround;
    private float m_CoyoteTimer = 0;
    private float m_LandTimer = 0;
    private PlayerState m_PlayerState = PlayerState.Idle;
    private PlayerJumpState m_PlayerJumpState = PlayerJumpState.Idle;
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    private GameObject m_CurrentFloor = null;
    
    // Components
    private Collider m_col;
    private Rigidbody m_rb;
    private AudioManager m_audio;
    private PlayerCombat m_plycombat;
    
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {
        this.m_col = this.GetComponent<Collider>();
        this.m_rb = this.GetComponent<Rigidbody>();
        this.m_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        this.m_OnGround = IsGrounded();
        this.m_plycombat = this.GetComponent<PlayerCombat>();
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (this.m_plycombat.GetCombatState() != PlayerCombat.CombatState.Pain)
            HandleControls();
    }
    
    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        // If we just jumped, skip the ground check. Otherwise, cache the result of whether we're on the ground
        if (this.m_JustJumped)
            this.m_JustJumped = false;
        else
            this.m_OnGround = IsGrounded();
        
        // Handle controls
        if (this.m_plycombat.GetCombatState() != PlayerCombat.CombatState.Pain)
            HandleFixedControls();
        
        // Handle character movement
        HandleMovement();
        
        // Handle the player state
        HandleState();
    }
    
    
    /*==============================
        IsGrounded
        Checks whether the player is grounded.
        Since the player collider is a cube, this
        is accomplished using 4 raycasts, one for
        each corner of the cube.
        @returns true if the player is touching  
                 the ground, false if otherwise
    ==============================*/
    
    public bool IsGrounded()
    {
        int i = 0;
        int highest = 0;
        bool[] col = new bool[4];
        RaycastHit[] hit = new RaycastHit[4];
        Dictionary<GameObject, int> collisions = new Dictionary<GameObject, int>();
        float xsize = this.m_col.bounds.size.x/2.0f-0.01f;
        float ysize = 0.01f;
        float zsize = this.m_col.bounds.size.z/2.0f-0.01f;
        float raylen = 0.2f;
        
        // Perform the raycasts
        col[0] = Physics.Raycast(this.transform.position + (new Vector3( xsize, ysize, 0)), Vector3.down, out hit[0], raylen);
        col[1] = Physics.Raycast(this.transform.position + (new Vector3(-xsize, ysize, 0)), Vector3.down, out hit[1], raylen);
        col[2] = Physics.Raycast(this.transform.position + (new Vector3(0, ysize,  zsize)), Vector3.down, out hit[2], raylen); 
        col[3] = Physics.Raycast(this.transform.position + (new Vector3(0, ysize, -zsize)), Vector3.down, out hit[3], raylen);
        
        // Draw a green ray for debugging where we're standing
        #if DEBUG
            Debug.DrawRay(this.transform.position + (new Vector3(0, ysize, 0)), Vector3.down*raylen, Color.green, 0, false); 
        #endif
        
        // Count which entities were hit
        foreach (RaycastHit h in hit)
        {
            if (col[i])
            {
                if (collisions.ContainsKey(hit[i].collider.gameObject))
                    collisions[hit[i].collider.gameObject]++;
                else
                    collisions.Add(hit[i].collider.gameObject, 0);
            }
            i++;
        }
        
        // Check we actually hit something
        if (collisions.Count == 0)
        {
            this.m_CurrentFloor = null;
            return false;
        }
        
        // Get the entity which was hit the most
        foreach (KeyValuePair<GameObject, int> entry in collisions)
        {
            if (entry.Value > highest)
            {
                this.m_CurrentFloor = entry.Key;
                highest = entry.Value;
            }
        }
        return true;
    }
    
    
    /*==============================
        HandleMovement
        Handles movement physics
    ==============================*/
    
    private void HandleMovement()
    {
        // Handle Coyote time
        if (!this.m_OnGround && this.m_JumpCount == 0 && this.m_CoyoteTimer == 0)
            this.m_CoyoteTimer = Time.unscaledTime + PlayerController.CoyoteTime;
        
        // If we're on the ground
        if (this.m_OnGround)
        {
            // Reset our jump counter
            this.m_JumpCount = 0;
            this.m_CoyoteTimer = 0;
    
        }
        else // Otherwise, add gravity as a downward force
            this.m_rb.AddForce(0, PlayerController.Gravity, 0);
        
        // Interpolate our current velocity to match our target
        this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity*(1/Time.timeScale), this.m_Acceleration);
        
        // Actually apply the velocity
        this.m_rb.velocity = new Vector3(this.m_CurrentVelocity.x, this.m_rb.velocity.y, this.m_CurrentVelocity.z);
        
        // If we're on a moving platform, move along with the floor
        if (this.m_CurrentFloor != null && this.m_CurrentFloor.GetComponent<Rigidbody>() != null)
        {
            Vector3 vel = this.m_CurrentFloor.GetComponent<Rigidbody>().velocity;
            this.m_rb.velocity += new Vector3(vel.x, Mathf.Min(vel.y, 0.0f), vel.z);
        }
    }
    
    
    /*==============================
        HandleState
        Handles movement state chanegs
    ==============================*/
    
    private void HandleState()
    {
        // Handle Forward/Backward movement states
        if (this.m_TargetVelocity.sqrMagnitude > 0 && this.m_OnGround)
        {
            GameObject fireattach = this.transform.Find("FireAttachment").gameObject;
            if ((this.m_TargetVelocity.x >= 0 && fireattach.transform.localPosition.z >= 0) || (this.m_TargetVelocity.x < 0 && fireattach.transform.localPosition.z < 0))
                this.m_PlayerState = PlayerState.Forward;
            else
                this.m_PlayerState = PlayerState.Backward;
        }
        else
            this.m_PlayerState = PlayerState.Idle;
        
        // Handle jump states
        if (this.m_rb.velocity.y > 2 && this.m_JumpCount < 2 && this.m_JumpCount > 0 && !this.m_OnGround)
            this.m_PlayerJumpState = PlayerJumpState.Jump;
        else if (this.m_rb.velocity.y > 2 && this.m_JumpCount > 0 && !this.m_OnGround)
            this.m_PlayerJumpState = PlayerJumpState.Jump2;
        else if (this.m_rb.velocity.y < 0 && !this.m_OnGround)
            this.m_PlayerJumpState = PlayerJumpState.Fall;
        else if (this.m_PlayerJumpState == PlayerJumpState.Fall && this.m_OnGround)
        {
            this.m_PlayerJumpState = PlayerJumpState.Land;
            this.m_LandTimer = Time.unscaledTime + 0.5f;
        }
        else if (this.m_PlayerJumpState == PlayerJumpState.Land && this.m_OnGround && this.m_LandTimer < Time.unscaledTime)
            this.m_PlayerJumpState = PlayerJumpState.Idle;
    }


    /*==============================
        GetPlayerState
        Returns the player's current movement state
        @returns The player's current movement state
    ==============================*/
    
    public PlayerState GetPlayerState()
    {
        return this.m_PlayerState;
    }


    /*==============================
        GetPlayerJumpState
        Returns the player's current jump state
        @returns The player's current jump state
    ==============================*/
    
    public PlayerJumpState GetPlayerJumpState()
    {
        return this.m_PlayerJumpState;
    }


    /*==============================
        OnTakeDamage
        Handle player movement when taking damage
        @param Where the damage was received from
    ==============================*/
    
    public void OnTakeDamage(Vector3 dmgpos)
    {
        this.m_CurrentVelocity = Vector3.zero;
        this.m_rb.velocity = Vector3.zero;
        if (dmgpos.x > this.transform.position.x)
            this.m_TargetVelocity = -Vector3.right*PlayerController.DamageForce;
        else
            this.m_TargetVelocity = Vector3.right*PlayerController.DamageForce;
        this.m_rb.AddForce(Vector3.up*PlayerController.DamageForce*100.0f);
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
        // Jumping
        if (Input.GetButtonDown("Jump"))
            OnJump();
    }
    
    
    /*==============================
        HandleFixedControls
        Handles buttons that should be 
        checked every physics update.
    ==============================*/
    
    private void HandleFixedControls()
    {
        // Forward/backward movement
        if (Input.GetButton("Forward"))
            OnForward();
        else if (Input.GetButton("Backward"))
            OnBackward();
        else
            this.m_TargetVelocity = Vector3.zero;
        
        // Crouching
        if (Input.GetButton("Duck"))
            OnDuck();
    }
    
    
    /*==============================
        OnForward
        Handle forward movement
    ==============================*/
    
    public void OnForward()
    {
        this.m_TargetVelocity = this.transform.forward*PlayerController.MoveSpeed;
    }
    
    
    /*==============================
        OnBackward
        Handle backward movement
    ==============================*/
    
    public void OnBackward()
    {
        this.m_TargetVelocity = -this.transform.forward*PlayerController.MoveSpeed;
    }
    
    
    /*==============================
        OnJump
        Handle jumping
    ==============================*/
    
    public void OnJump()
    {
        // Check if we didn't start by falling and that we haven't reached the jump limit
        if ((!this.m_OnGround && this.m_JumpCount == 0 && this.m_CoyoteTimer < Time.unscaledTime) || (this.m_JumpCount >= PlayerController.MaxJumps))
            return;
        this.m_JumpCount++;
        this.m_JustJumped = true;
        this.m_OnGround = false;
        this.m_rb.velocity = new Vector3(this.m_CurrentVelocity.x, 0, this.m_CurrentVelocity.z);
        this.m_rb.AddForce(this.transform.up*PlayerController.JumpPower);
    }
    
    
    /*==============================
        OnDuck
        Handle crouching
    ==============================*/
    
    public void OnDuck()
    {
        #if DEBUG
            Camera.main.GetComponent<CameraLogic>().AddTrauma(0.1f);
        #endif
    }
}
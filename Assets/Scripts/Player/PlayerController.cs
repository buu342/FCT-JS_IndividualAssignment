/****************************************************************
                       PlayerController.cs
    
This script handles all of the player movement physics. I went
ahead and pretty much implemented all the physics myself, because
Unity's default stuff is really awful. 
****************************************************************/

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const bool  DebugOn    = true;
    private const float Gravity    = -90.0f;    // Player gravity
    private const float MoveSpeed  = 10.0f;     // Movement speed
    private const float JumpPower  = 1000.0f;   // Jump force
    private const int   MaxJumps   = 2;         // Maximum number of allowed jumps
    private const float CoyoteTime = 0.1f;     // Coyote time (in seconds)
    
    // Movement
    private float m_Acceleration;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_TargetVelocity;
    private int m_JumpCount;
    private bool m_OnGround;
    private bool m_JustJumped; // Buffer for ground checking after jumping
    private float m_CoyoteTimer;
    
    // Components
    private Collider m_col;
    private Rigidbody m_rb;
    private AudioManager m_audio; 
    
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {
        this.m_JumpCount = 0;
        this.m_Acceleration = 0.5f;
        this.m_TargetVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        this.m_CurrentVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        this.m_col = this.GetComponent<Collider>();
        this.m_rb = this.GetComponent<Rigidbody>();
        this.m_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        this.m_OnGround = IsGrounded();
        this.m_JustJumped = false;
        this.m_CoyoteTimer = 0;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
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
        HandleFixedControls();
        
        // Handle character movement
        HandleMovement();
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
    
    private bool IsGrounded()
    {
        float xsize = this.m_col.bounds.size.x/2.0f-0.1f;
        float ysize = 0.01f;
        float zsize = this.m_col.bounds.size.z/2.0f-0.1f;
        float raylen = 0.1f;
        bool cast1 = Physics.Raycast(this.transform.position + (new Vector3( xsize, ysize, 0)), Vector3.down, raylen);
        bool cast2 = Physics.Raycast(this.transform.position + (new Vector3(-xsize, ysize, 0)), Vector3.down, raylen);
        bool cast3 = Physics.Raycast(this.transform.position + (new Vector3(0, ysize,  zsize)), Vector3.down, raylen); 
        bool cast4 = Physics.Raycast(this.transform.position + (new Vector3(0, ysize, -zsize)), Vector3.down, raylen);
        if (PlayerController.DebugOn)
            Debug.DrawRay(this.transform.position + (new Vector3(0, ysize, 0)), Vector3.down*raylen, Color.green, 0, false);
        return cast1 || cast2 || cast3 || cast4;
    }
    
    
    /*==============================
        HandleMovement
        Handles movement physics
    ==============================*/
    
    private void HandleMovement()
    {
        // Handle Coyote time
        if (!this.m_OnGround && this.m_JumpCount == 0 && this.m_CoyoteTimer == 0)
            this.m_CoyoteTimer = Time.time + PlayerController.CoyoteTime*Time.timeScale;
        
        // If we're on the ground, reset our jump counter
        if (this.m_OnGround)
        {
            this.m_JumpCount = 0;
            this.m_CoyoteTimer = 0;
        }
    
        // Add our own gravity as a downward force
        this.m_rb.AddForce(0, PlayerController.Gravity, 0);
        
        // Interpolate our current velocity to match our target
        this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity*(1/Time.timeScale), this.m_Acceleration);
        
        // Actually apply the velocity
        this.m_rb.velocity = new Vector3(this.m_CurrentVelocity.x, this.m_rb.velocity.y, this.m_CurrentVelocity.z);
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
            this.m_TargetVelocity = new Vector3(0, 0, 0);
        
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
        if ((!this.m_OnGround && this.m_JumpCount == 0 && this.m_CoyoteTimer < Time.time) || (this.m_JumpCount >= PlayerController.MaxJumps))
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
        Camera.main.GetComponent<CameraLogic>().AddTrauma(0.1f);
    }
}
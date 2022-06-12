/****************************************************************
                       PlayerController.cs
    
This script handles all of the player movement physics.
****************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public  const float Gravity      = -80.0f;
    private const float MoveSpeed    = 10.0f;
    private const float Acceleration = 0.5f;
    private const float TurnSpeed    = 0.1f;
    
    public enum PlayerMovementState
    {
        Idle,
        Moving,
    }
    
    public enum PlayerCombatState
    {
        Idle,
        Aiming,
        Firing,
        ReloadStart,
        ReloadLoop,
        ReloadEnd,
    }

    public Rigidbody  m_RigidBody;  
    private GameObject m_Camera;
    private CameraController m_CameraController;
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    private Vector2 m_MovementDirection;
    
    private PlayerMovementState m_MovementState = PlayerMovementState.Idle;
    private PlayerCombatState m_CombatState = PlayerCombatState.Idle;
    
    
    /*==============================
        Start
        Called when the player is initialized
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
        this.m_TargetVelocity = (this.m_MovementDirection.y*this.transform.forward + this.m_MovementDirection.x*this.transform.right)*PlayerController.MoveSpeed;
        
        // Turn the player to face the same direction as the camera
        if (this.m_TargetVelocity != Vector3.zero)
        {
            Quaternion targetang = Quaternion.Euler(0.0f, this.m_Camera.transform.eulerAngles.y, 0.0f); 
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetang, PlayerController.TurnSpeed);
        }
        else
            this.m_MovementState = PlayerMovementState.Idle;
    }
    
    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity, PlayerController.Acceleration);
        this.m_RigidBody.velocity = new Vector3(this.m_CurrentVelocity.x, this.m_RigidBody.velocity.y + this.m_CurrentVelocity.y, this.m_CurrentVelocity.z);
        this.m_RigidBody.AddForce(0, PlayerController.Gravity, 0);
    }
    
    
    /*==============================
        SetCamera
        Sets the player's camera object
        @param The camera object to assign
    ==============================*/
    
    public void SetCamera(GameObject cam)
    {
        this.m_Camera = cam;
        this.m_CameraController = this.m_Camera.GetComponent<CameraController>();
    }
    
    
    /*==============================
        OnLook
        Called when the player moves the mouse
        @param The input value
    ==============================*/
    
    void OnLook(InputValue value)
    {
        if (this.m_CameraController != null)
            this.m_CameraController.SetLookDirection(new Vector2(value.Get<Vector2>().x, value.Get<Vector2>().y));
    }
    
    
    /*==============================
        OnMove
        Called when the player presses a movement key
        @param The input value
    ==============================*/

    void OnMove(InputValue value) 
    {
        this.m_MovementDirection = new Vector2(value.Get<Vector2>().x,value.Get<Vector2>().y);
        this.m_MovementState = PlayerMovementState.Moving;
    }
    
    
    /*==============================
        GetPlayerMovementDirection
        Retrieves the value of the player movement direction
        @return The player movement direction
    ==============================*/

    public Vector3 GetPlayerMovementDirection() 
    {
        return this.m_MovementDirection;
    }
    
    
    /*==============================
        GetPlayerMovementState
        Retrieves the value of the player movement state
        @return The player movement state
    ==============================*/

    public PlayerMovementState GetPlayerMovementState() 
    {
        return this.m_MovementState;
    }
}
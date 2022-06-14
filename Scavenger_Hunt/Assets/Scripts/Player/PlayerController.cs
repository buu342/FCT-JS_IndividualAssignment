/****************************************************************
                       PlayerController.cs
    
This script handles all of the player controls, movement physics,
and combat.
****************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Photon.Pun;
public class PlayerController : MonoBehaviour
{
    //Sound utilities
    public delegate void MakeSound(Vector3 origin, float distance);
    public static event MakeSound makeSound;
    public static float distanceOfSound = 10.0f;
    

    public  const float Gravity      = -80.0f;
    public  const int   ClipSize     = 8;
    private const float MoveSpeed    = 10.0f;
    private const float Acceleration = 0.5f;
    private const float TurnSpeed    = 0.1f;
    private bool  multiplayer= JoinMultiplayer.Multiplayer;
    private const float FireTime        = 1.0f;
    private const float ReloadStartTime = 0.333f;
    private const float ReloadLoopTime  = 0.833f;
    private const float ReloadEndTime   = 0.333f;
    private PhotonView view;
    public enum PlayerMovementState
    {
        Idle,
        Moving
    }
    
    public enum PlayerAimState
    {
        Idle,
        Aiming
    }
    
    public enum PlayerCombatState
    {
        Idle,
        Firing,
        ReloadStart,
        ReloadLoop,
        ReloadEnd
    }

    public Rigidbody  m_RigidBody;  
    public GameObject m_FlashLight;
    public PlayerAnimations m_PlyAnims;
    public int m_AmmoClip = 8;
    public int m_AmmoReserve = 8;
    
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    private Vector3  m_MovementDirection=Vector3.zero;
    private float m_AimVerticalAngle = 0.0f;
    private Quaternion m_OriginalFlashLightAngles = Quaternion.identity;
    private Quaternion m_TargetFlashLightAngle = Quaternion.identity;
    private Quaternion m_CurrentFlashLightAngle = Quaternion.identity;
    private float m_NextFireTime = 0.0f;
    private bool m_CancelReload = false;
    
    private AudioManager m_Audio;
    private GameObject m_Camera;
    private CameraController m_CameraController;
    
    private PlayerMovementState m_MovementState = PlayerMovementState.Idle;
    private PlayerAimState m_AimState = PlayerAimState.Idle;
    private PlayerCombatState m_CombatState = PlayerCombatState.Idle;

    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {
        if(multiplayer)
        view=GetComponent<PhotonView>();
        this.m_OriginalFlashLightAngles = this.m_FlashLight.transform.rotation;
        this.m_Audio = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    void OnEnable() {
        InputManagerScript.playerInput.Player.Fire.started += Fire;
        InputManagerScript.playerInput.Player.Move.started += Move;
        InputManagerScript.playerInput.Player.Reload.started += Reload;
        InputManagerScript.playerInput.Player.Aim.started += Aim;
        InputManagerScript.playerInput.Player.Aim.canceled += Aim;

    }
    

    void OnDisable() {
        InputManagerScript.playerInput.Player.Fire.started -= Fire;
        InputManagerScript.playerInput.Player.Move.started -= Move;
        InputManagerScript.playerInput.Player.Reload.started -= Reload;
        InputManagerScript.playerInput.Player.Aim.started -= Aim;
         InputManagerScript.playerInput.Player.Aim.canceled -= Aim;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    { if(view!=null)
            if(!view.IsMine)
            return;  
        this.m_MovementDirection = m_CameraController.isInFreeMode() ? Vector2.zero:InputManagerScript.Move.ReadValue<Vector2>();
        this.m_TargetVelocity = (this.m_MovementDirection.y*this.transform.forward + this.m_MovementDirection.x*this.transform.right)*PlayerController.MoveSpeed;
        
        // Turn the player to face the same direction as the camera
        Quaternion targetang = Quaternion.Euler(0.0f, this.m_Camera.transform.eulerAngles.y, 0.0f);
        if (this.m_TargetVelocity == Vector3.zero)
        {
            this.m_MovementState = PlayerMovementState.Idle;
            if (this.GetPlayerAiming())
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetang, PlayerController.TurnSpeed);
        }
        else
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetang, PlayerController.TurnSpeed);
        
        // Aim
        this.m_AimVerticalAngle = this.m_Camera.transform.eulerAngles.x;
        if (this.GetPlayerAiming())
        {
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetang, PlayerController.TurnSpeed);
            this.m_TargetFlashLightAngle = this.m_OriginalFlashLightAngles*Quaternion.Euler(this.m_Camera.transform.eulerAngles.x, this.m_Camera.transform.eulerAngles.y, 0);
        }
        else
            this.m_TargetFlashLightAngle = this.transform.rotation*this.m_OriginalFlashLightAngles;
        this.m_CurrentFlashLightAngle = Quaternion.Slerp(this.m_CurrentFlashLightAngle, this.m_TargetFlashLightAngle, TurnSpeed);
        this.m_FlashLight.transform.rotation = this.m_CurrentFlashLightAngle;
        
    }
    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {   if(view!=null)
            if(!view.IsMine)
            return;
        this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity, PlayerController.Acceleration);
        this.m_RigidBody.velocity = new Vector3(this.m_CurrentVelocity.x, this.m_RigidBody.velocity.y + this.m_CurrentVelocity.y, this.m_CurrentVelocity.z);
        this.m_RigidBody.AddForce(0, PlayerController.Gravity, 0);
        
        if (this.m_NextFireTime != 0 && this.m_NextFireTime < Time.time)
        {
            switch (this.m_CombatState)
            {
                case PlayerCombatState.ReloadEnd:
                case PlayerCombatState.Firing:
                    this.m_CombatState = PlayerCombatState.Idle;
                    this.m_NextFireTime = 0;
                    break;
                case PlayerCombatState.ReloadStart:
                    if (!this.m_CancelReload)
                    {
                        this.m_CombatState = PlayerCombatState.ReloadLoop;
                        this.m_NextFireTime = Time.time + PlayerController.ReloadLoopTime;
                        this.m_PlyAnims.ReloadAnimation(this.m_AmmoClip == 0);
                    }
                    else
                    {
                        this.m_CombatState = PlayerCombatState.ReloadEnd;
                        this.m_NextFireTime = Time.time + PlayerController.ReloadEndTime;
                        this.m_PlyAnims.EndReloadAnimation();
                        this.m_CancelReload = false;
                    }
                    break;
                case PlayerCombatState.ReloadLoop:
                    if (this.m_AmmoReserve > 0 && this.m_AmmoClip != ClipSize && !this.m_CancelReload)
                    {
                        this.m_NextFireTime = Time.time + PlayerController.ReloadLoopTime;
                        this.m_PlyAnims.ReloadAnimation(false);
                    }
                    else
                    {
                        this.m_CombatState = PlayerCombatState.ReloadEnd;
                        this.m_NextFireTime = Time.time + PlayerController.ReloadEndTime;
                        this.m_PlyAnims.EndReloadAnimation();
                        this.m_CancelReload = false;
                    }
                    break;
            }
            
        }
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
        Fire
        Called when the player presses a movement direction
        @param The input value
    ==============================*/

    void Move(InputAction.CallbackContext context) 
    { 
        Vector2 movedir = context.ReadValue<Vector2>();
        this.m_MovementDirection = movedir;
        this.m_MovementState = PlayerMovementState.Moving;
    }
    
    
    /*==============================
        Fire
        Called when the player presses left click
        @param The input value
    ==============================*/

    void Fire(InputAction.CallbackContext context) 
    {
        if (this.m_AimState == PlayerAimState.Aiming && this.m_CombatState == PlayerCombatState.Idle)
        {
            if (this.m_AmmoClip > 0)
            {
                this.m_Audio.Play("Shotgun/Fire", this.transform.gameObject);
                this.m_CombatState = PlayerCombatState.Firing;
                this.m_NextFireTime = Time.time + PlayerController.FireTime;
                this.m_AmmoClip--;
                this.m_PlyAnims.FireAnimation();
                this.m_CameraController.AddTrauma(0.5f);
            }
            else
                this.m_Audio.Play("Shotgun/DryFire", this.transform.gameObject);
        }
        else if (this.m_CombatState == PlayerCombatState.ReloadStart || this.m_CombatState == PlayerCombatState.ReloadLoop || this.m_CombatState == PlayerCombatState.ReloadEnd)
            this.m_CancelReload = true;
    }
    
    
    /*==============================
        Aim
        Called when the player presses right click
        @param The input value
    ==============================*/

    void Aim(InputAction.CallbackContext context) 
    {   if(view!=null)
            if(!view.IsMine)
            return;
        this.m_AimState = context.ReadValue<float>() > 0 ? PlayerAimState.Aiming : PlayerAimState.Idle;
    }
    
    
    /*==============================
        Reload
        Called when the player presses R
        @param The input value
    ==============================*/

    void Reload(InputAction.CallbackContext context) 
    {
        if(view!=null)
            if(!view.IsMine)
            return;
        if (this.m_CombatState == PlayerCombatState.Idle && this.m_AmmoClip < PlayerController.ClipSize && this.m_AmmoReserve > 0)
        {
            this.m_CombatState = PlayerCombatState.ReloadStart;
            this.m_NextFireTime = Time.time + PlayerController.ReloadStartTime;
            this.m_PlyAnims.StartReloadAnimation(this.m_AmmoClip == 0);
        }
    }
    
    
    /*******************************
                Getters
    *******************************/
    
    /*==============================
        GetPlayerMovementDirection
        Retrieves the value of the player movement direction
        @return The player's movement direction
    ==============================*/

    public Vector3 GetPlayerMovementDirection() 
    {
        return this.m_MovementDirection;
    }
    
    
    /*==============================
        GetPlayerMovementState
        Retrieves the value of the player movement state
        @return The player's movement state
    ==============================*/

    public PlayerMovementState GetPlayerMovementState() 
    {
        return this.m_MovementState;
    }
    
    
    /*==============================
        GetPlayerCombatState
        Retrieves the value of the player combat state
        @return The player's combat state
    ==============================*/

    public PlayerCombatState GetPlayerCombatState() 
    {
        return this.m_CombatState;
    }
    
    
    /*==============================
        GetPlayerVerticalAim
        Gets the player's vertical aim
        @return The player's vertical aim
    ==============================*/

    public float GetPlayerVerticalAim() 
    {
        return this.m_AimVerticalAngle;
    }
    
    
    /*==============================
        GetPlayerAimState
        Gets the player's aim state
        @return The player aim state
    ==============================*/

    public PlayerAimState GetPlayerAimState()
    {
        return this.m_AimState;
    }
    
    
    /*==============================
        GetPlayerAiming
        Checks whether the player is aiming or not
        @return The player aim state
    ==============================*/

    public bool GetPlayerAiming()
    {   
        return (this.m_AimState == PlayerAimState.Aiming && this.m_CombatState != PlayerCombatState.ReloadStart && this.m_CombatState != PlayerCombatState.ReloadLoop);
    }
    
    
    /*==============================
        GetPlayerAmmoClip
        Gets how much ammo is currently in the player's clip
        @return The player's clip ammo count
    ==============================*/

    public int GetPlayerAmmoClip() 
    {
        return this.m_AmmoClip;
    }
    
    
    /*==============================
        GetPlayerAmmoReserve
        Gets how much ammo is currently in the player's reserve
        @return The player's reserve ammo count
    ==============================*/

    public int GetPlayerAmmoReserve() 
    {
        return this.m_AmmoReserve;
    }
    
    
    /*******************************
                Setters
    *******************************/
    
    /*==============================
        SetPlayerAmmoClip
        Sets how much ammo is currently in the player's clip
        @param The amount of ammo to set the current clip
    ==============================*/

    public void SetPlayerAmmoClip(int amount) 
    {
        this.m_AmmoClip = amount;
    }
    
    
    /*==============================
        SetPlayerAmmoReserve
        Sets how much ammo is currently in the player's reserve
        @param The amount of ammo to set in reserve
    ==============================*/

    public void SetPlayerAmmoReserve(int amount) 
    {
        this.m_AmmoReserve = amount;
    }
}

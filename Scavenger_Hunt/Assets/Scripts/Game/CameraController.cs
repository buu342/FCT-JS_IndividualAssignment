/****************************************************************
                       CameraLogic.cs
    
This script handles camera movement and logic
****************************************************************/

using UnityEngine;

using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private const float Sensitivity    = 0.1f;
    public  const int   LookMax_Up     = 60;
    public  const int   LookMax_Down   = -60;
    private const float TraumaSpeed    = 25.0f;
    private const float MaxTraumaAngle = 10.0f;
    private const float AimSpeed       = 0.1f;
    private const float DefaultFOV     = 60.0f;
    private const float ZoomFOV        = 40.0f;
    
    public GameObject  m_Target;
    private GameObject actualTarget;
    public float m_DefaultFOV = CameraController.DefaultFOV;
    public float m_ZoomFOV = CameraController.ZoomFOV;
    
    private float            m_NoiseSeed;
    private Quaternion       m_CamRotation;
    private PlayerController m_PlyCont;
    private Vector2          m_LookDirection;
    private float            m_Trauma = 0.0f;
    private float            m_CurrentFOV;
    private float            m_TargetFOV;
    private bool freeMode;
    private UnityEvent freeModeEvent;
    
    /*==============================
        Start
        Called when the camera is initialized
    ==============================*/
    
    void Start()
    {
        this.m_CurrentFOV = this.m_DefaultFOV;
        this.m_TargetFOV = this.m_DefaultFOV;
        this.m_NoiseSeed = Random.value;
        this.m_CamRotation = this.transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
        #if UNITY_EDITOR
            Cursor.visible = false;
        #endif
    
        actualTarget = m_Target;

        if(freeModeEvent == null) {
            freeModeEvent = new UnityEvent();
        }
    }
    void OnEnable() { 
        InputManagerScript.playerInput.Player.FreeLook.started +=  FreeMoveOptionActivated;
        
        if(!InputManagerScript.playerInput.Player.enabled)
            InputManagerScript.playerInput.Player.Enable();
    }


    void OnDisable() {
        InputManagerScript.playerInput.Player.FreeLook.started -=  FreeMoveOptionActivated;

        if(InputManagerScript.playerInput.Player.enabled)
            InputManagerScript.playerInput.Player.Disable();
            freeModeEvent.RemoveAllListeners();
    }

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        m_LookDirection = InputManagerScript.Look.ReadValue<Vector2>();
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaoffsetp = CameraController.MaxTraumaAngle*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 1, Time.time*CameraController.TraumaSpeed)*2 - 1);
        float traumaoffsety = CameraController.MaxTraumaAngle*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 2, Time.time*CameraController.TraumaSpeed)*2 - 1);
        float traumaoffsetr = CameraController.MaxTraumaAngle*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 3, Time.time*CameraController.TraumaSpeed)*2 - 1);
        
        // Handle aiming FOV
        if (this.m_PlyCont != null)
        {
            this.m_TargetFOV = this.m_PlyCont.GetPlayerAiming() ? this.m_ZoomFOV : this.m_DefaultFOV;
            this.m_CurrentFOV = Mathf.Lerp(this.m_CurrentFOV, this.m_TargetFOV, AimSpeed);
            Camera.main.fieldOfView = this.m_CurrentFOV;
        }
        
        // Calculate the final camera position and rotation
        this.m_CamRotation.x += this.m_LookDirection.x*CameraController.Sensitivity;
        this.m_CamRotation.y -= this.m_LookDirection.y*CameraController.Sensitivity;
        this.m_CamRotation.y = Mathf.Clamp(this.m_CamRotation.y, CameraController.LookMax_Down, CameraController.LookMax_Up);
        this.transform.rotation = Quaternion.Euler(this.m_CamRotation.y, this.m_CamRotation.x, 0.0f);
        this.transform.rotation *= Quaternion.Euler(traumaoffsetp, traumaoffsety, traumaoffsetr);
        
        // Decrease screen shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime/2.0f);
    }
  

    /*==============================
        LateUpdate
        Called at the end of every frame
    ==============================*/
    
    void LateUpdate()
    {
        Vector3 finalpos = Vector3.zero;
        if (this.actualTarget != null) {
            finalpos += this.m_Target.transform.position;
            this.transform.position = finalpos;
        } else {
            this.transform.position += (InputManagerScript.Move.ReadValue<Vector2>().y*transform.forward) + (InputManagerScript.Move.ReadValue<Vector2>().x*transform.right);
        }
    
    }
    

    public void FreeMoveOptionActivated(InputAction.CallbackContext context) {
        freeMode = !freeMode;
        if(freeMode) {
            actualTarget = null;
        } else {
            actualTarget = m_Target;
        }

        Debug.Log("Activated free move");
    }

    /*==============================
        SetTarget
        Sets the camera's target
        @param The GameObject for the camera to follow
    ==============================*/
    
    public void SetTarget(GameObject target)
    {
        this.m_Target = target;
        target = target.transform.parent.gameObject;
        if (target != null)
            this.m_PlyCont = target.GetComponent<PlayerController>();
    }
    
    /*==============================
        AddTrauma
        Makes the camera shake
        @param The amount to shake by (from 0 to 1)
    ==============================*/
    
    public void AddTrauma(float amount)
    {
        this.m_Trauma = Mathf.Min(1.0f, this.m_Trauma + amount);
    }
    
    
    /*==============================
        SetLookDirection
        Set's the camera's look direction
        @param The look direction vector
    ==============================*/
    
    public void SetLookDirection(Vector2 lookdir)
    {
        this.m_LookDirection = lookdir;
    }

     /*==============================
        Add new listener to event
        Adds listener to the free move event
        @param the function to call
    ==============================*/
    
    public void AddListenerFreeMoveEvent(Events.UnityAction callback) 
    {
        freeModeEvent.AddListener(callback);
    }
    /*==============================
        Remove listener to event
        Removes listener to the free move event
        @param the function to call
    ==============================*/
    
    public void RemoveListenerFreeMoveEvent(Events.UnityAction callback) 
    {
        freeModeEvent.RemoveListener(callback);
    }

}
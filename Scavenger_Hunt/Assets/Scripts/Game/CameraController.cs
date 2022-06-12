/****************************************************************
                       CameraLogic.cs
    
This script handles camera movement and logic
****************************************************************/

using UnityEngine;

using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private const float Sensitivity    = 0.1f;
    private const int   LookMax_Up     = 60;
    private const int   LookMax_Down   = -60;
    private const float TraumaSpeed    = 25.0f;
    private const float MaxTraumaAngle = 10.0f;
    
    public GameObject  m_Target;
    private float      m_NoiseSeed;
    private Quaternion m_CamRotation;
    private Vector2    m_LookDirection;
    private float      m_Trauma = 0.0f;

    
    /*==============================
        Start
        Called when the camera is initialized
    ==============================*/
    
    void Start()
    {
        this.m_NoiseSeed = Random.value;
        this.m_CamRotation = this.transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
        #if UNITY_EDITOR
            Cursor.visible = false;
        #endif
    }
    void OnEnable() {
        if(!InputManagerScript.playerInput.Player.enabled)
            InputManagerScript.playerInput.Player.Enable();
    }


    void OnDisable() {
        if(InputManagerScript.playerInput.Player.enabled)
            InputManagerScript.playerInput.Player.Disable();
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
        
        // Calculate the final camera position and rotation
        this.m_CamRotation.x += this.m_LookDirection.x*Sensitivity;
        this.m_CamRotation.y -= this.m_LookDirection.y*Sensitivity;
        this.m_CamRotation.y = Mathf.Clamp(this.m_CamRotation.y, LookMax_Down, LookMax_Up);
        this.transform.rotation = Quaternion.Euler(this.m_CamRotation.y, this.m_CamRotation.x, 0.0f);
        this.transform.rotation *= Quaternion.Euler(traumaoffsetp, traumaoffsety, traumaoffsetr);
        
        // Decrease screen shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime);
    }
  

    /*==============================
        LateUpdate
        Called at the end of every frame
    ==============================*/
    
    void LateUpdate()
    {
        Vector3 finalpos = Vector3.zero;
        if (this.m_Target != null)
            finalpos += this.m_Target.transform.position;
        this.transform.position = finalpos;
    }
    

    /*==============================
        SetTarget
        Sets the camera's target
        @param The GameObject for the camera to follow
    ==============================*/
    
    public void SetTarget(GameObject target)
    {
        this.m_Target = target;
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
}
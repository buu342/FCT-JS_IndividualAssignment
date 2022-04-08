/****************************************************************
                       CameraLogic.cs
    
This script handles camera movement and logic
****************************************************************/

using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    // Constants
    private const float CameraHCorrectionSpeed = 0.05f;
    private const float CameraVCorrectionSpeed = 0.01f;
    private const float CameraPoISpeed = 0.01f;
    private const float TraumaSpeed     = 25.0f;
    private const float MaxTraumaAngle  = 10.0f;
    private const float MaxTraumaOffset = 1.0f;
    private float m_NoiseSeed;
    
    // Public values
    public GameObject m_Player;
    public Vector3 m_TargetPoI = Vector3.zero;
    public bool m_FollowPlayer = true;
    public float m_SkyboxRotateSpeed = 0.0f;
    
    // Private values
    private float m_CurrentSkyboxRotation = 0.0f;
    private Vector3 m_CurrentPlayerPos = Vector3.zero;
    private Vector3 m_TargetPlayerPos;
    private float m_Trauma = 0.0f;
    private Vector3 m_CurrentPoI; 
    private Quaternion m_OriginalRotation; 
    
    
    /*==============================
        Start
        Called when the camera is initialized
    ==============================*/
    
    void Start()
    {
        this.m_NoiseSeed = Random.value;
        if (this.m_FollowPlayer && this.m_Player != null)
            this.m_CurrentPlayerPos = this.m_Player.transform.position;
        this.m_TargetPlayerPos = this.m_CurrentPlayerPos;
        this.m_CurrentPoI = this.m_TargetPoI;
        this.m_OriginalRotation = this.transform.rotation;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaang = CameraLogic.MaxTraumaAngle*shake*(Mathf.PerlinNoise(this.m_NoiseSeed, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        float traumaoffsetx = CameraLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 1, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        float traumaoffsety = CameraLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 2, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        
        // Fake parallax by rotating the skybox
        this.m_CurrentSkyboxRotation += this.m_SkyboxRotateSpeed;
        RenderSettings.skybox.SetFloat("_Rotation", this.transform.position.x/5.0f + this.m_CurrentSkyboxRotation);
        
        // Smoothly move the camera to go to the player
        if (this.m_Player != null)
        {
            if (this.m_FollowPlayer)
                this.m_TargetPlayerPos = this.m_Player.transform.position;
            this.m_CurrentPlayerPos.x = Mathf.Lerp(this.m_CurrentPlayerPos.x, this.m_TargetPlayerPos.x, Time.deltaTime*200*CameraLogic.CameraHCorrectionSpeed);
            this.m_CurrentPlayerPos.y = Mathf.Lerp(this.m_CurrentPlayerPos.y, this.m_TargetPlayerPos.y, Time.deltaTime*200*(CameraLogic.CameraVCorrectionSpeed + Mathf.Max(0.0f, -this.m_Player.GetComponent<Rigidbody>().velocity.y/1000)));
        }
        
        // Smoothly move the camera to go to our PoI
        this.m_CurrentPoI = Vector3.Lerp(this.m_CurrentPoI, this.m_TargetPoI, CameraLogic.CameraPoISpeed);
        
        // Calculate the camera position
        this.transform.localPosition = new Vector3(this.m_CurrentPlayerPos.x, this.m_CurrentPlayerPos.y, 0);
        this.transform.localPosition += this.m_CurrentPoI;
        this.transform.localPosition += new Vector3(traumaoffsetx, traumaoffsety, 0);
        this.transform.localRotation = this.m_OriginalRotation;
        this.transform.localRotation *= Quaternion.Euler(0, 0, traumaang);
        
        // Decrease screen shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime);
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
        SetFollowPlayer
        Changes whether the camera should follow the player
        @param Whether to follow the player or not
        @param The target position to focus on instead
    ==============================*/
    
    public void SetFollowPlayer(bool follow, Vector3 target = default(Vector3))
    {
        this.m_FollowPlayer = follow;
        if (!follow)
            this.m_TargetPlayerPos = target;
    }
    

    /*==============================
        SetPoI
        Changes the camera's PoI
        @param The PoI vector
    ==============================*/
    
    public void SetPoI(Vector3 poi)
    {
        this.m_TargetPoI = poi;
    }
    

    /*==============================
        GetFollowPlayer
        Checks whether the camera is following the player
        @returns Whether we're following the player
    ==============================*/
    
    public bool GetFollowPlayer()
    {
        return this.m_FollowPlayer;
    }
    

    /*==============================
        UpdatePlayerPosition
        Updates the camera to lock onto the player position
    ==============================*/
    
    public void UpdatePlayerPosition()
    {
        if (this.m_Player == null)
            return;
        this.m_CurrentPlayerPos = this.m_Player.transform.position;
        this.m_TargetPlayerPos = this.m_CurrentPlayerPos;
    }
    

    /*==============================
        SetPlayer
        TODO
    ==============================*/
    
    public void SetPlayer(GameObject player)
    {
        this.m_Player = player;
    }
}
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    private const float CameraHCorrectionSpeed = 0.05f;
    private const float CameraVCorrectionSpeed = 0.01f;
    private const float TraumaSpeed     = 25.0f;
    private const float MaxTraumaAngle  = 10.0f;
    private const float MaxTraumaOffset = 1.0f;
    private float m_NoiseSeed;
    
    public GameObject m_Player;
    public Vector3 m_PoI = Vector3.zero;
    public bool m_FollowPlayer = true;
    private Vector3 m_CurrentPlayerPos;
    private Vector3 m_TargetPlayerPos;
    private float m_Trauma = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        this.m_NoiseSeed = Random.value;
        this.m_CurrentPlayerPos = this.m_Player.transform.position;
        this.m_TargetPlayerPos = this.m_Player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaang = CameraLogic.MaxTraumaAngle*shake*(Mathf.PerlinNoise(this.m_NoiseSeed, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        float traumaoffsetx = CameraLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 1, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        float traumaoffsety = CameraLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 2, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        
        // Smoothly move the camera to go to the player
        if (this.m_FollowPlayer)
        {
            this.m_TargetPlayerPos = this.m_Player.transform.position;
            this.m_CurrentPlayerPos.x = Mathf.Lerp(this.m_CurrentPlayerPos.x, this.m_TargetPlayerPos.x, Time.deltaTime*200*CameraLogic.CameraHCorrectionSpeed);
            this.m_CurrentPlayerPos.y = Mathf.Lerp(this.m_CurrentPlayerPos.y, this.m_TargetPlayerPos.y, Time.deltaTime*200*(CameraLogic.CameraVCorrectionSpeed + Mathf.Max(0.0f, -this.m_Player.GetComponent<Rigidbody>().velocity.y/1000)));
        }
        
        // Calculate the camera position
        this.transform.localPosition = new Vector3(this.m_CurrentPlayerPos.x, this.m_CurrentPlayerPos.y, this.transform.position.z);
        this.transform.localPosition += new Vector3(this.m_PoI.x, this.m_PoI.y, 0);
        this.transform.localPosition += new Vector3(traumaoffsetx, traumaoffsety, 0);
        this.transform.localRotation = Quaternion.identity;
        this.transform.localRotation *= Quaternion.Euler(0, 0, traumaang);
        
        // Decrease screen shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime);
    }
    
    public void AddTrauma(float amount)
    {
        this.m_Trauma = Mathf.Min(1.0f, this.m_Trauma + amount);
    }
}

using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    private const float TraumaSpeed     = 25.0f;
    private const float MaxTraumaAngle  = 10.0f;
    private const float MaxTraumaOffset = 1.0f;
    private float m_NoiseSeed;
    
    private float m_Trauma = 0.0f;
    public GameObject m_Player;
    public Vector3 m_PoI;
    
    // Start is called before the first frame update
    void Start()
    {
        this.m_NoiseSeed = Random.value;
    }

    // Update is called once per frame
    void Update()
    {
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaang = CameraLogic.MaxTraumaAngle*shake*(Mathf.PerlinNoise(this.m_NoiseSeed, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        float traumaoffsetx = CameraLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 1, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        float traumaoffsety = CameraLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 2, Time.time*CameraLogic.TraumaSpeed)*2 - 1);
        
        // Calculate the camera position
        this.transform.localPosition = new Vector3(this.m_Player.transform.position.x, this.m_Player.transform.position.y, this.transform.position.z);
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

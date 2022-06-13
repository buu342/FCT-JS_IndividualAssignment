/****************************************************************
                       CameraLogic.cs
    
This script handles camera collision with walls
****************************************************************/

using UnityEngine;

public class CameraPhysics : MonoBehaviour
{
    private const float MinZoom = 0.75f;
    private const float MaxZoom = 2.75f;
    private const float LerpSpeed = 10.0f;
    
    private Vector3 m_CamDir;
    private float   m_CamZoom;
    
    
    /*==============================
        Awake
        Called when the camera is created
    ==============================*/
    
    void Awake()
    {
        this.m_CamDir = this.transform.localPosition.normalized;
        this.m_CamZoom = this.transform.localPosition.magnitude;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        RaycastHit hit;
        Vector3 targetpos = this.transform.parent.TransformPoint(this.m_CamDir*CameraPhysics.MaxZoom);
        
        if (Physics.Linecast(this.transform.parent.position, targetpos, out hit) && !hit.collider.isTrigger)
            this.m_CamZoom = Mathf.Clamp((hit.distance*0.75f), CameraPhysics.MinZoom, CameraPhysics.MaxZoom);
        else
            this.m_CamZoom = CameraPhysics.MaxZoom;
        
        this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.m_CamDir*this.m_CamZoom, Time.unscaledDeltaTime*CameraPhysics.LerpSpeed);
    }
}
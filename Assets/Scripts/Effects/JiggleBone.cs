/****************************************************************
                          JiggleBone.cs
    
This script handles a very basic jigglebone effect
****************************************************************/

using UnityEngine;
 
public class JiggleBone : MonoBehaviour
{
    // Physics constants
    public float m_BounceFactor = 20;
    public float m_WobbleFactor = 10;
 
    // Rotation constants
    public float m_MaxRotation = 5;
 
    // Private values
    private Quaternion m_OldBoneRotation;
    private Quaternion m_CurrentBoneRotation;
    private Quaternion m_TargetBoneRotation;
    
    
    /*==============================
        Awake
        Called before the bone is initialized
    ==============================*/
 
    void Awake()
    {
        this.m_OldBoneRotation = this.transform.rotation;
    }
    
    
    /*==============================
        LateUpdate
        Called after all updates have finished
    ==============================*/
 
    void LateUpdate()
    {
        JiggleBonesUpdate();
    }
    
    
    /*==============================
        JiggleBonesUpdate
        Handles the jigglebone
    ==============================*/
 
    void JiggleBonesUpdate()
    {
        this.m_CurrentBoneRotation = this.transform.rotation;
        
        // Interpolate to our target position
        this.m_TargetBoneRotation = Quaternion.Slerp(this.m_OldBoneRotation, this.transform.rotation, Time.deltaTime*m_WobbleFactor);
        this.transform.rotation = Quaternion.RotateTowards(this.m_CurrentBoneRotation, this.m_TargetBoneRotation, this.m_MaxRotation);
        
        // Update the old rotation value
        this.m_OldBoneRotation = this.transform.rotation;
    }
}
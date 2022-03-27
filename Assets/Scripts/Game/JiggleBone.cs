using UnityEngine;
 
public class JiggleBone : MonoBehaviour {
 
    public float bounceFactor = 20;
    public float wobbleFactor = 10;
 
    public float maxRotationDegrees = 5;
 
    private Quaternion oldBoneWorldRotation;
    private Quaternion animatedBoneWorldRotation;
    private Quaternion goalRotation;
 
    void Awake()
    {
        oldBoneWorldRotation = transform.rotation;
    }
 
    void LateUpdate()
    {
        JiggleBonesUpdate();
    }
 
    void JiggleBonesUpdate()
    {
        animatedBoneWorldRotation = transform.rotation;
        goalRotation = Quaternion.Slerp(oldBoneWorldRotation, transform.rotation, Time.deltaTime * wobbleFactor);
        transform.rotation = Quaternion.RotateTowards(animatedBoneWorldRotation, goalRotation, maxRotationDegrees);
        oldBoneWorldRotation = transform.rotation;
    }
}
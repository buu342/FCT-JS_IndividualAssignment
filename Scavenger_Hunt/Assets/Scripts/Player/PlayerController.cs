/****************************************************************
                       PlayerController.cs
    
This script handles all of the player movement physics.
****************************************************************/

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public  const float Gravity      = -80.0f;
    private const float MoveSpeed    = 10.0f;
    private const float Acceleration = 0.5f;
    private const float TurnSpeed    = 0.1f;

    
    public GameObject m_Camera;
    public Rigidbody  m_RigidBody;
    
    private Vector3 m_CurrentVelocity = Vector3.zero;
    private Vector3 m_TargetVelocity = Vector3.zero;
    
    
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
        this.m_TargetVelocity = Vector3.zero;
        
        // Handle movement input
        if (Input.GetButton("Backward"))
            this.m_TargetVelocity = -this.transform.forward*PlayerController.MoveSpeed;
        if (Input.GetButton("Left"))
            this.m_TargetVelocity = -this.transform.right*PlayerController.MoveSpeed;
        if (Input.GetButton("Right"))
            this.m_TargetVelocity = this.transform.right*PlayerController.MoveSpeed;
        if (Input.GetButton("Forward"))
            this.m_TargetVelocity = this.transform.forward*PlayerController.MoveSpeed;
        
        // Turn the player to face the same direction as the camera
        if (this.m_TargetVelocity != Vector3.zero)
        {
            Quaternion targetang = Quaternion.Euler(0.0f, this.m_Camera.transform.eulerAngles.y, 0.0f); 
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetang, PlayerController.TurnSpeed);
        }
    }
    
    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        this.m_CurrentVelocity = Vector3.Lerp(this.m_CurrentVelocity, this.m_TargetVelocity, PlayerController.Acceleration);
        this.m_RigidBody.velocity = new Vector3(this.m_CurrentVelocity.x, this.m_CurrentVelocity.y, this.m_CurrentVelocity.z);
        this.m_RigidBody.AddForce(0, PlayerController.Gravity, 0);
    }
}

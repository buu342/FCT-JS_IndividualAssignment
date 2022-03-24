/****************************************************************
                          EnemyLogic.cs
    
This script handles base enemy logic.
****************************************************************/

using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    private const float DepthPerception = 15.0f;
    private const float BlasterFireRate = 0.5f;
    
    // Health
    public int m_Health = 10;
    private float m_Tremor;
    
    // Aim
    private Vector3 m_OriginalAimPos;
    private Quaternion m_OriginalAimAng;
    private float m_NextFire = 0;
    
    // Components
    public  GameObject m_bulletprefab; 
    private GameObject m_target;
    private GameObject m_fireattachment;
    private GameObject m_shoulder;
    private AudioManager m_audio; 
    
    
    /*==============================
        Start
        Called when the enemy is initialized
    ==============================*/
    
    void Start()
    {
        this.m_target = GameObject.Find("Player");
        this.m_audio = FindObjectOfType<AudioManager>();
        this.m_shoulder = this.transform.Find("Shoulder").gameObject;
        this.m_fireattachment = this.transform.Find("FireAttachment").gameObject;
        this.m_OriginalAimPos = this.m_fireattachment.transform.localPosition;
        this.m_OriginalAimAng = this.m_fireattachment.transform.localRotation;
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        HandleTargeting();
        
        if (this.m_Health <= 0)
            Destroy(this.gameObject);
    }
    
    public void TakeDamage(int amount)
    {
        //this.m_Health -= amount;
    }
    
    private void HandleTargeting()
    {
        Vector3 targetpos = this.m_target.transform.Find("Shoulder").gameObject.transform.position;
        
        // If the player is within shooting distance
        if (Vector3.Distance(targetpos, this.m_fireattachment.transform.position) < EnemyLogic.DepthPerception)
        {
            // Calculate the direction to face the player
            Vector3 direction = this.m_fireattachment.transform.position - targetpos;
            direction.Normalize();
            
            // Rotate the firing attachment to point at the player
            this.m_fireattachment.transform.localPosition = this.m_OriginalAimPos;
            this.m_fireattachment.transform.localRotation = this.m_OriginalAimAng;
            this.m_fireattachment.transform.RotateAround(this.m_shoulder.transform.position, Vector3.forward, Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg);
            
            // Fire bullets
            if (this.m_NextFire < Time.time)
            {
                // Create the bullet object
                ProjectileLogic bullet = Instantiate(this.m_bulletprefab, this.m_fireattachment.transform.position, this.m_fireattachment.transform.rotation).GetComponent<ProjectileLogic>();
                bullet.SetOwner(this.gameObject);
                bullet.SetSpeed(15.0f);
                
                // Play the shooting sound and set the next fire time
                this.m_audio.Play("Weapons/Laser_Fire");
                this.m_NextFire = Time.time + EnemyLogic.BlasterFireRate;
            }
        }
    }
}

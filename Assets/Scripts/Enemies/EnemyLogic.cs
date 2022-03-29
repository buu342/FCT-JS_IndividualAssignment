/****************************************************************
                          EnemyLogic.cs
    
This script handles base enemy logic.
****************************************************************/

using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    // Constants
    private const float DepthPerception = 18.7f;
    private const float BlasterFireRate = 0.5f;
    private const float TraumaSpeed     = 25.0f;
    private const float MaxTraumaOffset = 2.0f;
    private float m_NoiseSeed;
    
    // Enemy attack styles
    public enum AttackStyle
    {
        Aiming,
        Straight,
    }
    
    // Health
    public int m_Health = 10;
    private float m_Trauma = 0.0f;
    
    // Aim
    public AttackStyle m_AttackStyle = AttackStyle.Aiming;
    public GameObject m_Ragdoll;
    private Vector3 m_DamagePos;
    private Vector3 m_OriginalMeshPos;
    private Vector3 m_OriginalAimPos;
    private Quaternion m_OriginalAimAng;
    private float m_NextFire = 0;
    
    // Components
    public  GameObject m_bulletprefab;
    private GameObject m_mesh;
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
        this.m_NoiseSeed = Random.value;
        this.m_target = GameObject.Find("Player");
        this.m_audio = FindObjectOfType<AudioManager>();
        this.m_mesh = this.transform.Find("Model").gameObject;
        this.m_shoulder = this.transform.Find("Shoulder").gameObject;
        this.m_fireattachment = this.transform.Find("FireAttachment").gameObject;
        this.m_OriginalAimPos = this.m_fireattachment.transform.localPosition;
        this.m_OriginalAimAng = this.m_fireattachment.transform.localRotation;
        this.m_OriginalMeshPos = this.m_mesh.transform.localPosition;
        this.m_DamagePos = this.transform.position;
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        HandleTargeting();
        
        // Calculate shake when hurt
        float shake = this.m_Trauma*this.m_Trauma;
        float traumaoffsetx = EnemyLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed, Time.time*EnemyLogic.TraumaSpeed)*2 - 1);
        float traumaoffsety = EnemyLogic.MaxTraumaOffset*shake*(Mathf.PerlinNoise(this.m_NoiseSeed + 1, Time.time*EnemyLogic.TraumaSpeed)*2 - 1);
        
        // Calculate the shake position
        this.m_mesh.transform.localPosition = this.m_OriginalMeshPos;
        this.m_mesh.transform.localPosition += new Vector3(traumaoffsetx, traumaoffsety, 0);
        
        // Decrease shake over time
        this.m_Trauma = Mathf.Clamp01(this.m_Trauma - Time.deltaTime);
        
        // If we ran out of health, then commit sudoku
        if (this.m_Health <= 0)
            Die();
    }


    /*==============================
        TakeDamage
        Makes the enemy take damage
        @param The amount of damage to take
        @param The coordinate where the damage came from
    ==============================*/
    
    public void TakeDamage(int amount, Vector3 position)
    {
        this.m_Health -= amount;
        this.m_Trauma = Mathf.Min(0.5f, this.m_Trauma + ((float)amount)/30.0f);
        this.m_DamagePos = position;
    }


    /*==============================
        HandleTargeting
        Handles the enemy targeting
    ==============================*/
    
    private void HandleTargeting()
    {
        Vector3 targetpos = this.m_target.transform.Find("Shoulder").gameObject.transform.position;
        
        // Attack based on the style
        switch (this.m_AttackStyle)
        {
            case AttackStyle.Aiming:
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
                    
                    // Fire the bullet
                    FireBullet();
                }
                break;
            case AttackStyle.Straight:
                // If the player is within shooting distance
                if (Vector3.Distance(targetpos, this.m_fireattachment.transform.position) < EnemyLogic.DepthPerception*2)
                    FireBullet();
                break;
        }
    }


    /*==============================
        FireBullet
        Makes the enemy fire a bullet
    ==============================*/
    
    private void FireBullet()
    {
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


    /*==============================
        Die
        Turns the enemy into a ragdoll
    ==============================*/
    
    private void Die()
    {
        // Spawn a ragdoll prefab
        GameObject ragdoll = Instantiate(this.m_Ragdoll, this.transform.position, this.transform.rotation);
        
        // Apply physics to the bones based on where the damage came from
        Collider[] colliders = Physics.OverlapSphere(this.m_DamagePos, 10);
        foreach (Collider hit in colliders)
            if (hit.GetComponent<Rigidbody>())
                hit.GetComponent<Rigidbody>().AddExplosionForce(500, this.m_DamagePos, 10, 0);
            
        // Destroy this object
        Destroy(this.gameObject);
    }
}

/****************************************************************
                       ProjectileLogic.cs
    
This script handles projectile logic
****************************************************************/

//#define DEBUG

using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    // Constants
    private const float MaxPlayerAngleDifference = 110.0f;
    
    // Public values
    public GameObject m_Owner = null;
    public float m_Speed = 0;
    public float m_Damage = 10;
    
    // Private values
    private Vector3 m_PrevPosition;
    private int IgnoreLayers = 0;
    
    
    /*==============================
        Start
        Called when the projectile is initialized
    ==============================*/
    
    void Start()
    {
        this.GetComponent<Rigidbody>().velocity = this.transform.forward*m_Speed;
        this.m_PrevPosition = this.transform.position;
        
        // Disable collisions between self and the projectile's owner
        if (this.m_Owner != null)
            Physics.IgnoreCollision(this.m_Owner.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
        IgnoreLayers |= 1 << LayerMask.NameToLayer("NoCollide");
        IgnoreLayers |= 1 << LayerMask.NameToLayer("Bullet");
        IgnoreLayers |= 1 << LayerMask.NameToLayer("PlayerTrigger");
    }
    

    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        Vector3 raydir = this.m_PrevPosition - this.transform.position;
        
        // Because projectiles move quickly, raycast from our previous position to check we hit something
        #if DEBUG
            Debug.DrawRay(this.transform.position, raydir.normalized*raydir.magnitude, Color.red, Time.deltaTime, false);
        #endif
        RaycastHit[] hitstuff = Physics.RaycastAll(this.transform.position, raydir.normalized, raydir.magnitude);
        foreach (RaycastHit hit in hitstuff)
            if ((hit.collider.gameObject.layer & IgnoreLayers) > 0)
                OnTriggerEnter(hit.collider);
            
        // Update our previous position
        this.m_PrevPosition = this.transform.position;
    }


    /*==============================
        GetOwner
        Retrieves the projectile's owner
        @returns The projectile's owner
    ==============================*/
    
    public GameObject GetOwner()
    {
        return this.m_Owner;
    }


    /*==============================
        SetOwner
        Sets the projectile's owner
        @param The gameobject to set as the owner
    ==============================*/
    
    public void SetOwner(GameObject owner)
    {
        // Collide with our previous owner
        if (this.m_Owner != null)
            Physics.IgnoreCollision(this.m_Owner.GetComponent<Collider>(), this.GetComponent<Collider>(), false);
        
        // Set the owner, and don't collide with him anymore
        this.m_Owner = owner;
        Physics.IgnoreCollision(this.m_Owner.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
    }


    /*==============================
        SetOrigin
        Sets the bullet origin (to prevent shooting through walls)
        @param The origin vector
    ==============================*/
    
    public void SetOrigin(Vector3 origin)
    {
        this.m_PrevPosition = origin;
    }


    /*==============================
        SetSpeed
        Sets the projectile's speed
        @param The new speed value
    ==============================*/
    
    public void SetSpeed(float speed)
    {
        this.m_Speed = speed;
    }


    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Sword":
                SwordLogic sword = other.gameObject.GetComponent<SwordLogic>();
                
                // If the owner of this bullet is different from the sword's, then reflect the projectile
                if (sword.GetOwner() != this.GetOwner())
                {
                    this.SetOwner(sword.GetOwner());
                    this.transform.rotation = other.gameObject.transform.rotation;
                    this.GetComponent<Rigidbody>().velocity = this.transform.forward*m_Speed*2;
                }
                return;
            case "Player":
                // If we hit our owner, don't bother checking anything else
                if (this.GetOwner() == null || this.GetOwner() == other.gameObject)
                    return;
                
                // Check if the player was using a melee attack when we hit
                PlayerCombat ply = other.gameObject.GetComponent<PlayerCombat>();
                if (ply.GetCombatState() == PlayerCombat.CombatState.Melee)
                {
                    GameObject plyfireattach = ply.GetFireAttachment();
                    float angledif = Vector3.Angle(this.transform.forward, plyfireattach.transform.forward);
                    
                    // And the player was facing towards the bullet, then reflect the projectile
                    if (angledif > ProjectileLogic.MaxPlayerAngleDifference)
                    {
                        this.SetOwner(other.gameObject);
                        this.transform.rotation = ply.GetFireAttachment().transform.rotation;
                        this.GetComponent<Rigidbody>().velocity = this.transform.forward*m_Speed*2;
                        return;
                    }
                }
                break;
            case "Enemies":
                // Ignore enemies if our owner is an enemy
                if (this.GetOwner() == null || this.GetOwner().tag == "Enemies")
                    return;
                EnemyLogic enemy = other.gameObject.GetComponent<EnemyLogic>();
                enemy.TakeDamage((int)this.m_Damage, this.m_PrevPosition);
                break;
            case "BreakableProp":
                if (other.gameObject.GetComponent<BreakGlass>().Break(this.m_Speed, this.transform.position))
                    return;
                break;
            case "Bullet":
            case "NoCollide":
                return;
            default:
                break;
        }
        Destroy(this.gameObject);
    }


    /*==============================
        OnBecameInvisible
        Handles the projectile no longer being visible on camera
    ==============================*/
    
    private void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }
}
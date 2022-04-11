/****************************************************************
                       ProjectileLogic.cs
    
This script handles projectile logic
****************************************************************/

//#define DEBUG

using UnityEngine;
using System.Collections;

public class ProjectileLogic : MonoBehaviour
{
    // Constants
    private const int KillScore = 30;
    private const int ReflectScore = 10;
    private const float DestroyTime = 3.0f;
    private const float MaxPlayerAngleDifference = 110.0f;
    
    // Public values
    public GameObject m_Owner = null;
    public float m_Speed = 0.0f;
    public float m_Damage = 10.0f;
    public GameObject m_AlienParticle;
    public GameObject m_ExplodeParticle;
    public GameObject m_Mesh;
    public GameObject m_SuperMesh;
    
    // Private values
    private bool m_Penetrating = false;
    private float m_DestroyTime = 0.0f;
    private Vector3 m_PrevPosition;
    private int IgnoreLayers = 0;
    private Rigidbody m_rb;
    
    
    /*==============================
        Awake
        Called before the projectile is initialized
    ==============================*/
    
    void Awake()
    {
        this.m_rb = this.GetComponent<Rigidbody>();
        this.m_PrevPosition = this.transform.position;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        // Make the bullet spin if the player owns it
        if (this.m_Owner != null && this.m_Owner.tag == "Player")
        {
            this.m_Mesh.transform.localRotation *= Quaternion.Euler(0, 0, 5*Time.timeScale);
            this.m_SuperMesh.transform.localRotation *= Quaternion.Euler(0, 0, 5*Time.timeScale);
        }
    }
    

    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        Vector3 raydir = this.m_PrevPosition - this.transform.position;
        
        // Destroy ourselves if we're off camera for too long
        if (this.m_DestroyTime != 0 && this.m_DestroyTime < Time.unscaledTime)
            Destroy(this.gameObject);
        
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
        GetDamage
        Retrieves the projectile's damage
        @returns The projectile's damage
    ==============================*/
    
    public float GetDamage()
    {
        return this.m_Damage;
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
        
        // Disable collisions between self and the projectile's owner
        if (this.m_Owner != null)
            Physics.IgnoreCollision(this.m_Owner.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
        IgnoreLayers |= 1 << LayerMask.NameToLayer("NoCollide");
        IgnoreLayers |= 1 << LayerMask.NameToLayer("Bullet");
        IgnoreLayers |= 1 << LayerMask.NameToLayer("PlayerTrigger");
        
        // Set the bullet model
        switch (owner.tag)
        {
            case "Player":
                this.m_Mesh.transform.localRotation *= Quaternion.Euler(0, 0, Random.Range(0, 360));
                this.m_Mesh.SetActive(true);
                this.m_AlienParticle.SetActive(false);
                break;
            case "Boss":
            case "Enemies":
                this.m_Mesh.SetActive(false);
                this.m_AlienParticle.SetActive(true);
                ParticleSystem.MainModule mm = this.m_AlienParticle.GetComponent<ParticleSystem>().main;
                mm.startRotation = Vector3.SignedAngle(this.m_rb.velocity, Vector3.left, Vector3.forward)*Mathf.Deg2Rad;
                this.m_AlienParticle.GetComponent<ParticleSystem>().TriggerSubEmitter(0);
                break;
        }
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
        this.m_rb.velocity = this.transform.forward*speed;
    }


    /*==============================
        SetDamage
        Sets the projectile damage
        @param The new damage value
    ==============================*/
    
    public void SetDamage(float damage)
    {
        this.m_Damage = damage;
    }


    /*==============================
        SetPenetrating
        Enables/Disables projectile penetration
        @param The new penetration value
    ==============================*/
    
    public void SetPenetrating(bool enable)
    {
        this.m_Penetrating = enable;
        if (enable)
        {
            this.m_Mesh.SetActive(false);
            this.m_SuperMesh.SetActive(true);
        }
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
                if (sword.GetOwner() != this.m_Owner)
                {
                    this.SetOwner(sword.GetOwner());
                    this.transform.rotation = other.gameObject.transform.rotation;
                    this.m_rb.velocity = this.transform.forward*m_Speed*2;
                    FindObjectOfType<AudioManager>().Play("Weapons/Bullet_Reflect", this.transform.position);
                    if (sword.GetOwner().tag == "Player")
                        sword.GetOwner().gameObject.GetComponent<PlayerCombat>().GiveScore(ReflectScore);
                }
                return;
            case "Player":
            
                // If we hit our owner, don't bother checking anything else
                if (this.m_Owner == other.gameObject)
                    return;
                
                // Check if the player was using a melee attack when we hit, if not then take damage
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
                        this.m_rb.velocity = this.transform.forward*m_Speed*2;
                        return;
                    }
                }

                // Take damage otherwise
                ply.TakeDamage((int)this.m_Damage, this.m_PrevPosition);
                break;
            case "Enemies":
            
                // Ignore enemies if our owner is an enemy
                if (this.m_Owner != null && this.m_Owner.tag == "Enemies")
                    return;
                EnemyLogic enemy = other.gameObject.GetComponent<EnemyLogic>();
                enemy.TakeDamage((int)this.m_Damage, this.m_PrevPosition, this.m_Owner);
                if (this.m_Owner != null && this.m_Owner.tag == "Player")
                    this.m_Owner.gameObject.GetComponent<PlayerCombat>().GiveScore(KillScore);
                if (this.m_Penetrating)
                {
                    Physics.IgnoreCollision(other.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
                    return;
                }
                break;
            case "Boss":
            
                // Ignore bosses if our owner is the boss
                if (this.m_Owner != null && this.m_Owner.tag == "Boss")
                    return;
                BossLogic boss = other.gameObject.transform.root.GetComponent<BossLogic>();
                boss.TakeDamage((int)this.m_Damage);
                if (this.m_Owner != null && this.m_Owner.tag == "Player")
                    this.m_Owner.gameObject.GetComponent<PlayerCombat>().GiveScore(KillScore/3);
                if (this.m_Penetrating)
                {
                    Physics.IgnoreCollision(other.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
                    return;
                }
                break;
            case "BreakableProp":
                if (other.gameObject.GetComponent<BreakGlass>().Break(this.m_Speed, this.m_PrevPosition))
                    return;
                break;
            case "Bullet":
            case "EntityClip":
            case "NoCollide":
                return;
            default:
                break;
        }
        if (this.m_Owner.tag == "Enemies" || this.m_Owner.tag == "Boss")
            Instantiate(this.m_ExplodeParticle, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }


    /*==============================
        OnBecameInvisible
        Handles the projectile no longer being visible on camera
    ==============================*/
    
    private void OnBecameInvisible()
    {
        if (this.m_Owner != null && this.m_Owner.tag == "Player")
        {
            Destroy(this.gameObject);
            return;
        }
        this.m_DestroyTime = Time.unscaledTime + ProjectileLogic.DestroyTime;
    }


    /*==============================
        OnBecameVisible
        Handles the projectile suddenly being visible on camera
    ==============================*/
    
    private void OnBecameVisible()
    {
        this.m_DestroyTime = 0.0f;
    }
}
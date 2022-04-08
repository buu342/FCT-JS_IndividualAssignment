/****************************************************************
                          SwordLogic.cs
    
This script handles the sword hitbox logic.
****************************************************************/

using UnityEngine;

public class SwordLogic : MonoBehaviour
{
    // Constants
    private const int KillScore = 50;
    private const float LifeTime = 0.3f;
    
    // Settings
    private int m_Damage = 20;
    private float m_DeathTime;
    
    // Components
    private GameObject m_Owner = null;
    private Vector3 m_OwnerOffset = Vector3.zero;
    
    
    /*==============================
        Start
        Called when the sword hitbox is initialized
    ==============================*/
    
    void Start()
    {
        this.m_DeathTime = Time.time + SwordLogic.LifeTime;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/

    void Update()
    {
        // Move to always be attached to our owner
        if (this.m_Owner != null)
            this.transform.localPosition = this.m_Owner.transform.position + this.m_OwnerOffset;
        
        // Remove this object once its lifetime has expired
        if (this.m_DeathTime < Time.time)
            Destroy(this.gameObject);
    }


    /*==============================
        GetOwner
        Retrieves the sword hitbox's owner
        @returns The sword hitbox's owner
    ==============================*/
    
    public GameObject GetOwner()
    {
        return this.m_Owner;
    }


    /*==============================
        SetOwner
        Sets the sword hitbox's owner
        @param The gameobject to set as the owner
    ==============================*/
    
    public void SetOwner(GameObject owner)
    {
        this.m_Owner = owner;
        this.m_OwnerOffset = this.transform.position - owner.transform.position;
    }


    /*==============================
        SetDamage
        Sets the sword hitbox's damage
        @param The amount of damage this hitbox will inflict
    ==============================*/
    
    public void SetDamage(int damage)
    {
        this.m_Damage = damage;
    }
    

    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Enemies":
                // Ignore enemies if our owner is an enemy
                if (this.GetOwner().tag == "Enemies")
                    return;
                EnemyLogic enemy = other.gameObject.GetComponent<EnemyLogic>();
                enemy.TakeDamage((int)this.m_Damage, this.transform.position, this.m_Owner);
                if (this.m_Owner.tag == "Player")
                    this.m_Owner.gameObject.GetComponent<PlayerCombat>().GiveScore(KillScore);
                break;
            case "Boss":
                // Ignore bosses if our owner is a boss
                if (this.GetOwner().tag == "Boss")
                    return;
                BossLogic boss = other.gameObject.transform.root.GetComponent<BossLogic>();
                boss.TakeDamage((int)this.m_Damage);
                if (this.m_Owner.tag == "Player")
                    this.m_Owner.gameObject.GetComponent<PlayerCombat>().GiveScore(KillScore);
                Physics.IgnoreCollision(boss.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
                break;
        }
    }
}
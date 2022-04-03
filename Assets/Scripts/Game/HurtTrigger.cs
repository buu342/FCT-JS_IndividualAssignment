/****************************************************************
                         HurtTrigger.cs
    
This script handles a hurt trigger
****************************************************************/


using UnityEngine;

public class HurtTrigger : MonoBehaviour
{
    public int m_Damage = 10;
    public bool m_RemoveOnTrigger = false;
    public GameObject m_Owner = null;
    public float m_DieTime = 0.0f;
    
    private bool m_Remove = false;
    

    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        if (this.m_Remove || (this.m_DieTime != 0 && this.m_DieTime < Time.time))
            Destroy(this.gameObject);
    }
    
    
    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == this.m_Owner)
            return;
        
        // Damage what touched us
        switch (other.tag)
        {
            case "Player":
                PlayerCombat ply = other.gameObject.GetComponent<PlayerCombat>();
                ply.TakeDamage(this.m_Damage, this.transform.position);
                break;
            case "Enemies":
                EnemyLogic enemy = other.gameObject.GetComponent<EnemyLogic>();
                enemy.TakeDamage(this.m_Damage,  this.transform.position);
                break;
            case "Boss":
                BossLogic boss = other.gameObject.transform.root.GetComponent<BossLogic>();
                boss.TakeDamage((int)this.m_Damage);
                break;
            default:
                return;
        }
        
        // If set, remove ourselves next game tick (doing it next tick so we can collide with other stuff)
        if (this.m_RemoveOnTrigger)
            this.m_Remove = true;
    }
    

    /*==============================
        SetDamage
        Sets the trigger's damage
        @param The damage amount
    ==============================*/
    
    public void SetDamage(int damage)
    {
        this.m_Damage = damage;
    }
    

    /*==============================
        SetOwner
        Sets the trigger's owner
        @param The gameobject to set as the owner
    ==============================*/
    
    public void SetOwner(GameObject owner)
    {
        this.m_Owner = owner;
    }
    

    /*==============================
        SetRadius
        Sets the trigger's radius
        @param The radius
    ==============================*/
    
    public void SetRadius(float radius)
    {
        this.GetComponent<SphereCollider>().radius = radius;
    }
    

    /*==============================
        SetDieTime
        Sets the trigger's die time
        @param The die time
    ==============================*/
    
    public void SetDieTime(float time)
    {
        this.m_DieTime = Time.time + time;
    }
    
    
    /*==============================
        OnDrawGizmos
        Draws extra debug stuff in the editor
    ==============================*/
    
    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (this.GetComponent<SphereCollider>() != null)
            Gizmos.DrawWireSphere(this.transform.position, this.GetComponent<SphereCollider>().radius);
        if (this.GetComponent<BoxCollider>() != null)
            Gizmos.DrawWireCube(this.transform.position, this.GetComponent<BoxCollider>().size);
    }
}
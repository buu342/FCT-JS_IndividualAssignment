/****************************************************************
                          SwordLogic.cs
    
This script handles the sword hitbox logic.
****************************************************************/

using UnityEngine;

public class SwordLogic : MonoBehaviour
{
    private const float LifeTime = 0.3f;
    
    private GameObject m_Owner = null;
    private int m_Damage = 20;
    private float m_DeathTime;
    private Vector3 m_OwnerOffset = Vector3.zero;
    
    void Start()
    {
        this.m_DeathTime = Time.time + SwordLogic.LifeTime;
    }

    void Update()
    {
        if (this.m_Owner != null)
            this.transform.localPosition = this.m_Owner.transform.position + this.m_OwnerOffset;
        if (this.m_DeathTime < Time.time)
            Destroy(this.gameObject);
    }
    
    public GameObject GetOwner()
    {
        return this.m_Owner;
    }
    
    public void SetOwner(GameObject owner)
    {
        this.m_Owner = owner;
        this.m_OwnerOffset = this.transform.position - owner.transform.position;
    }
    
    public void SetDamage(int damage)
    {
        this.m_Damage = damage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Enemies":
                // Ignore enemies if our owner is an enemy
                if (this.GetOwner().tag == "Enemies")
                    return;
                EnemyLogic enemy = other.gameObject.GetComponent<EnemyLogic>();
                enemy.TakeDamage((int)this.m_Damage);
                break;
        }
    }
}
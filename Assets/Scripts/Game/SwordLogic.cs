/****************************************************************
                          SwordLogic.cs
    
This script handles the sword hitbox logic.
****************************************************************/

using UnityEngine;

public class SwordLogic : MonoBehaviour
{
    private const float LifeTime = 0.3f;
    
    public GameObject m_Owner = null;
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
}
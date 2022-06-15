using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHurtbox : MonoBehaviour
{
    private float m_DieTime = 0;
    
    void Start()
    {
        this.m_DieTime = Time.time + 0.1f;
    }
    
    void FixedUpdate()
    {
        if (this.m_DieTime != 0 && this.m_DieTime < Time.time)
            Destroy(this.gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            Debug.Log("Hit player");
    }
}

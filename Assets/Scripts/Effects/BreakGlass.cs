/****************************************************************
                          BreakGlass.cs
    
This script handles glass breaking logic
****************************************************************/

using UnityEngine;
using System.Collections;

public class BreakGlass : MonoBehaviour
{
    // Public fields
    public Transform m_BrokenObject;
    public float m_BreakForce;
    public float m_ExplodeRadius;
    public float m_ExplodePower;
    public float m_UpwardsForce;
    
    
    /*==============================
        Break
        Attempts to break the glass
        @returns Whether the glass was broken or not
    ==============================*/
    
    public bool Break(float velocity, Vector3 hitpos)
    {
        // If the collider object was moving fast enough
        if (velocity >= this.m_BreakForce)
        {
            // Destroy the glass object and replace it with the broken glass shards
            Destroy(this.gameObject);
            Instantiate(this.m_BrokenObject, this.transform.position, this.transform.rotation);
            this.m_BrokenObject.localScale = this.transform.localScale;
            
            // Find all the glass shards near our hit position, and apply some push physics to all the pieces 
            Collider[] colliders = Physics.OverlapSphere(hitpos, this.m_ExplodeRadius);
            foreach (Collider hit in colliders)
                if (hit.GetComponent<Rigidbody>())
                    hit.GetComponent<Rigidbody>().AddExplosionForce(this.m_ExplodePower*velocity, hitpos, this.m_ExplodeRadius, this.m_UpwardsForce, ForceMode.Impulse);
                
            // Return that we successfully broke
            return true;
        }
        
        // We failed to break
        return false;
    }


    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sword")
            Break(this.m_BreakForce, other.gameObject.transform.position);
    }
}
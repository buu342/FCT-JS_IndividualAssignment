using UnityEngine;
using System.Collections;

public class BreakGlass : MonoBehaviour
{
    public Transform m_BrokenObject;
    public float m_BreakForce;
    public float m_ExplodeRadius;
    public float m_ExplodePower;
    public float m_UpwardsForce;
    
    public bool Break(float velocity, Vector3 hitpos)
    {
        if (velocity >= this.m_BreakForce)
        {
            Destroy(this.gameObject);
            Instantiate(this.m_BrokenObject, this.transform.position, this.transform.rotation);
            this.m_BrokenObject.localScale = this.transform.localScale;
            Collider[] colliders = Physics.OverlapSphere(hitpos, this.m_ExplodeRadius);

            foreach (Collider hit in colliders)
                if (hit.GetComponent<Rigidbody>())
                    hit.GetComponent<Rigidbody>().AddExplosionForce(this.m_ExplodePower*velocity, hitpos, this.m_ExplodeRadius, this.m_UpwardsForce);
            return true;
        }
        return false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sword")
            Break(this.m_BreakForce, other.gameObject.transform.position);
    }
}
/****************************************************************
                         PatrolPoint.cs
    
This is a basic patrol point class, for enemy path finding.
****************************************************************/

using UnityEngine.Audio;
using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    /*==============================
        OnDrawGizmos
        Draws extra debug stuff in the editor
    ==============================*/
    
    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, 0.5f);
    }
}
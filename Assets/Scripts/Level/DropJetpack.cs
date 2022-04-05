/****************************************************************
                         DropJetpack.cs
    
This script drops a jetpack for the player when the flying enemy
in the hotel balcony is killed
****************************************************************/

using UnityEngine;

public class DropJetpack : MonoBehaviour
{
    public GameObject m_JetpackPickup = null;
    private EnemyLogic m_enemylogic = null;
    
    
    /*==============================
        Start
        Called when the trigger is initialized
    ==============================*/
    
    void Start()
    {
        this.m_enemylogic = this.GetComponent<EnemyLogic>();
    }
    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        if (this.m_enemylogic.GetEnemyState() == EnemyLogic.EnemyState.Dead)
        {
            this.transform.Find("Model").Find("Jetpack").GetComponent<SkinnedMeshRenderer>().enabled = false;
            Instantiate(this.m_JetpackPickup, this.transform.position, Quaternion.Euler(-90, 0, 0));
            Destroy(this);
            return;
        }
    }
}
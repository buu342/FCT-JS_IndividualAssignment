/****************************************************************
                         JetpackLogic.cs
    
This script handles the jetpack pickup logic
****************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JetpackLogic : MonoBehaviour
{
    private int angle;
    private int PlayerLayer;
    private GameObject m_mesh;
    private float m_DestroyTimer = 0;
    
    
    /*==============================
        Start
        Called when the coin is initialized
    ==============================*/
    
    void Start()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
        this.m_mesh = this.transform.Find("Mesh").gameObject;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        angle = (angle + 1)%360;
        this.m_mesh.transform.position = this.transform.position + (new Vector3(0, Mathf.Sin(Time.time*2)/8, 0));
        this.m_mesh.transform.localEulerAngles = new Vector3(0, 0, angle);
        if (this.m_DestroyTimer != 0 && this.m_DestroyTimer < Time.time)
            Destroy(this.gameObject);
    }


    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == PlayerLayer)
        {
            other.GetComponent<PlayerCombat>().SetPlayerLastStreakTime(10.0f);
            other.GetComponent<PlayerCombat>().SetPlayerInvulTime(10.0f);
            other.gameObject.GetComponent<PlayerCombat>().SayLine("Voice/Shell/Pickup_Jetpack", true);
            other.gameObject.AddComponent<Sequence_ShellExit_Level1_1>();
            Physics.IgnoreCollision(other.gameObject.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
            this.GetComponent<Rigidbody>().isKinematic = true;
            this.transform.position = other.gameObject.transform.position + (new Vector3(0.0f, 2.5f, 0.0f));
            this.m_DestroyTimer = Time.time + 1.8f;
            foreach (EnemyLogic enemy in FindObjectsOfType<EnemyLogic>())
                enemy.TakeDamage(100, Vector3.zero, null);
        }
    }
}
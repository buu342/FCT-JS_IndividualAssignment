/****************************************************************
                           CoinLogic.cs
    
This script handles the coin pickup logic
****************************************************************/

using UnityEngine;

public class CoinLogic : MonoBehaviour
{
    private int angle;
    private int PlayerLayer;
    private GameObject m_mesh;
    
    
    /*==============================
        Start
        Called when the coin is initialized
    ==============================*/
    
    void Start()
    {
        // Destroy ourselves if we have been collected already
        if (FindObjectOfType<SceneController>().IsTokenCollected(this.gameObject.name))
        {
            Destroy(this.gameObject);
            return;
        }
        
        // Setup our data
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
            GameObject.Find("SceneController").GetComponent<SceneController>().CollectToken(this.gameObject.name);
            other.gameObject.GetComponent<PlayerCombat>().SayLine("Voice/Shell/Pickup_Coin", true);
            Destroy(this.gameObject);
        }
    }
}
/****************************************************************
                     HotelElevatorTrigger.cs
    
This script turns on the elevator in the hotel lobby section
****************************************************************/

using UnityEngine;

public class HotelElevatorTrigger : MonoBehaviour
{
    public GameObject m_Elevator = null;
    public GameObject m_PlayerClip = null;
    private int PlayerLayer;
    
    // Debug stuff
    #if UNITY_EDITOR
        [SerializeField]
        private bool DebugTrigger = false;
    #endif
    
    
    /*==============================
        Start
        Called when the trigger is initialized
    ==============================*/
    
    void Start()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
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
            this.m_Elevator.GetComponent<MovingPlatform>().SetActivated(true);
            this.m_PlayerClip.GetComponent<BoxCollider>().enabled = true;
            GameObject.Find("SceneController").GetComponent<SceneController>().LoadScene("Level1_2");
            Destroy(this.gameObject);
        }
    }
    
    
    /*==============================
        OnDrawGizmos
        Draws extra debug stuff in the editor
    ==============================*/
    
    public virtual void OnDrawGizmos()
    {
        if (DebugTrigger)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(this.transform.position, this.transform.localScale);
        }
    }
}
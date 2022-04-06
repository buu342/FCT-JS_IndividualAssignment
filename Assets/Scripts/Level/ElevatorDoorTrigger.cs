/****************************************************************
                     ElevatorDoorTrigger.cs
    
This script opens the elevator doors to the roof of the hotel.
****************************************************************/

using UnityEngine;

public class ElevatorDoorTrigger : MonoBehaviour
{
    public GameObject m_DoorLeft = null;
    public GameObject m_DoorRight = null;
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
        if (other.gameObject.layer != PlayerLayer)
            return;
        this.m_DoorLeft.GetComponent<MovingPlatform>().SetActivated(true);
        this.m_DoorRight.GetComponent<MovingPlatform>().SetActivated(true);
        Destroy(this.gameObject);
    }
    
    
    #if UNITY_EDITOR
        /*==============================
            OnDrawGizmos
            Draws extra debug stuff in the editor
        ==============================*/
        
        public virtual void OnDrawGizmos()
        {
            if (DebugTrigger)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(this.transform.position, this.transform.localScale);
            }
        }
    #endif
}
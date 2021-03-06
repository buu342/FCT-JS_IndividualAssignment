/****************************************************************
                        CameraTrigger.cs
    
This script handles camera triggers, which are used to reposition
the camera focus points during certain sections of the level.
****************************************************************/

using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public bool m_FollowPlayer = true;
    public Vector3 m_TargetPoI = Vector3.zero;
    public bool m_OnlyIfNotFollowing = true;
    public string m_TriggerSound = "";
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
            CameraLogic cam =  Camera.main.GetComponent<CameraLogic>();
            if (!this.m_FollowPlayer && this.m_OnlyIfNotFollowing && !cam.GetFollowPlayer())
                return;
            cam.SetFollowPlayer(this.m_FollowPlayer, this.transform.position);
            cam.SetPoI(this.m_TargetPoI);
            if (this.m_TriggerSound != "")
                FindObjectOfType<AudioManager>().Play(this.m_TriggerSound, this.transform.position);
        }
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
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(this.transform.position, this.transform.localScale);
            }
        }
    #endif
}
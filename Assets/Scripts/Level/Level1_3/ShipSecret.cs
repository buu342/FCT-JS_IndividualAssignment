/****************************************************************
                          ShipSecret.cs
    
This script makes the side of the ship transparent while the 
player is behind it
****************************************************************/

using UnityEngine;

public class ShipSecret : MonoBehaviour
{
    public Material m_HiddenMaterial = null;
    private Color m_TargetColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
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
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        this.m_HiddenMaterial.color = Color.Lerp(this.m_HiddenMaterial.color, this.m_TargetColor, 0.1f);
        if (this.m_HiddenMaterial.color.a > 0.95)
            this.m_HiddenMaterial.renderQueue = 2000;
        else
            this.m_HiddenMaterial.renderQueue = 3000;
    }
    

    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == PlayerLayer)
            this.m_TargetColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
    }
    

    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == PlayerLayer)
            this.m_TargetColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
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
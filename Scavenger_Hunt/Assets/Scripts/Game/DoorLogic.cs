/****************************************************************
                          DoorLogic.cs
    
This script handles door logic
****************************************************************/

using UnityEngine;


public class DoorLogic : MonoBehaviour
{
    private const float DoorHoldTime = 3;
    
    public float m_OpenSpeed = 0.5f;
    public GameObject m_ModelPrefab;
    
    private AudioManager m_Audio;
    private bool m_IsOpen = false;
    private Vector3 m_Displacement = Vector3.zero;
    private float m_Timer = 0;
    private Vector3 m_StartingPos;
    
    
    /*==============================
        Start
        Called when the door is initialized
    ==============================*/
    
    void Start()
    {
        this.m_StartingPos = this.m_ModelPrefab.transform.position;
        this.m_Audio = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }
    

    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/
    
    void FixedUpdate()
    {
        // Open the door
        if (this.m_IsOpen || (!this.m_IsOpen && this.m_Timer > Time.time))
        {
            if (this.m_Displacement.y < ProcGenner.GridScale)
            {
                this.m_Displacement += new Vector3(0, this.m_OpenSpeed, 0);
                if (this.m_Displacement.y > ProcGenner.GridScale)
                    this.m_Displacement.y = ProcGenner.GridScale;
            }
        }
        
        // Close the door
        if (!this.m_IsOpen && this.m_Displacement.y > 0)
        {
            if (this.m_Timer < Time.time && this.m_Timer != 0)
            {
                this.m_Timer = 0;
                this.m_Audio.Play("Physics/DoorClose", this.transform.position + new Vector3(0, 1, 0), this.m_ModelPrefab.transform.gameObject);
            }
            if (this.m_Timer == 0 && this.m_Displacement.y > 0)
            {
                this.m_Displacement -= new Vector3(0, this.m_OpenSpeed, 0);
                if (this.m_Displacement.y < 0)
                    this.m_Displacement.y = 0;
            }
        }
        
        // Actually move the door
        this.m_ModelPrefab.transform.position = this.m_StartingPos + new Vector3(0, this.m_Displacement.y, 0);
    }


    /*==============================
        OnTriggerStay
        Handles trigger collision response when an object is currently touching the trigger
        @param The object we're colliding with
    ==============================*/
    
    void OnTriggerStay(Collider other)
    {
        this.m_IsOpen = true;
        this.m_Timer = Time.time + DoorLogic.DoorHoldTime;
        if (this.m_Displacement.y == 0)
            this.m_Audio.Play("Physics/DoorOpen", this.transform.position + new Vector3(0, 1, 0), this.m_ModelPrefab.transform.gameObject);
    }


    /*==============================
        OnTriggerExit
        Handles trigger collision response when an object leaves
        @param The object we're no longer colliding with
    ==============================*/
    
    void OnTriggerExit(Collider other)
    {
        this.m_IsOpen = false;
    }
}

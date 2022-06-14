/****************************************************************
                          DoorLogic.cs
    
This script handles door logic
****************************************************************/

using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    private const float DoorHoldTime = 3;
    
    public enum DoorOpenType
    {
        Up,
        Left,
        Right
    }
    
    public DoorOpenType m_DoorType = DoorOpenType.Up;
    public float m_OpenSpeed = 0.5f;
    public float m_OpenAmount = ProcGenner.GridScale;
    public GameObject m_ModelPrefab;
    public string m_OpenSound = "Physics/DoorOpen";
    public string m_CloseSound = "Physics/DoorClose";
    
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
            switch (this.m_DoorType)
            {
                case DoorOpenType.Up:
                    if (this.m_Displacement.y < this.m_OpenAmount)
                    {
                        this.m_Displacement += new Vector3(0, this.m_OpenSpeed, 0);
                        if (this.m_Displacement.y > this.m_OpenAmount)
                            this.m_Displacement.y = this.m_OpenAmount;
                    }
                    break;
                case DoorOpenType.Right:
                    if (this.m_Displacement.x < this.m_OpenAmount)
                    {
                        this.m_Displacement += new Vector3(this.m_OpenSpeed, 0, 0);
                        if (this.m_Displacement.x > this.m_OpenAmount)
                            this.m_Displacement.x = this.m_OpenAmount;
                    }
                    break;
                case DoorOpenType.Left:
                    if (this.m_Displacement.x > -this.m_OpenAmount)
                    {
                        this.m_Displacement -= new Vector3(this.m_OpenSpeed, 0, 0);
                        if (this.m_Displacement.x < -this.m_OpenAmount)
                            this.m_Displacement.x = -this.m_OpenAmount;
                    }
                    break;
            }
        }
        
        // Close the door
        if (!this.m_IsOpen && this.IsDoorOpen())
        {
            if (this.m_Timer < Time.time && this.m_Timer != 0)
            {
                this.m_Timer = 0;
                if (this.m_CloseSound != "")
                    this.m_Audio.Play(this.m_CloseSound, this.m_ModelPrefab.transform.position + new Vector3(0, 1, 0), this.m_ModelPrefab.transform.gameObject);
            }
            if (this.m_Timer == 0 && this.IsDoorOpen())
            {
                switch (this.m_DoorType)
                {
                    case DoorOpenType.Up:
                        this.m_Displacement -= new Vector3(0, this.m_OpenSpeed, 0);
                        if (this.m_Displacement.y < 0)
                            this.m_Displacement.y = 0;
                        break;
                    case DoorOpenType.Right:
                        this.m_Displacement -= new Vector3(this.m_OpenSpeed, 0, 0);
                        if (this.m_Displacement.x < 0)
                            this.m_Displacement.x = 0;
                        break;
                    case DoorOpenType.Left:
                        this.m_Displacement += new Vector3(this.m_OpenSpeed, 0, 0);
                        if (this.m_Displacement.x > 0)
                            this.m_Displacement.x = 0;
                        break;
                }
            }
        }
        
        // Actually move the door
        this.m_ModelPrefab.transform.position = this.m_StartingPos + this.m_Displacement;
    }


    /*==============================
        IsDoorOpen
        Checks whether the door is open
        @return Whether the door is open or not
    ==============================*/
    
    public bool IsDoorOpen()
    {
        switch (this.m_DoorType)
        {
            case DoorOpenType.Up:
                return (this.m_Displacement.y > 0);
            case DoorOpenType.Right:
                return (this.m_Displacement.x > 0);
            case DoorOpenType.Left:
                return (this.m_Displacement.x < 0);
        }
        return false;
    }
    
    public void StartOpen(bool canclose)
    {
        this.m_IsOpen = true;
        this.m_Timer = Time.time + (canclose ? DoorLogic.DoorHoldTime : 10000000);
        if (!IsDoorOpen() && this.m_OpenSound != "")
            this.m_Audio.Play(this.m_OpenSound, this.transform.position + new Vector3(0, 1, 0), this.m_ModelPrefab.transform.gameObject);
    }
    
    public void CloseDoor()
    {
        this.m_IsOpen = false;
    }
    
    public void ForceCloseDoor()
    {
        if (this.m_IsOpen)
        {
            this.m_IsOpen = false;
            this.m_Timer = 0;
            if (this.m_CloseSound != "")
                this.m_Audio.Play(this.m_CloseSound, this.transform.position + new Vector3(0, 1, 0), this.m_ModelPrefab.transform.gameObject);
        }
    }
}
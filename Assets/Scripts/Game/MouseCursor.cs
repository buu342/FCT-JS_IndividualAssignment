/****************************************************************
                         MouseCursor.cs
    
This script handles the mouse cursor
****************************************************************/

using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    private const float RotateSpeed = 150.0f;
    private const float OscillateSpeed = 5.0f;
    private const float OscillateDistance = 8.0f;
    
    public GameObject m_TopBar;
    public GameObject m_LeftBar;
    public GameObject m_RightBar;
    public GameObject m_BottomBar;
    
    private Vector2 m_InitialPosTop;
    private Vector2 m_InitialPosLeft;
    private Vector2 m_InitialPosRight;
    private Vector2 m_InitialPosBottom;
    
    
    /*==============================
        Start
        Called when the cursor is initialized
    ==============================*/
    
    void Start()
    {
        Cursor.visible = false;
        this.m_InitialPosTop = this.m_TopBar.transform.localPosition;
        this.m_InitialPosLeft = this.m_LeftBar.transform.localPosition;
        this.m_InitialPosRight = this.m_RightBar.transform.localPosition;
        this.m_InitialPosBottom = this.m_BottomBar.transform.localPosition;
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
   
    void Update()
    {
        float oscillation = Mathf.Sin(Time.unscaledTime*OscillateSpeed)*OscillateDistance;
        
        // Rotate the cursor
        this.transform.position = Input.mousePosition;
        this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Time.unscaledTime*RotateSpeed);
        
        // Make the bars oscillate
        this.m_TopBar.transform.localPosition = this.m_InitialPosTop + (new Vector2(0.0f, oscillation));
        this.m_LeftBar.transform.localPosition = this.m_InitialPosLeft - (new Vector2(oscillation, 0.0f));
        this.m_RightBar.transform.localPosition = this.m_InitialPosRight + (new Vector2(oscillation, 0.0f));
        this.m_BottomBar.transform.localPosition = this.m_InitialPosBottom - (new Vector2(0.0f, oscillation));
    }
}
/****************************************************************
                        Init_Menu.cs
    
This script initializes the menu.
****************************************************************/

using UnityEngine;

public class Init_Menu : MonoBehaviour
{
    public MenuGUI m_GUI;
    private float m_TimeToStart;
    
    
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        this.m_TimeToStart = Time.unscaledTime + 3.0f;
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void FixedUpdate()
    {
        if (this.m_TimeToStart < Time.unscaledTime)
        {
            this.m_GUI.StartCreditsAnimation();
            FindObjectOfType<MusicManager>().PlaySong("Music/Menu", true, true);
            Destroy(this);
        }
    }
}
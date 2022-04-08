/****************************************************************
                        Init_Level1_2.cs
    
This script initializes Level1_2.
****************************************************************/

using UnityEngine;

public class Init_Level1_2 : MonoBehaviour
{
    public HUD m_hud;
    private float m_Fade = 1.0f;
    
    
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        foreach (MusicManager mm in FindObjectsOfType<MusicManager>())
            mm.PlaySong("Music/Level1", true, false, 1);
        if (FindObjectOfType<SceneController>().IsRespawning())
            FindObjectOfType<HUD>().PlayerRespawned();
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        GameObject.Find("Player").GetComponent<PlayerCombat>().SetPlayerLastStreakTime(10.0f);
        this.m_Fade = Mathf.Lerp(this.m_Fade, 0.0f, Time.deltaTime);
        Camera.main.GetComponent<CameraLogic>().SetPoI(new Vector3(4-9*this.m_Fade, 2-9*this.m_Fade, -7));
        this.m_hud.SetFade(this.m_Fade);
        if (this.m_Fade < 0.01f)
            Destroy(this);
    }
}
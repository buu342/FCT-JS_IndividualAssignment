/****************************************************************
                        Init_Level1_1.cs
    
This script initializes Level1_1.
****************************************************************/

using UnityEngine;

public class Init_Level1_1 : MonoBehaviour
{
    public GameObject m_Player;
    public GameObject m_Glass;
    
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        if (!FindObjectOfType<SceneController>().IsRespawning())
        {
            this.m_Player.GetComponent<Sequence_ShellSpawn_Level1_1>().enabled = true;
            this.m_Glass.GetComponent<BreakGlass>().Break(5, this.m_Glass.transform.position + this.m_Glass.transform.forward*0.01f);
        }
        else
            FindObjectOfType<HUD>().PlayerRespawned();
        FindObjectOfType<MusicManager>().PlaySong("Music/Level1", true, false);
        Destroy(this);
    }
}
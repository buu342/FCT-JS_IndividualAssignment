/****************************************************************
                        Init_Level1_Boss.cs
    
This script initializes Level1_Boss.
****************************************************************/

using UnityEngine;

public class Init_Level1_Boss : MonoBehaviour
{
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        foreach (MusicManager mm in FindObjectsOfType<MusicManager>())
            mm.PlaySong("Music/Level1_Boss", true, true);
        Destroy(this);
    }
}
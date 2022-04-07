/****************************************************************
                        Init_Level1_2.cs
    
This script initializes Level1_2.
****************************************************************/

using UnityEngine;

public class Init_Level1_2 : MonoBehaviour
{
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        foreach (MusicManager mm in FindObjectsOfType<MusicManager>())
            mm.PlaySong("Music/Level1", true, false, 1);
        Destroy(this);
    }
}
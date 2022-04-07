/****************************************************************
                        Init_Level1_1.cs
    
This script initializes Level1_1.
****************************************************************/

using UnityEngine;

public class Init_Level1_1 : MonoBehaviour
{
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        FindObjectOfType<MusicManager>().PlaySong("Music/Level1", true, true);
        Destroy(this);
    }
}
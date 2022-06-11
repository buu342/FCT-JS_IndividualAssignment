/****************************************************************
                         SceneDirector.cs
    
A script which handles the scene.
****************************************************************/

using UnityEngine;

public class SceneDirector : MonoBehaviour
{
    enum MusicType
    {
        None,
        Calm,
        Tense,
        LessCalm
    }
    
    private MusicType m_MusicType;
    private MusicManager m_Music;
    
    
    /*==============================
        Start
        Called when the scene director is initialized
    ==============================*/
    
    void Start()
    {
        this.GetComponent<ProcGenner>().GenerateScene();
        this.m_Music = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        this.m_Music.PlaySong("Music/Calm", true, true);
        this.m_MusicType = MusicType.Calm;
    }
    

    /*==============================
        GetMusicTense
        Checks if the music is tense
        @return Checks whether the music is tense or not
    ==============================*/
    
    public bool GetMusicTense()
    {
        return (this.m_MusicType == MusicType.Tense);
    }
    

    /*==============================
        SetMusicTense
        Sets the music to be tense
        @param Whether to set the music to be tense or not
    ==============================*/
    
    public void SetMusicTense(bool enable)
    {
        if (enable)
        {
            this.m_Music.PlaySong("Music/Tense", true, true, true);
            this.m_MusicType = MusicType.Tense;
        }
        else
        {
            this.m_Music.PlaySong("Music/LessCalm", true, true, true);
            this.m_MusicType = MusicType.LessCalm;
        }
    }
}
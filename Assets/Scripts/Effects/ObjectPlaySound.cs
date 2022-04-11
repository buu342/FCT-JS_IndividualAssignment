/****************************************************************
                        ObjectPlaySound.cs
    
This script plays a given sound when on an object
****************************************************************/

using UnityEngine;

public class ObjectPlaySound : MonoBehaviour
{
    public AudioManager m_audio;
    public string m_Sound;
    
    
    /*==============================
        Start
        Called when the object is initialized
    ==============================*/
    
    void Start()
    {
        m_audio.Play(this.m_Sound, this.gameObject);
    }
}
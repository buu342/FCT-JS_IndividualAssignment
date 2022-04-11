/****************************************************************
                        ObjectPlaySound.cs
    
This script plays a given sound when on an object
****************************************************************/

using UnityEngine;

public class ObjectPlaySound : MonoBehaviour
{
    public string m_Sound;
    
    
    /*==============================
        Start
        Called when the object is initialized
    ==============================*/
    
    void Start()
    {
        FindObjectOfType<AudioManager>().Play(this.m_Sound, this.gameObject);
    }
}
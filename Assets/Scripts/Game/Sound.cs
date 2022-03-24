/****************************************************************
                         Sound.cs
    
This is a basic sound class, for the audio manager.
****************************************************************/

using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
    [Range(0.1f, 3.0f)]
    public float pitch = 1.0f;
    
    public bool pitchBulletTime = true;
    public bool loop = false;
    
    [HideInInspector]
    public AudioSource source;
}

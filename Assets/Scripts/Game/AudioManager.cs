/****************************************************************
                         AudioManager.cs
    
A custom audio manager, which abstracts away the need to add all
sounds as objects in the scene. Unity's audio system is weird...
****************************************************************/

using System;
using UnityEngine.Audio;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    
    // Awake is called before start
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return;
        }
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.Play();
    }
    
    public void Update()
    {
        // Pitch all sounds that are affected by bullet time.
        foreach (Sound s in sounds)
            if (s.pitchBulletTime)
                s.source.pitch = s.pitch*Time.timeScale;
    }
    
    public void StopPlaying(string name)
    {
        Sound s = Array.Find(sounds, item => item.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return;
        }
        s.source.Stop();
    }
}

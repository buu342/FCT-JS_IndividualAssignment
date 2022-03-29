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
    
    
    /*==============================
        Awake
        Called before the audio manager is initialized
    ==============================*/
    
    void Awake()
    {
        // Create an audio source for each registered sound effect
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
        }
    }
    
    
    /*==============================
        Play
        Plays a given sound
        @param The name of the sound to play
    ==============================*/

    public void Play(string name)
    {
        // Find all sounds that have the given name
        Sound[] slist = Array.FindAll(sounds, sound => sound.name == name);
        
        // If no sound was found, throw a warning
        if (slist.Length == 0)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return;
        }
        
        // Pick a random sound from the list and play it
        Sound s = slist[(new System.Random()).Next(0, slist.Length)];
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.Play();
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    public void Update()
    {
        // Pitch all sounds that are affected by bullet time.
        foreach (Sound s in sounds)
            if (s.pitchBulletTime)
                s.source.pitch = s.pitch*Time.timeScale;
    }
    

    /*==============================
        StopPlaying
        Stops a given sound from playing
        @param The name of the sound to stop
    ==============================*/
    
    public void StopPlaying(string name)
    {
        // Find all sounds that have the given name
        Sound[] slist = Array.FindAll(sounds, sound => sound.name == name);
        
        // If no sound was found, throw a warning
        if (slist.Length == 0)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return;
        }
        
        // Stop all sounds with the given name from playing
        foreach (Sound s in slist)
            s.source.Stop();
    }
}
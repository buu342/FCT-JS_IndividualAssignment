/****************************************************************
                            Music.cs
    
This is a basic music class, for the music manager.
****************************************************************/

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Music
{
    public string name;
    
    public AudioClip SongIntro;
    public List<AudioClip> SongTracks;
    
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
}
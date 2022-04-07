/****************************************************************
                            Sound.cs
    
This is a basic sound class, for the audio manager.
****************************************************************/

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

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
    public bool canStack = false;
    public bool is3D = false;
    public float maxDistance = 16.0f;
    
    [HideInInspector]
    public List<GameObject> sources;
    [HideInInspector]
    public float maxDistanceSqr;
}
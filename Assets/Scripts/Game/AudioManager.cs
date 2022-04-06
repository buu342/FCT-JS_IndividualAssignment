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
    public Sound[] m_RegisteredSoundsList;
    private GameObject m_listener;
    
    
    /*==============================
        Awake
        Called before the audio manager is initialized
    ==============================*/
    
    void Awake()
    {
        this.m_listener = GameObject.FindObjectOfType<AudioListener>().gameObject;
        foreach (Sound s in this.m_RegisteredSoundsList)
            s.maxDistanceSqr = s.maxDistance*s.maxDistance;
    }
    
    
    /*==============================
        Play
        Plays a given sound
        @param The name of the sound to play
        @param The position where the sound was played
    ==============================*/

    public void Play(string name, Vector3 position = default(Vector3))
    {
        // Find all sounds that have the given name
        Sound[] slist = Array.FindAll(this.m_RegisteredSoundsList, sound => sound.name == name);
        
        // If no sound was found, throw a warning
        if (slist.Length == 0)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return;
        }
        
        // If this sound isn't allowed to stack, kill all of the ones that are already playing
        foreach (Sound snds in slist)
        {
            if (!snds.canStack)
            {
                for (int i=snds.sources.Count-1; i>=0; i--)
                {
                    Destroy(snds.sources[i]);
                    snds.sources.RemoveAt(i);
                }
            }
        }
        
        // Pick a random sound from the list and set it up
        Sound s = slist[(new System.Random()).Next(0, slist.Length)];
        GameObject sndobj = new GameObject();
        #if DEBUG
            sndobj.name = "SndFX - " + s.name;
            sndobj.transform.SetParent(this.gameObject.transform);
        #endif
        AudioSource source = sndobj.AddComponent<AudioSource>();
        source.clip = s.clip;
        source.loop = s.loop;
        
        // Calculate the volume and panning
        if (s.is3D)
        {
            Vector2 srcpos = new Vector2(position.x, position.y);
            Vector2 listenerpos = new Vector2(this.m_listener.transform.position.x, this.m_listener.transform.position.y);
            sndobj.transform.position = position;
            source.volume = s.volume*Calc3DSoundVolume(s.maxDistanceSqr, listenerpos, srcpos);
            source.panStereo = Calc3DSoundPan(s.maxDistanceSqr, listenerpos, srcpos);
        }
        else
        {
            source.volume = s.volume;
        }
        
        // Play the sound
        source.pitch = s.pitch;
        source.Play();
        s.sources.Add(sndobj);
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    public void Update()
    {
        Vector2 listenerpos = new Vector2(this.m_listener.transform.position.x, this.m_listener.transform.position.y);
        
        // Go through all active sounds
        foreach (Sound s in this.m_RegisteredSoundsList)
        {
            for (int i=s.sources.Count-1; i>=0; i--)
            {
                GameObject sndobj = s.sources[i];
                AudioSource source = sndobj.GetComponent<AudioSource>();
                
                // If the sound is done playing, remove the object
                if (!source.isPlaying)
                {
                    Destroy(sndobj);
                    s.sources.RemoveAt(i);
                    continue;
                }
                
                // Pitch all sounds that are affected by bullet time.
                if (s.pitchBulletTime)
                    source.pitch = s.pitch*Time.timeScale;
                
                // Calculate volume and panning
                if (s.is3D)
                {
                    Vector2 srcpos = new Vector2(sndobj.transform.position.x, sndobj.transform.position.y);
                    source.volume = s.volume*Calc3DSoundVolume(s.maxDistanceSqr, listenerpos, srcpos);
                    source.panStereo = Calc3DSoundPan(s.maxDistanceSqr, listenerpos, srcpos);
                }
            }
        }
    }
    

    /*==============================
        Stop
        Stops a given sound from playing
        @param The name of the sound to stop
    ==============================*/
    
    public void Stop(string name)
    {
        // Find all sounds that have the given name
        Sound[] slist = Array.FindAll(this.m_RegisteredSoundsList, s => s.name == name);
        
        // If no sound was found, throw a warning
        if (slist.Length == 0)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return;
        }
        
        // Stop all sounds with the given name from playing
        foreach (Sound s in slist)
        {
            for (int i=s.sources.Count-1; i>=0; i--)
            {
                AudioSource source = s.sources[i].GetComponent<AudioSource>();
                source.Stop();
                s.sources.RemoveAt(i);
            }
        }
    }
    

    /*==============================
        Calc3DSoundVolume
        Calculate 3D audio volume
        @param The maximum square distance for the sound to play
        @param The 2D position of the listener
        @param The 2D position of the source
        @returns The volume, ranging from 0 to 1
    ==============================*/
    
    private float Calc3DSoundVolume(float maxdistsqr, Vector2 listenerpos, Vector2 srcpos)
    {
        Vector2 dist = listenerpos - srcpos;
        float distsqr = dist.sqrMagnitude;
        if (distsqr > maxdistsqr)
            return 0.0f;
        return (maxdistsqr - distsqr)/maxdistsqr;
    }
    

    /*==============================
        Calc3DSoundPan
        Calculate 3D audio panning
        @param The maximum square distance for the sound to play
        @param The 2D position of the listener
        @param The 2D position of the source
        @returns The panning, ranging from -1 to 1
    ==============================*/
    
    private float Calc3DSoundPan(float maxdistsqr, Vector2 listenerpos, Vector2 srcpos)
    {
        Vector2 dist = listenerpos - srcpos;
        float distsqr = dist.sqrMagnitude;
        if (distsqr == maxdistsqr)
            return 0.0f;
        else if (listenerpos.x > srcpos.x)
            return -(1.0f - Mathf.Min((maxdistsqr - distsqr)/maxdistsqr, 1.0f));
        else
            return (1.0f - Mathf.Min((maxdistsqr - distsqr)/maxdistsqr, 1.0f));
    }
}
/****************************************************************
                         AudioManager.cs
    
A custom audio manager, which abstracts away the need to add all
sounds as objects in the scene. Unity's audio system is weird...
****************************************************************/

using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public Sound[] m_RegisteredSoundsList;
    private GameObject m_listener;
    private ProcGenner m_procgen;
    
    
    /*==============================
        Awake
        Called before the audio manager is initialized
    ==============================*/
    
    void Awake()
    {
        this.m_procgen = GameObject.Find("SceneController").GetComponent<ProcGenner>();
        this.m_listener = GameObject.FindObjectOfType<AudioListener>().gameObject;
        foreach (Sound s in this.m_RegisteredSoundsList)
            s.maxDistanceSqr = s.maxDistance*s.maxDistance;
    }
    
    
    /*==============================
        Play
        Plays a given sound
        @param The name of the sound to play
        @param The object that played the sound
    ==============================*/

    public void Play(string name, GameObject obj, GameObject ignore=null)
    {
        GameObject ret = Play(name, obj.transform.position, ignore);
        ret.transform.SetParent(obj.transform);
    }
    
    
    /*==============================
        Play
        Plays a given sound
        @param The name of the sound to play
        @param The position where the sound was played
        @returns The created sound object
    ==============================*/

    public GameObject Play(string name, Vector3 position = default(Vector3), GameObject ignore=null)
    {
        // Find all sounds that have the given name
        Sound[] slist = Array.FindAll(this.m_RegisteredSoundsList, sound => sound.name == name);
        
        // If no sound was found, throw a warning
        if (slist.Length == 0)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return null;
        }
        
        // If this sound isn't allowed to stack, kill all of the ones that are already playing
        foreach (Sound snds in slist)
        {
            if (!snds.canStack)
            {
                for (int i=snds.sources.Count-1; i>=0; i--)
                {
                    Destroy(snds.sources[i].Item1);
                    snds.sources.RemoveAt(i);
                }
            }
        }
        
        // Pick a random sound from the list and set it up
        Sound s = slist[(new System.Random()).Next(0, slist.Length)];
        GameObject sndobj = new GameObject();
        #if UNITY_EDITOR
            sndobj.name = "SndFX - " + s.name;
            sndobj.transform.SetParent(this.gameObject.transform);
        #endif
        AudioSource source = sndobj.AddComponent<AudioSource>();
        AudioLowPassFilter filter = sndobj.AddComponent<AudioLowPassFilter>();
        filter.cutoffFrequency = 2000.0f;
        filter.enabled = false;
        if (s.canReverb)
        {
            AudioReverbFilter reverb = sndobj.AddComponent<AudioReverbFilter>();
            List<ProcGenner.RoomDef> rooms = this.m_procgen.GetRoomDefs();
            reverb.reverbPreset = AudioReverbPreset.Hallway;
            foreach (ProcGenner.RoomDef room in rooms)
            {
                // Check if the sound is in bounds of a room
                Vector3 realroomstart = room.midpoint;
                Vector3 realroomsize = ((Vector3)room.size)*ProcGenner.GridScale/2;
                if (position.x >= realroomstart.x-realroomsize.x && position.x <= realroomstart.x+realroomsize.x &&
                    position.y >= realroomstart.y-realroomsize.y && position.y <= realroomstart.y+realroomsize.y &&
                    position.z >= realroomstart.z-realroomsize.z && position.z <= realroomstart.z+realroomsize.z
                )
                {
                    if (room.size.y == 2)
                        reverb.reverbPreset = AudioReverbPreset.Cave;
                    else
                        reverb.reverbPreset = AudioReverbPreset.Auditorium;
                    break;
                }
            }
        }
        source.clip = s.clip;
        source.loop = s.loop;
        
        // Calculate the volume and panning
        if (s.is3D)
        {
            sndobj.transform.position = position;
            source.volume = s.volume*Calc3DSoundVolume(s.maxDistanceSqr, this.m_listener.transform.position, position);
            source.panStereo = Calc3DSoundPan(s.maxDistance, this.m_listener.transform.position, position);
        
            // Calculate sound muffling
            if (s.canMuffle)
            {
                bool hitsomething = false;
                RaycastHit[] hits = Physics.RaycastAll(position, position-this.m_listener.transform.position, s.maxDistance);
                for (int i = 0; i < hits.Length; i++)
                {
                    GameObject hit = hits[i].transform.gameObject;
                    if (hit.tag == "Wall" || hit.tag == "Floor" || hit.tag == "Ceiling" || hit.tag == "Door")
                    {
                        hitsomething = true;
                        break;
                    }
                }
                filter.enabled = hitsomething;
            }
        }
        else
            source.volume = s.volume;
        
        // Play the sound
        source.pitch = s.pitch;
        source.Play();
        s.sources.Add((sndobj, ignore));
        return sndobj;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    public void Update()
    {
        Vector3 listenerpos = this.m_listener.transform.position;
        
        // Go through all active sounds
        foreach (Sound s in this.m_RegisteredSoundsList)
        {
            for (int i=s.sources.Count-1; i>=0; i--)
            {
                GameObject sndobj = s.sources[i].Item1;
                GameObject ignore = s.sources[i].Item2;
                if (sndobj == null)
                {
                    s.sources.RemoveAt(i);
                    continue;
                }
                
                // If the sound is done playing, remove the object
                AudioSource source = sndobj.GetComponent<AudioSource>();
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
                    Vector3 srcpos = sndobj.transform.position;
                    source.volume = s.volume*Calc3DSoundVolume(s.maxDistanceSqr, listenerpos, srcpos);
                    source.panStereo = Calc3DSoundPan(s.maxDistance, listenerpos, srcpos);
                    
                    // Calculate sound muffling
                    float dist = (listenerpos-srcpos).magnitude;
                    if (s.canMuffle && dist < s.maxDistance)
                    {
                        bool hitsomething = false;
                        RaycastHit[] hits = Physics.RaycastAll(srcpos, (listenerpos-srcpos).normalized, dist);
                        //Debug.DrawRay(srcpos, listenerpos-srcpos, Color.white, 1.0f, true);
                        for (int j = 0; j < hits.Length; j++)
                        {
                            GameObject hit = hits[j].transform.gameObject;
                            if (hit != ignore && (hit.tag == "Wall" || hit.tag == "Floor" || hit.tag == "Ceiling" || hit.tag == "Door"))
                            {
                                hitsomething = true;
                                break;
                            }
                        }
                        sndobj.GetComponent<AudioLowPassFilter>().enabled = hitsomething;
                    }
                }
            }
        }
    }
    

    /*==============================
        Stop
        Stops a given sound from playing
        @param The name of the sound to stop
        @param The object to stop emitting said sound 
               If null, stops all sounds with the given name.
    ==============================*/
    
    public void Stop(string name, GameObject emitter=null)
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
                if (emitter == null || s.sources[i].Item1.transform.parent == emitter)
                {
                    AudioSource source = s.sources[i].Item1.GetComponent<AudioSource>();
                    source.Stop();
                    Destroy(s.sources[i].Item1);
                    s.sources.RemoveAt(i);
                }
            }
        }
    }
    

    /*==============================
        Calc3DSoundVolume
        Calculate 3D audio volume
        @param The maximum square distance for the sound to play
        @param The 3D position of the listener
        @param The 3D position of the source
        @returns The volume, ranging from 0 to 1
    ==============================*/
    
    private float Calc3DSoundVolume(float maxdistsqr, Vector3 listenerpos, Vector3 srcpos)
    {
        Vector3 dist = listenerpos - srcpos;
        float volume = 3.0f/dist.magnitude - 3.0f/Mathf.Sqrt(maxdistsqr);
        return Mathf.Max(0.0f, volume);
    }
    

    /*==============================
        Calc3DSoundPan
        Calculate 3D audio panning
        @param The maximum square distance for the sound to play
        @param The 3D position of the listener
        @param The 3D position of the source
        @returns The panning, ranging from -1 to 1
    ==============================*/
    
    public float Calc3DSoundPan(float maxdistsqr, Vector3 listenerpos, Vector3 srcpos)
    {
        Vector3 side = Vector3.Cross(this.m_listener.transform.up, this.m_listener.transform.forward);
        side.Normalize();
        float x = Vector3.Dot(srcpos - listenerpos, side);
        return Mathf.Clamp(x/maxdistsqr, -1, 1);
    }
    

    /*==============================
        GetSoundFromName
        Gets a registered sound data from the given name
        @param The name of the sound
        @returns The sound's corresponding data
    ==============================*/
    
    public Sound GetSoundFromName(string name)
    {
        // Find all sounds that have the given name
        Sound[] slist = Array.FindAll(this.m_RegisteredSoundsList, sound => sound.name == name);
        
        // If no sound was found, throw a warning
        if (slist.Length == 0)
        {
            Debug.LogWarning("Sound: '"+name+"' not found!");
            return null;
        }
        
        // Pick a random sound from the list and set it up
        Sound s = slist[(new System.Random()).Next(0, slist.Length)];
        return s;
    }        
}
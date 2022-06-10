/****************************************************************
                         AudioManager.cs
    
A custom music manager, to allow for dynamic music which plays
between scenes.
****************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    private const float MuffleSpeed = 0.2f;
    private const float FadeTime = 0.05f;
    
    public class MusicQueueData
    {
        public int Source;
        public double StartTime;
        public double EndTime;
        public double Duration;
    }
    
    // Public values
    public Music[] m_RegisteredMusicList;
    
    // Song settings
    private Music m_CurrentSong = null;
    private bool m_LoopSong = false;
    private int m_CurrentTrack = 0;
    private bool m_FadeOut = false;
    private double m_CurrentBarTime = 0;
    
    // Queue
    private float m_CurrentMuffle = 0.0f;
    private float m_TargetMuffle = 0.0f;
    private static List<MusicQueueData> m_MusicQueue = new List<MusicQueueData>();
    
    // Components
    private static AudioSource[] m_src = new AudioSource[2];
    private AudioLowPassFilter m_filter;
    private GameObject m_listener;
    private static MusicManager Instance = null;  
    
    
    /*==============================
        Awake
        Called before the music manager is initialized
    ==============================*/
    
    void Awake()
    {
        // Check if this instance already exists, if not, then set itself to this instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this) // If the instance already exists, but isn't us, then destroy ourselves.
        {
            Destroy(this.gameObject);
            return;
        }

        // Set this to not be destroyed when changing scenes
        DontDestroyOnLoad(this.gameObject);
        this.m_listener = GameObject.FindObjectOfType<AudioListener>().gameObject;
        m_src[0] = this.GetComponents<AudioSource>()[0];
        m_src[1] = this.GetComponents<AudioSource>()[1];
        this.m_filter = this.GetComponent<AudioLowPassFilter>();
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        // Filter sound based on the timescale
        this.m_TargetMuffle = 1-(Mathf.Clamp(Time.timeScale-0.5f, 0.0f, 1.0f)/0.5f);
        this.m_CurrentMuffle = Mathf.Lerp(this.m_CurrentMuffle, this.m_TargetMuffle, MusicManager.MuffleSpeed);
        this.m_filter.cutoffFrequency = Mathf.Lerp(22000.0f, 2000.0f, this.m_CurrentMuffle);
        
        // If the top of the list is done playing, pop it
        if (m_MusicQueue.Count > 0)
        {
            if (m_MusicQueue[0].EndTime < AudioSettings.dspTime)
            {
                m_src[m_MusicQueue[0].Source].Stop();
                m_MusicQueue.RemoveAt(0);
            }
        }
            
        // Fade music out
        if (this.m_FadeOut)
        {
            m_src[0].volume = Mathf.Lerp(m_src[0].volume, 0.0f, MusicManager.FadeTime);
            m_src[1].volume = Mathf.Lerp(m_src[1].volume, 0.0f, MusicManager.FadeTime);
            if (m_src[0].volume < 0.01f && m_src[1].volume < 0.01f)
                StopMusic();
        }
        
        // Update the song bar timer
        if (this.m_CurrentBarTime < AudioSettings.dspTime && this.m_CurrentSong != null)
            this.m_CurrentBarTime = AudioSettings.dspTime + this.m_CurrentSong.BarDuration;
        
        // If we having a looping song, and one of the sources isn't playing, then queue the next loop of this song
        if (this.m_CurrentSong != null && this.m_LoopSong && m_MusicQueue.Count < 2)
        {
            this.m_CurrentBarTime = AudioSettings.dspTime + this.m_CurrentSong.BarDuration;
            QueueSong(this.m_CurrentSong.SongTracks[this.m_CurrentTrack], this.m_CurrentSong.volume);
        }
    }
    
    
    /*==============================
        PlaySong
        Play the song with the given name
        @param The name of the song
        @param Whether to loop the song
        @param Whether to play the intro clip
        @param Whether to interrupt the current playing song
        @param Which track to play
    ==============================*/
    
    public void PlaySong(string name, bool loop=true, bool playintro=false, bool interrupt=false, int track=0)
    {
        Music song = Array.Find(this.m_RegisteredMusicList, song => song.name == name);
        
        // If no sound was found, throw a warning
        if (song == null)
        {
            Debug.LogWarning("Song: '"+name+"' not found!");
            return;
        }
        
        // If the specified track doesn't exist, throw a warning
        if (song.SongTracks[track] == null)
        {
            Debug.LogWarning("Track '"+track+"' in song '"+name+"' not found!");
            return;
        }

        // Set the audio clip
        this.m_FadeOut = false;
        this.m_LoopSong = loop;
        this.m_CurrentSong = song;
        this.m_CurrentTrack = track;
        if (this.m_CurrentBarTime == 0)
            this.m_CurrentBarTime = AudioSettings.dspTime + song.BarDuration; 
        if (playintro && song.SongIntro != null)
            QueueSong(song.SongIntro, song.volume, interrupt);
        else
            QueueSong(song.SongTracks[track], song.volume, interrupt);
    }
    
    
    /*==============================
        QueueSong
        Queues a song to play next
        @param The AudioClip to play next
        @param The volume to set for the song
        @param Whether to interrupt the current song at the end of the bar
    ==============================*/
    
    private void QueueSong(AudioClip song, float volume, bool interrupt=false)
    {
        MusicQueueData mdata = new MusicQueueData();
        mdata.Duration = (double)song.samples/song.frequency;
        
        // Add the data to the queue
        switch (m_MusicQueue.Count)
        {
            case 1:
                mdata.Source = (m_MusicQueue[0].Source+1)%2;
                if (interrupt)
                {
                    mdata.StartTime = this.m_CurrentBarTime;
                    m_MusicQueue[0].EndTime = this.m_CurrentBarTime;
                }
                else
                    mdata.StartTime = m_MusicQueue[0].EndTime;
                mdata.EndTime = mdata.StartTime + mdata.Duration;
                m_MusicQueue.Add(mdata);
                break;
            case 2:
                mdata.Source = m_MusicQueue[1].Source;
                if (interrupt)
                {
                    mdata.StartTime = this.m_CurrentBarTime;
                    m_MusicQueue[0].EndTime = this.m_CurrentBarTime;
                }
                else
                    mdata.StartTime = m_MusicQueue[0].EndTime;
                mdata.EndTime = mdata.StartTime + mdata.Duration;
                m_MusicQueue[1] = mdata;
                break;
            default:
                mdata.Source = 0;
                mdata.StartTime = AudioSettings.dspTime;
                mdata.EndTime = mdata.StartTime + mdata.Duration;
                m_MusicQueue.Add(mdata);
                break;
        }
        
        // Schedule the next song
        m_src[mdata.Source].clip = song;
        m_src[mdata.Source].volume = volume;
        m_src[mdata.Source].PlayScheduled(mdata.StartTime);
    }
    
    
    /*==============================
        StopMusic
        Stops all currently playing music,
        and empties the music queue.
    ==============================*/
    
    public void StopMusic()
    {
        m_src[0].Stop();
        m_src[1].Stop();
        m_MusicQueue.Clear();
        this.m_CurrentSong = null;
        this.m_TargetMuffle = 0;
        this.m_CurrentMuffle = 0;
        this.m_filter.cutoffFrequency = 22000.0f;
    }
    
    
    /*==============================
        FadeMusic
        Fades all currently playing music out
    ==============================*/
    
    public void FadeMusic()
    {
        this.m_FadeOut = true;
    }
}
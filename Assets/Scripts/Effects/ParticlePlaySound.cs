/****************************************************************
                      ParticlePlaySound.cs
    
This script plays a given sound when a particle emits
****************************************************************/

using UnityEngine;

public class ParticlePlaySound : MonoBehaviour
{
    public string m_Sound;
    
    private AudioManager m_am;
    private int m_LastParticleCount = 0;
    
    
    /*==============================
        Awake
        Called before the particle is initialized
    ==============================*/

    void Awake()
    {
        this.m_am = FindObjectOfType<AudioManager>();
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        int count = GetComponent<ParticleSystem>().particleCount;
        if (count > this.m_LastParticleCount)
            this.m_am.Play(m_Sound, this.transform.position);
        this.m_LastParticleCount = count; 
    }
}
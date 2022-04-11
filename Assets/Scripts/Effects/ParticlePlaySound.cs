/****************************************************************
                      ParticlePlaySound.cs
    
This script plays a given sound when a particle emits
****************************************************************/

using UnityEngine;

public class ParticlePlaySound : MonoBehaviour
{
    public string m_Sound;
    public AudioManager m_audiomanager;
    
    private int m_LastParticleCount = 0;
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        int count = GetComponent<ParticleSystem>().particleCount;
        if (count > this.m_LastParticleCount)
            this.m_audiomanager.Play(m_Sound, this.transform.position);
        this.m_LastParticleCount = count; 
    }
}
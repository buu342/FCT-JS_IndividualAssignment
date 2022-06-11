using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTenser : MonoBehaviour
{
    private SceneDirector m_SceneDirector;
    
    void Start()
    {
        this.m_SceneDirector = GameObject.Find("SceneController").GetComponent<SceneDirector>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!this.m_SceneDirector.GetMusicTense())
            this.m_SceneDirector.SetMusicTense(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (this.m_SceneDirector.GetMusicTense())
            this.m_SceneDirector.SetMusicTense(false);
    }
}
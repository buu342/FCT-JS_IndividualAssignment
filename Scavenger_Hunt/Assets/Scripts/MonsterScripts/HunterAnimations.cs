/****************************************************************
                       HunterAnimations.cs
    
This script handles the Hunter model's animations
****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterAnimations : MonoBehaviour
{   
    public Animator m_Animator;
    public MonsterAI m_AI;
    public UnityEngine.AI.NavMeshAgent m_NavAgent;
    
    private AudioManager m_Audio;
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {    
        this.m_Audio = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (m_AI.monsterCombatState == MonsterAI.MonsterCombatState.Idle)
            this.m_Animator.SetFloat("MoveSpeed", this.m_NavAgent.velocity.magnitude/5.0f);
        else
            this.m_Animator.SetFloat("MoveSpeed", 1.0f);
    }
    
    
    /*==============================
        AnimationEventSound
        Called when an animation event sound happens
        @param The sound to play
    ==============================*/

    void AnimationEventSound(string sound) 
    {
        this.m_Audio.Play(sound, this.transform.gameObject);
    }
    
    
    /*==============================
        TriggerStagger
        Trigger the stagger animation
    ==============================*/

    public void TriggerStagger() 
    {
        this.m_Animator.SetTrigger("Stagger");
    }
    
    
    /*==============================
        TriggerAttack
        Trigger the attack animation
    ==============================*/

    public void TriggerAttack() 
    {
        this.m_Animator.SetTrigger("Attack");
    }
}
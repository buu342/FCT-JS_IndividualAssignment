using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private int LayerIndex_Shooting;
    private int LayerIndex_Aim0;
    private int LayerIndex_Aim1_2;
    private int LayerIndex_Aim3;
    private int LayerIndex_Aim4_5;
    private int LayerIndex_Aim6;
    
    private PlayerController m_plycont;
    private PlayerCombat m_plycombat;
    private GameObject m_fireattach;
    private Animator m_anim;
    
    // Start is called before the first frame update
    void Start()
    {
        this.m_plycont = this.transform.parent.gameObject.GetComponent<PlayerController>();
        this.m_plycombat = this.transform.parent.gameObject.GetComponent<PlayerCombat>();
        this.m_fireattach = this.transform.parent.gameObject.transform.Find("FireAttachment").gameObject;
        this.m_anim = this.GetComponent<Animator>();
        
        this.LayerIndex_Shooting = this.m_anim.GetLayerIndex("Shooting");
        this.LayerIndex_Aim0 = this.m_anim.GetLayerIndex("Aim_0");
        this.LayerIndex_Aim1_2 = this.m_anim.GetLayerIndex("Aim_1-2");
        this.LayerIndex_Aim3 = this.m_anim.GetLayerIndex("Aim_3");
        this.LayerIndex_Aim4_5 = this.m_anim.GetLayerIndex("Aim_4-5");
        this.LayerIndex_Aim6 = this.m_anim.GetLayerIndex("Aim_6");
    }

    // Update is called once per frame
    void Update()
    {
        // Set the animation speed based on the timescale
        this.m_anim.SetFloat("AnimSpeed", Time.timeScale);
        
        // Set the base mesh angle depending whether we're facing left or right
        if (this.m_fireattach.transform.position.x < this.transform.position.x)
            this.transform.localEulerAngles = new Vector3(0, 0, 0);
        else
            this.transform.localEulerAngles = new Vector3(0, 180, 0);
        
        // Aiming layers based on mouse angles
        float aimang = (this.m_fireattach.transform.localEulerAngles.x + 90)%360;
        if (aimang < 45*1)
        {
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim0, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim1_2, (aimang%45)/45);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim3, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim4_5, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim6, 0.0f);
        }
        else if (aimang < 45*2)
        {
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim0, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim1_2, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim3, (aimang%45)/45);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim4_5, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim6, 0.0f);
        }
        else if (aimang < 45*3)
        {
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim0, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim1_2, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim3, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim4_5, (aimang%45)/45);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim6, 0.0f);
        }
        else
        {
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim0, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim1_2, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim3, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim4_5, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Aim6, (aimang%45)/45);
        }
        
        // Shooting animation
        if (this.m_plycombat.GetCombatState() == PlayerCombat.CombatState.Shooting)
            this.m_anim.SetLayerWeight(this.LayerIndex_Shooting, 1.0f);
        else
            this.m_anim.SetLayerWeight(this.LayerIndex_Shooting, 0.0f);
        
        // Running animations
        if (this.m_plycont.GetPlayerState() == PlayerController.PlayerState.Forward)
        {
            this.m_anim.SetBool("RunningForwards", true);
            this.m_anim.SetBool("IsMoving", true);
        }
        else
            this.m_anim.SetBool("RunningForwards", false);
        if (this.m_plycont.GetPlayerState() == PlayerController.PlayerState.Backward)
        {
            this.m_anim.SetBool("RunningBackwards", true);
            this.m_anim.SetBool("IsMoving", true);
        }
        else
            this.m_anim.SetBool("RunningBackwards", false);
        if (this.m_plycont.GetPlayerState() != PlayerController.PlayerState.Forward && this.m_plycont.GetPlayerState() != PlayerController.PlayerState.Backward)
            this.m_anim.SetBool("IsMoving", false);
        
        // Jumping animations
        if (this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Jump)
            this.m_anim.SetBool("IsJumping", true);
        else
            this.m_anim.SetBool("IsJumping", false);
        if (this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Fall)
            this.m_anim.SetBool("IsFalling", true);
        else
            this.m_anim.SetBool("IsFalling", false);
        if (this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Land || this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Idle)
            this.m_anim.SetBool("IsGrounded", true);
        else
            this.m_anim.SetBool("IsGrounded", false);
    }
}

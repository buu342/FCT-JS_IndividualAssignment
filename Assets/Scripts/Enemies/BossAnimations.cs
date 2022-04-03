using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimations : MonoBehaviour
{
    // Animation layer indices
    private int LayerIndex_Legs;
    private int LayerIndex_AimRight;
    private int LayerIndex_AimLeft;
    private int LayerIndex_TurnRight;
    private int LayerIndex_TurnLeft;
    private int LayerIndex_ShootRight;
    private int LayerIndex_ShootLeft;
    private int LayerIndex_RocketRight;
    private int LayerIndex_RocketLeft;
    private int LayerIndex_Death;
    
    // Private values
    private int m_LastArmShot = 0;
    private int m_LastAim = -1;
    
    // Components
    private BossLogic m_bosslogic;
    private GameObject m_fireattach;
    private Animator m_anim;
    
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {
        this.m_bosslogic = this.transform.parent.gameObject.GetComponent<BossLogic>();
        this.m_fireattach = this.transform.parent.gameObject.transform.Find("FireAttachment").gameObject;
        this.m_anim = this.GetComponent<Animator>();
        
        // Get all the layer indices so that this doesn't have to be done at runtime
        this.LayerIndex_Legs = this.m_anim.GetLayerIndex("Legs");
        this.LayerIndex_AimRight = this.m_anim.GetLayerIndex("AimRight");
        this.LayerIndex_AimLeft = this.m_anim.GetLayerIndex("AimLeft");
        this.LayerIndex_TurnRight = this.m_anim.GetLayerIndex("TurnRight");
        this.LayerIndex_TurnLeft = this.m_anim.GetLayerIndex("TurnLeft");
        this.LayerIndex_ShootRight = this.m_anim.GetLayerIndex("ShootRight");
        this.LayerIndex_ShootLeft = this.m_anim.GetLayerIndex("ShootLeft");
        this.LayerIndex_RocketRight = this.m_anim.GetLayerIndex("RocketRight");
        this.LayerIndex_RocketLeft = this.m_anim.GetLayerIndex("RocketLeft");
        this.LayerIndex_Death = this.m_anim.GetLayerIndex("Death");
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        int oldaim = this.m_LastAim;
        
        // Play the death animation if we died
        if (this.m_bosslogic.GetBossState() == BossLogic.BossState.Dead || this.m_bosslogic.GetBossState() == BossLogic.BossState.Dying)
        {
            if (this.m_bosslogic.GetBossState() == BossLogic.BossState.Dead && this.m_anim.GetLayerWeight(this.LayerIndex_Death) != 1.0f)
            {
                this.m_anim.SetLayerWeight(this.LayerIndex_Death, 1.0f);
                this.m_anim.Play("Death", this.LayerIndex_Death, 0f);
            }
            return;
        }
        
        // Set the directional aim blending
        if (this.m_bosslogic.GetBossCombatState() != BossLogic.CombatState.Rocket)
        {
            Vector3 aimdir = this.m_bosslogic.GetAimDirection();
            float aimang = Vector3.Angle(aimdir, Vector3.right);
            this.m_anim.SetFloat("AimX", aimdir.x);
            this.m_anim.SetFloat("AimY", -aimdir.y);
            if (aimang < 90.0f)
            {
                this.m_LastAim = -1;
                this.m_anim.SetLayerWeight(this.LayerIndex_AimRight, 1.0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_AimLeft, 0.0f);
            }
            else
            {
                this.m_LastAim = 1;
                this.m_anim.SetLayerWeight(this.LayerIndex_AimRight, 0.0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_AimLeft, 1.0f);
            }

            // Turn the upper body to face the target
            if (oldaim != this.m_LastAim)
            {
                if (this.m_LastAim == -1)
                {
                    this.m_anim.SetLayerWeight(this.LayerIndex_TurnRight, 1.0f);
                    this.m_anim.SetLayerWeight(this.LayerIndex_TurnLeft, 0.0f);
                    this.m_anim.Play("Turn", this.LayerIndex_TurnRight, 0f);
                }
                else
                {
                    this.m_anim.SetLayerWeight(this.LayerIndex_TurnRight, 0.0f);
                    this.m_anim.SetLayerWeight(this.LayerIndex_TurnLeft, 1.0f);
                    this.m_anim.Play("Turn", this.LayerIndex_TurnLeft, 0f);
                }
            }
        }
        
        // Handle movement
        switch (this.m_bosslogic.GetBossState())
        {
            case BossLogic.BossState.Idle:
                this.m_anim.SetBool("MovingForwards", false);
                this.m_anim.SetBool("MovingBackwards", false);
                break;
            case BossLogic.BossState.MovingForwards:
                this.m_anim.SetBool("MovingForwards", true);
                this.m_anim.SetBool("MovingBackwards", false);
                break;
            case BossLogic.BossState.MovingBackwards:
                this.m_anim.SetBool("MovingForwards", false);
                this.m_anim.SetBool("MovingBackwards", true);
                break;
        }
        
        // Jumping animations
        if (this.m_bosslogic.GetBossJumpState() != BossLogic.BossJumpState.Idle && this.m_bosslogic.GetBossJumpState() != BossLogic.BossJumpState.Land)
            this.m_anim.SetBool("IsJumping", true);
        else
            this.m_anim.SetBool("IsJumping", false);
        if (this.m_bosslogic.GetBossJumpState() == BossLogic.BossJumpState.Land)
            this.m_anim.SetBool("IsGrounded", true);
        else
            this.m_anim.SetBool("IsGrounded", false);
        
        // Rocket attack animation
        if (this.m_bosslogic.GetBossCombatState() == BossLogic.CombatState.Rocket)
        {
            switch (this.m_LastAim)
            {
                case 1:
                    if (this.m_anim.GetLayerWeight(this.LayerIndex_RocketLeft) != 1.0f)
                    {
                        this.m_anim.SetLayerWeight(this.LayerIndex_RocketRight, 0.0f);
                        this.m_anim.SetLayerWeight(this.LayerIndex_RocketLeft, 1.0f);
                        this.m_anim.Play("Rocket", this.LayerIndex_RocketLeft, 0.0f);
                    }
                    break;
                case -1:
                    if (this.m_anim.GetLayerWeight(this.LayerIndex_RocketRight) != 1.0f)
                    {
                        this.m_anim.SetLayerWeight(this.LayerIndex_RocketRight, 1.0f);
                        this.m_anim.SetLayerWeight(this.LayerIndex_RocketLeft, 0.0f);
                        this.m_anim.Play("Rocket", this.LayerIndex_RocketRight, 0.0f);
                    }
                    break;
            }
        }
        else
        {
            this.m_anim.SetLayerWeight(this.LayerIndex_RocketRight, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_RocketLeft, 0.0f);
        }
    }
    

    /*==============================
        PlayFireAnimation
        Plays the shooting animation
    ==============================*/
    
    public void PlayFireAnimation()
    {
        switch (this.m_LastArmShot)
        {
            case 0:
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootRight, 0.0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootLeft, 1.0f);
                this.m_anim.Play("Shoot", this.LayerIndex_ShootLeft, 0.0f);
                this.m_LastArmShot = 1;
                break;
            case 1:
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootRight, 1.0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootLeft, 0.0f);
                this.m_anim.Play("Shoot", this.LayerIndex_ShootRight, 0.0f);
                this.m_LastArmShot = 0;
                break;
        }
    }
}
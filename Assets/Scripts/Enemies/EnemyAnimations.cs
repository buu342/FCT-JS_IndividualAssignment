/****************************************************************
                       EnemyAnimations.cs
    
This script handles base enemy animations.
****************************************************************/

using UnityEngine;

public class EnemyAnimations : MonoBehaviour
{
    // Animation layer indices
    private int LayerIndex_Legs;
    private int LayerIndex_Aim;
    private int LayerIndex_ShootFast;
    private int LayerIndex_ShootCare;
    
    // Enemy mesh angle
    private float m_OriginalBodyRot;
    private float m_CurrentBodyRot;
    private float m_TargetBodyRot;
    
    // Components
    private EnemyLogic m_enemylogic;
    private GameObject m_fireattach;
    private Animator m_anim;
    
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {
        this.m_enemylogic = this.transform.parent.gameObject.GetComponent<EnemyLogic>();
        this.m_fireattach = this.transform.parent.gameObject.transform.Find("FireAttachment").gameObject;
        this.m_anim = this.GetComponent<Animator>();
        
        // Get all the layer indices so that this doesn't have to be done at runtime
        this.LayerIndex_Legs = this.m_anim.GetLayerIndex("Legs");
        this.LayerIndex_Aim = this.m_anim.GetLayerIndex("Aim");
        this.LayerIndex_ShootFast = this.m_anim.GetLayerIndex("ShootFast");
        this.LayerIndex_ShootCare = this.m_anim.GetLayerIndex("ShootCare");
        this.m_OriginalBodyRot = this.transform.localEulerAngles.y;
        this.m_CurrentBodyRot = this.m_OriginalBodyRot;
        this.m_TargetBodyRot = this.m_OriginalBodyRot;
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (this.m_enemylogic.GetEnemyState() == EnemyLogic.EnemyState.Dead)
            return;
        
        // Set animation speed
        this.m_anim.speed = Time.timeScale;
        
        // Set the base mesh angle depending whether we're facing left or right
        this.transform.localEulerAngles = new Vector3(0, this.m_CurrentBodyRot, 0);
        this.m_CurrentBodyRot = Mathf.Lerp(this.m_CurrentBodyRot, this.m_TargetBodyRot, 0.1f);
        
        // Set the directional aim blending
        Vector3 aimdir = this.m_enemylogic.GetAimDirection();
        float aimang = Vector3.Angle(aimdir, Vector3.right);
        this.m_anim.SetFloat("AimX", aimdir.x);
        this.m_anim.SetFloat("AimY", -aimdir.y);
        
        // If we're an aiming enemy, set the body to face either the target or the patrol point
        if (this.m_enemylogic.GetEnemyAttackStyle() == EnemyLogic.AttackStyle.Aiming)
        {
            // If the target isn't near, turn to face the patrol point
            if (!this.m_enemylogic.GetTargetNear() && this.m_enemylogic.GetPatrolPoint() != null)
            {
                float patroldir = (this.transform.parent.transform.position.x - this.m_enemylogic.GetPatrolPoint().transform.position.x);
                if (patroldir >= 0)
                    this.m_TargetBodyRot = this.m_OriginalBodyRot;
                else
                    this.m_TargetBodyRot = 0.0f;
            }
            else // Otherwise, turn to face the target
            {
                if (aimang < 90.0f)
                    this.m_TargetBodyRot = this.m_OriginalBodyRot;
                else
                    this.m_TargetBodyRot = 0.0f;
            }
        }
        
        // Shooting animations
        switch (this.m_enemylogic.GetEnemyCombatState())
        {
            case EnemyLogic.CombatState.Aiming:
            case EnemyLogic.CombatState.TakeAim:
                if (this.m_enemylogic.GetEnemyAttackStyle() == EnemyLogic.AttackStyle.Straight)
                {
                    this.m_anim.SetBool("AimCare", false);
                    this.m_anim.SetBool("AimFast", true);
                }
                else if (this.m_enemylogic.GetEnemyAttackStyle() == EnemyLogic.AttackStyle.Aiming)
                {
                    this.m_anim.SetBool("AimCare", true);
                    this.m_anim.SetBool("AimFast", false);
                }
                break;
            case EnemyLogic.CombatState.Idle:
            case EnemyLogic.CombatState.RemoveAim:
                this.m_anim.SetBool("AimCare", false);
                this.m_anim.SetBool("AimFast", false);
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootFast, 0.0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootCare, 0.0f);
                break;
        }
        
        // Handle movement
        this.m_anim.SetBool("IsFlying", this.m_enemylogic.IsFlying());
        switch (this.m_enemylogic.GetEnemyState())
        {
            case EnemyLogic.EnemyState.Idle:
                this.m_anim.SetBool("RunningForwards", false);
                this.m_anim.SetBool("RunningBackwards", false);
                break;
            case EnemyLogic.EnemyState.Running:
                if (this.m_enemylogic.GetPatrolPoint() == null)
                    Debug.LogWarning("Enemy running without patrol point. This should not happen!");
            
                // If we're an aiming enemy
                if (this.m_enemylogic.GetEnemyAttackStyle() == EnemyLogic.AttackStyle.Aiming)
                {
                    // If we're trying to shoot the target while patrolling
                    if (this.m_enemylogic.GetTargetNear()) 
                    {
                        // Set the running style based on our target direction
                        float patroldir = (this.transform.parent.transform.position.x - this.m_enemylogic.GetPatrolPoint().transform.position.x);
                        if ((patroldir >= 0 && this.m_enemylogic.GetAimDirection().x >= 0) || (patroldir < 0 && this.m_enemylogic.GetAimDirection().x < 0))
                        {
                            this.m_anim.SetBool("RunningForwards", true);
                            this.m_anim.SetBool("RunningBackwards", false);
                        }
                        else
                        {
                            this.m_anim.SetBool("RunningForwards", false);
                            this.m_anim.SetBool("RunningBackwards", true);
                        }
                    }
                    else // Otherwise, always run forward
                    {
                        this.m_anim.SetBool("RunningForwards", true);
                        this.m_anim.SetBool("RunningBackwards", false);
                    }
                }
                else // Straight enemies will always face the same way, so set their run animation based on the target location
                {
                    Vector3 targetdir = this.m_enemylogic.GetPatrolPoint().transform.position - this.transform.parent.gameObject.transform.position;
                    if (targetdir.x <= 0)
                    {
                        this.m_anim.SetBool("RunningForwards", true);
                        this.m_anim.SetBool("RunningBackwards", false);
                    }
                    else
                    {
                        this.m_anim.SetBool("RunningForwards", false);
                        this.m_anim.SetBool("RunningBackwards", true);
                    }
                }
                
                break;
        }
    }
    

    /*==============================
        PlayFireAnimation
        Plays the shooting animation
    ==============================*/
    
    public void PlayFireAnimation()
    {
        switch (this.m_enemylogic.GetEnemyAttackStyle())
        {
            case EnemyLogic.AttackStyle.Aiming:
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootCare, 1.0f);
                this.m_anim.Play("ShootCare", this.LayerIndex_ShootCare, 0f);
                break;
            case EnemyLogic.AttackStyle.Straight:
                this.m_anim.SetLayerWeight(this.LayerIndex_ShootFast, 1.0f);
                this.m_anim.Play("ShootFast", this.LayerIndex_ShootFast, 0f);
                break;
        }
    }
}
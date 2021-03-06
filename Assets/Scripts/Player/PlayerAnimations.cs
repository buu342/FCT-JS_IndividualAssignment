/****************************************************************
                       PlayerAnimations.cs
    
This script handles the player model's animations
****************************************************************/

using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public Texture m_PainEyes;
    public Texture m_PainMouth;
    public Texture m_GrinEyes;
    public Texture m_GrinMouth;
    
    // Animation layer indices
    private int LayerIndex_Shooting;
    private int LayerIndex_MeleeLeft;
    private int LayerIndex_MeleeRight;
    private int LayerIndex_Melee2Left;
    private int LayerIndex_Melee2Right;
    private int LayerIndex_AimRight;
    private int LayerIndex_AimLeft;
    private int LayerIndex_Pain;
    private int LayerIndex_Pickup;
    private int LayerIndex_HardLanding;
    
    // Player mesh angle
    private float m_CurrentBodyRot = -179;
    private float m_TargetBodyRot = -179;
    
    // Other
    private bool m_BigDrop = false;
    
    // Components
    public MeshTrail m_swordtrail;
    private SkinnedMeshRenderer m_meshsword;
    private SkinnedMeshRenderer m_meshguns;
    private SkinnedMeshRenderer m_body;
    private PlayerController m_plycont;
    private PlayerCombat m_plycombat;
    private GameObject m_fireattach;
    private Animator m_anim;
    
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {
        this.m_plycont = this.transform.parent.gameObject.GetComponent<PlayerController>();
        this.m_plycombat = this.transform.parent.gameObject.GetComponent<PlayerCombat>();
        this.m_fireattach = this.transform.parent.gameObject.transform.Find("FireAttachment").gameObject;
        this.m_anim = this.GetComponent<Animator>();
        this.m_meshsword = this.transform.Find("Sword").GetComponent<SkinnedMeshRenderer>();
        this.m_meshguns = this.transform.Find("Gun").GetComponent<SkinnedMeshRenderer>();
        this.m_body = this.transform.Find("Shell").GetComponent<SkinnedMeshRenderer>();
        this.m_body.materials[1] = new Material(this.m_body.materials[1]);
        this.m_body.materials[2] = new Material(this.m_body.materials[2]);
        this.m_body.materials[1].SetTexture("_MainTex", this.m_GrinEyes);
        this.m_body.materials[2].SetTexture("_MainTex", this.m_GrinMouth);
        
        // Get all the layer indices so that this doesn't have to be done at runtime
        this.LayerIndex_Shooting = this.m_anim.GetLayerIndex("Shooting");
        this.LayerIndex_MeleeLeft = this.m_anim.GetLayerIndex("MeleeLeft");
        this.LayerIndex_MeleeRight = this.m_anim.GetLayerIndex("MeleeRight");
        this.LayerIndex_Melee2Left = this.m_anim.GetLayerIndex("Melee2Left");
        this.LayerIndex_Melee2Right = this.m_anim.GetLayerIndex("Melee2Right");
        this.LayerIndex_AimLeft = this.m_anim.GetLayerIndex("AimLeft");
        this.LayerIndex_AimRight = this.m_anim.GetLayerIndex("AimRight");
        this.LayerIndex_Pain = this.m_anim.GetLayerIndex("Pain");
        this.LayerIndex_Pickup = this.m_anim.GetLayerIndex("Pickup");
        this.LayerIndex_HardLanding = this.m_anim.GetLayerIndex("HardLanding");
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        // Set the animation speed based on the timescale
        this.m_anim.SetFloat("AnimSpeed", Time.timeScale);
        
        // Set the base mesh angle depending whether we're facing left or right
        if (this.m_plycombat.GetCombatState() != PlayerCombat.CombatState.Pain && this.m_anim.GetLayerWeight(LayerIndex_HardLanding) != 1.0f)
        {
            if (this.m_anim.GetLayerWeight(LayerIndex_Pickup) != 1.0f)
                this.transform.localEulerAngles = new Vector3(0, this.m_CurrentBodyRot, 0);
            else
                this.transform.localEulerAngles = new Vector3(0, 90, 0);
        }
        this.m_CurrentBodyRot = Mathf.Lerp(this.m_CurrentBodyRot, this.m_TargetBodyRot, 0.1f);
        
        // Set the directional aim blending
        Vector3 aimdir = this.m_plycombat.GetAimDirection();
        float aimang = Vector3.Angle(aimdir, Vector3.right);
        this.m_anim.SetFloat("AimX", aimdir.x);
        this.m_anim.SetFloat("AimY", aimdir.y);
        if (aimang < 90.0f)
        {
            this.m_TargetBodyRot = -179;
            this.m_anim.SetLayerWeight(this.LayerIndex_AimRight, 1.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_AimLeft, 0.0f);
        }
        else
        {
            this.m_TargetBodyRot = 0.0f;
            this.m_anim.SetLayerWeight(this.LayerIndex_AimRight, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_AimLeft, 1.0f);
        }
        
        // Shooting animation
        if (this.m_plycombat.GetCombatState() == PlayerCombat.CombatState.Shooting)
        {
            if (this.m_anim.GetLayerWeight(this.LayerIndex_Shooting) != 1.0f)
                this.m_anim.Play("Firing", this.LayerIndex_Shooting, 0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Shooting, 1.0f);
        }
        else
            this.m_anim.SetLayerWeight(this.LayerIndex_Shooting, 0.0f);
        
        // Melee animations
        if (this.m_plycombat.GetCombatState() == PlayerCombat.CombatState.Melee)
        {
            // Set the left/right attack animations
            if (aimang < 90.0f)
            {
                if (this.m_anim.GetLayerWeight(this.LayerIndex_MeleeRight) != 1.0f)
                    this.m_anim.Play("Melee", this.LayerIndex_MeleeRight, 0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_MeleeRight, 1.0f);
            }
            else
            {
                if (this.m_anim.GetLayerWeight(this.LayerIndex_MeleeLeft) != 1.0f)
                    this.m_anim.Play("Melee", this.LayerIndex_MeleeLeft, 0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_MeleeLeft, 1.0f);
            }
            
            // Don't allow for the melee 2 layer to play
            this.m_anim.SetLayerWeight(this.LayerIndex_Melee2Right, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Melee2Left, 0.0f);
            
            // Hide the guns and show the sword, and enable the trail
            this.m_meshsword.enabled = true;
            this.m_meshguns.enabled = false;
            if (!this.m_swordtrail.IsEnabled())
                this.m_swordtrail.EnableTrail(true);
        }
        else if (this.m_plycombat.GetCombatState() == PlayerCombat.CombatState.Melee2)
        {
            // Set the left/right attack animations
            if (aimang < 90.0f)
            {
                if (this.m_anim.GetLayerWeight(this.LayerIndex_Melee2Right) != 1.0f)
                    this.m_anim.Play("Melee", this.LayerIndex_Melee2Right, 0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_Melee2Right, 1.0f);
            }
            else
            {
                if (this.m_anim.GetLayerWeight(this.LayerIndex_Melee2Left) != 1.0f)
                    this.m_anim.Play("Melee", this.LayerIndex_Melee2Left, 0f);
                this.m_anim.SetLayerWeight(this.LayerIndex_Melee2Left, 1.0f);
            }
            // Don't allow for the melee 2 layer to play
            this.m_anim.SetLayerWeight(this.LayerIndex_MeleeRight, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_MeleeLeft, 0.0f);
        }
        else
        {
            // Disable all the melee layers
            this.m_anim.SetLayerWeight(this.LayerIndex_MeleeRight, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_MeleeLeft, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Melee2Right, 0.0f);
            this.m_anim.SetLayerWeight(this.LayerIndex_Melee2Left, 0.0f);
            
            // Hide the sword, show the guns, and disable the weapon trail
            this.m_meshsword.enabled = false;
            this.m_meshguns.enabled = true;
            if (this.m_swordtrail.IsEnabled())
                this.m_swordtrail.EnableTrail(false);
        }
        
        // Pain animation
        if (this.m_plycombat.GetCombatState() == PlayerCombat.CombatState.Pain)
        {
            this.m_anim.SetLayerWeight(this.LayerIndex_Pain, 1.0f);
            this.m_body.materials[1].SetTexture("_MainTex", this.m_PainEyes);
            this.m_body.materials[2].SetTexture("_MainTex", this.m_PainMouth);
        }
        else
        {
            this.m_anim.SetLayerWeight(this.LayerIndex_Pain, 0.0f);
            this.m_body.materials[1].SetTexture("_MainTex", this.m_GrinEyes);
            this.m_body.materials[2].SetTexture("_MainTex", this.m_GrinMouth);
        }
        
        // Running animations
        this.m_anim.SetBool("IsFlying", this.m_plycont.IsFlying());
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
        if (this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Jump2)
            this.m_anim.SetBool("IsJumping2", true);
        else
            this.m_anim.SetBool("IsJumping2", false);
        if (this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Fall)
            this.m_anim.SetBool("IsFalling", true);
        else
            this.m_anim.SetBool("IsFalling", false);
        if (this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Land || this.m_plycont.GetPlayerJumpState() == PlayerController.PlayerJumpState.Idle)
        {
            // Perform a stronger landing animation if we hit the ground hard
            if (this.m_BigDrop)
            {
                this.m_anim.SetLayerWeight(this.LayerIndex_HardLanding, 1.0f);
                this.m_anim.Play("HardLanding", this.LayerIndex_HardLanding, 0f);
                this.m_BigDrop = false;
                FindObjectOfType<AudioManager>().Play("Physics/HardLanding", this.transform.position);
            }
            if (this.m_anim.GetLayerWeight(this.LayerIndex_HardLanding) == 1.0f && this.m_anim.GetCurrentAnimatorStateInfo(this.LayerIndex_HardLanding).normalizedTime > 1)
                this.m_anim.SetLayerWeight(this.LayerIndex_HardLanding, 0.0f);
            this.m_anim.SetBool("IsGrounded", true);
        }
        else
            this.m_anim.SetBool("IsGrounded", false);
    }
    

    /*==============================
        BigDrop
        Set the current fall as a big one
        @param Whether or not to set the fall as a big one
    ==============================*/
    
    public void BigDrop(bool enable)
    {
        this.m_BigDrop = enable;
    }
}
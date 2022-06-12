/****************************************************************
                       PlayerAnimations.cs
    
This script handles the player model's animations
****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public PlayerController m_PlyCont;
    public Animator m_Animator;
    public SkinnedMeshRenderer m_MeshBody;
    public List<SkinnedMeshRenderer> m_ReserveBullets;
    public Material m_LightOnMaterial;
    public Material m_LightOffMaterial;
    
    private Vector3 m_LastMoveDir = Vector3.zero;
    private int m_LastAmmoClip = PlayerController.ClipSize;
    private int m_LastAmmoReserve = 0;
    
    private AudioManager m_Audio;
    
    private int LayerIndex_Legs;
    private int LayerIndex_Aim;
    private int LayerIndex_Shoot1;
    private int LayerIndex_Shoot2;
    private int LayerIndex_Shoot3;
    private int LayerIndex_Reload;
    
    
    /*==============================
        Start
        Called when the player is initialized
    ==============================*/
    
    void Start()
    {    
        this.m_Audio = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        
        // Make a copy of all the materials so we can dynamically alter them
        for (int i=0; i<this.m_MeshBody.materials.Length; i++)
            this.m_MeshBody.materials[i] = new Material(this.m_MeshBody.materials[i]);
        
        // Initialize the diegetic interface
        HandleAmmoClipInterface();
        HandleAmmoReserveInterface();
            
        // Get all the layer indices so that this doesn't have to be done at runtime
        this.LayerIndex_Legs = this.m_Animator.GetLayerIndex("Legs");
        this.LayerIndex_Aim = this.m_Animator.GetLayerIndex("Aim");
        this.LayerIndex_Shoot1 = this.m_Animator.GetLayerIndex("Shoot1");
        this.LayerIndex_Shoot2 = this.m_Animator.GetLayerIndex("Shoot2");
        this.LayerIndex_Shoot3 = this.m_Animator.GetLayerIndex("Shoot3");
        this.LayerIndex_Reload = this.m_Animator.GetLayerIndex("Reload");
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        // Handle leg movement
        if (this.m_PlyCont.GetPlayerMovementState() == PlayerController.PlayerMovementState.Moving)
        {
            Vector3 dir = this.m_PlyCont.GetPlayerMovementDirection();
            this.m_LastMoveDir = Vector3.Lerp(this.m_LastMoveDir, dir, 0.5f);
            this.m_Animator.SetBool("Moving", true);
            this.m_Animator.SetFloat("MoveX", this.m_LastMoveDir.x);
            this.m_Animator.SetFloat("MoveY", this.m_LastMoveDir.y);
        }
        else
        {
            this.m_Animator.SetBool("Moving", false);
            this.m_LastMoveDir = Vector3.Lerp(this.m_LastMoveDir, Vector3.zero, 0.5f);
        }
        
        // Handle aiming
        if (this.m_PlyCont.GetPlayerAiming())
        {
            float yaim = this.m_PlyCont.GetPlayerVerticalAim();
            this.m_Animator.SetFloat("AimY", yaim > 180 ? ((90-yaim%90)/CameraController.LookMax_Up) : yaim/CameraController.LookMax_Down); // Lerp this
            this.m_Animator.SetBool("Aiming", true);
        }
        else
            this.m_Animator.SetBool("Aiming", false);
        
        // Handle ammo diegetic interface
        if (this.m_LastAmmoClip != this.m_PlyCont.GetPlayerAmmoClip())
            HandleAmmoClipInterface();
        if (this.m_LastAmmoReserve != this.m_PlyCont.GetPlayerAmmoReserve())
            HandleAmmoReserveInterface();
        
        if (this.m_Animator.GetLayerWeight(LayerIndex_Reload) > 0.0f && this.m_PlyCont.GetPlayerCombatState() == PlayerController.PlayerCombatState.Idle)
            this.m_Animator.SetLayerWeight(LayerIndex_Reload, 0.0f);
    }
    
    
    /*==============================
        HandleAmmoClipInterface
        Handles the ammo clip lights on the player's back
    ==============================*/
    
    private void HandleAmmoClipInterface()
    {
        Material[] mats = (Material[]) this.m_MeshBody.materials.Clone();
        this.m_LastAmmoClip = this.m_PlyCont.GetPlayerAmmoClip();
        for (int i=0; i<=PlayerController.ClipSize-1; i++)
        {
            switch (i)
            {
                case 7: mats[2+0] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
                case 6: mats[2+4] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
                case 5: mats[2+1] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
                case 4: mats[2+5] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
                case 3: mats[2+2] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
                case 2: mats[2+6] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
                case 1: mats[2+3] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
                case 0: mats[2+7] = (i < this.m_LastAmmoClip) ? this.m_LightOnMaterial : this.m_LightOffMaterial; break;
            }
        }
        this.m_MeshBody.materials = mats;
    }
    
    
    /*==============================
        HandleAmmoReserveInterface
        Handles the shotgun shells the player's belt
    ==============================*/
    
    private void HandleAmmoReserveInterface()
    {
        this.m_LastAmmoReserve = this.m_PlyCont.GetPlayerAmmoReserve();
        for (int i=0; i<PlayerController.ClipSize; i++)
            this.m_ReserveBullets[i].enabled = (i < this.m_LastAmmoReserve);
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
        FireAnimation
        Called when the shooting event happens
    ==============================*/
    
    public void FireAnimation()
    {
        int[] shootanimlist = {LayerIndex_Shoot1, LayerIndex_Shoot2, LayerIndex_Shoot3};
        int chosen = shootanimlist[Random.Range(0, 2)];
        for (int i=0; i<3; i++)
        {
            float weight = 0.0f;
            if (shootanimlist[i] == chosen)
                weight = 1.0f;
            this.m_Animator.SetLayerWeight(shootanimlist[i], weight);
        }
        this.m_Animator.Play("Shoot", chosen, 0f);
    }
    
    
    /*==============================
        StartReloadAnimation
        Called when the reload start event happens
        @param Whether the clip was empty
    ==============================*/
    
    public void StartReloadAnimation(bool empty)
    {
        if (empty)
            this.m_Animator.SetTrigger("ReloadEmpty");
        else
            this.m_Animator.SetTrigger("Reload");
        this.m_Animator.SetBool("IsReloading", true);
        this.m_Animator.SetLayerWeight(LayerIndex_Reload, 1.0f);
        this.m_Animator.Play("Empty", LayerIndex_Reload, 0f);
    }
    
    
    /*==============================
        ReloadAnimation
        Called when the reload event happens
        @param Whether the clip was empty
    ==============================*/
    
    public void ReloadAnimation(bool empty)
    {
        this.m_Animator.SetTrigger("InsertShell");
    }
    
    
    /*==============================
        EndReloadAnimation
        Called when the reload end event happens
    ==============================*/
    
    public void EndReloadAnimation()
    {
        this.m_Animator.SetBool("IsReloading", false);
    }
    
    
    /*==============================
        RemoveShell
        Called when a shell is removed
    ==============================*/
    
    public void RemoveShell()
    {
        this.m_PlyCont.SetPlayerAmmoReserve(this.m_PlyCont.GetPlayerAmmoReserve()-1);
    }
    
    
    /*==============================
        InsertShell
        Called when a shell is inserted
    ==============================*/
    
    public void InsertShell()
    {
        this.m_PlyCont.SetPlayerAmmoClip(this.m_PlyCont.GetPlayerAmmoClip()+1);
    }
}
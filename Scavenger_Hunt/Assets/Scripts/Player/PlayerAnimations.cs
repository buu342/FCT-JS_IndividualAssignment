/****************************************************************
                       PlayerAnimations.cs
    
This script handles the player model's animations
****************************************************************/

using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public PlayerController m_PlyCont;
    public Animator m_Animator;
    public SkinnedMeshRenderer m_MeshBody;
    
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
        // Make a copy of all the materials so we can dynamically alter them
        for (int i=0; i<this.m_MeshBody.materials.Length; i++)
            this.m_MeshBody.materials[i] = new Material(this.m_MeshBody.materials[i]);
        //this.m_MeshBody.materials[1].SetTexture("_MainTex", this.m_GrinEyes);
        //this.m_MeshBody.materials[2].SetTexture("_MainTex", this.m_GrinMouth);
        
        // Get all the layer indices so that this doesn't have to be done at runtime
        this.LayerIndex_Legs = this.m_Animator.GetLayerIndex("Legs");
        this.LayerIndex_Aim = this.m_Animator.GetLayerIndex("Aim");
        this.LayerIndex_Shoot2 = this.m_Animator.GetLayerIndex("Shoot2");
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
            this.m_Animator.SetBool("Moving", true);
            this.m_Animator.SetFloat("MoveX", dir.x);
            this.m_Animator.SetFloat("MoveY", dir.y);
        }
        else
            this.m_Animator.SetBool("Moving", false);
    }
}

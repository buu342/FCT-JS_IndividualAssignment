/****************************************************************
                 Sequence_ShellExit_Level1_1.cs
    
This script handles the level1_1 exit scene
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Sequence_ShellExit_Level1_1 : MonoBehaviour
{
    private int LayerIndex_Pickup;
    
    private Animator m_plyanim;
    private SkinnedMeshRenderer m_plymesh;
    private SkinnedMeshRenderer m_jetpack;
    private float m_SequenceTime;
    private int m_CurrentSequence = 0;
    private RawImage m_Clouds;
    
    /*==============================
        Start
        Called when the sequence is initialized
    ==============================*/
    
    void Start()
    {
        GameObject mdl = this.transform.Find("Model").gameObject;
        GameObject ply = mdl.transform.Find("Shell").gameObject;
        this.GetComponent<PlayerController>().SetControlsEnabled(false);
        this.GetComponent<PlayerCombat>().SetTimeScaleOverride(1.0f);
        this.m_plymesh = ply.GetComponent<SkinnedMeshRenderer>();
        this.m_plyanim = mdl.GetComponent<Animator>();
        this.m_jetpack = mdl.transform.Find("Jetpack").gameObject.GetComponent<SkinnedMeshRenderer>();
        this.m_SequenceTime = Time.time + 1.8f;
        LayerIndex_Pickup = this.m_plyanim.GetLayerIndex("Pickup");
        this.m_plyanim.SetLayerWeight(LayerIndex_Pickup, 1.0f);
        this.GetComponent<Rigidbody>().velocity = Vector3.zero;
        this.m_Clouds = GameObject.Find("HUD").transform.Find("Clouds").gameObject.GetComponent<RawImage>();
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (this.m_SequenceTime < Time.time)
        {
            switch (this.m_CurrentSequence)
            {
                case 0:
                    this.m_SequenceTime = Time.time + 0.2f;
                    this.m_plyanim.SetLayerWeight(LayerIndex_Pickup, 0.0f);
                    this.GetComponent<PlayerCombat>().SayLine("Voice/Shell/Jump", true);
                    this.GetComponent<PlayerController>().SetPlayerJumpState(PlayerController.PlayerJumpState.Jump);
                    this.GetComponent<Rigidbody>().AddForce(this.transform.up*PlayerController.JumpPower);
                    this.m_jetpack.enabled = true;
                    break;
                case 1:
                    this.m_SequenceTime = Time.time + 2.5f;
                    this.GetComponent<PlayerController>().SetPlayerFlying(true);
                    break;
                case 2:
                    GameObject.Find("SceneController").GetComponent<SceneController>().StartNextScene();
                    break;
            }
            this.m_CurrentSequence++;
        }
        
        // Fly with the jetpack
        if (this.m_CurrentSequence > 0)
        {
            this.m_Clouds.rectTransform.localPosition = Vector3.Lerp(this.m_Clouds.rectTransform.localPosition, new Vector3(0, -256, 0), Time.deltaTime/2.0f);
            if (this.m_Clouds.rectTransform.localPosition.y < 0)
                this.m_Clouds.rectTransform.localPosition = Vector3.zero;
            this.m_Clouds.uvRect = new Rect(Time.time/5.0f, 0, 1, -1);
            this.GetComponent<Rigidbody>().AddForce(this.transform.up*4.0f);
            this.GetComponent<Rigidbody>().AddForce(this.transform.forward*2.0f);
        }
    }
}
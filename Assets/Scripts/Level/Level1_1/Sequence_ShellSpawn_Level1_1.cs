/****************************************************************
                 Sequence_ShellSpawn_Level1_1.cs
    
This script handles the tutorial section.
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Sequence_ShellSpawn_Level1_1 : MonoBehaviour
{
    private int m_CurrSequence = 0;
    private float m_NextSequenceTime;
    private Image m_ImageTutorial_Aim;
    private Image m_ImageTutorial_Shoot;
    
    private float m_TargetImageAlpha = 0.0f;
    
    
    /*==============================
        Start
        Called when the sequence is initialized
    ==============================*/
    
    void Start()
    {
        this.m_ImageTutorial_Aim = GameObject.Find("HUD").transform.Find("Tutorial_Aim").gameObject.GetComponent<Image>();
        this.m_ImageTutorial_Shoot = GameObject.Find("HUD").transform.Find("Tutorial_Shoot").gameObject.GetComponent<Image>();
        FindObjectOfType<AudioManager>().Play("Gameplay/Slowmo_In");
        this.GetComponent<PlayerCombat>().SetTimeScaleOverride(0.5f);
        this.GetComponent<PlayerController>().SetControlsEnabled(false);
        this.GetComponent<Rigidbody>().AddForce(1, 15, 0, ForceMode.Impulse);
        this.m_NextSequenceTime = Time.unscaledTime + 0.7f;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        Vector2 center = Camera.main.WorldToScreenPoint(this.transform.Find("Shoulder").gameObject.transform.position);
        float radius = 96.0f*this.m_ImageTutorial_Aim.transform.parent.GetComponent<Canvas>().scaleFactor;
        
        // Handle aim icon transformations
        if (this.m_CurrSequence < 4)
        {
            this.m_ImageTutorial_Aim.color = Color.Lerp(this.m_ImageTutorial_Aim.color, new Color(1.0f, 1.0f, 1.0f, this.m_TargetImageAlpha), Time.unscaledDeltaTime*5);
            float angle = Mathf.Sin(Time.unscaledTime*3) + 1.7f;
            center += new Vector2(radius*Mathf.Sin(angle), radius*Mathf.Cos(angle));
            this.m_ImageTutorial_Aim.rectTransform.position = center;
        }
        
        // Handle shoot icon transformations
        if (this.m_CurrSequence > 3)
        {
            this.m_ImageTutorial_Aim.color = Color.clear;
            this.m_ImageTutorial_Shoot.color = Color.Lerp(this.m_ImageTutorial_Shoot.color, new Color(1.0f, 1.0f, 1.0f, this.m_TargetImageAlpha), Time.unscaledDeltaTime*5);
        }
        
        // Do the rest of the tutorial animation
        if (this.m_NextSequenceTime < Time.unscaledTime && this.m_CurrSequence < 4)
        {
            switch (this.m_CurrSequence)
            {
                case 0:
                    this.GetComponent<PlayerCombat>().SetTimeScaleOverride(0.0f);
                    this.m_NextSequenceTime = Time.unscaledTime + 0.7f;
                    break;
                case 1:
                    this.m_TargetImageAlpha = 1.0f;
                    this.m_NextSequenceTime = Time.unscaledTime + 2f;
                    break;
                case 2:
                    this.m_TargetImageAlpha = 0.0f;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.7f;
                    break;
                case 3:
                    center += new Vector2(radius*Mathf.Sin(1.855f), radius*Mathf.Cos(1.855f));
                    this.m_ImageTutorial_Shoot.rectTransform.position = center;
                    this.m_TargetImageAlpha = 1.0f;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.6f;
                    break;
            }
            this.m_CurrSequence++;
        }
        
        // Stop the tutorial
        if (this.m_CurrSequence == 4 && this.m_NextSequenceTime < Time.unscaledTime && Input.GetButton("Fire"))
        {
            this.GetComponent<PlayerCombat>().SetTimeScaleOverride(-1.0f);
            this.GetComponent<PlayerController>().SetControlsEnabled(true);
            this.m_ImageTutorial_Shoot.color = Color.clear;
            Destroy(this);
        }
    }
}
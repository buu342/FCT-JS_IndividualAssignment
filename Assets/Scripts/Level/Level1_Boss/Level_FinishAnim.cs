/****************************************************************
                       Level_FinishAnim.cs
    
This script handles level finishing animations.
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Level_FinishAnim : MonoBehaviour
{
    private const float LogoSpeed = 12.5f;
    
    public Image m_Fade;
    public Image m_LogoTop;
    public Image m_LogoBot;
    public Text  m_CompleteText;
    
    private int   m_CurrSequence = 0;
    private float m_NextSequenceTime = 0.0f;
    private float m_TargetLogoTopX;
    private float m_TargetLogoBotX;
    private float m_ScaleFactor;
    
    
    /*==============================
        Start
        Called when the script is initialized
    ==============================*/
    
    void Start()
    {
        this.m_ScaleFactor = GameObject.Find("HUD").GetComponent<Canvas>().scaleFactor;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (this.m_NextSequenceTime != 0.0f && this.m_NextSequenceTime < Time.unscaledTime)
        {
            switch (this.m_CurrSequence)
            {
                case 0:
                    FindObjectOfType<MusicManager>().PlaySong("Music/LevelComplete", false);
                    this.m_Fade.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
                    this.m_NextSequenceTime = Time.unscaledTime + 0.651f;
                    break;
                case 1:
                    this.m_TargetLogoTopX = 0.0f;
                    this.m_TargetLogoBotX = 0.0f;
                    this.m_NextSequenceTime = Time.unscaledTime + 0.651f;
                    break;
                case 2:
                    this.m_CompleteText.text = "Tokens Collected: " + FindObjectOfType<SceneController>().GetCollectedTokenCount();
                    this.m_NextSequenceTime = Time.unscaledTime + 0.651f;
                    FindObjectOfType<AudioManager>().Play("Gameplay/Level_Info");
                    break;
                case 3:
                    this.m_CompleteText.text += "\nTotal Deaths: " + FindObjectOfType<SceneController>().GetDeathCount();
                    this.m_NextSequenceTime = Time.unscaledTime + 0.651f;
                    FindObjectOfType<AudioManager>().Play("Gameplay/Level_Info");
                    break;
                case 4:
                    this.m_CompleteText.text += "\nFinal Score: " + FindObjectOfType<PlayerCombat>().GetScore();
                    this.m_NextSequenceTime = Time.unscaledTime + 2.0f;
                    FindObjectOfType<AudioManager>().Play("Gameplay/Level_Info");
                    break;
                case 5:
                    FindObjectOfType<SceneController>().StartingNewLevel();
                    FindObjectOfType<SceneController>().LoadScene("Menu");
                    Destroy(this);
                    FindObjectOfType<SceneController>().StartNextScene();
                    break;
            }
            this.m_CurrSequence++;
        }
        
        // Move the logos
        if (this.m_CurrSequence > 0)
        {
            this.m_LogoTop.rectTransform.localPosition = Vector2.Lerp(this.m_LogoTop.rectTransform.localPosition, new Vector2(this.m_TargetLogoTopX, this.m_LogoTop.rectTransform.localPosition.y), Level_FinishAnim.LogoSpeed*Time.deltaTime);
            this.m_LogoBot.rectTransform.localPosition = Vector2.Lerp(this.m_LogoBot.rectTransform.localPosition, new Vector2(this.m_TargetLogoBotX, this.m_LogoBot.rectTransform.localPosition.y), Level_FinishAnim.LogoSpeed*Time.deltaTime);
        }
    }
    

    /*==============================
        SetLevelFinished
        Marks the level as finished, so that 
        we can play the level complete transition
    ==============================*/
    
    public void SetLevelFinished()
    {
        FindObjectOfType<PlayerCombat>().SayLine("Voice/Shell/BossKill", true);
        this.m_NextSequenceTime = Time.unscaledTime + 3.0f;
    }
}
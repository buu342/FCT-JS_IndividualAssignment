using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ScreenGUI : MonoBehaviour
{
    public Image m_FadeImage;
    public TextMeshProUGUI m_DeathText;
    private int m_DeathState = 0;
    private float m_DeathStateTimer = 0;
    private bool m_PlayerDead = false;
    private float m_Fade = 255.0f;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (!this.m_PlayerDead)
        {
            if (this.m_Fade > 0)
            {
                this.m_Fade -= 30.0f*Time.unscaledDeltaTime;
                if (this.m_Fade < 0)
                    this.m_Fade = 0;
                this.m_FadeImage.color = new Color(0.0f, 0.0f, 0.0f, this.m_Fade/255.0f);
            }
        }
        if (this.m_DeathStateTimer != 0 && this.m_DeathStateTimer <= Time.unscaledTime)
        {
            switch (this.m_DeathState)
            {
                case 0:
                    this.m_DeathText.enabled = true;
                    this.m_DeathStateTimer = Time.unscaledTime + 2.0f;
                    break;
                case 1:
                    SceneManager.LoadScene("StartMenu");
                    break;
            }
            this.m_DeathState++;
        }
    }
    
    public void PlayerDied()
    {
        this.m_PlayerDead = true;
        this.m_Fade = 255;
        this.m_FadeImage.color = new Color(0.0f, 0.0f, 0.0f, this.m_Fade/255.0f);
        this.m_DeathStateTimer = Time.unscaledTime + 2.0f;
    }
}
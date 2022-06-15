using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using Photon.Pun;
public class ScreenGUI : MonoBehaviour
{
    public Image m_FadeImage;
    public TextMeshProUGUI m_DeathText;
    public TextMeshProUGUI m_WinningText;
    public TextMeshProUGUI m_LevelCompleteText;
    public TextMeshProUGUI m_ItemsCollectedText;
    public TextMeshProUGUI m_CurrentScoreText;
    public TextMeshProUGUI m_FinalScoreText;
    public TextMeshProUGUI m_LoadingText;
    private int   m_DeathState = 0;
    private float m_DeathStateTimer = 0;
    private int   m_NextLevelState = 0;
    private float m_NextLevelTimer = 0;
    private bool  m_PlayerDead = false;
    private float m_Fade = 255.0f;
    private bool multiplayer=JoinMultiplayer.Multiplayer;
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
        if (this.m_DeathStateTimer != 0 && this.m_DeathStateTimer < Time.unscaledTime)
        {
            switch (this.m_DeathState)
            {
                case 0:
                    this.m_DeathText.enabled = true;
                    this.m_DeathStateTimer = Time.unscaledTime + 2.0f;
                    break;
                case 1:
                    this.m_FinalScoreText.enabled = true;
                    this.m_DeathStateTimer = Time.unscaledTime + 1.0f;
                    break;
                case 2:
                    this.m_FinalScoreText.text += "\n$"+GameObject.Find("LevelManager").GetComponent<LevelManager>().GetScore();
                    this.m_DeathStateTimer = Time.unscaledTime + 2.0f;
                    break;
                case 3:
                    if(multiplayer)
                        PhotonNetwork.LeaveRoom();
                    SceneManager.LoadScene("StartMenu");
                    break;
            }
            this.m_DeathState++;
        }
        if(multiplayer)
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2){
            this.m_WinningText.enabled = true;
             this.m_Fade = 255;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("StartMenu");
        }
        if(!multiplayer)
        {
            if (this.m_NextLevelTimer != 0 && this.m_NextLevelTimer < Time.unscaledTime)
            {
                switch (this.m_NextLevelState)
                {
                    case 0:
                        this.m_LevelCompleteText.enabled = true;
                        this.m_NextLevelTimer = Time.unscaledTime + 2.0f;
                        break;
                    case 1:
                        this.m_ItemsCollectedText.enabled = true;
                        this.m_ItemsCollectedText.text += GameObject.Find("LevelManager").GetComponent<LevelManager>().GetCollectedPickupCount() + "/" + GameObject.Find("LevelManager").GetComponent<LevelManager>().GetPickupCount();
                        this.m_NextLevelTimer = Time.unscaledTime + 1.0f;
                        break;
                    case 2:
                        this.m_CurrentScoreText.enabled = true;
                        this.m_CurrentScoreText.text += "$"+GameObject.Find("LevelManager").GetComponent<LevelManager>().GetScore();
                        this.m_NextLevelTimer = Time.unscaledTime + 3.0f;
                        break;
                    case 3:
                        this.m_LoadingText.enabled = true;
                        this.m_NextLevelTimer = Time.unscaledTime + 2.0f;
                        break;
                    case 4:
                        GameObject.Find("LevelManager").GetComponent<LevelManager>().LoadNextLevel();
                        break;
                }
                this.m_NextLevelState++;
            }
        }
            
        if (this.m_NextLevelState > 0 && this.m_NextLevelState < 3)
        {
            if (this.m_Fade < 128)
            {
                this.m_Fade += 200*Time.unscaledDeltaTime;
                if (this.m_Fade > 128)
                    this.m_Fade = 128;
                this.m_FadeImage.color = new Color(0.0f, 0.0f, 0.0f, this.m_Fade/255.0f);
                this.m_LevelCompleteText.rectTransform.localPosition = new Vector2(-700 + 2200*(1-this.m_Fade/128), this.m_LevelCompleteText.rectTransform.localPosition.y);
                this.m_ItemsCollectedText.rectTransform.localPosition = new Vector2(this.m_LevelCompleteText.rectTransform.localPosition.x, this.m_ItemsCollectedText.rectTransform.localPosition.y);
                this.m_CurrentScoreText.rectTransform.localPosition = new Vector2(this.m_LevelCompleteText.rectTransform.localPosition.x, this.m_CurrentScoreText.rectTransform.localPosition.y);
            }
        }
        else if (this.m_NextLevelState == 4)
        {
            this.m_Fade += 4;
            if (this.m_Fade > 255)
                this.m_Fade = 255;
            this.m_FadeImage.color = new Color(0.0f, 0.0f, 0.0f, this.m_Fade/255.0f);
            this.m_LoadingText.color = new Color(1.0f, 1.0f, 1.0f, (2*(this.m_Fade-128))/255.0f);
            this.m_LevelCompleteText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f-(2*(this.m_Fade-128))/255.0f);
            this.m_ItemsCollectedText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f-(2*(this.m_Fade-128))/255.0f);
            this.m_CurrentScoreText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f-(2*(this.m_Fade-128))/255.0f);
        }
        
    }
    
    public void PlayerDied()
    {   
        this.m_PlayerDead = true;
        this.m_Fade = 255;
        this.m_FadeImage.color = new Color(0.0f, 0.0f, 0.0f, this.m_Fade/255.0f);
        this.m_DeathStateTimer = Time.unscaledTime + 2.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void LoadNextLevel()
    {   if(!multiplayer)    
            if (this.m_NextLevelState == 0 && this.m_NextLevelTimer == 0)
            {
                this.m_LevelCompleteText.enabled = true;
                this.m_NextLevelTimer = Time.unscaledTime + 3.0f;
                GameObject.Find("MusicManager").GetComponent<MusicManager>().FadeMusic();
                this.transform.parent.gameObject.GetComponent<SceneDirector>().PlayerCompleted();
            }
    }
}

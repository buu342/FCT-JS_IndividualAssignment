/****************************************************************
                             HUD.cs
    
This script handles the heads up display logic.
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private const float HealthTime = 10.0f;
    private const float StreakTime = 10.0f;
    private const float TextScaleTime = 10.0f;
    private const float TextFadeTime = 10.0f;
    private const float BossTextScaleTime = 10.0f;
    private const float BossTextFadeTime = 10.0f;
    
    // Public values
    public Image m_HealthBar;
    public Image m_HealthIcon;
    public Image m_StaminaBar;
    public Image m_ScoreBar;
    public Image m_ScoreIcon;
    public Image m_BossHealthBar;
    public Text  m_BossTitle;
    public Image m_Fade;
    public Text m_TokenText;
    public GameObject m_Player;
    public Sprite[] StreakSprite = new Sprite[6];
    public GameObject m_Boss;
    public Image m_BlackBarTop;
    public Image m_BlackBarBottom;
    
    // Useful color values
    private Color32 HealthColor          = new Color32(255, 0, 0, 255);
    private Color32 StaminaColor         = new Color32(0, 255, 0, 255);
    private Color32 StaminaRecoveryColor = new Color32(0, 192, 0, 255);
    private Color32 StreakColor1         = new Color32(0, 182, 255, 255);
    private Color32 StreakColor2         = new Color32(0, 254, 254, 255);
    private Color32 StreakColor3         = new Color32(0, 254, 8, 255);
    private Color32 StreakColor4         = new Color32(255, 255, 0, 255);
    private Color32 StreakColor5         = new Color32(246, 147, 0, 255);
    private Color32 StreakColor6         = new Color32(228, 0, 0, 255);
    
    // Private values
    private bool  m_PlayerRespawned = false;
    private float m_PlayerDieTime = 0;
    private int   m_CurrentStreak = 0;
    private float m_CurrentHealth;
    private float m_TargetHealth;
    private float m_CurrentBossHealth;
    private float m_TargetBossHealth;
    private float m_StreakSize = 1.0f;
    private float m_TokenTime;
    private float m_FadeBossName = 0.0f;
    private PlayerCombat m_plycombat;
    private BossLogic m_bosslogic;
    
    
    /*==============================
        Start
        Called when the HUD is initialized
    ==============================*/
    
    void Start()
    {
        this.m_plycombat = this.m_Player.GetComponent<PlayerCombat>();
        this.m_TargetHealth = this.m_plycombat.GetHealth();
        this.m_CurrentHealth = this.m_TargetHealth;
        if (this.m_Boss != null)
        {
            this.m_bosslogic = this.m_Boss.GetComponent<BossLogic>();
            this.m_CurrentBossHealth = this.m_bosslogic.GetHealth();
            this.m_TargetBossHealth = this.m_CurrentBossHealth;
        }
    }


    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        // Health bar
        this.m_TargetHealth = Mathf.Max(0.0f, this.m_plycombat.GetHealth());
        this.m_CurrentHealth = Mathf.Lerp(this.m_CurrentHealth, this.m_TargetHealth, HUD.HealthTime*Time.deltaTime);
        this.m_HealthBar.rectTransform.localScale = new Vector3(this.m_CurrentHealth/100.0f, 1.0f, 1.0f);

        // Stamina bar
        this.m_StaminaBar.rectTransform.localScale = new Vector3(this.m_plycombat.GetStamina()/100.0f, 1.0f, 1.0f);
        if (!this.m_plycombat.GetStaminaRecovering())
            this.m_StaminaBar.GetComponent<Image>().color = StaminaColor;
        else
            this.m_StaminaBar.GetComponent<Image>().color = StaminaRecoveryColor;
        
        // Kill streak bar
        int laststreak = this.m_CurrentStreak;
        this.m_ScoreBar.rectTransform.localScale  = new Vector3(this.m_plycombat.GetStreak()/100.0f, 1.0f, 1.0f);
        Vector3 barscale = this.m_ScoreBar.rectTransform.localScale;
        if (barscale.x == 1.0f)
        {
            this.m_CurrentStreak = 5;
            this.m_ScoreBar.GetComponent<Image>().color = StreakColor6;
        }
        else if (barscale.x >= 4.0f/5.0f) // Why am I not doing this in a loop? Because like this, the compiler can precalculate the divisions. It's faster.
        {
            this.m_CurrentStreak = 4;
            this.m_ScoreBar.GetComponent<Image>().color = Color.Lerp(StreakColor5, StreakColor6, (barscale.x - 4.0f/5.0f)/(1.0f/5.0f));
        }
        else if (barscale.x >= 3.0f/5.0f)
        {
            this.m_CurrentStreak = 3;
            this.m_ScoreBar.GetComponent<Image>().color = Color.Lerp(StreakColor4, StreakColor5, (barscale.x - 3.0f/5.0f)/(1.0f/5.0f));
        }
        else if (barscale.x >= 2.0f/5.0f)
        {
            this.m_CurrentStreak = 2;
            this.m_ScoreBar.GetComponent<Image>().color = Color.Lerp(StreakColor3, StreakColor4, (barscale.x - 2.0f/5.0f)/(1.0f/5.0f));
        }
        else if (barscale.x >= 1.0f/5.0f)
        {
            this.m_CurrentStreak = 1;
            this.m_ScoreBar.GetComponent<Image>().color = Color.Lerp(StreakColor2, StreakColor3, (barscale.x - 1.0f/5.0f)/(1.0f/5.0f));
        }
        else
        {
            this.m_CurrentStreak = 0;
            this.m_ScoreBar.GetComponent<Image>().color = Color.Lerp(StreakColor1, StreakColor2, (barscale.x - 0.0f/5.0f)/(1.0f/5.0f));
        }
        
        // Kill streak icon
        this.m_ScoreIcon.sprite = StreakSprite[this.m_CurrentStreak];
        if (this.m_StreakSize > 1.0f)
            this.m_StreakSize = Mathf.Lerp(this.m_StreakSize, 1.0f, HUD.StreakTime*Time.deltaTime);
        this.m_ScoreIcon.rectTransform.localScale = new Vector3(-this.m_StreakSize, this.m_StreakSize, 1.0f);
        if (this.m_CurrentStreak > laststreak)
            this.m_StreakSize = 1.5f;
        
        // Fade out token text
        if (this.m_TokenTime < Time.unscaledTime)
        {
            Color col = this.m_TokenText.color;
            this.m_TokenText.color = Color.Lerp(col, new Color(1.0f, 1.0f, 1.0f, 0.0f), HUD.TextFadeTime*Time.deltaTime);
        }
        else
        {
            this.m_TokenText.rectTransform.localScale = Vector2.Lerp(this.m_TokenText.rectTransform.localScale, new Vector2(1.0f, 1.0f), HUD.TextScaleTime*Time.deltaTime);
        }
        
        // Boss name
        if (this.m_Boss != null && this.m_bosslogic.GetEnabled())
        {
            this.m_BossTitle.gameObject.SetActive(true);
            this.m_BossHealthBar.transform.parent.gameObject.SetActive(true);
            if (this.m_BossTitle.rectTransform.localScale.x > 1.1f)
            {
                this.m_BossTitle.rectTransform.localScale = Vector2.Lerp(this.m_BossTitle.rectTransform.localScale, new Vector2(1.0f, 1.0f), HUD.BossTextScaleTime*Time.deltaTime);
            }
            else
            {
                if (this.m_FadeBossName == 0.0f)
                    this.m_FadeBossName = Time.unscaledTime + 0.5f;
                if (this.m_FadeBossName != 0.0f && this.m_FadeBossName < Time.unscaledTime)
                {
                    this.m_BossTitle.color = Color.Lerp(this.m_BossTitle.color, new Color(1.0f, 1.0f, 1.0f, 0.0f), HUD.BossTextFadeTime*Time.deltaTime);
                    this.m_BossTitle.transform.GetChild(0).GetComponent<Text>().color = this.m_BossTitle.color;
                }
            }
        }
        
        // Boss health bar
        if (this.m_Boss != null && this.m_CurrentBossHealth > 0)
        {
            this.m_TargetBossHealth = this.m_bosslogic.GetHealth();
            this.m_CurrentBossHealth = Mathf.Lerp(this.m_CurrentBossHealth, this.m_TargetBossHealth, HUD.HealthTime*Time.deltaTime);
            this.m_BossHealthBar.rectTransform.localScale = new Vector3(this.m_CurrentBossHealth/this.m_bosslogic.GetMaxHealth(), 1.0f, 1.0f);
            
            // Boss is dead, move the bar down
            if (this.m_CurrentBossHealth < 1)
            {
                Vector2 curpos = this.m_BossHealthBar.transform.parent.position;
                this.m_BossHealthBar.transform.parent.position = new Vector2(curpos.x, Mathf.Lerp(curpos.y, -32*this.GetComponent<Canvas>().scaleFactor, 10.0f*Time.deltaTime));
            }
        }
        
        // Death bar effect
        if (this.m_PlayerDieTime != 0 && this.m_PlayerDieTime < Time.unscaledTime)
        {
            this.m_BlackBarTop.rectTransform.localPosition = new Vector2(0.0f, Mathf.Lerp(this.m_BlackBarTop.rectTransform.localPosition.y, 0.0f, 5.0f*Time.unscaledDeltaTime));
            this.m_BlackBarBottom.rectTransform.localPosition = new Vector2(0.0f, Mathf.Lerp(this.m_BlackBarBottom.rectTransform.localPosition.y, 0.0f, 5.0f*Time.unscaledDeltaTime));
        }
        
        // Respawn bar effect
        if (this.m_PlayerRespawned)
        {
            this.m_BlackBarTop.rectTransform.localPosition = new Vector2(0.0f, Mathf.Lerp(this.m_BlackBarTop.rectTransform.localPosition.y, 1024.0f, 5.0f*Time.unscaledDeltaTime));
            this.m_BlackBarBottom.rectTransform.localPosition = new Vector2(0.0f, Mathf.Lerp(this.m_BlackBarBottom.rectTransform.localPosition.y, -1024.0f, 5.0f*Time.unscaledDeltaTime));
        }
    }


    /*==============================
        CollectedToken
        Signals that a token was just collected
        @param The current number of collected tokens
    ==============================*/
    
    public void CollectedToken(int tokencount)
    {
        this.m_TokenText.text = "Token Collected\n"+tokencount+" out of 3";
        this.m_TokenText.rectTransform.localScale = Vector2.zero;
        this.m_TokenText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        this.m_TokenTime = Time.unscaledTime + 2.0f;
    }


    /*==============================
        SetFade
        Set's the fade alpha
        @param the fade amount
    ==============================*/
    
    public void SetFade(float amount)
    {
        this.m_Fade.color = new Color(this.m_Fade.color.r, this.m_Fade.color.g, this.m_Fade.color.b, amount);
    }
    

    /*==============================
        PlayerDied
        TODO
    ==============================*/
    
    public void PlayerDied()
    {
        this.gameObject.transform.Find("HealthBar").gameObject.SetActive(false);
        this.gameObject.transform.Find("StaminaBar").gameObject.SetActive(false);
        this.gameObject.transform.Find("StreakBar").gameObject.SetActive(false);
        this.gameObject.transform.Find("TokenCollected").gameObject.SetActive(false);
        this.gameObject.transform.Find("Cursor").gameObject.SetActive(false);
        this.m_PlayerDieTime = Time.unscaledTime + 0.5f;
        this.m_PlayerRespawned = false;
    }
    

    /*==============================
        PlayerRespawned
        TODO
    ==============================*/
    
    public void PlayerRespawned()
    {
        this.m_PlayerRespawned = true;
        this.m_BlackBarTop.rectTransform.anchoredPosition = Vector2.zero;
        this.m_BlackBarBottom.rectTransform.anchoredPosition = Vector2.zero;
    }
}
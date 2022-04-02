/****************************************************************
                             HUD.cs
    
This script handles the heads up display logic.
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private const float HealthTime = 0.1f;
    private const float StreakTime = 0.1f;
    
    // Public values
    public Image m_HealthBar;
    public Image m_HealthIcon;
    public Image m_StaminaBar;
    public Image m_ScoreBar;
    public Image m_ScoreIcon;
    public Image m_BossHealthBar;
    public GameObject m_Player;
    public Sprite[] StreakSprite = new Sprite[6];
    public GameObject m_Boss;
    
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
    private int   m_CurrentStreak = 0;
    private float m_CurrentHealth;
    private float m_TargetHealth;
    private float m_CurrentBossHealth;
    private float m_TargetBossHealth;
    private float m_StreakSize = 1.0f;
    private PlayerCombat m_plycombat;
    private BossLogic m_bosslogic;
    
    
    /*==============================
        Start
        Called when the HUD is initialized
    ==============================*/
    
    void Start()
    {
        this.m_plycombat = this.m_Player.GetComponent<PlayerCombat>();
        this.m_CurrentHealth = this.m_plycombat.GetHealth();
        this.m_TargetHealth = this.m_CurrentHealth;
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
        this.m_TargetHealth = this.m_plycombat.GetHealth();
        this.m_CurrentHealth = Mathf.Lerp(this.m_CurrentHealth, this.m_TargetHealth, HUD.HealthTime);
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
            this.m_StreakSize = Mathf.Lerp(this.m_StreakSize, 1.0f, StreakTime);
        this.m_ScoreIcon.rectTransform.localScale = new Vector3(-this.m_StreakSize, this.m_StreakSize, 1.0f);
        if (this.m_CurrentStreak > laststreak)
            this.m_StreakSize = 1.5f;
        
        // Boss health bar
        if (this.m_Boss != null && this.m_CurrentBossHealth > 0)
        {
            this.m_TargetBossHealth = this.m_bosslogic.GetHealth();
            this.m_CurrentBossHealth = Mathf.Lerp(this.m_CurrentBossHealth, this.m_TargetBossHealth, HUD.HealthTime);
            this.m_BossHealthBar.rectTransform.localScale = new Vector3(this.m_CurrentBossHealth/this.m_bosslogic.GetMaxHealth(), 1.0f, 1.0f);
            
            // Boss is dead, move the bar down
            if (this.m_CurrentBossHealth < 1)
            {
                Vector2 curpos = this.m_BossHealthBar.transform.parent.position;
                this.m_BossHealthBar.transform.parent.position = new Vector2(curpos.x, Mathf.Lerp(curpos.y, -4, 0.1f));
            }
        }
    }
}
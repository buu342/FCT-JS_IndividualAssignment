/****************************************************************
                        Init_Level1_Boss.cs
    
This script initializes Level1_Boss.
****************************************************************/

using UnityEngine;

public class Init_Level1_Boss : MonoBehaviour
{
    public GameObject m_Player;
    public GameObject m_Boss;
    public GameObject m_dustland;
    private int m_CurrentSequence = 0;
    private float m_NextSequenceTime = 0;
    
    
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        if (!FindObjectOfType<SceneController>().IsRespawning())
        {
            this.m_NextSequenceTime = Time.unscaledTime + 0.01f;
            this.m_Player.GetComponent<PlayerController>().SetControlsEnabled(false);
            this.m_Boss.GetComponent<BossLogic>().SetEnabled(false);
            FindObjectOfType<SceneController>().SetupPlayer(this.m_Player);
            this.m_Player.SetActive(false);
            this.m_Boss.SetActive(false);
            foreach (MusicManager mm in FindObjectsOfType<MusicManager>())
                mm.PlaySong("Music/Level1_Boss", true, true);
        }
        else
        {
            this.m_NextSequenceTime = 0.0f;
            this.m_Player.transform.position = new Vector3(this.m_Player.transform.position.x, 0.1f, this.m_Player.transform.position.z);
            this.m_Boss.transform.position = new Vector3(this.m_Boss.transform.position.x, 0.1f, this.m_Boss.transform.position.z);
            this.m_Player.GetComponent<PlayerController>().SetControlsEnabled(true);
            this.m_Boss.GetComponent<BossLogic>().SetEnabled(true);
            this.m_Player.SetActive(true);
            this.m_Boss.SetActive(true);
            FindObjectOfType<HUD>().PlayerRespawned();
            foreach (MusicManager mm in FindObjectsOfType<MusicManager>())
                mm.PlaySong("Music/Level1_Boss", true, false);
            Destroy(this);
        }
    }
    
    
    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        this.m_Player.GetComponent<PlayerCombat>().SetPlayerLastStreakTime(10.0f);
        if (this.m_NextSequenceTime != 0 && this.m_NextSequenceTime < Time.unscaledTime)
        {
            switch (this.m_CurrentSequence)
            {
                case 0:
                    this.m_NextSequenceTime = Time.unscaledTime + 1.55f;
                    break;
                case 1:
                    this.m_Player.SetActive(true);
                    this.m_Player.GetComponent<PlayerController>().SetPlayerJumpState(PlayerController.PlayerJumpState.Fall);
                    this.m_Player.transform.Find("Model").GetComponent<PlayerAnimations>().BigDrop(true);
                    this.m_NextSequenceTime = Time.unscaledTime + 4.11f;
                    break;
                case 2:
                    this.m_Boss.SetActive(true);
                    this.m_Boss.GetComponent<BossLogic>().SetBossJumpState(BossLogic.BossJumpState.Jump);
                    this.m_NextSequenceTime = Time.unscaledTime + 0.5f;
                    break;
                case 3:
                    this.m_Boss.SetActive(true);
                    this.m_Boss.GetComponent<BossLogic>().SetBossJumpState(BossLogic.BossJumpState.Land);
                    this.m_NextSequenceTime = Time.unscaledTime + 0.5f;
                    FindObjectOfType<AudioManager>().Play("Voice/Boss/Land", this.m_Boss.transform.position);
                    Camera.main.GetComponent<CameraLogic>().AddTrauma(0.5f);
                    Instantiate(this.m_dustland, this.m_Boss.transform.position, Quaternion.identity);
                    break;
                case 4:
                    this.m_Boss.GetComponent<BossLogic>().SetEnabled(true);
                    this.m_Player.GetComponent<PlayerController>().SetControlsEnabled(true);
                    Destroy(this);
                    break;
            }
            
            this.m_CurrentSequence++;
        }
    }
}
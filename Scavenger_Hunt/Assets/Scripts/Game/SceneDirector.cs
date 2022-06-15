/****************************************************************
                         SceneDirector.cs
    
A script which handles the scene.
****************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;


public class SceneDirector : MonoBehaviour
{
    enum MusicType
    {
        None,
        Calm,
        Tense,
        LessCalm
    }
    

    [Header("Tutorial Prefabs")]
    public GameObject MovementPrefab;
    public GameObject ReloadPrefab;
    public GameObject AimPrefab;
    public GameObject FirePrefab;
    public GameObject PilferPrefab;

    private bool m_PlayerDead = false;
    private bool m_PlayerCompleted = false;
    private MonsterAI m_Monster = null;
    private MusicManager m_Music = null;
    private MusicType m_MusicType = MusicType.None;
    private GameObject m_Player = null;
    
    void OnEnable() {
        EnableAllInputEvents();
    }

    void OnDisable() {
        DisableAllInputEvents();
    }
    private void DisableAllInputEvents() {
        InputManagerScript.playerInput.Player.Aim.started -=  PressedAim;
        InputManagerScript.playerInput.Player.Fire.started -= Fire;
    }

    private void EnableAllInputEvents() {
        InputManagerScript.playerInput.Player.Aim.started +=  PressedAim;
        InputManagerScript.playerInput.Player.Fire.started += Fire;
    }

    /*==============================
        Start
        Called when the scene director is initialized
    ==============================*/
    
    void Start()
    {
        ProcGenner proc = this.GetComponent<ProcGenner>();
        int levelCount = GameObject.Find("LevelManager").GetComponent<LevelManager>().GetLevelCount();
        proc.GenerateScene(levelCount);
        this.m_Music = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        this.m_Music.PlaySong("Music/Calm", true, true);
        this.m_MusicType = MusicType.Calm;
        if(levelCount == 1) {
            Transform airlockStartPosition = proc.GetAirlockTransform();
            GameObject.Instantiate(MovementPrefab, airlockStartPosition);
            GameObject.Instantiate(AimPrefab, airlockStartPosition);
        } else {
            Debug.Log("Disabling all inputs");
             DisableAllInputEvents();
        }
    }
    
    void FixedUpdate()
    {
        if (this.m_Monster != null && !this.m_PlayerDead && !this.m_PlayerCompleted)
        {
            if (this.m_Monster.monsterState == MonsterAI.MonsterState.ChasingPlayer && !this.GetMusicTense())
                this.SetMusicTense(true);
            else if (this.m_Monster.monsterState == MonsterAI.MonsterState.Patrolling && this.GetMusicTense())
                this.SetMusicTense(false);
        }
    }

    void PressedAim(InputAction.CallbackContext context) {
        //instantiate aim tutorial to object
        Debug.Log("Pressed aim in director");
        if(m_Player != null) {
            GameObject.Instantiate(FirePrefab,m_Player.transform);
            InputManagerScript.playerInput.Player.Aim.started -=  PressedAim;
        }
    }

    void Fire(InputAction.CallbackContext context) 
    {
        if(m_Player != null && m_Player.GetComponent<PlayerController>().GetPlayerAmmoClip() == 0) {
            GameObject.Instantiate(ReloadPrefab,m_Player.transform);
            InputManagerScript.playerInput.Player.Fire.started -= Fire;
        }

    }
    

    /*==============================
        SetMonster
        Checks if the music is tense
        @return Checks whether the music is tense or not
    ==============================*/
    
    public void SetMonster(GameObject monster)
    {
        if (monster != null)
            this.m_Monster = monster.GetComponent<MonsterAI>();
        else
            this.m_Monster = null;
    }
    

    /*==============================
        GetMusicTense
        Checks if the music is tense
        @return Checks whether the music is tense or not
    ==============================*/
    
    public bool GetMusicTense()
    {
        return (this.m_MusicType == MusicType.Tense);
    }
    

    /*==============================
        SetMusicTense
        Sets the music to be tense
        @param Whether to set the music to be tense or not
    ==============================*/
    
    public void SetMusicTense(bool enable)
    {
        if (enable)
        {
            this.m_Music.PlaySong("Music/Tense", true, true, true);
            this.m_MusicType = MusicType.Tense;
        }
        else if (!enable && this.m_MusicType != MusicType.LessCalm)
        {
            this.m_Music.PlaySong("Music/LessCalm", true, true, true);
            this.m_MusicType = MusicType.LessCalm;
        }
    }

    public void SetPlayer(GameObject player) {
        m_Player = player;
    }

    public void PlayerDied()
    {
        this.m_PlayerDead = true;
    }

    public void PlayerCompleted()
    {
        this.m_PlayerCompleted = true;
    }
}
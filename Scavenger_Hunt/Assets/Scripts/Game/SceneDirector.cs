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

    private MonsterAI m_Monster = null;
    private MusicManager m_Music = null;
    private MusicType m_MusicType = MusicType.None;
    private GameObject m_Player = null;
    private bool ShowAimInstruction;
    
    void OnEnable() {
        InputManagerScript.playerInput.Player.Aim.started +=  PressedAim;
        if(!InputManagerScript.playerInput.Player.enabled)
            InputManagerScript.playerInput.Player.Enable();

    }
    void OnDisable() {
        InputManagerScript.playerInput.Player.Aim.started -=  PressedAim;
        if(InputManagerScript.playerInput.Player.enabled)
            InputManagerScript.playerInput.Player.Disable();

    }

    /*==============================
        Start
        Called when the scene director is initialized
    ==============================*/
    
    void Start()
    {
        ShowAimInstruction = false;
        this.GetComponent<ProcGenner>().GenerateScene();
        this.m_Music = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        this.m_Music.PlaySong("Music/Calm", true, true);
        this.m_MusicType = MusicType.Calm;
        
    }
    
    void FixedUpdate()
    {
        if (this.m_Monster != null)
        {
            if (this.m_Monster.monsterState == MonsterAI.MonsterState.ChasingPlayer && !this.GetMusicTense())
                this.SetMusicTense(true);
            else if (this.m_Monster.monsterState == MonsterAI.MonsterState.Patrolling && this.GetMusicTense())
                this.SetMusicTense(false);
        }
    }

    void PressedAim(InputAction.CallbackContext context) {
        //instantiate aim tutorial to object
        if(m_Player != null) {
            GameObject.Instantiate(FirePrefab,m_Player.transform);
            InputManagerScript.playerInput.Player.Aim.started -=  PressedAim;
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
}
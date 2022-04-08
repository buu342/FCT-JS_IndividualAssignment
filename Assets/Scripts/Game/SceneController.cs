/****************************************************************
                       SceneController.cs
    
This script handles the loading of scenes, and keeping track of
data persistence between scenes.
****************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Game difficulty enums
    public enum Difficulty
    {
        Easy,
        Hard
    }
    
    // Player data struct
    struct PlayerData
    {
        public bool Persist;
        public int Health;
        public float Stamina;
        public int Streak;
        public int Score;
        public int Deaths;
        public List<string> CollectedTokens;
        public string LastCheckpointName;
        public string LastSceneName;
        public Vector3 Position;
        public List<string> DestroyOnLoad;
    }
    
    // Private values
    private static SceneController Instance = null;  
    private bool m_GoToNextScene = false;
    private static PlayerData PlyData;
    private static Difficulty GameDifficulty = Difficulty.Hard;

    
    /*==============================
        Awake
        Called before the scene controller is initialized
    ==============================*/
 
    void Awake()
    {
        // Check if this instance already exists, if not, then set itself to this instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this) // If the instance already exists, but isn't us, then destroy ourselves.
        {
            Destroy(this.gameObject);
            return;
        }

        // Set this to not be destroyed when changing scenes
        DontDestroyOnLoad(this.gameObject);
        StartingNewLevel();
    }
    
    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        // Level skipping feature that the professors asked for
        if (Input.GetButtonDown("JumpLevel1_1"))
        {
            LoadScene("Level1_1");
            this.GetComponent<MusicManager>().StopMusic();
            StartNextScene();
        }
        if (Input.GetButtonDown("JumpLevel1_2"))
        {
            LoadScene("Level1_2");
            this.GetComponent<MusicManager>().StopMusic();
            StartNextScene();
        }
        if (Input.GetButtonDown("JumpLevel1_3"))
        {
            LoadScene("Level1_3");
            this.GetComponent<MusicManager>().StopMusic();
            StartNextScene();
        }
        if (Input.GetButtonDown("JumpLevel1_Boss"))
        {
            LoadScene("Level1_Boss");
            this.GetComponent<MusicManager>().StopMusic();
            StartNextScene();
        }
    }
    
    
    /*==============================
        StartingNewLevel
        Signals that we're starting a new level.
    ==============================*/
 
    public void StartingNewLevel()
    {
        PlyData.Score = 0;
        PlyData.LastCheckpointName = "";
        PlyData.Persist = false;
        PlyData.CollectedTokens = new List<string>();
        PlyData.DestroyOnLoad = new List<string>();
        PlyData.Position = Vector3.zero;
        PlyData.LastSceneName = "";
        PlyData.Deaths = 0;
    }
    
    
    /*==============================
        CheckpointCrossed
        Signals that we crossed a checkpoint
        @param The player that crossed the checkpoint
        @param The list of objects to destroy next load
    ==============================*/
 
    public void CheckpointCrossed(String checkpointname, GameObject ply, List<string> destroyonload)
    {
        if (PlyData.LastCheckpointName == checkpointname)
            return;
        PlayerCombat plycombat = ply.GetComponent<PlayerCombat>();
        PlyData.LastCheckpointName = checkpointname;
        PlyData.Score = plycombat.GetScore();
        PlyData.Position = ply.transform.position;
        foreach (string obj in destroyonload)
        {
            #if DEBUG
                if (PlyData.DestroyOnLoad.Contains(obj))
                {
                    Debug.LogWarning("Object "+obj+" duplicated in this checkpoint!");
                    continue;
                }
            #endif
            PlyData.DestroyOnLoad.Add(obj);
        }
    }
    
    
    /*==============================
        CollectToken
        Signals that we just collected a new token.
        @param The token's name
    ==============================*/
 
    public void CollectToken(string name)
    {
        if (!IsTokenCollected(name))
        {
            PlyData.CollectedTokens.Add(name);
            GameObject.Find("HUD").GetComponent<HUD>().CollectedToken(PlyData.CollectedTokens.Count);
        }
    }
    
    
    /*==============================
        GetCollectedTokenCount
        TODO
    ==============================*/
    
    public int GetCollectedTokenCount()
    {
        return PlyData.CollectedTokens.Count;
    }
    
    
    /*==============================
        GetDeathCount
        TODO
    ==============================*/
    
    public int GetDeathCount()
    {
        return PlyData.Deaths;
    }
    
    
    /*==============================
        IsTokenCollected
        Checks whether a specific token has been collected
        @param The token's name
        @returns Whether it has been collected already
    ==============================*/
 
    public bool IsTokenCollected(string name)
    {
        return PlyData.CollectedTokens.Contains(name);
    }
    
    
    /*==============================
        IsRespawning
        Checks whether the player is respawning
        @returns Whether the player is respawning
    ==============================*/
 
    public bool IsRespawning()
    {
        return (PlyData.LastSceneName == SceneManager.GetActiveScene().name);
    }
    
    
    /*==============================
        SetupPlayer
        Sets the player data based on their 
        state in the previous scene
        @param The PlayerCombat script
    ==============================*/
 
    public void SetupPlayer(GameObject ply)
    {
        PlayerCombat plycombat = ply.GetComponent<PlayerCombat>();
        plycombat.SetScore(PlyData.Score);
        
        // If we're not supposed to persist data (AKA we died and are restarting), then stop loading other data
        if (!PlyData.Persist)
        {
            if (PlyData.DestroyOnLoad != null)
                foreach (string obj in PlyData.DestroyOnLoad)
                    Destroy(GameObject.Find(obj));
            if (PlyData.Position != Vector3.zero)
                ply.transform.position = PlyData.Position;
            Camera.main.GetComponent<CameraLogic>().UpdatePlayerPosition();
            return;
        }
        
        // Otherwise, it means we changed scene. Set the player's data
        plycombat.SetHealth(PlyData.Health);
        plycombat.SetStamina(PlyData.Stamina);
        plycombat.SetStreak(PlyData.Streak);
        plycombat.SetPlayerLastStreakTime(Time.time + PlayerCombat.StreakLoseTime);
        PlyData.Persist = false;
        PlyData.LastCheckpointName = "";
        PlyData.DestroyOnLoad = new List<string>();
    }
    
    
    /*==============================
        SetDifficulty
        Sets the game's difficulty level
        @Param The difficulty level to set
    ==============================*/
 
    public void SetDifficulty(Difficulty difficulty)
    {
        GameDifficulty = difficulty;
    }
    
    
    /*==============================
        GetDifficulty
        Retrieves the current difficulty level
        @returns The current difficulty
    ==============================*/
 
    public Difficulty GetDifficulty()
    {
        return GameDifficulty;
    }
    
    
    /*==============================
        LoadAsyncScene
        An asynchronous coroutine that loads
        a given scene
        @param The scene name to load
    ==============================*/
    
    IEnumerator LoadAsyncScene(string scenename)
    {
        AsyncOperation nextscene = SceneManager.LoadSceneAsync(scenename);
        nextscene.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!nextscene.isDone)
        {
            if (this.m_GoToNextScene)
            {
                this.m_GoToNextScene = false;
                nextscene.allowSceneActivation = true;
                break;
            }
            yield return null;
        }
    }
    
    
    /*==============================
        RestartCurrentScene
        Restarts the currently active scene
    ==============================*/
    
    public void RestartCurrentScene()
    {
        PlyData.LastSceneName = SceneManager.GetActiveScene().name;
        PlyData.Deaths++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    
    /*==============================
        LoadScene
        Loads a scene with a given name
        @param The scene name to load
    ==============================*/
    
    public void LoadScene(string scenename)
    {
        StartCoroutine(LoadAsyncScene(scenename));
    }
    
    
    /*==============================
        StartNextScene
        Allows the scene switch to occur
        (If the loading is finished)
    ==============================*/
    
    public void StartNextScene()
    {
        PlayerCombat plycombat = FindObjectOfType<PlayerCombat>();
        if (plycombat != null)
        {
            PlyData.Health = plycombat.GetHealth();
            PlyData.Stamina = plycombat.GetStamina();
            PlyData.Score = plycombat.GetScore();
            PlyData.Streak = plycombat.GetStreak();
            PlyData.Persist = true;
            PlyData.Position = Vector3.zero;
            PlyData.DestroyOnLoad = null;
        }
        PlyData.LastSceneName = SceneManager.GetActiveScene().name;
        this.m_GoToNextScene = true;
    }
}
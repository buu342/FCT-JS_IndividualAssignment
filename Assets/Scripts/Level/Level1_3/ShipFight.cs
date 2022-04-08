/****************************************************************
                          ShipFight.cs
    
This script handles the ship battle in Level1_3.
****************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFight : MonoBehaviour
{
    public GameObject m_PatrolPoint2;
    public GameObject m_PatrolPoint4;
    public GameObject m_PatrolPoint8;
    public GameObject m_PatrolPoint10;
    public GameObject m_EnemyPrefab;
    public GameObject m_PlayerCollider;
    
    private float m_CameraEndY = 3;
    private int m_CurrentEndScene = 0;
    private bool m_Activated = false;
    private int m_CurrentSpawn = 0;
    private List<Tuple<float, EnemyLogic.AttackStyle, Vector3, GameObject[]>> m_Spawns;
    private float m_NextSpawn = 0.0f;
    private int PlayerLayer;
    private GameObject m_player;
    
    // Debug stuff
    #if UNITY_EDITOR
        [SerializeField]
        private bool DebugTrigger = false;
    #endif
    
    
    /*==============================
        Start
        Called when the trigger is initialized
    ==============================*/
    
    void Start()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
        this.m_Spawns = new List<Tuple<float, EnemyLogic.AttackStyle, Vector3, GameObject[]>>{
            Tuple.Create(
                1.0f, EnemyLogic.AttackStyle.Aiming, Vector3.zero, new GameObject[]{
                    m_PatrolPoint2, 
                    m_PatrolPoint10, 
                }
            ),
            Tuple.Create(
                2.0f, EnemyLogic.AttackStyle.Aiming, Vector3.zero, new GameObject[]{
                    m_PatrolPoint8, 
                    m_PatrolPoint4, 
                }
            ),
            Tuple.Create(
                3.0f, EnemyLogic.AttackStyle.Straight, new Vector3(-1.0f, 0.5f, 0.0f), new GameObject[]{
                    m_PatrolPoint10, 
                    m_PatrolPoint2, 
                }
            ),
            Tuple.Create(
                3.0f, EnemyLogic.AttackStyle.Straight, new Vector3(1.0f, 0.5f, 0.0f), new GameObject[]{
                    m_PatrolPoint4, 
                    m_PatrolPoint8, 
                }
            ),
            Tuple.Create(
                1.0f, EnemyLogic.AttackStyle.Aiming, Vector3.zero, new GameObject[]{
                    m_PatrolPoint8, 
                    m_PatrolPoint4, 
                }
            ),
            Tuple.Create(
                0.0f, EnemyLogic.AttackStyle.Aiming, Vector3.zero, new GameObject[]{
                    m_PatrolPoint2, 
                    m_PatrolPoint10, 
                }
            ),
            Tuple.Create(7.0f, EnemyLogic.AttackStyle.Aiming, Vector3.zero, new GameObject[]{null}),
        };
    }
    

    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != PlayerLayer)
            return;
        this.m_Activated = true;
        this.m_PlayerCollider.GetComponent<BoxCollider>().enabled = true;
        this.m_NextSpawn = Time.time + this.m_Spawns[0].Item1;
        this.m_player = other.gameObject;
    }

    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        if (!this.m_Activated)
            return;
        
        // If the spawn timer ran out
        if (this.m_CurrentEndScene == 0 && this.m_NextSpawn < Time.time)
        {
            // If we finished our spawns, then do the level outro
            if (this.m_CurrentSpawn+1 == this.m_Spawns.Count)
            {
                this.m_CurrentEndScene++;
                return;
            }
            
            // Spawn a new enemy
            EnemyLogic enemy = Instantiate(this.m_EnemyPrefab, this.m_Spawns[this.m_CurrentSpawn].Item4[0].transform.position, Quaternion.Euler(0, -90, 0)).gameObject.GetComponent<EnemyLogic>();
            enemy.SetEnemyAttackStyle(this.m_Spawns[this.m_CurrentSpawn].Item2);
            enemy.SetEnemyAimDir(this.m_Spawns[this.m_CurrentSpawn].Item3);
            enemy.SetEnemyDepthPerception(14.0f);
            foreach (GameObject point in this.m_Spawns[this.m_CurrentSpawn].Item4)
                enemy.AddPatrolPoint(point);
            enemy.SetEnemyRemoveOnPatrolFinish(true);
            
            // Set the timer to spawn the next enemy
            this.m_CurrentSpawn++;
            this.m_NextSpawn = Time.time + this.m_Spawns[this.m_CurrentSpawn].Item1;
            
            // Start loading the next scene if we're on the last guy
            if (this.m_CurrentSpawn+1 == this.m_Spawns.Count)
                GameObject.Find("SceneController").GetComponent<SceneController>().LoadScene("Level1_Boss");
        }
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        // If we finished our spawns, play the ending sequence
        if (this.m_CurrentEndScene > 0)
        {
            if (this.m_NextSpawn < Time.time)
            {
                switch (this.m_CurrentEndScene)
                {
                    case 1:
                        FindObjectOfType<MusicManager>().FadeMusic();
                        this.m_NextSpawn = Time.time + 2.0f;
                        break;
                    case 2:
                        this.m_NextSpawn = Time.time + 1.0f;
                        break;
                    case 3:
                        this.m_NextSpawn = Time.time + 3.0f;
                        break;
                    case 4:
                        GameObject.Find("SceneController").GetComponent<SceneController>().StartNextScene();
                        break;
                }
                this.m_CurrentEndScene++;
            }
            
            // Don't allow the player's streak to decrease
            this.m_player.GetComponent<PlayerCombat>().SetPlayerLastStreakTime(10.0f);
        
            // Shake the camera
            if (this.m_CurrentEndScene > 2)
                Camera.main.GetComponent<CameraLogic>().AddTrauma(0.1f);
            
            // Move the camera down
            if (this.m_CurrentEndScene > 3)
            {
                this.m_CameraEndY = Mathf.Lerp(this.m_CameraEndY, 13.0f, 1.0f*Time.deltaTime);
                Camera.main.GetComponent<CameraLogic>().SetPoI(new Vector3(0, this.m_CameraEndY, -8));
            }
        }
    }
    
    
    #if UNITY_EDITOR
        /*==============================
            OnDrawGizmos
            Draws extra debug stuff in the editor
        ==============================*/
        
        public virtual void OnDrawGizmos()
        {
            if (DebugTrigger)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(this.transform.position, this.transform.localScale);
            }
        }
    #endif
}
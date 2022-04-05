/****************************************************************
                           SkyFight.cs
    
This script handles the sky battle in Level1_2.
****************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyFight : MonoBehaviour
{
    public GameObject m_PatrolPoint1;
    public GameObject m_PatrolPoint2;
    public GameObject m_PatrolPoint4;
    public GameObject m_PatrolPoint5;
    public GameObject m_PatrolPoint7;
    public GameObject m_PatrolPoint8;
    public GameObject m_PatrolPoint10;
    public GameObject m_PatrolPoint11;
    public GameObject m_PatrolPointCenterRight;
    public GameObject m_PatrolPointCenterLeft;
    public GameObject m_EnemyPrefab;
    
    private int m_CurrentSpawn = 0;
    private List<Tuple<float, EnemyLogic.AttackStyle, Vector3, GameObject[]>> m_Spawns;
    private float m_NextSpawn = 0.0f;
    
    
    /*==============================
        Start
        Called when the trigger is initialized
    ==============================*/
    
    void Start()
    {
        this.m_Spawns = new List<Tuple<float, EnemyLogic.AttackStyle, Vector3, GameObject[]>>{
            Tuple.Create(
                3.0f, EnemyLogic.AttackStyle.Aiming, Vector3.zero, new GameObject[]{
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
                2.0f, EnemyLogic.AttackStyle.Straight, new Vector3(-1, 0, 0), new GameObject[]{
                    m_PatrolPoint10, 
                    m_PatrolPoint4, 
                }
            ),
            Tuple.Create(
                3.0f, EnemyLogic.AttackStyle.Straight, Vector3.zero, new GameObject[]{
                    m_PatrolPoint2, 
                    m_PatrolPointCenterRight, 
                    m_PatrolPoint5, 
                }
            ),
            Tuple.Create(
                3.0f, EnemyLogic.AttackStyle.Straight, Vector3.zero, new GameObject[]{
                    m_PatrolPoint1, 
                    m_PatrolPoint5, 
                }
            ),
            Tuple.Create(
                0.0f, EnemyLogic.AttackStyle.Straight, new Vector3(-1, 0, 0), new GameObject[]{
                    m_PatrolPoint7, 
                    m_PatrolPoint11, 
                }
            ),
            Tuple.Create(5.0f, EnemyLogic.AttackStyle.Aiming, Vector3.zero, new GameObject[]{null}),
        };
        this.m_NextSpawn = Time.time + this.m_Spawns[0].Item1;
    }

    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        // If the spawn timer ran out
        if (this.m_NextSpawn != 0 && this.m_NextSpawn < Time.time)
        {
            // If we finished our spawns, then load the next level
            if (this.m_CurrentSpawn+1 == this.m_Spawns.Count)
            {
                GameObject.Find("SceneController").GetComponent<SceneController>().StartNextScene();
                this.m_NextSpawn = 0;
                return;
            }
            
            // Spawn a new enemy
            EnemyLogic enemy = Instantiate(this.m_EnemyPrefab, this.m_Spawns[this.m_CurrentSpawn].Item4[0].transform.position, Quaternion.Euler(0, -90, 0)).gameObject.GetComponent<EnemyLogic>();
            enemy.SetEnemyAttackStyle(this.m_Spawns[this.m_CurrentSpawn].Item2);
            enemy.SetEnemyAimDir(this.m_Spawns[this.m_CurrentSpawn].Item3);
            foreach (GameObject point in this.m_Spawns[this.m_CurrentSpawn].Item4)
                enemy.AddPatrolPoint(point);
            enemy.SetEnemyRemoveOnPatrolFinish(true);
            
            // Set the timer to spawn the next enemy
            this.m_CurrentSpawn++;
            this.m_NextSpawn = Time.time + this.m_Spawns[this.m_CurrentSpawn].Item1;
            
            // Start loading the next scene if we're on the last guy
            if (this.m_CurrentSpawn+1 == this.m_Spawns.Count)
                GameObject.Find("SceneController").GetComponent<SceneController>().LoadScene("Level1_3");
        }
    }
}
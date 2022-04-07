/****************************************************************
                         CheckpointTrigger.cs
    
This script handles a checkpoint trigger
****************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    public List<GameObject> m_RemoveOnLoad;
    private List<string> m_RemoveOnLoadNames = new List<string>();
    private int PlayerLayer;
    
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
        foreach (GameObject obj in this.m_RemoveOnLoad)
            this.m_RemoveOnLoadNames.Add(obj.name);
    }


    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == PlayerLayer)
        {
            FindObjectOfType<SceneController>().CheckpointCrossed(this.gameObject.name, other.gameObject, this.m_RemoveOnLoadNames);
            Destroy(this.gameObject);
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
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(this.transform.position, this.transform.localScale);
            }
        }
    #endif
}
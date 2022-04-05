/****************************************************************
                         JetpackLogic.cs
    
This script handles the jetpack pickup logic
****************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JetpackLogic : MonoBehaviour
{
    private int angle;
    private int PlayerLayer;
    private GameObject m_mesh;
    
    
    /*==============================
        Start
        Called when the coin is initialized
    ==============================*/
    
    void Start()
    {
        PlayerLayer = LayerMask.NameToLayer("Player");
        this.m_mesh = this.transform.Find("Mesh").gameObject;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        angle = (angle + 1)%360;
        this.m_mesh.transform.position = this.transform.position + (new Vector3(0, Mathf.Sin(Time.time*2)/8, 0));
        this.m_mesh.transform.localEulerAngles = new Vector3(0, 0, angle);
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
            Destroy(this.gameObject);
            GameObject.Find("SceneController").GetComponent<SceneController>().StartNextScene();
        }
    }
}
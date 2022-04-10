/****************************************************************
                        Init_Level1_3.cs
    
This script initializes Level1_3.
****************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class Init_Level1_3 : MonoBehaviour
{
    public RawImage m_Clouds;
    public GameObject m_Jetpack;
    public GameObject m_Player;
    public GameObject m_PlayerClip;
    private float m_SequenceTime;
    private int m_CurrentSequence = 0;
    private Vector3 m_TargetPoint = new Vector3(-1.93f, 0, 0);
    
    
    /*==============================
        Start
        Called when the scene is initialized
    ==============================*/
    
    void Start()
    {
        foreach (MusicManager mm in FindObjectsOfType<MusicManager>())
            mm.PlaySong("Music/Level1", true, false, 1);
        if (!FindObjectOfType<SceneController>().IsRespawning())
        {
            this.m_Player.SetActive(false);
            this.m_Player.GetComponent<PlayerController>().SetControlsEnabled(false);
            this.m_SequenceTime = Time.time + 1.5f;
            Camera.main.GetComponent<CameraLogic>().ForcePosition(new Vector3(-2, 5, -8));
            Camera.main.GetComponent<CameraLogic>().SetPoI(new Vector3(4, -3, -8));
        }
        else
        {
            this.m_Clouds.rectTransform.position = new Vector2(0.0f, -1024.0f);
            this.m_Player.transform.position = this.m_TargetPoint;
            Camera.main.GetComponent<CameraLogic>().SetPlayer(this.m_Player);
            Camera.main.GetComponent<CameraLogic>().SetFollowPlayer(true);
            Camera.main.GetComponent<CameraLogic>().SetPoI(new Vector3(4, 2, -8));
            FindObjectOfType<HUD>().PlayerRespawned();
            Destroy(this.m_Jetpack);
            Destroy(this.gameObject);
        }
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        if (this.m_SequenceTime < Time.time)
        {
            switch (this.m_CurrentSequence)
            {
                case 0:
                    this.m_Player.SetActive(true);
                    this.m_Player.GetComponent<Rigidbody>().AddForce(this.m_Player.transform.forward*20.0f, ForceMode.Impulse);
                    this.m_SequenceTime = Time.time + 0.176447f;
                    break;
                case 1:
                    this.m_SequenceTime = Time.time + 0.1f;
                    this.m_Jetpack.transform.parent = null;
                    break;
                case 2:
                    this.m_Player.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    this.m_PlayerClip.GetComponent<BoxCollider>().enabled = true;
                    this.m_Player.GetComponent<PlayerController>().SetControlsEnabled(true);
                    Camera.main.GetComponent<CameraLogic>().SetPlayer(this.m_Player);
                    Camera.main.GetComponent<CameraLogic>().SetFollowPlayer(true);
                    Camera.main.GetComponent<CameraLogic>().SetPoI(new Vector3(4, 2, -8));
                    this.m_SequenceTime = Time.time + 3.0f;
                    break;
                case 3:
                    Destroy(this.m_Jetpack);
                    Destroy(this.gameObject);
                    break;
            }
            this.m_CurrentSequence++;
        }
        
        // Make the jetpack fly away
        if (this.m_CurrentSequence > 1)
            this.m_Jetpack.transform.position += new Vector3(0.5f, 1.0f, 0.0f)*10.0f*Time.deltaTime;
        
        // Push the clouds down
        if (this.m_Clouds != null && this.m_Clouds.rectTransform.position.y > -1024)
        {
            this.m_Clouds.rectTransform.localPosition = Vector3.Lerp(this.m_Clouds.rectTransform.localPosition, new Vector3(0, -1024-256, 0), Time.deltaTime/2.0f);
            this.m_Clouds.uvRect = new Rect(Time.time/5.0f, 0, 1, 0.995f);
        }
        else
            Destroy(this.m_Clouds);
    }
}
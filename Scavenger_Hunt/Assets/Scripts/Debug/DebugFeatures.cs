using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugFeatures : MonoBehaviour
{
    public TextMeshProUGUI FPSText;
    public CameraController m_Camera;
    public GameObject m_Monster;
    public GameObject m_Player;
    private string m_TargetName = "";
    private int frameRate;
    private bool calculateFrames;
    public static bool pauseAnimations;
    
    private bool m_PlayerDead = false;
    private Vector3[] JumpPoints = new Vector3[2];
    private Quaternion[] JumpPointsDir = new Quaternion[2];

    private void OnEnable() {
        InputManagerScript.playerInput.UI.FPSCounter.started += calculateFramesTriggered;
        InputManagerScript.playerInput.Player.PauseAnimations.started += pauseAnimationsTriggered;
        InputManagerScript.playerInput.Player.JumpPoint1.started += JumpPoint1;
        InputManagerScript.playerInput.Player.JumpPoint2.started += JumpPoint2;
        InputManagerScript.playerInput.Player.JumpPoint3.started += JumpPoint3;
        InputManagerScript.playerInput.Player.JumpPoint4.started += JumpPoint4;
        if(!InputManagerScript.playerInput.UI.enabled)
            InputManagerScript.playerInput.UI.Enable();
    }

    private void OnDisable() {
        InputManagerScript.playerInput.UI.FPSCounter.started -= calculateFramesTriggered;
        InputManagerScript.playerInput.Player.PauseAnimations.started -= pauseAnimationsTriggered;
        InputManagerScript.playerInput.Player.JumpPoint1.started -= JumpPoint1;
        InputManagerScript.playerInput.Player.JumpPoint2.started -= JumpPoint2;
        InputManagerScript.playerInput.Player.JumpPoint3.started -= JumpPoint3;
        InputManagerScript.playerInput.Player.JumpPoint4.started -= JumpPoint4;
        if(InputManagerScript.playerInput.UI.enabled)
            InputManagerScript.playerInput.UI.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        //calculate frameRate
        if (!this.m_PlayerDead)
        {
            if(pauseAnimations && Time.timeScale != 0) 
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0, 0.02f);
                if(Time.timeScale<0.02f) { 
                    Time.timeScale = 0;
                }
            } else if (!pauseAnimations && Time.timeScale != 1.0f) {
                Time.timeScale = Mathf.Lerp(Time.timeScale,1, 0.02f);
                if(Time.timeScale > 0.98f)
                    Time.timeScale = 1.0f;
            } 

            FPSText.text = "";
            if (calculateFrames)
            {
                frameRate = (int)(1.0f / Time.unscaledDeltaTime);
                FPSText.text=frameRate.ToString()+" FPS";
            }
            if (Time.timeScale != 1.0f)
            {
                if (FPSText.text == "")
                    FPSText.text = "Game Paused";
                else
                    FPSText.text += "\nGame Paused";
            }
            
            if (this.m_Camera != null && this.m_Camera.isInFreeMode())
            {
                if (FPSText.text == "")
                    FPSText.text = "Camera Free";
                else
                    FPSText.text += "\nCamera Free";
            }
            
            if (this.m_Camera != null && this.m_Camera.isInFreeMode() && this.m_TargetName != "")
            {
                if (FPSText.text == "")
                    FPSText.text = this.m_TargetName;
                else
                    FPSText.text += "\n"+this.m_TargetName;
            }
            else if (this.m_TargetName != "")
                this.m_TargetName = "";
        }
    }

    private void calculateFramesTriggered(InputAction.CallbackContext context) {
        calculateFrames = !calculateFrames;
    }

    private void pauseAnimationsTriggered(InputAction.CallbackContext context) {
        pauseAnimations = !pauseAnimations;
    }

    private void JumpPoint1(InputAction.CallbackContext context) {
        pauseAnimations = true;
        this.m_TargetName = "Targeting Spawn";
        this.m_Camera.EnableFreeMode();
        this.m_Camera.transform.position = JumpPoints[0];
        this.m_Camera.transform.rotation = JumpPointsDir[0];
    }

    private void JumpPoint2(InputAction.CallbackContext context) {
        pauseAnimations = true;
        this.m_TargetName = "Targeting Exit";
        this.m_Camera.EnableFreeMode();
        this.m_Camera.transform.position = JumpPoints[1];
        this.m_Camera.transform.rotation = JumpPointsDir[1];
    }

    private void JumpPoint3(InputAction.CallbackContext context) {
        pauseAnimations = true;
        this.m_TargetName = "Target: Hunter";
        this.m_Camera.EnableFreeMode();
        this.m_Camera.transform.position = this.m_Monster.transform.position;
        this.m_Camera.transform.rotation = Quaternion.identity;
    }

    private void JumpPoint4(InputAction.CallbackContext context) {
        pauseAnimations = true;
        this.m_TargetName = "Target: Hunter Destination";
        this.m_Camera.EnableFreeMode();
        this.m_Camera.transform.position = this.m_Monster.GetComponent<MonsterAI>().GetDestination();
        this.m_Camera.transform.rotation = Quaternion.identity;
    }

    public void SetJumpPoint(int point, Vector3 position, Quaternion dir)
    {
        JumpPoints[point] = position;
        JumpPointsDir[point] = dir;
    }

    public void SetMonster(GameObject monster)
    {
        this.m_Monster = monster;
    }

    public void SetCamera(GameObject camera)
    {
        this.m_Camera = camera.GetComponent<CameraController>();
    }

    public void SetPlayer(GameObject player)
    {
        this.m_Player = player.transform.Find("CameraTarget").gameObject;
    }

    public void PlayerDied()
    {
        this.m_PlayerDead = true;
    }
    
    public bool getPausedGame() {
        return pauseAnimations;
    }
    public bool getCalculateFrames() {
        return calculateFrames;
    }
}

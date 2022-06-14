using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugFeatures : MonoBehaviour
{
    private int frameRate;
    private bool calculateFrames;
    private bool pauseGame;
    public TextMeshProUGUI FPSText;

    

    private void OnEnable() {
        InputManagerScript.playerInput.UI.FPSCounter.started += calculateFramesTriggered;
        InputManagerScript.playerInput.Player.PauseAnimations.started += PauseGameTriggered;
        if(!InputManagerScript.playerInput.UI.enabled)
            InputManagerScript.playerInput.UI.Enable();
    }

    private void OnDisable() {
        InputManagerScript.playerInput.UI.FPSCounter.started -= calculateFramesTriggered;
        InputManagerScript.playerInput.Player.PauseAnimations.started -= PauseGameTriggered;
        
        if(InputManagerScript.playerInput.UI.enabled)
            InputManagerScript.playerInput.UI.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        //calculate frameRate
        
        if(pauseGame) 
        {
            if(Time.timeScale<0.1f) { 
                Time.timeScale = 0;
            } else {   
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0, 0.1f);
            }
        } else {
            if(Time.timeScale >0.9) {
                Time.timeScale = 1.0f;
            } else {
                Time.timeScale = Mathf.Lerp(Time.timeScale,1, 0.1f);
            }
        } 

        if (calculateFrames)
        {
            frameRate = (int)(1.0f / Time.unscaledDeltaTime);
            FPSText.text=frameRate.ToString()+" FPS";
        }
    }

    private void calculateFramesTriggered(InputAction.CallbackContext context) {
        calculateFrames = !calculateFrames;
        if(!calculateFrames)
            FPSText.text="";
    }

    private void PauseGameTriggered(InputAction.CallbackContext context) {
        pauseGame = !pauseGame;
    }

}

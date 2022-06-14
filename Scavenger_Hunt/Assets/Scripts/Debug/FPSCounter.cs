using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    private int frameRate;
    private bool calculateFrames;
    public TextMeshProUGUI FPSText;

    

    private void OnEnable() {
        InputManagerScript.playerInput.UI.FPSCounter.started += calculateFramesTriggered;
        if(!InputManagerScript.playerInput.UI.enabled)
            InputManagerScript.playerInput.UI.Enable();
    }

    private void OnDisable() {
        InputManagerScript.playerInput.UI.FPSCounter.started -= calculateFramesTriggered;
            if(InputManagerScript.playerInput.UI.enabled)
                InputManagerScript.playerInput.UI.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        //calculate frameRate
        if (calculateFrames)
        {
            frameRate = (int)(1.0f / Time.unscaledDeltaTime);
            Debug.Log(frameRate);
            FPSText.text=frameRate.ToString()+" FPS";
        }
    }

    private void calculateFramesTriggered(InputAction.CallbackContext context) {
        calculateFrames = !calculateFrames;
        if(!calculateFrames)
            FPSText.text="";
    }

}

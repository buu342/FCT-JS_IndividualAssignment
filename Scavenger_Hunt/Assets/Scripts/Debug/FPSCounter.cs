using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public PlayerInput playerControls;
    private InputAction calculateFramesAction;
    private int frameRate;
    private bool calculateFrames;
    public TextMeshProUGUI FPSText;

    private void Awake() {
        playerControls = new PlayerInput();
    }

    private void OnEnable() {

        calculateFramesAction = playerControls.UI.FPSCounter;
        calculateFramesAction.Enable();
        calculateFramesAction.performed += calculateFramesTriggered;
    }

    private void OnDisable() {
        calculateFramesAction.performed -= calculateFramesTriggered;
         calculateFramesAction.Disable();
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
    }

}

using UnityEngine;
using UnityEngine.InputSystem;
public class FPSCounter : MonoBehaviour
{
    public PlayerInput playerControls;
    private InputAction calculateFramesAction;
    private InputAction MoveForward;
    private int frameRate;
    private bool calculateFrames;
    
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
        }
    }

    private void calculateFramesTriggered(InputAction.CallbackContext context) {
        calculateFrames = !calculateFrames;
    }

    private void moveForward(InputAction.CallbackContext context) {
        

    }
}

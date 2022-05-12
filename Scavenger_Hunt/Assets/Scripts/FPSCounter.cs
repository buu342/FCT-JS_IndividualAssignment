using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private int frameRate;
    private bool calculateFrames;
    // Update is called once per frame
    void Update()
    {
        //calculate frameRate
        if (calculateFrames)
        {
            frameRate = (int)(1.0f / Time.unscaledDeltaTime);
        }
    }
}

using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private int frameRate;
    
    // Update is called once per frame
    void Update()
    {
        //calculate frameRate
        frameRate =(int)(1.0f / Time.unscaledDeltaTime);
    }
}

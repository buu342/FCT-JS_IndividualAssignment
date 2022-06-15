using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPopUp : MonoBehaviour
{
    
    public float TimeToFadeOut;
    public float TimeToFadeIn;
    private SpriteRenderer m_Sprite;
    private float remainingTime;
    private enum SpriterState {
        fadeIn,
        fadeOut
    }

    private SpriterState m_State;
    // Start is called before the first frame update
    void Start()
    {
        this.m_Sprite = this.transform.Find("Controls").GetComponent<SpriteRenderer>();
        this.m_Sprite.color = Color.clear;
        m_State = SpriterState.fadeIn;
        remainingTime = TimeToFadeIn;
    }

    // Update is called once per frame
    void Update()
    {        
        switch(m_State) {
                case SpriterState.fadeIn:
                    if(this.m_Sprite.color.a > 0.9f) {
                        m_State = SpriterState.fadeOut;  
                        remainingTime = TimeToFadeOut;     
                    } else {
                        float factor = remainingTime/TimeToFadeIn;
                        remainingTime -= Time.deltaTime;
                        Debug.Log(factor);
                        this.m_Sprite.color = new Color(1.0f,1.0f, 1.0f,Mathf.Lerp(1.0f,0.0f, factor));
                    }  
                break;
                case SpriterState.fadeOut:
                    if(this.m_Sprite.color.a < 0.02f && remainingTime <=0) {
                        Destroy(gameObject);
                    } else {
                    float factor = remainingTime/TimeToFadeOut;
                    remainingTime -= Time.deltaTime;
                    Debug.Log(factor);
                    this.m_Sprite.color = new Color(1.0f,1.0f, 1.0f,Mathf.Lerp(0.0f,1.0f, factor));
                    }  
                break;
            }
    }
}

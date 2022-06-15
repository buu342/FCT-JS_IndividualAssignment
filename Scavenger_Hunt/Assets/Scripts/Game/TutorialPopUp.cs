using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPopUp : MonoBehaviour
{
    public float TimeToFade;

    private SpriteRenderer m_Sprite;
    private float remainingTime;
    // Start is called before the first frame update
    void Start()
    {
        this.m_Sprite = this.transform.Find("Controls").GetComponent<SpriteRenderer>();
        this.m_Sprite.color = new Color(1.0f,1.0f,1.0f,1.0f);
        remainingTime = TimeToFade;
    }

    // Update is called once per frame
    void Update()
    {        
            if(this.m_Sprite.color.a < 0.02f) {
                this.m_Sprite.enabled = false;
                Destroy(gameObject);
            } else {
            float factor = remainingTime/TimeToFade;
            remainingTime -= Time.deltaTime;
            Debug.Log(factor);
            this.m_Sprite.color = new Color(1.0f,1.0f, 1.0f,Mathf.Lerp(0.0f,1.0f, factor));
            }        

    }
}

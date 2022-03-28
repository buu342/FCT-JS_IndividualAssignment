using UnityEngine;

public class FadeoutDestroy : MonoBehaviour
{
    public float m_LifeTime;
    public float m_FadeTime;
    
    private float m_StartFade = 0.0f;
    private float m_Alpha = 255.0f;
    
    void Start()
    {
        this.m_StartFade = Time.time + this.m_LifeTime;
    }

    void Update()
    {
        if (this.m_StartFade < Time.time) 
        {
            this.m_Alpha -= 255*(1/this.m_FadeTime)*Time.deltaTime;
            if (this.m_Alpha < 1)
                Destroy(this.gameObject);
            for(int i=0; i < this.transform.childCount; i++)
            {
                GameObject obj = this.transform.GetChild(i).gameObject;
                Color c = obj.GetComponent<MeshRenderer>().material.color;
                c = new Color(c.r, c.g, c.b, this.m_Alpha);
            }
        }
    }
}
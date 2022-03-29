/****************************************************************
                        FadeoutDestroy.cs
    
Fades out an object before destroying it. The fade effect doesn't
seem to work... Maybe because I'm not very familiar with Unity's
material system?
****************************************************************/

using UnityEngine;

public class FadeoutDestroy : MonoBehaviour
{
    // Public fields
    public float m_LifeTime;
    public float m_FadeTime;
    
    // Private values
    private float m_StartFade = 0.0f;
    private float m_Alpha = 255.0f;
    
    
    /*==============================
        Start
        Called when the projectile is initialized
    ==============================*/
    
    void Start()
    {
        this.m_StartFade = Time.time + this.m_LifeTime;
    }
    

    /*==============================
        Update
        Called every frame
    ==============================*/

    void Update()
    {
        // If it's time to start fading out
        if (this.m_StartFade < Time.time) 
        {
            // Perform the fade out, and destroy if the alpha is close to zero
            this.m_Alpha -= 255*(1/this.m_FadeTime)*Time.deltaTime;
            if (this.m_Alpha < 1)
                Destroy(this.gameObject);
            
            // Update the alpha on all the mesh renderers
            for (int i=0; i<this.transform.childCount; i++)
            {
                GameObject obj = this.transform.GetChild(i).gameObject;
                if (obj.GetComponent<MeshRenderer>() != null)
                {
                    Color c = obj.GetComponent<MeshRenderer>().material.color;
                    c = new Color(c.r, c.g, c.b, this.m_Alpha);
                }
                else if (obj.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    Color c = obj.GetComponent<SkinnedMeshRenderer>().material.color;
                    c = new Color(c.r, c.g, c.b, this.m_Alpha);
                }
            }
        }
    }
}
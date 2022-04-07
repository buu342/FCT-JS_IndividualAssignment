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
        
        // Make a copy of the materials on all mesh renderers
        if (this.GetComponent<MeshRenderer>() != null)
            for (int j=0; j<this.GetComponent<MeshRenderer>().materials.Length; j++)
                this.GetComponent<MeshRenderer>().materials[j] = new Material(this.GetComponent<MeshRenderer>().materials[j]);
        if (this.GetComponent<SkinnedMeshRenderer>() != null)
            for (int j=0; j<this.GetComponent<SkinnedMeshRenderer>().materials.Length; j++)
                this.GetComponent<SkinnedMeshRenderer>().materials[j] = new Material(this.GetComponent<SkinnedMeshRenderer>().materials[j]);
        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject obj = this.transform.GetChild(i).gameObject;
            if (obj.GetComponent<MeshRenderer>() != null)
            {
                for (int j=0; j<obj.GetComponent<MeshRenderer>().materials.Length; j++)
                    this.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().materials[j] = new Material(obj.GetComponent<MeshRenderer>().materials[j]);
            }
            else if (obj.GetComponent<SkinnedMeshRenderer>() != null)
            {
                for (int j=0; j<obj.GetComponent<SkinnedMeshRenderer>().materials.Length; j++)
                    this.transform.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>().materials[j] = new Material(obj.GetComponent<SkinnedMeshRenderer>().materials[j]);
            }
        }
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
            if (this.GetComponent<MeshRenderer>() != null)
            {
                for (int j=0; j<this.GetComponent<MeshRenderer>().materials.Length; j++)
                {
                    Color c = this.GetComponent<MeshRenderer>().materials[j].color;
                    this.GetComponent<MeshRenderer>().materials[j].renderQueue = 3000;
                    this.GetComponent<MeshRenderer>().materials[j].color = new Color(c.r, c.g, c.b, Mathf.Min(c.a, this.m_Alpha/255.0f));
                }
            }
            if (this.GetComponent<SkinnedMeshRenderer>() != null)
            {
                for (int j=0; j<this.GetComponent<SkinnedMeshRenderer>().materials.Length; j++)
                {
                    Color c = this.GetComponent<SkinnedMeshRenderer>().materials[j].color;
                    this.GetComponent<SkinnedMeshRenderer>().materials[j].renderQueue = 3000;
                    this.GetComponent<SkinnedMeshRenderer>().materials[j].color = new Color(c.r, c.g, c.b, Mathf.Min(c.a, this.m_Alpha/255.0f));
                }
            }
            
            // Update the alpha on all the mesh children renderers
            for (int i=0; i<this.transform.childCount; i++)
            {
                GameObject obj = this.transform.GetChild(i).gameObject;
                if (obj.GetComponent<MeshRenderer>() != null)
                {
                    for (int j=0; j<obj.GetComponent<MeshRenderer>().materials.Length; j++)
                    {
                        Color c = obj.GetComponent<MeshRenderer>().materials[j].color;
                        this.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().materials[j].renderQueue = 3000;
                        this.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().materials[j].color = new Color(c.r, c.g, c.b, Mathf.Min(c.a, this.m_Alpha/255.0f));
                    }
                }
                else if (obj.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    for (int j=0; j<obj.GetComponent<SkinnedMeshRenderer>().materials.Length; j++)
                    {
                        Color c = obj.GetComponent<SkinnedMeshRenderer>().materials[j].color;
                        this.transform.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>().materials[j].renderQueue = 3000;
                        this.transform.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>().materials[j].color = new Color(c.r, c.g, c.b, Mathf.Min(c.a, this.m_Alpha/255.0f));
                    }
                }
            }
        }
    }
}
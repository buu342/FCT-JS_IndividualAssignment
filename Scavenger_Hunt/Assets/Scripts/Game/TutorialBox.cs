using UnityEngine;

public class TutorialBox : MonoBehaviour
{
    public float m_Distance = 10.0f;
    
    public GameObject m_Player;
    private SpriteRenderer m_Sprite;
    private float m_TargetAlpha = 0.0f;
    
    // Debug stuff
    #if UNITY_EDITOR
        [SerializeField]
        private bool DebugRadius = false;
    #endif
    
    
    /*==============================
        Start
        Called when the object is initialized
    ==============================*/
    
    void Start()
    {
        this.m_Sprite = this.transform.Find("Controls").GetComponent<SpriteRenderer>();
        this.m_Sprite.color = Color.clear;
        this.m_Distance *= this.m_Distance;
        this.m_Sprite.enabled = false;
    }
    
    
    /*==============================
        Start
        Called when the object is initialized
    ==============================*/
    
    void Update()
    {
        Vector3 dist = this.m_Player.transform.position - this.transform.position;
        
        // Handle render queue to hide outline
        if (this.m_Sprite.color.a < 0.1f)
            this.m_Sprite.enabled = false;
        
        // Handle alpha
        if (dist.sqrMagnitude < this.m_Distance)
        {
            this.m_Sprite.enabled = true;
            this.m_TargetAlpha = 1.0f;
        }
        else
            this.m_TargetAlpha = 0.0f;
        
        // Actually set the color
        this.m_Sprite.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(this.m_Sprite.color.a, this.m_TargetAlpha, 0.05f));
    }
    
    
    #if UNITY_EDITOR
        /*==============================
            OnDrawGizmos
            Draws extra debug stuff in the editor
        ==============================*/
        
        public virtual void OnDrawGizmos()
        {
            if (DebugRadius)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(this.transform.position, this.m_Distance);
            }
        }
    #endif
}
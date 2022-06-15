using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
public class Pickup : MonoBehaviour
{
    public enum ItemType
    {
        Ammo,
        Scavange
    }
    
    public float m_Distance = 5.0f;
    public ItemType m_ItemType = ItemType.Scavange;
    public int m_Value = 1000;
    
    private GameObject m_Player;
    private SpriteRenderer m_Sprite;
    private float m_TargetAlpha = 0.0f;
    
    // Debug stuff
    #if UNITY_EDITOR
        [SerializeField]
        private bool DebugRadius = false;
    #endif
    
    
    void OnEnable() {
        InputManagerScript.playerInput.Player.Pickup.started +=  PickedUp;
    }
    void OnDisable() {
        InputManagerScript.playerInput.Player.Pickup.started -=  PickedUp;
    }
    
    
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
        if (this.m_Player == null)
            return;
        Vector3 dist = this.m_Player.transform.position - this.transform.position;

        // Handle render queue to hide outline
        if (this.m_Sprite.color.a < 0.1f)
            this.m_Sprite.enabled = false;
        
        // If we have too much reserve ammo, don't show ammo pickup
        if (this.m_ItemType == ItemType.Ammo && this.m_Player.GetComponent<PlayerController>().GetPlayerAmmoReserve() == PlayerController.ClipSize)
        {
            this.m_TargetAlpha = 0.0f;
            this.m_Sprite.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(this.m_Sprite.color.a, this.m_TargetAlpha, 0.05f));
            return;
        }
        
        // Rotate billboard
        this.m_Sprite.gameObject.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        
        // Handle alpha
        if (dist.magnitude < this.m_Distance)
        {
            this.m_Sprite.enabled = true;
            this.m_TargetAlpha = 1.0f;
        }
        else
            this.m_TargetAlpha = 0.0f;
        
        // Actually set the color
        this.m_Sprite.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(this.m_Sprite.color.a, this.m_TargetAlpha, 0.05f));
    }
    
    void PickedUp(InputAction.CallbackContext context)
    {
        if (this.m_ItemType == ItemType.Ammo && this.m_Player.GetComponent<PlayerController>().GetPlayerAmmoReserve() == PlayerController.ClipSize)
            return;
        if ((this.m_Player.transform.position - this.transform.position).magnitude < this.m_Distance)
        {
            if (this.m_ItemType == ItemType.Scavange)
            {
                LevelManager levelmanager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
                levelmanager.PickedUpPickups();
                levelmanager.GiveScore(this.m_Value);
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Gameplay/Pickup", this.transform.position);
            }
            else
            {
                this.m_Player.GetComponent<PlayerController>().SetPlayerAmmoReserve(Mathf.Min(this.m_Player.GetComponent<PlayerController>().GetPlayerAmmoReserve()+this.m_Value, PlayerController.ClipSize));
                GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Gameplay/PickupAmmo", this.transform.position);
            }
            Destroy(this.transform.parent.gameObject);
        }
    }
    
    public void SetPlayer(GameObject obj)
    {
        this.m_Player = obj;
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
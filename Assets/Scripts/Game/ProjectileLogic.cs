using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    private const float MaxPlayerAngleDifference = 110.0f;
    
    public GameObject m_Owner = null;
    public float m_Speed = 0;
    public float m_Damage = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody>().velocity = this.transform.forward*m_Speed;
        if (this.m_Owner != null)
            Physics.IgnoreCollision(this.m_Owner.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
    }
    
    public GameObject GetOwner()
    {
        return this.m_Owner;
    }
    
    public void SetOwner(GameObject owner)
    {
        if (this.m_Owner != null)
            Physics.IgnoreCollision(this.m_Owner.GetComponent<Collider>(), this.GetComponent<Collider>(), false);
        this.m_Owner = owner;
        Physics.IgnoreCollision(this.m_Owner.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
    }
    
    public void SetSpeed(float speed)
    {
        this.m_Speed = speed;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Sword":
                SwordLogic sword = other.gameObject.GetComponent<SwordLogic>();
                
                // If the owner of this bullet is different from the sword's, then reflect the projectile
                if (sword.GetOwner() != this.GetOwner())
                {
                    this.SetOwner(sword.GetOwner());
                    this.transform.rotation = other.gameObject.transform.rotation;
                    this.GetComponent<Rigidbody>().velocity = this.transform.forward*m_Speed;
                }
                return;
            case "Player":
                // If we hit our owner, don't bother checking anything else
                if (this.GetOwner() == other.gameObject)
                    return;
                
                // Check if the player was using a melee attack when we hit
                PlayerCombat ply = other.gameObject.GetComponent<PlayerCombat>();
                if (ply.GetCombatState() == PlayerCombat.CombatState.Melee)
                {
                    GameObject plyfireattach = ply.GetFireAttachment();
                    float angledif = Vector3.Angle(this.transform.forward, plyfireattach.transform.forward);
                    
                    // And the player was facing towards the bullet, then reflect the projectile
                    if (angledif > ProjectileLogic.MaxPlayerAngleDifference)
                    {
                        this.SetOwner(other.gameObject);
                        this.transform.rotation = ply.GetFireAttachment().transform.rotation;
                        this.GetComponent<Rigidbody>().velocity = this.transform.forward*m_Speed;
                        return;
                    }
                }
                break;
            case "Enemies":
                EnemyLogic enemy = other.gameObject.GetComponent<EnemyLogic>();
                enemy.TakeDamage(this.m_Damage);
                break;
            default:
                break;
        }
        Destroy(this.gameObject);
    }
    
    private void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }
}
/****************************************************************
                       ProjectileLogic.cs
    
This script handles homing rocket logic
****************************************************************/

using UnityEngine;

public class RocketLogic : MonoBehaviour
{
    private const float RotateSpeed = 10;
    
    public GameObject m_Owner = null;
    public GameObject m_Target = null;
    public float m_ExplodeRadius = 1.5f;
    public GameObject m_HurtPrefab = null;
    public float m_Speed = 5.0f;
    public int m_Damage = 20;
    public int m_Health = 20;
    public GameObject m_ExplodeEffect;
    
    private Rigidbody m_rb;
    private AudioManager m_audio; 
    
    
    /*==============================
        Start
        Called when the rocket is initialized
    ==============================*/
    
    void Start()
    {
        this.m_rb = this.GetComponent<Rigidbody>();
        this.m_audio = FindObjectOfType<AudioManager>();
    }
    
    
    /*==============================
        FixedUpdate
        Called every engine tick
    ==============================*/

    void FixedUpdate()
    {
        Vector3 direction = this.m_Target.transform.position - this.transform.position;
        direction.Normalize();
        this.m_rb.angularVelocity = -Vector3.Cross(direction, this.transform.forward)*RocketLogic.RotateSpeed;
        this.m_rb.velocity = this.transform.forward*this.m_Speed;
    }
    

    /*==============================
        SetTarget
        Sets the rocket's owner
        @param The gameobject to set as the target
    ==============================*/
    
    public void SetTarget(GameObject target)
    {
        this.m_Target = target;
    }
    

    /*==============================
        SetOwner
        Sets the rocket's owner
        @param The gameobject to set as the owner
    ==============================*/
    
    public void SetOwner(GameObject owner)
    {
        this.m_Owner = owner;
    }
    
    
    /*==============================
        OnTriggerEnter
        Handles collision response
        @param The object we collided with
    ==============================*/
    
    void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Sword":
            case "Bullet":
                this.m_Health = Mathf.Max(0, this.m_Health - 10);
                if (this.m_Health == 0)
                    Explode();
                Destroy(other.gameObject);
                return;
            case "NoCollide":
                return;
            default:
                Explode();
                break;
        }
    }
    
    
    /*==============================
        Explode
        Makes this object explode
    ==============================*/
    
    void Explode()
    {
        HurtTrigger explosion = Instantiate(this.m_HurtPrefab, this.transform.position, this.transform.rotation).GetComponent<HurtTrigger>();
        explosion.SetDamage(this.m_Damage);
        explosion.SetRadius(this.m_ExplodeRadius);
        explosion.SetDieTime(0.1f);
        Instantiate(this.m_ExplodeEffect, this.transform.position, Quaternion.identity);
        this.m_audio.Play("Effects/Explosion_Big", this.transform.position);
        Destroy(this.gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLights : MonoBehaviour
{
    public Light m_Light; 
    public ParticleSystem particles;
    public float timeToFlick;
    public MeshRenderer mesh;
    public Material OnMaterial;
    public Material OffMaterial;
    private float flickTimer;
    private bool isFlickering;
    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        this.m_Light = GetComponent<Light>();
        for (int i=0; i<this.mesh.materials.Length; i++)
            this.mesh.materials[i] = new Material(this.mesh.materials[i]);
    }

    // Update is called once per frame
    void Update()
    {
        if(!isFlickering) {
            StartCoroutine(Flicker());
        }
    }

    IEnumerator Flicker() {
        isFlickering = true;
        GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("Gameplay/LightFlicker");
        Material[] mats = (Material[]) this.mesh.materials.Clone();
        bool sparkle = Random.Range(10,20) <= 15;
        flickTimer = 1.5f;
        if(sparkle)
        {
            this.m_Light.enabled = false;
            particles.Play();
            mats[0] = OffMaterial;
        }
        this.mesh.materials = mats;
        yield return new WaitForSeconds(flickTimer);
        particles.Stop();
        mats[0] = OnMaterial;
        this.m_Light.enabled = true;
        flickTimer = Random.Range(timeToFlick/2, timeToFlick);
        this.mesh.materials = mats;
        yield return new WaitForSeconds(flickTimer);
        particles.Stop();
        isFlickering = false;
    }
}

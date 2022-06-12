using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLights : MonoBehaviour
{
    public Light light; 
    public ParticleSystem particles;
    public float timeToFlick;
    public bool isFlickering;
    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        light = GetComponent<Light>();
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
        bool sparkle = Random.Range(10,20) <= 15;
        light.enabled = false;
        timeToFlick = Random.Range(0.5f, 20f);
        if(sparkle) {
        }
        yield return new WaitForSeconds(timeToFlick);
        if(sparkle)
            particles.Play();
        light.enabled = true;
        timeToFlick = Random.Range(0.5f, 1f);
        yield return new WaitForSeconds(timeToFlick);
        particles.Stop();
        isFlickering = false;
    }
}

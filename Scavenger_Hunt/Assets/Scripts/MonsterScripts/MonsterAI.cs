using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    bool hearsPlayer;
    public int hearingDistance;
    // Start is called before the first frame update
    void Start()
    {
        hearsPlayer = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HearsSound(Vector3 origin, float distance) {
        if(Vector3.Distance(origin, transform.position) < (distance + hearingDistance)) {
            hearsPlayer = true;
        }

    }

}

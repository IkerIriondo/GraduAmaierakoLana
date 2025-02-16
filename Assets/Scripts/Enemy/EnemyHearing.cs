using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    public float hearingRadius = 10f;
    public LayerMask noiseLayer;

    [HideInInspector]
    public Vector3 lastHeardPosition;
    [HideInInspector]
    public bool hasHeardNoise = false;

    public void OnNoiseHeard(Vector3 noisePosition)
    {
        if(Vector3.Distance(transform.position, noisePosition) <= hearingRadius)
        {
            hasHeardNoise = true;
            lastHeardPosition = noisePosition;
        }
    }

    public bool GetHasHeardNoise()
    {
        if(hasHeardNoise)
        {
            hasHeardNoise = false;
            return true;
        }
        return false;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Range(0, 360)]
    public float angle;
    public float radius;

    public GameObject player;
    
    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");  
        StartCoroutine(FOVRuntime()); 
    }

    public void StartFOVCheck()
    {
        StartCoroutine(FOVRuntime());
    }

    private void Update()
    {
        
    }

    private IEnumerator FOVRuntime(){
        
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);

        while(true){
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck(){
        
        Collider[] rangeCheck = Physics.OverlapSphere(transform.position, radius, targetMask);

        if(rangeCheck.Length != 0)
        {
            Transform target = rangeCheck[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                
                float distanceToTarget = Vector3.Distance(transform.position, player.transform.position);
                
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = true;
                }
                else
                {
                    canSeePlayer = false;
                }

            }
            else
            {
                canSeePlayer = false;
            }
        }
        else
        {
            if(canSeePlayer) canSeePlayer = false;
        }
    }

}

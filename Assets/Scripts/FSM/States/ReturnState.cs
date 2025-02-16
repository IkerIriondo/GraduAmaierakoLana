using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/ReturnState")]
public class ReturnState : State
{
    public float returnSpeed = 3.0f;

    public override void EnterState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} entering Return State...");
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        agent.speed = returnSpeed;
        enemy.SetTargetZone(enemy.assignedZone);
        Zone targetZone = enemy.GetTargetZone();

        if (targetZone != null)
        {
            enemy.SetTargetEdgeWaypoint(enemy.FindClosestEdgeWaypoint(enemy.transform.position, targetZone));
            Waypoint targetEdgeWaypoint = enemy.GetTargetEdgeWaypoint();
            if (targetEdgeWaypoint != null)
            {
                agent.SetDestination(targetEdgeWaypoint.transform.position);
            }
            else
            {
                Debug.LogError("No edge waypoint found in ReturnState!");
            }
        }
        else
        {
            Debug.LogError("No target zone assign to go back");
        }
    }

    public override void UpdateState(EnemyFSM enemy)
    {
        FieldOfView fieldOfView = enemy.GetComponent<FieldOfView>();
        EnemyHearing enemyHearing = enemy.GetComponent<EnemyHearing>();
        NavMeshAgent agent = enemy.GetComponent <NavMeshAgent>();

        if (fieldOfView.canSeePlayer)
        {
            enemy.SwitchState(StatesManager.Instance.chaseState);
            return;
        }
        if (enemyHearing.hasHeardNoise)
        {
            enemy.SwitchState(StatesManager.Instance.investigateState);
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            enemy.SwitchState(StatesManager.Instance.patrolState);
        }
    }

    public override void ExitState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} exiting Return State...");
    }


}

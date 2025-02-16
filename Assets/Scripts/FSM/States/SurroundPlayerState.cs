using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/SurroundPlayerState")]
public class SurroundPlayerState : State
{
    public override void EnterState(EnemyFSM enemy)
    {
        Debug.Log("Entering SurroundPlayerState...");
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        agent.SetDestination(enemy.GetSurroundPoint().transform.position);
    }

    public override void UpdateState(EnemyFSM enemy)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        FieldOfView fieldOfView = enemy.GetComponent<FieldOfView>();
        EnemyHearing enemyHearing = enemy.GetComponent<EnemyHearing>();

        if (fieldOfView.canSeePlayer)
        {
            enemy.SwitchState(StatesManager.Instance.chaseState);
            return;
        }
        if (enemy.HasReachedDestination(agent))
        {
            Waypoint nextWaypoint = FSMTacticalAI.Instance.FindNearestPlayerWaypoint();
            if (nextWaypoint != null)
            {
                enemy.SetSurroundPoint(nextWaypoint);
                agent.SetDestination(nextWaypoint.transform.position);
            }
        }
    }

    public override void ExitState(EnemyFSM enemy)
    {
        Debug.Log("Leaving SurroundPlayerState...");
    }

}

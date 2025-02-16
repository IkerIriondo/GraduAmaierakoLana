using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/PatrolState")]
public class PatrolState : State
{
    public float minDistance = 0.5f;
    public float patrolSpeed = 4.0f;

    public override void EnterState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} entering Patrol State...");

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        Zone assignedZone = enemy.assignedZone;
        agent.speed = patrolSpeed;

        Waypoint nearestWaypoint = enemy.FindNearestWaypoint(enemy.transform.position, assignedZone);
        if (nearestWaypoint != null)
        {
            if(nearestWaypoint.type == WaypointType.Regular)
            {
                agent.SetDestination(nearestWaypoint.transform.position);
                enemy.SetCurrentWaypoint(nearestWaypoint);
            }
            else // WaypointType = Edge
            {
                List<Waypoint> posibleWaypoints = nearestWaypoint.connectedWaypoints.FindAll(wp => assignedZone == wp.zones[0]);

                if (posibleWaypoints.Count > 0)
                {
                    Waypoint nextWaypoint = posibleWaypoints[Random.Range(0, posibleWaypoints.Count)];
                    agent.SetDestination(nextWaypoint.transform.position);
                    enemy.SetCurrentWaypoint(nextWaypoint);
                }
            }
            
        }
    }

    public override void UpdateState(EnemyFSM enemy)
    {
        NavMeshAgent agent = enemy.agent;
        FieldOfView fieldOfView = enemy.GetComponent<FieldOfView>();
        EnemyHearing enemyHearing = enemy.GetComponent<EnemyHearing>();

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

        if (enemy.HasReachedDestination(agent))
        {
            Waypoint previousWaypoint = enemy.GetPreviousWaypoint();
            Waypoint currentWaypoint = enemy.GetCurrentWaypoint();

            Waypoint nextWaypoint = enemy.SelectNextWaypoint(agent, currentWaypoint, previousWaypoint);
            if(nextWaypoint != null)
            {
                agent.SetDestination(nextWaypoint.transform.position);
                enemy.SetCurrentWaypoint(nextWaypoint);
            }
            else
            {
                Debug.Log($"{enemy.name}: No valid next waypoint.");
            }
        }

    }

    public override void ExitState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} exiting Patrol State...");
    }

}

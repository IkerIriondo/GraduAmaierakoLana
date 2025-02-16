using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/SearchState")]
public class SearchState : State
{
    public float searchTime = 30f;
    public float searchSpeed = 2.5f;
    public float minDistance = 0.5f;
    public float predictionDistance = 5.0f;

    public float searchMoveDistance = 5f;
    public float angleOffset = 90f;
    public float waypointDetectionRadius = 10f;

    public override void EnterState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} entering Search State...");
        NavMeshAgent agent = enemy.agent;
        agent.speed = searchSpeed;

        enemy.SetAtPredictedPosition(false);
        MoveToPredictedPosition(enemy);
    }

    public override void UpdateState(EnemyFSM enemy)
    {
        FieldOfView fieldOfView = enemy.GetComponent<FieldOfView>();
        EnemyHearing enemyHearing = enemy.GetComponent<EnemyHearing>();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

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

        if(!enemy.GetAtPredictedPosition() && enemy.HasReachedDestination(agent))
        {
            enemy.SetAtPredictedPosition(true);
            TryMoveInDirections(enemy);
        }
        else if(enemy.GetAtPredictedPosition() && enemy.HasReachedDestination(agent))
        {
            Waypoint nearbyWaypoint = FindNearbyWaypoint(enemy);
            if (nearbyWaypoint != null)
            {
                Debug.Log($"{enemy.name} found a waypoint. Switching to PatrolState...");
                enemy.SetCurrentWaypoint(nearbyWaypoint);

                Zone currentZone = enemy.GetCurrentZone();
                if (currentZone == enemy.assignedZone)
                {
                    enemy.SwitchState(StatesManager.Instance.patrolState);
                }
                else
                {
                    enemy.SwitchState(StatesManager.Instance.returnState);
                }
            }
            else
            {
                TryMoveInDirections(enemy);
            }

        }

    }

    public override void ExitState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} exiting Search State...");
        enemy.SetAtPredictedPosition(false);
    }

    private void MoveToPredictedPosition(EnemyFSM enemy)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        Vector3 predictedPosition = enemy.GetPredictedPosition();
        agent.SetDestination(predictedPosition);
    }

    private void TryMoveInDirections(EnemyFSM enemy)
    {
        if (TryMoveInDirection(enemy, enemy.transform.forward))
        {
            Debug.Log($"{enemy.name} moving forward.");
            return;
        }

        if (TryMoveInDirection(enemy, Quaternion.Euler(0, angleOffset, 0) * enemy.transform.forward))
        {
            Debug.Log($"{enemy.name} moving right.");
            return;
        }

        if (TryMoveInDirection(enemy, Quaternion.Euler(0, -angleOffset, 0) * enemy.transform.forward))
        {
            Debug.Log($"{enemy.name} moving left.");
            return;
        }

        Debug.Log($"{enemy.name} failed all directions, falling back to waypoint navigation.");
        SwitchToWaypointNavigation(enemy);
    }

    private bool TryMoveInDirection(EnemyFSM enemy, Vector3 direction)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        Vector3 targetPosition = enemy.transform.position + direction * searchMoveDistance;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            return true;
        }

        return false;
    }

    private void SwitchToWaypointNavigation(EnemyFSM enemy)
    {
        Waypoint nearbyWaypoint = FindNearbyWaypoint(enemy);
        if (nearbyWaypoint != null)
        {
            enemy.SetCurrentWaypoint(nearbyWaypoint);
            enemy.SwitchState(StatesManager.Instance.patrolState);
        }
        else
        {
            Debug.LogWarning($"{enemy.name} could not find a waypoint to navigate to.");
        }
    }

    private Waypoint FindNearbyWaypoint(EnemyFSM enemy)
    {
        Waypoint closestWaypoint = null;
        float closestDistance = Mathf.Infinity;

        Zone assignedZone = enemy.GetCurrentZone();
        if (assignedZone != null)
        {
            foreach (Waypoint waypoint in assignedZone.waypointsDictionary[WaypointType.Regular])
            {
                float distance = Vector3.Distance(enemy.transform.position, waypoint.transform.position);
                if (distance <= waypointDetectionRadius && distance < closestDistance)
                {
                    closestWaypoint = waypoint;
                    closestDistance = distance;
                }
            }
        }

        return closestWaypoint;
    }

}

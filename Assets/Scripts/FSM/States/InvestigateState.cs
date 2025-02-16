using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

[CreateAssetMenu(menuName = "FSM/InvestigateState")]
public class InvestigateState : State
{
    public float investigateTimePerPoint = 3f;
    public int maxInterestPoints = 5;
    public float interestPointRadius = 10f;

    public override void EnterState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} entering Investigate State...");
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        EnemyHearing enemyHearing = enemy.GetComponent<EnemyHearing>();
        Vector3 noiseOrigin = enemyHearing.lastHeardPosition;

        enemy.SetCurrentTarget(noiseOrigin);
        agent.SetDestination(noiseOrigin);
        enemy.SetInterestsPoints(GenerateInterestPoints(enemy, noiseOrigin));

        enemy.ResetInvestigateTimer();
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

        if (enemyHearing.hasHeardNoise)
        {
            Debug.Log($"{enemy.name} heard another noise. Redirecting to new noise origin...");
            enemyHearing.hasHeardNoise = false;

            Vector3 noiseOrigin = enemyHearing.lastHeardPosition;
            enemy.SetCurrentTarget(noiseOrigin);
            agent.SetDestination(noiseOrigin);

            enemy.ResetInvestigateTimer();
            return;
        }

        if (enemy.HasReachedDestination(agent))
        {
            enemy.SumTime(Time.deltaTime);

            if (enemy.GetInvestigateTimer() >= investigateTimePerPoint)
            {
                enemy.ResetInvestigateTimer();

                Queue<Vector3> interestPoints = enemy.GetInterestPoints();

                if (interestPoints.Count > 0)
                {
                    Vector3 nextTarget = interestPoints.Dequeue();
                    enemy.SetCurrentTarget(nextTarget);
                    agent.SetDestination(nextTarget);
                }
                else
                {
                    Debug.Log($"{enemy.name} finished investigating. Returning to patrol...");
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
            }
        }
    }

    public override void ExitState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} exiting Investigate State...");
        enemy.SetInterestsPoints(new Queue<Vector3>());
        enemy.ResetInvestigateTimer();
        enemy.SetCurrentTarget(Vector3.zero);
    }

    private Queue<Vector3> GenerateInterestPoints(EnemyFSM enemy, Vector3 origin)
    {
        Queue<Vector3> points = new Queue<Vector3>();

        Zone assignedZone = enemy.assignedZone;
        if(assignedZone != null)
        {
            foreach (var waypointList in assignedZone.waypointsDictionary.Values)
            {
                foreach (var waypoint in waypointList)
                {
                    if (Vector3.Distance(waypoint.transform.position, origin) <= interestPointRadius)
                    {
                        points.Enqueue(waypoint.transform.position);
                        if (points.Count >= maxInterestPoints) return points;
                    }
                }
            }
        }
        Debug.Log($"{enemy.name} found {points.Count} interesting points");
        return points;
    }

}

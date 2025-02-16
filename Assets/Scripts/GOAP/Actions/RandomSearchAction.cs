using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomSearchAction : Action
{
    private NavMeshAgent agent;
    private WorldState worldState;
    private bool isDone = false;

    public RandomSearchAction(GameObject enemy, WorldState worldState, NavMeshAgent agent) : base(enemy, "RandomSearch", 2)
    {
        this.agent = agent;
        this.worldState = worldState;

        Preconditions.Add(WorldStateKeys.AtPredictedPosition, true);

        Effects.Add(WorldStateKeys.SearchingRandomly, true);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.AtPredictedPosition);
    }

    public override bool IsDone()
    {
        return isDone;
    }

    public override void ResetAction()
    {
        isDone = false;
    }

    public override bool PerformAction()
    {
        if (target == null) return false;

        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += agent.transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            isDone = true;
        }

        return true;
    }
}

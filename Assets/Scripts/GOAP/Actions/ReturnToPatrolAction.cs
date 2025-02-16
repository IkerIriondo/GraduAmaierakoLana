using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ReturnToPatrolAction : Action
{
    private WorldState worldState;
    private NavMeshAgent agent;
    private bool isDone = false;

    public ReturnToPatrolAction(GameObject enemy, WorldState worldState, NavMeshAgent navMeshAgent) : base(enemy, "ReturnToPatrol", 1)
    {
        this.worldState = worldState;
        this.agent = navMeshAgent;

        Preconditions.Add(WorldStateKeys.SearchedNoiseLocation, true);

        Effects.Add(WorldStateKeys.NoiseHeard, false);
        Effects.Add(WorldStateKeys.SearchedNoiseLocation, false);
        Effects.Add(WorldStateKeys.AtNoiseLocation, false);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.SearchedNoiseLocation);
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
        isDone = true;
        return true;
    }

}

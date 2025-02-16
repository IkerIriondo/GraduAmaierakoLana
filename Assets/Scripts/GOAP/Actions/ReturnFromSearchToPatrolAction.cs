using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ReturnFromSearchToPatrolAction : Action
{
    private NavMeshAgent agent;
    private WorldState worldState;
    private bool isDone = false;

    public ReturnFromSearchToPatrolAction(GameObject enemy, WorldState worldState, NavMeshAgent agent) : base(enemy, "ReturnFromSearchToPatrol", 0)
    {
        this.agent = agent;
        this.worldState = worldState;

        Preconditions.Add(WorldStateKeys.SearchingRandomly, true);

        Effects.Add(WorldStateKeys.SearchPlayer, false);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.SearchingRandomly);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToNoiseAction : Action
{
    private NavMeshAgent agent;
    private WorldState worldState;
    private bool isDone = false;

    public MoveToNoiseAction(GameObject enemy, WorldState state, NavMeshAgent agent) : base(enemy, "MoveToNoise", 2)
    {
        this.agent = agent;
        this.worldState = state;

        Preconditions.Add(WorldStateKeys.NoiseHeard, true);

        Effects.Add(WorldStateKeys.AtNoiseLocation, true);
        Effects.Add(WorldStateKeys.IsPatrolling, false);
        Effects.Add(WorldStateKeys.AtWaypoint, false);
    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.NoiseHeard);
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

        GOAPAgent goapAgent = target.GetComponent<GOAPAgent>();
        if (goapAgent == null) return false;

        Vector3 noisePosition = goapAgent.GetNoisePosition();
        agent.SetDestination(noisePosition);

        if (goapAgent.HasReachedDestination())
        {
            isDone = true;
            return true;
        }
        return false;
    }

}

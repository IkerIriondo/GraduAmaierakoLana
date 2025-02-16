using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToWaypointAction : Action
{
    private WorldState worldState;
    private NavMeshAgent agent;
    private bool isDone = false;

    public MoveToWaypointAction(GameObject enemy, WorldState worldState, NavMeshAgent agent) : base(enemy, "MoveToWaypoint", 2)
    {
        this.worldState = worldState;
        this.agent = agent;

        Preconditions.Add(WorldStateKeys.IsPatrolling, true);
        Preconditions.Add(WorldStateKeys.AtWaypoint, false);

        Effects.Add(WorldStateKeys.IsPatrolling, false);
        Effects.Add(WorldStateKeys.AtWaypoint, true);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.IsPatrolling) && !worldState.HasState(WorldStateKeys.AtWaypoint);
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

        if (goapAgent.HasReachedDestination())
        {
            isDone = true;
            return true;
        }
        return false;
    }

}

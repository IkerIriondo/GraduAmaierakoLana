using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FindNextWaypointAction : Action
{
    private WorldState worldState;
    private NavMeshAgent agent;
    private bool isDone = false;

    public FindNextWaypointAction(GameObject enemy, WorldState worldState, NavMeshAgent agent) : base(enemy, "FindNextWaypoint", 0)
    {
        this.worldState = worldState;
        this.agent = agent;

        Preconditions.Add(WorldStateKeys.IsPatrolling, false);
        Preconditions.Add(WorldStateKeys.AtWaypoint, true);

        Effects.Add(WorldStateKeys.IsPatrolling, true);
        Effects.Add(WorldStateKeys.AtWaypoint, false);

    }

    public override bool IsAchievable()
    {
        return !worldState.HasState(WorldStateKeys.IsPatrolling) && worldState.HasState(WorldStateKeys.AtWaypoint);
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

        Waypoint nextWaypoint = goapAgent.SelectNextWaypoint();
        if(nextWaypoint != null)
        {
            agent.SetDestination(nextWaypoint.transform.position);
            isDone = true;
            return true;
        }
        return false;
    }
}

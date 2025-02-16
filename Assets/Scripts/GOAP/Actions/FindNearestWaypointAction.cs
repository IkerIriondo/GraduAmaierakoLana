using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FindNearestWaypointAction : Action
{
    private NavMeshAgent agent;
    private WorldState worldState;
    private bool isDone = false;

    public FindNearestWaypointAction(GameObject enemy, WorldState state, NavMeshAgent agent) : base(enemy, "FindNearestWaypoint", 0)
    {
        this.agent = agent;
        worldState = state;

        Preconditions.Add(WorldStateKeys.IsPatrolling, false);
        Preconditions.Add(WorldStateKeys.AtWaypoint, false);

        Effects.Add(WorldStateKeys.IsPatrolling, true);
        Effects.Add(WorldStateKeys.AtWaypoint, false);
    }

    public override bool IsAchievable()
    {
        return !worldState.HasState(WorldStateKeys.IsPatrolling) && !worldState.HasState(WorldStateKeys.AtWaypoint);
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
        agent.speed = 4;
        if (target == null) return false;

        GOAPAgent goapAgent = target.GetComponent<GOAPAgent>();
        if(goapAgent == null) return false;

        Waypoint nearestWaypoint = goapAgent.FindNearestWaypoint();
        if(nearestWaypoint != null)
        {
            agent.SetDestination(nearestWaypoint.transform.position);
            isDone = true;
            return true;
        }
        return false;
    }

}

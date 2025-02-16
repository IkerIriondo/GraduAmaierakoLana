using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToSurroundPositionAction : Action
{
    private NavMeshAgent agent;
    private WorldState worldState;
    private bool isDone = false;

    public MoveToSurroundPositionAction(GameObject enemy, WorldState worldState, NavMeshAgent agent) : base(enemy, "MoveToSurroundPosition", 3)
    {
        this.worldState = worldState;
        this.agent = agent;

        Preconditions.Add(WorldStateKeys.MoveToSurroundPosition, true);
        Effects.Add(WorldStateKeys.MoveToSurroundPosition, false);
    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.PlayerVisible);
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

        NavMeshAgent agent = goapAgent.GetComponent<NavMeshAgent>();
        agent.speed = 6;

        Waypoint assignedWaypoint = goapAgent.GetSurroundPoint();
        if (assignedWaypoint != null)
        {
            agent.SetDestination(assignedWaypoint.transform.position);

            if (goapAgent.HasReachedDestination())
            {
                Waypoint nextWaypoint = GetNextSurroundWaypoint(goapAgent);
                if (nextWaypoint != null)
                {
                    goapAgent.SetSurroundPoint(nextWaypoint);
                    agent.SetDestination(nextWaypoint.transform.position);
                }
                return false;
            }
        }
        return false;
    }

    private Waypoint GetNextSurroundWaypoint(GOAPAgent goapAgent)
    {
        Waypoint closestWaypoint = GOAPTacticalAI.Instance.ClosestWaypoint;
        if (closestWaypoint == null) return null;

        List<Waypoint> connectedWaypoints = new List<Waypoint>(closestWaypoint.connectedWaypoints);

        connectedWaypoints.RemoveAll(wp => GOAPTacticalAI.Instance.IsWaypointOccupied(wp));

        if (connectedWaypoints.Count > 0)
        {
            return connectedWaypoints[Random.Range(0, connectedWaypoints.Count)];
        }

        return closestWaypoint;
    }
}

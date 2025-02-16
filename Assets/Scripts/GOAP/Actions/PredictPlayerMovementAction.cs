using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PredictPlayerMovementAction : Action
{
    private WorldState worldState;
    private NavMeshAgent agent;
    private bool isDone = false;

    public PredictPlayerMovementAction(GameObject enemy, WorldState worldState, NavMeshAgent agent) : base(enemy, "PredictPlayerMovement", 1)
    {
        this.worldState = worldState;
        this.agent = agent;

        Preconditions.Add(WorldStateKeys.PlayerVisible, false);
        Preconditions.Add(WorldStateKeys.HasLastKnownPosition, true);

        Effects.Add(WorldStateKeys.PredictedPosition, true);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.HasLastKnownPosition) && !worldState.HasState(WorldStateKeys.PlayerVisible);
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
        PredictPlayerMovement();
        isDone = true;
        return true;
    }

    private void PredictPlayerMovement()
    {
        if (worldState.lastKnownPosition == null || worldState.lastKnownForward == Vector3.zero)
        {
            Debug.LogWarning("Cannot predict player movement: missing data.");
            return;
        }

        Vector3 forward = worldState.lastKnownForward * 5.0f;
        Vector3 right = Quaternion.Euler(0, 90, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -90, 0) * forward;

        Zone zone = target.GetComponent<GOAPAgent>().GetAssignedZone();

        if (ValidateDirection(worldState.lastKnownPosition, forward))
        {
            agent.SetDestination(worldState.lastKnownPosition + forward);
        }
        else if (ValidateDirection(worldState.lastKnownPosition, right))
        {
            agent.SetDestination(worldState.lastKnownPosition + right);
        }
        else if (ValidateDirection(worldState.lastKnownPosition, left))
        {
            agent.SetDestination(worldState.lastKnownPosition + left);
        }
        else
        {
            Waypoint fallbackWaypoint = FindWaypointInDirection(worldState.lastKnownPosition, forward, zone);
            if (fallbackWaypoint != null)
            {
                agent.SetDestination(fallbackWaypoint.transform.position);
            }
            else
            {
                Debug.LogWarning("No valid prediction or waypoints.");
                agent.SetDestination(Vector3.zero);
            }
        }
    }

    public Waypoint FindWaypointInDirection(Vector3 origin, Vector3 direction, Zone zone)
    {
        List<Waypoint> candidates = new List<Waypoint>();
        foreach (var waypointList in zone.waypointsDictionary.Values)
        {
            foreach (var waypoint in waypointList)
            {
                Vector3 toWaypoint = (waypoint.transform.position - origin).normalized;
                if (Vector3.Dot(direction.normalized, toWaypoint) > 0.7f)
                {
                    candidates.Add(waypoint);
                }
            }
        }

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    public bool ValidateDirection(Vector3 origin, Vector3 direction)
    {
        Vector3 targetPosition = origin + direction;
        return NavMesh.SamplePosition(targetPosition, out _, 1.0f, NavMesh.AllAreas);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class TacticalAI : MonoBehaviour
{
    public static TacticalAI ActiveInstance { get; protected set; }

    public bool coordinated = false;
    public Vector3 LastKnownPlayerPosition = Vector3.zero;
    public Waypoint ClosestWaypoint = null;
    public MonoBehaviour leader = null;
    public List<MonoBehaviour> coordinatedEnemies = new List<MonoBehaviour>();
    public List<Waypoint> connectedWaypoints;

    protected virtual void Awake()
    {
        StartCoroutine(UpdateClosestWaypointRoutine());
    }

    public void StartCoordination(Vector3 position, MonoBehaviour leader)
    {
        Debug.Log("Starting coordination...");
        coordinated = true;
        LastKnownPlayerPosition = position;
        this.leader = leader;

        ClosestWaypoint = FindNearestPlayerWaypoint();
        connectedWaypoints = new List<Waypoint>(ClosestWaypoint.connectedWaypoints);

        AssignEnemiesForCoordination();
        AssignWaypointsToEnemies();
    }

    public void SetLastKnownPlayerPosition(Vector3 position)
    {
        LastKnownPlayerPosition = position;
    }

    public Waypoint FindNearestPlayerWaypoint()
    {
        Zone playerZone = EnviromentManager.Instance.playerCurrentZone;
        float minDistance = Mathf.Infinity;
        Waypoint closestWaypoint = null;

        foreach (var wp in playerZone.waypointsDictionary[WaypointType.Regular])
        {
            float distance = Vector3.Distance(LastKnownPlayerPosition, wp.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestWaypoint = wp;
            }
        }
        return closestWaypoint;
    }

    private IEnumerator UpdateClosestWaypointRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 2f));
            if (coordinated)
            {
                Waypoint newClosestWaypoint = FindNearestPlayerWaypoint();
                if (newClosestWaypoint != null && newClosestWaypoint != ClosestWaypoint)
                {
                    Debug.Log("Updating closest waypoint...");
                    ClosestWaypoint = newClosestWaypoint;
                    UpdateEnemyWaypoints();
                }
            }
        }
    }

    protected void UpdateEnemyWaypoints()
    {
        if (ClosestWaypoint == null) return;
        connectedWaypoints = new List<Waypoint>(ClosestWaypoint.connectedWaypoints);
        AssignWaypointsToEnemies();
    }

    public void OnPlayerZoneChanged(Zone newZone)
    {
        if (coordinated)
        {
            Debug.Log("Player changed zones. Updating TacticalAI...");
            LastKnownPlayerPosition = newZone.transform.position;
            ClosestWaypoint = FindNearestPlayerWaypoint();
            connectedWaypoints = new List<Waypoint>(ClosestWaypoint.connectedWaypoints);
            AssignEnemiesForCoordination();
            AssignWaypointsToEnemies();
        }
    }

    protected void AssignWaypointsToEnemies()
    {
        int min = Mathf.Min(coordinatedEnemies.Count, connectedWaypoints.Count);
        for (int i = 0; i < min; i++)
        {
            ApplySurroundStrategy(coordinatedEnemies[i], connectedWaypoints[i]);
        }
    }

    protected abstract void AssignEnemiesForCoordination();
    protected abstract void ApplySurroundStrategy(MonoBehaviour enemy, Waypoint waypoint);
}

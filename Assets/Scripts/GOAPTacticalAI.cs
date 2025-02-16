using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPTacticalAI : TacticalAI
{
    private static GOAPTacticalAI instance;
    public static GOAPTacticalAI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GOAPTacticalAI>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GOAPTacticalAI");
                    instance = go.AddComponent<GOAPTacticalAI>();
                }
            }
            return instance;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ActiveInstance = this;
    }

    protected override void AssignEnemiesForCoordination()
    {
        Zone playerZone = EnviromentManager.Instance.playerCurrentZone;
        foreach (var zone in playerZone.connectedZones)
        {
            if (zone.goapEnemiesInZone.Count > 0)
            {
                coordinatedEnemies.Add(zone.goapEnemiesInZone[0]);
            }
        }
    }

    protected override void ApplySurroundStrategy(MonoBehaviour enemy, Waypoint waypoint)
    {
        GOAPAgent goapEnemy = enemy as GOAPAgent;
        if (goapEnemy != null)
        {
            goapEnemy.SetSurroundPoint(waypoint);
            goapEnemy.SetState(WorldStateKeys.MoveToSurroundPosition, true);
            // goapEnemy.UpdateGoalPriority("SurroundPlayer", 3);
        }
    }

    private HashSet<Waypoint> occupiedWaypoints = new HashSet<Waypoint>();

    public bool IsWaypointOccupied(Waypoint waypoint)
    {
        return occupiedWaypoints.Contains(waypoint);
    }

    public void AssignWaypoint(GOAPAgent agent, Waypoint waypoint)
    {
        if (!occupiedWaypoints.Contains(waypoint))
        {
            occupiedWaypoints.Add(waypoint);
            agent.SetSurroundPoint(waypoint);
        }
    }
}

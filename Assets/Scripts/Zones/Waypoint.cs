using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class Waypoint : MonoBehaviour
{
    public string waypointID;
    public WaypointType type;
    public List<Zone> zones = new List<Zone>();
    public List<Waypoint> connectedWaypoints = new List<Waypoint>();

    private float maxConnectionDistance = 25f;

    private void Start()
    {
        waypointID = gameObject.name;
    }

    public void WaypointZoneConnect()
    {
        Collider[] overlappingColliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var collider in overlappingColliders)
        {
            Zone zone = collider.GetComponent<Zone>();
            if (zone != null && !zones.Contains(zone))
            {
                zones.Add(zone);
                zone.AddWaypoint(this);
            }
        }
    }

    public void AutoConnectWaypoints()
    {
        HashSet<Waypoint> candidateWaypoints = new HashSet<Waypoint>();

        foreach(var zone in zones)
        {
            foreach(var waypointList in zone.waypointsDictionary.Values)
            {
                foreach(var waypoint in waypointList)
                {
                    candidateWaypoints.Add(waypoint);
                }
            }
        }

        foreach(var otherWaypoint in candidateWaypoints)
        {
            if(otherWaypoint == this) continue;

            if(type == WaypointType.Regular && !zones.Contains(otherWaypoint.zones[0])) continue;

            if(CanNavigateTo(otherWaypoint))
            {
                connectedWaypoints.Add(otherWaypoint); 
            }          
            else
            {
                // Debug.LogWarning($"{waypointID} cannot navigate to {otherWaypoint.waypointID}");
            }  
        }
    }

    private bool CanNavigateTo(Waypoint targetWaypoint)
    {
        float distance = Vector3.Distance(transform.position, targetWaypoint.transform.position);

        if (distance > maxConnectionDistance)
        {
            return false;
        }

        Vector3 direction = targetWaypoint.transform.position - transform.position;

        if(Physics.Raycast(transform.position, direction.normalized, distance, LayerMask.GetMask("Obstruction")))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        bool pathExists = NavMesh.CalculatePath(transform.position, targetWaypoint.transform.position, NavMesh.AllAreas, path);

        if(!pathExists || path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1f);

        Gizmos.color = Color.blue;
        foreach(var connectedWaypoint in connectedWaypoints)
        {
            if(connectedWaypoint != null)
            {
                Gizmos.DrawLine(transform.position, connectedWaypoint.transform.position);
            }
        }
    }
}

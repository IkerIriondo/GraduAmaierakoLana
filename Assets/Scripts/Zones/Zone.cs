using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zone : MonoBehaviour
{
   public string zoneID;
   public Dictionary<WaypointType, List<Waypoint>> waypointsDictionary = new Dictionary<WaypointType, List<Waypoint>>();
   public List<Zone> connectedZones = new List<Zone>();
   public List<EnemyFSM> enemiesInZone = new List<EnemyFSM>();
   public List<GOAPAgent> goapEnemiesInZone = new List<GOAPAgent>();

    private void Awake()
   {
     zoneID = gameObject.name;

     foreach (WaypointType type in System.Enum.GetValues(typeof(WaypointType)))
     {
          waypointsDictionary[type] = new List<Waypoint>();
     }
   }

   public void AddWaypoint(Waypoint waypoint)
   {
     if(!waypointsDictionary[waypoint.type].Contains(waypoint))
     {
          waypointsDictionary[waypoint.type].Add(waypoint);   
     }
   }

   public void AutoConnectZones()
   {
     foreach(var waypoint in waypointsDictionary[WaypointType.Edge])
     {
          foreach(var zone in waypoint.zones)
          {
               if(zone != this && !connectedZones.Contains(zone))
               {
                    connectedZones.Add(zone);
               }
          }
     }
   }

    public void AddEnemy(EnemyFSM enemy)
    {
        enemiesInZone.Add(enemy);
    }

    public void AddGOAPEnemy(GOAPAgent enemy)
    {
        goapEnemiesInZone.Add(enemy);
    }

    public Waypoint GetCommonEdgeWaypoint(Zone zone)
    {

        List<Waypoint> zoneEdgeWaypoints = zone.waypointsDictionary[WaypointType.Edge];
        List<Waypoint> edgeWaypoints = waypointsDictionary[WaypointType.Edge];

        foreach (Waypoint wp1 in zoneEdgeWaypoints)
        {
            if (edgeWaypoints.Contains(wp1))
            {
                Debug.Log($"Common Edge Waypoint found: {wp1.waypointID}");
                return wp1;
            }
        }

        Debug.Log("No common Edge Waypoint found between the two zones.");
        return null;
    }

    private void OnTriggerEnter(Collider other)
   {
        if(other.CompareTag("Player"))
        {
            Debug.Log($"Player entered Zone: {zoneID}");
            if (zoneID == "End")
            {
                GameManager.Instance.GameWon();
            }
            else
            {
                EnviromentManager.Instance.SetCurrentZone(this);
            }

        }
        else if(other.CompareTag("Enemy"))
        {
            GOAPAgent enemy = other.GetComponent<GOAPAgent>();
            if(enemy != null)
            {
                if(this != enemy.GetAssignedZone())
                {
                    enemy.UpdateZoneState(false);
                }
                else
                {
                    enemy.UpdateZoneState(true);
                }
            }
        }
   }

    public Vector3 GetCenterPosition()
    {
        var allWaypoints = waypointsDictionary.Values.SelectMany(wpList => wpList).ToList();

        if (allWaypoints.Count == 0)
        {
            Debug.LogWarning($"Zone {zoneID} has no waypoints. Returning zero vector.");
            return Vector3.zero;
        }

        Vector3 sum = Vector3.zero;
        foreach (var waypoint in allWaypoints)
        {
            sum += waypoint.transform.position;
        }

        return sum / allWaypoints.Count;
    }

    private void OnDrawGizmos()
    {
         Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.2f);
         BoxCollider boxCollider = GetComponent<BoxCollider>();

         if(boxCollider != null)
         {
              Gizmos.matrix = transform.localToWorldMatrix;
              Gizmos.DrawCube(boxCollider.center, boxCollider.size);
         }

    }
}

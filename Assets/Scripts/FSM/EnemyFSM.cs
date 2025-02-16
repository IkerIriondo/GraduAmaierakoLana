using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

public class EnemyFSM : MonoBehaviour
{
    public State initialState;
    public State currentState;

    public Zone assignedZone;
    private bool isActive = false;

    private float minDistance = 0.5f;

    public NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameManager.Instance.enemies.Add(this);
        DeactivateEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;
        currentState.UpdateState(this);
    }

    public void ActivateEnemy()
    {
        isActive = true;
        gameObject.SetActive(true);

        assignedZone = AssignZone();
        if (assignedZone == null)
        {
            Debug.LogError($"Enemy '{gameObject.name}' could not find its assigned zone.");
            return;
        }
        assignedZone.AddEnemy(this);

        Debug.Log($"Enemy '{gameObject.name}' activated and assigned to zone '{assignedZone.name}'.");

        currentState = initialState;
        currentState.EnterState(this);
    }

    public void DeactivateEnemy()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    public void SwitchState(State newState){
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    public Zone AssignZone()
    {
        Collider[] overlappingColliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach(var collider in overlappingColliders)
        {
            Zone zone = collider.GetComponent<Zone>();
            if(zone != null)
            {
                return zone;
            }
        }
        Debug.LogError("Enemy no zone found");
        return null;
    }

    public Waypoint FindNearestWaypoint(Vector3 position, Zone zone)
    {
        Waypoint nearestWaypoint = null;
        float shortestDistance = Mathf.Infinity;

        if (zone == null) return null;

        foreach (var waypointList in zone.waypointsDictionary.Values)
        {
            foreach (var waypoint in waypointList)
            {
                float distance = Vector3.Distance(position, waypoint.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestWaypoint = waypoint;
                }
            }
        }
        return nearestWaypoint;
    }

    public bool HasReachedDestination(NavMeshAgent agent)
    {
        return agent.remainingDistance <= minDistance && !agent.pathPending;
    }

    public Waypoint SelectNextWaypoint(NavMeshAgent agent, Waypoint currentWaypoint, Waypoint previousWaypoint)
    {
        List<Waypoint> validWaypoints = currentWaypoint.connectedWaypoints.FindAll(wp => wp != previousWaypoint && wp.type == WaypointType.Regular);

        if (validWaypoints.Count == 0)
        {
            validWaypoints = currentWaypoint.connectedWaypoints.FindAll(wp => wp.type == WaypointType.Regular);
        }

        previousWaypoint = currentWaypoint;
        return validWaypoints.Count > 0 ? validWaypoints[Random.Range(0, validWaypoints.Count)] : null;

    }

    public Waypoint FindClosestEdgeWaypoint(Vector3 position, Zone zone)
    {
        Waypoint closestWaypoint = null;
        float shortestDistance = Mathf.Infinity;
        foreach (var wp in zone.waypointsDictionary[WaypointType.Edge])
        {
            float distance = Vector3.Distance(position, wp.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestWaypoint = wp;
            }
        }
        return closestWaypoint;
    }

    public bool IsFacingPlayer(Transform enemy, Vector3 playerPosition, float detectionDistance)
    {
        Vector3 directionToPlayer = (playerPosition - enemy.position).normalized;
        float distance = Vector3.Distance(enemy.position, playerPosition);

        if (distance <= detectionDistance)
        {
            float dotProduct = Vector3.Dot(enemy.forward, directionToPlayer);
            return dotProduct > 0.75f;
        }
        return false;
    }

    public void ResetFSM()
    {
        isActive = true;
        gameObject.SetActive(true);

        currentState = initialState;
        currentState.EnterState(this);
    }

    internal void AssignedZone(Zone zone)
    {
        assignedZone = zone;
        Debug.Log($"Enemy reassigned to Zone: {zone.zoneID}");
    }

    public void PredictPlayerMovement()
    {
        if (lastKnownPosition == null || lastKnownForward == Vector3.zero)
        {
            Debug.LogWarning("Cannot predict player movement: missing data.");
            return;
        }

        Vector3 forward = lastKnownForward * 5.0f;
        Vector3 right = Quaternion.Euler(0, 90, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -90, 0) * forward;

        Zone zone = GetSearchZone();

        if (ValidateDirection(lastKnownPosition, forward))
        {
            SetPredictedPosition(lastKnownPosition + forward);
        }
        else if (ValidateDirection(lastKnownPosition, right))
        {
            SetPredictedPosition(lastKnownPosition + right);
        }
        else if (ValidateDirection(lastKnownPosition, left))
        {
            SetPredictedPosition(lastKnownPosition + left);
        }
        else
        {
            Waypoint fallbackWaypoint = FindWaypointInDirection(lastKnownPosition, forward, zone);
            if (fallbackWaypoint != null)
            {
                SetPredictedPosition(fallbackWaypoint.transform.position);
            }
            else
            {
                Debug.LogWarning("No valid prediction or waypoints.");
                SetPredictedPosition(Vector3.zero);
            }
        }
    }

    public Waypoint FindWaypointInDirection(Vector3 origin, Vector3 direction, Zone zone)
    {
        List<Waypoint> candidates = new List<Waypoint>();
        foreach(var waypointList in zone.waypointsDictionary.Values)
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

    public Waypoint FindValidWaypointInDirection(Vector3 origin, Vector3 direction, Zone zone)
    {
        List<Waypoint> candidates = new List<Waypoint>();

        foreach (var waypointList in zone.waypointsDictionary.Values)
        {
            foreach (var waypoint in waypointList)
            {
                Vector3 toWaypoint = (waypoint.transform.position - origin).normalized;
                if (Vector3.Dot(direction.normalized, toWaypoint) > 0.7f) 
                {
                    if (IsPathValid(origin, waypoint.transform.position))
                    {
                        candidates.Add(waypoint);
                    }
                }
            }
        }

        Waypoint bestWaypoint = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var candidate in candidates)
        {
            float distance = Vector3.Distance(origin, candidate.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                bestWaypoint = candidate;
            }
        }

        return bestWaypoint;
    }

    public void LookAtPoint(Vector3 point)
    {
        Vector3 direction = (point - transform.position).normalized;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private bool IsPathValid(Vector3 origin, Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(origin, target, NavMesh.AllAreas, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }


    private Vector3 predictedPosition;
    public void SetPredictedPosition(Vector3 position)
    {
        predictedPosition = position;
    }

    public Vector3 GetPredictedPosition()
    {
        return predictedPosition;
    }

    public Zone GetCurrentZone()
    {
        Collider[] overlappingColliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach(Collider collider in overlappingColliders)
        {
            Zone zone = collider.GetComponent<Zone>();
            if(zone != null)
            {
                return zone;
            }
        }
        Debug.LogWarning($"{name} could not determine its current zone.");
        return null;
    }

    // Patrol State
    private Waypoint currentWaypoint;
    private Waypoint previousWaypoint;

    public void SetCurrentWaypoint(Waypoint waypoint)
    {
        previousWaypoint = currentWaypoint;
        currentWaypoint = waypoint;
    }

    public Waypoint GetCurrentWaypoint()
    {
        return currentWaypoint;
    }

    public Waypoint GetPreviousWaypoint()
    {
        return previousWaypoint;
    }

    // Chase State
    private Vector3 lastKnownPosition;
    private Vector3 lastKnownForward;

    public void SetLastKnownPosition(Vector3 position)
    {
        lastKnownPosition = position;
    }

    public Vector3 GetLastKnownPosition()
    {
        return lastKnownPosition;
    }
    public void SetLastKnownForward(Vector3 forward)
    {
        lastKnownForward = forward;
    }

    public Vector3 GetLastKnownForward()
    {
        return lastKnownForward;
    }

    // Investigate State
    private Queue<Vector3> interestPoints = new Queue<Vector3>();
    private float investigateTimer;
    private Vector3 currentTarget;

    public Queue<Vector3> GetInterestPoints()
    {
        return interestPoints;
    }

    public void SetInterestsPoints(Queue<Vector3> points)
    {
        interestPoints = points;
    }

    public float GetInvestigateTimer()
    {
        return investigateTimer;
    }

    public void ResetInvestigateTimer()
    {
        investigateTimer = 0;
    }

    public void SumTime(float time)
    {
        investigateTimer += time;
    }

    public Vector3 GetCurrentTarget()
    {
        return currentTarget;
    }

    public void SetCurrentTarget(Vector3 target)
    {
        currentTarget = target;
    }


    // Search State
    private Zone searchZone;
    private bool atPredictedPosition;

    public void SetSearchZone(Zone zone)
    {
        searchZone = zone;
    }

    public Zone GetSearchZone()
    {
        return searchZone;
    }

    public void SetAtPredictedPosition(bool value)
    {
        atPredictedPosition = value;
    }

    public bool GetAtPredictedPosition()
    {
        return atPredictedPosition;
    }

    // Return State 
    private Zone targetZone;
    private Waypoint targetEdgeWaypoint;

    public void SetTargetZone(Zone zone)
    {
        targetZone = zone;
    }

    public Zone GetTargetZone()
    {
        return targetZone;
    }

    public void SetTargetEdgeWaypoint(Waypoint waypoint)
    {
        targetEdgeWaypoint = waypoint;
    }

    public Waypoint GetTargetEdgeWaypoint()
    {
        return targetEdgeWaypoint;
    }

    // Surround
    private Waypoint SurroundPoint;

    public void SetSurroundPoint(Waypoint waypoint)
    {
        SurroundPoint = waypoint;
    }

    public Waypoint GetSurroundPoint()
    {
        return SurroundPoint;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Player directions
        Vector3 forward = lastKnownForward * 5.0f;
        Vector3 right = Quaternion.Euler(0, 90, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -90, 0) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(lastKnownPosition, lastKnownPosition + forward);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(lastKnownPosition, lastKnownPosition + right);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(lastKnownPosition, lastKnownPosition + left);

        if(searchZone != null)
        {
            // Selected waypoint
            Gizmos.color = Color.yellow;
            Waypoint waypoint = FindValidWaypointInDirection(lastKnownPosition, forward, searchZone);
            if (waypoint != null)
            {
                Gizmos.DrawSphere(waypoint.transform.position, 0.5f);
            }
        }
        
    }

}

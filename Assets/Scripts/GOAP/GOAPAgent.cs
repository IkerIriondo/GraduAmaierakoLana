using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GOAPAgent : MonoBehaviour
{
    private bool isActive = true;
    private Zone assignedZone;
    private NavMeshAgent agent;
    private FieldOfView fov;
    private EnemyHearing enemyHearing;
    private float minDistance = 0.5f;

    private Queue<Action> currentActions;
    private GOAPPlanner planner;
    private List<Action> availableActions;
    private WorldState worldState;
    private List<Goal> goals;
    private Goal currentGoal;

    private Goal isPatrolling;
    private Goal notPatrolling;
    private Goal investigateNoise;
    private Goal caughtPlayer;
    private Goal searchPlayer; 
    private Goal surroundPlayer;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.goapEnemies.Add(this);
        DeactivateEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        UpdateWorldState();

        Goal nextGoal = SelectGoal();

        if (currentActions == null || currentActions.Count == 0 || nextGoal != currentGoal)
        {
            currentGoal = nextGoal;
            Dictionary<string, bool> goal = currentGoal.GoalState;

            currentActions = planner.Plan(gameObject, availableActions, worldState.state, goal);
            if (currentActions == null)
            {
                Debug.Log($"No plans found for goal {currentGoal.name}");
                return;
            }
        }

        Action action = currentActions.Peek();
        if (action.IsDone())
        {
            Debug.Log($"{action.name} finished");
            currentActions.Dequeue();

            foreach (var effect in action.Effects)
            {
                worldState.state[effect.Key] = effect.Value;
            }
            action.ResetAction();
        }
        else
        {
            action.PerformAction();
        }
    }

    void UpdateWorldState()
    {
        if (enemyHearing.GetHasHeardNoise() && currentGoal != investigateNoise)
        {
            Debug.Log("Has heard noise");
            worldState.SetState(WorldStateKeys.NoiseHeard, true);
            Debug.Log($" Noise heard in World State -> {worldState.state[WorldStateKeys.NoiseHeard]}");
            investigateNoise.Priority = 3;
        }

        if (fov.canSeePlayer)
        {
            worldState.SetState(WorldStateKeys.PlayerVisible, true);
            caughtPlayer.Priority = 3;
            Transform player = GameObject.FindWithTag("Player").transform;
            worldState.lastKnownPosition = player.position;
            Vector3 worldForward = player.transform.TransformDirection(Vector3.forward);
            worldState.lastKnownForward = worldForward;
            worldState.SetState(WorldStateKeys.HasLastKnownPosition, true);

            if (!GOAPTacticalAI.Instance.coordinated)
            {
                GOAPTacticalAI.Instance.StartCoordination(worldState.lastKnownPosition, this);
            }

        }
        else
        {
            if (worldState.state[WorldStateKeys.PlayerVisible])
            {
                worldState.SetState(WorldStateKeys.PlayerVisible, false);
                caughtPlayer.Priority = 0;
                worldState.SetState(WorldStateKeys.SearchPlayer, true);
            }
            
        }
        
    }

    internal void DeactivateEnemy()
    {
        if(isActive)
        {
            isActive = false;
            gameObject.SetActive(false);
        }
    }

    internal void ActivateEnemy()
    {
        if (!isActive)
        {
            isActive = true;

            planner = new GOAPPlanner();
            assignedZone = AssignZone();
            if (assignedZone == null)
            {
                Debug.LogError($"Enemy '{gameObject.name}' could not find its assigned zone.");
                return;
            }
            else
            {
                assignedZone.AddGOAPEnemy(this);
            }
            agent = GetComponent<NavMeshAgent>();
            worldState = new WorldState();

            isPatrolling = new Goal(new Dictionary<string, bool> { { WorldStateKeys.IsPatrolling, true } }, 1, "Patrol");
            notPatrolling = new Goal(new Dictionary<string, bool> { { WorldStateKeys.IsPatrolling, false } }, 1, "Not Patrol");
            investigateNoise = new Goal(new Dictionary<string, bool> { { WorldStateKeys.NoiseHeard, false } }, 2, "Investigate");
            caughtPlayer = new Goal(new Dictionary<string, bool> { { WorldStateKeys.PlayerCaught, true } }, 0, "Chase");
            searchPlayer = new Goal(new Dictionary<string, bool> { { WorldStateKeys.SearchPlayer, false } }, 2, "Return");
            surroundPlayer = new Goal(new Dictionary<string, bool> { { WorldStateKeys.MoveToSurroundPosition, false } }, 3, "SurroundPlayer");

            goals = new List<Goal> {
                isPatrolling,
                notPatrolling,
                investigateNoise,
                caughtPlayer,
                searchPlayer,
                surroundPlayer
            };

            availableActions = new List<Action> { 
                new FindNearestWaypointAction(gameObject, worldState, agent),
                new MoveToWaypointAction(gameObject, worldState, agent),
                new FindNextWaypointAction(gameObject, worldState, agent),

                new MoveToNoiseAction(gameObject, worldState, agent),
                new WaitAtNoiseLocationAction(gameObject, worldState, agent),
                new ReturnToPatrolAction(gameObject, worldState, agent),

                new ChasePlayerAction(gameObject, worldState, agent),

                new MoveToPredictedPositionAction(gameObject, worldState, agent),
                new PredictPlayerMovementAction(gameObject, worldState, agent),
                new RandomSearchAction(gameObject, worldState, agent),
                new ReturnFromSearchToPatrolAction(gameObject, worldState, agent),

                new MoveToSurroundPositionAction(gameObject, worldState, agent),
            };

            currentGoal = SelectGoal();

            currentActions = new Queue<Action>();

            gameObject.SetActive(true);

            fov = GetComponent<FieldOfView>();
            fov.StartFOVCheck();

            enemyHearing = GetComponent<EnemyHearing>();

            Debug.Log($"Enemy '{gameObject.name}' activated and assigned to zone '{assignedZone.name}'.");
        }
    }

    private Zone AssignZone()
    {
        Collider[] overlappingColliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var collider in overlappingColliders)
        {
            Zone zone = collider.GetComponent<Zone>();
            if (zone != null)
            {
                return zone;
            }
        }
        Debug.LogError("Enemy no zone found");
        return null;
    }

    private Goal SelectGoal()
    {
        Goal bestGoal = null;
        int maxPriority = int.MinValue;

        foreach (var goal in goals)
        {
            bool goalSatisfied = true;

            foreach (var kvp in goal.GoalState)
            {
                if (!worldState.state.ContainsKey(kvp.Key) || worldState.state[kvp.Key] != kvp.Value)
                {
                    goalSatisfied = false;
                    break;
                }
            }

            if (!goalSatisfied && goal.Priority > maxPriority)
            {
                bestGoal = goal;
                maxPriority = goal.Priority;
            }
        }

        return bestGoal;
    }

    internal Waypoint FindNearestWaypoint()
    {
        Waypoint nearestWaypoint = null;
        float shortestDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        if (assignedZone == null) return null;

        foreach (var waypointList in assignedZone.waypointsDictionary.Values)
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
        worldState.CurrentWaypoint = nearestWaypoint;
        return nearestWaypoint;
    }

    internal bool HasReachedDestination()
    {
        return agent.remainingDistance <= minDistance && !agent.pathPending;
    }

    internal Waypoint SelectNextWaypoint()
    {
        Waypoint current = worldState.CurrentWaypoint;
        Waypoint previous = worldState.PreviousWaypoint;

        List<Waypoint> validWaypoints = current.connectedWaypoints.FindAll(wp => wp != previous && wp.type == WaypointType.Regular);

        if (validWaypoints.Count == 0)
        {
            validWaypoints = current.connectedWaypoints.FindAll(wp => wp.type == WaypointType.Regular);
        }

        Waypoint next = validWaypoints.Count > 0 ? validWaypoints[Random.Range(0, validWaypoints.Count)] : null;
        worldState.PreviousWaypoint = current;
        worldState.CurrentWaypoint = next;

        return next;
    }

    public Vector3 GetNoisePosition()
    {
        return enemyHearing.lastHeardPosition;
    }

    public Zone GetAssignedZone()
    {
        return assignedZone;
    }

    public void UpdateZoneState(bool isInZone)
    {
        worldState.SetState(WorldStateKeys.InAssignedZone, isInZone);
    }

    public Waypoint GetSurroundPoint()
    {
        return worldState.SurroundWaypoint;
    }

    public void SetSurroundPoint(Waypoint waypoint)
    {
        worldState.SurroundWaypoint = waypoint;
    }

    public void SetState(string key, bool value)
    {
        worldState.SetState(key, value);
    }

    public void UpdateGoalPriority(string goalName, int priority)
    {
        foreach(var goal in goals)
        {
            if (goal.name == goalName)
            {
                goal.Priority = priority;
                return;
            }
        }
    }

}

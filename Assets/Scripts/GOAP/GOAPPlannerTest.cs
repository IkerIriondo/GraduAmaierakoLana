using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GOAPPlannerTest : MonoBehaviour
{
    [SerializeField] private bool isPatrolling;
    [SerializeField] private bool atWaypoint;

    [SerializeField] private bool noiseHeard;
    [SerializeField] private bool atNoiseLocation;
    [SerializeField] private bool searchedNoiseLocation;

    [SerializeField] private bool isPlayerVisible;
    [SerializeField] private bool isPlayerCaught;

    [SerializeField] private bool HasLastKnownPosition;
    [SerializeField] private bool PredictedPosition;
    [SerializeField] private bool atPredictedPosition;
    [SerializeField] private bool SearchingRandomly;
    [SerializeField] private bool SearchPlayer;

    [SerializeField] private bool MoveToSurroundPosition;

    Goal goalPatrolling;
    Goal goalFindWaypoint;
    Goal goalInvestigateNoise;
    Goal goalCatchPlayer;
    Goal goalSearchPlayer;
    Goal goalSurroundPlayer;
    Goal currentGoal;
    Dictionary<string, bool> worldState;
    List<Action> actions;
    GOAPPlanner planner;

    private void Start()
    {
        worldState = new Dictionary<string, bool>
        {
            { "isPatrolling", isPatrolling },
            { "atWaypoint", atWaypoint },

            { "noiseHeard", noiseHeard },
            { "atNoiseLocation", atNoiseLocation },
            { "searchedNoiseLocation", searchedNoiseLocation },

            { "isPlayerVisible", isPlayerVisible },
            { "isPlayerCaught", isPlayerCaught },

            { "HasLastKnownPosition", HasLastKnownPosition },
            { "PredictedPosition", PredictedPosition },
            { "atPredictedPosition", atPredictedPosition },
            { "SearchingRandomly", SearchingRandomly },
            { "SearchPlayer", SearchPlayer },

            { "MoveToSurroundPosition", MoveToSurroundPosition },
        };

        goalPatrolling = new Goal( new Dictionary<string, bool> { { "isPatrolling", true } }, 1, "Patrol");
        goalFindWaypoint = new Goal( new Dictionary<string, bool> { { "isPatrolling", false } }, 1, "Find Waypoint");
        goalInvestigateNoise = new Goal(new Dictionary<string, bool> { { "noiseHeard", false } }, 2, "Investigate");
        goalCatchPlayer = new Goal(new Dictionary<string, bool> { { "isPlayerCaught", true } }, 0, "Catch Player");
        goalSearchPlayer = new Goal(new Dictionary<string, bool> { { "SearchPlayer", false } }, 2, "Search Player");
        goalSurroundPlayer = new Goal(new Dictionary<string, bool> { { "MoveToSurroundPosition", false } }, 5, "Surround Player");

        currentGoal = SelectGoal();

        actions = new List<Action>
        {
            new TestAction("Find Nearest Waypoint", new Dictionary<string, bool> { { "isPatrolling", false }, { "atWaypoint", false } }, new Dictionary<string, bool> { { "isPatrolling", true }, { "atWaypoint", false } }, 1, worldState),
            new TestAction("Move to Waypoint", new Dictionary<string, bool> { { "isPatrolling", true }, { "atWaypoint", false } }, new Dictionary<string, bool> { { "isPatrolling", false }, { "atWaypoint", true } }, 1, worldState),
            new TestAction("Find Next Waypoint", new Dictionary<string, bool> { { "isPatrolling", false }, { "atWaypoint", true } }, new Dictionary<string, bool> { { "isPatrolling", true }, { "atWaypoint", false } }, 1, worldState),
            new TestAction("Idle", new Dictionary<string, bool> { { "isPatrolling", false }, { "atWaypoint", true } }, new Dictionary<string, bool> { { "atWaypoint", true }, { "isPatrolling", false } }, 0, worldState),

            new TestAction("Move to Noise", new Dictionary<string, bool> { { "noiseHeard", true } }, new Dictionary<string, bool> { { "atNoiseLocation", true }, { "isPatrolling", false }, { "atWaypoint", false } }, 2, worldState),
            new TestAction("Wait at Noise Location", new Dictionary<string, bool> { { "atNoiseLocation", true } }, new Dictionary<string, bool> { { "searchedNoiseLocation", true } }, 3, worldState),
            new TestAction("Return to Patrol", new Dictionary<string, bool> { { "searchedNoiseLocation", true } }, new Dictionary<string, bool> { { "noiseHeard", false }, { "searchedNoiseLocation", false }, { "atNoiseLocation", false } }, 1, worldState),

            new TestAction("Chase Player", new Dictionary<string, bool> { { "isPlayerVisible", true } }, new Dictionary<string, bool> { { "isPlayerCaught", true } }, 2, worldState),

            new TestAction("Predict Player Movement", new Dictionary<string, bool> { { "isPlayerVisible", false }, { "HasLastKnownPosition", true } }, new Dictionary<string, bool> { { "PredictedPosition", true } }, 3, worldState),
            new TestAction("Move to Predicted Position", new Dictionary<string, bool> { { "PredictedPosition", true } }, new Dictionary<string, bool> { { "atPredictedPosition", true } }, 3, worldState),
            new TestAction("Random Search", new Dictionary<string, bool> { { "atPredictedPosition", true } }, new Dictionary<string, bool> { { "SearchingRandomly", true } }, 1, worldState),
            new TestAction("Return to Patrol", new Dictionary<string, bool> { { "SearchingRandomly", true } }, new Dictionary<string, bool> { { "SearchPlayer", false } }, 0, worldState),

            new TestAction("Move to Surround Position", new Dictionary<string, bool> { { "MoveToSurroundPosition", true } }, new Dictionary<string, bool> { { "AtSurroundPosition", false } }, 3, worldState),
            new TestAction("Move to Surround Position", new Dictionary<string, bool> { { "AtSurroundPosition", true } }, new Dictionary<string, bool> { { "MoveToSurroundPosition", false } }, 3, worldState)


        };
        planner = new GOAPPlanner();
        RunTest();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateWorldState();
            currentGoal = SelectGoal();
            RunTest();
        }
    }

    private Goal SelectGoal()
    {
        List<Goal> posibleGoals = new List<Goal>();
        foreach (var goal in new List<Goal> { goalInvestigateNoise, goalFindWaypoint, goalPatrolling, goalCatchPlayer, goalSearchPlayer, goalSurroundPlayer })
        {
            bool goalSatisfied = true;

            foreach (var kvp in goal.GoalState)
            {
                if (!worldState.ContainsKey(kvp.Key) || worldState[kvp.Key] != kvp.Value)
                {
                    goalSatisfied = false;
                    break;
                }
            }

            if (!goalSatisfied)
            {
                posibleGoals.Add(goal);
            }
        }

        int maxPrioriyGoal = 0;
        Goal selectedGoal = null;
        foreach (var goal in posibleGoals)
        {
            if(goal.Priority > maxPrioriyGoal)
            {
                selectedGoal = goal;
                maxPrioriyGoal = goal.Priority;
            }
        }
        return selectedGoal;
    }

    bool GoalAchieved(Dictionary<string, bool> goal)
    {
        foreach (var kvp in goal)
        {
            if (!worldState.ContainsKey(kvp.Key) || worldState[kvp.Key] != kvp.Value)
                return false;
        }
        return true;
    }

    void RunTest()
    {
        Queue<Action> plan = planner.Plan(null, actions, worldState, currentGoal.GoalState);

        Debug.Log($"Current plan for Goal:");
        foreach(var kvp in currentGoal.GoalState)
        {
            Debug.Log($"{kvp.Key} -> {kvp.Value}");
        }
        if(plan != null)
        {
            foreach (var act in plan)
            {
                foreach (var effect in act.Effects)
                {
                    worldState[effect.Key] = effect.Value;
                }
                Debug.Log(act.name);
            }
        }
        else
        {
            Debug.Log("No plan found for current world state");
        }
    }

    void UpdateWorldState()
    {
        worldState["isPatrolling"] = isPatrolling;
        worldState["atWaypoint"] = atWaypoint;

        worldState["noiseHeard"] = noiseHeard;
        worldState["atNoiseLocation"] = atNoiseLocation;
        worldState["searchedNoiseLocation"] = searchedNoiseLocation;

        worldState["isPlayerVisible"] = isPlayerVisible;
        worldState["isPlayerCaught"] = isPlayerCaught;

        worldState["HasLastKnownPosition"] = HasLastKnownPosition;
        worldState["PredictedPosition"] = PredictedPosition;
        worldState["atPredictedPosition"] = atPredictedPosition;
        worldState["SearchingRandomly"] = SearchingRandomly;
        worldState["Searchplayer"] = SearchPlayer;

        worldState["MoveToSurroundPosition"] = MoveToSurroundPosition;

    }

}


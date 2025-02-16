using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class WorldState
{
    public Dictionary<string, bool> state;
    public Waypoint CurrentWaypoint;
    public Waypoint PreviousWaypoint;
    public Vector3 lastKnownPosition;
    public Vector3 lastKnownForward;
    public Waypoint SurroundWaypoint;

    public WorldState()
    {
        state = new Dictionary<string, bool>
        {
            { WorldStateKeys.IsPatrolling, false },
            { WorldStateKeys.AtWaypoint, false },

            { WorldStateKeys.NoiseHeard, false },
            { WorldStateKeys.AtNoiseLocation, false },
            { WorldStateKeys.SearchedNoiseLocation, false },
            { WorldStateKeys.InAssignedZone, true },

            { WorldStateKeys.PlayerVisible, false },
            { WorldStateKeys.PlayerCaught, false },

            { WorldStateKeys.HasLastKnownPosition, false },
            { WorldStateKeys.PredictedPosition, false },
            { WorldStateKeys.AtPredictedPosition, false },
            { WorldStateKeys.SearchingRandomly, false },
            { WorldStateKeys.SearchPlayer, false },

            { WorldStateKeys.MoveToSurroundPosition, false }
        };

        CurrentWaypoint = null;
        PreviousWaypoint = null;
        lastKnownPosition = Vector3.zero;
        lastKnownForward = Vector3.zero;
        SurroundWaypoint = null;

    }

    public bool HasState(string key)
    {
        return state.ContainsKey(key) && state[key];
    }

    internal void SetState(string key, bool value)
    {
        state[key] = value;
    }

}

public static class WorldStateKeys
{
    // Patrol
    public static string IsPatrolling = "IsPatrolling";
    public static string AtWaypoint = "AtWaypoint";

    // Investigate
    public static string NoiseHeard = "NoiseHeard";
    public static string AtNoiseLocation = "AtNoiseLocation";
    public static string SearchedNoiseLocation = "SearchedNoiseLocation";
    public static string InAssignedZone = "InAssignedZone";

    // Chase
    public static string PlayerVisible = "PlayerVisible";
    public static string PlayerCaught = "PlayerCaught";

    // Search
    public static string HasLastKnownPosition = "HasLastKnownPosition";
    public static string PredictedPosition = "PredictedPosition";
    public static string AtPredictedPosition = "AtPredictedPosition";
    public static string SearchingRandomly = "SearchingRandomly";
    public static string SearchPlayer = "SearchPlayer";

    // Surround
    public static string MoveToSurroundPosition = "MoveToSurroundPosition";

}

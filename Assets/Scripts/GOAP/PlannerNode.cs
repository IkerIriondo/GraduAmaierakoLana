using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlannerNode
{
    public Dictionary<string, bool> State;
    public PlannerNode Parent;
    public float Cost;
    public Action Action;
    public List<Action> AvailableActions;

    public PlannerNode(Dictionary<string, bool> state, PlannerNode parent, float cost, Action action, List<Action> availableActions)
    {
        State = new Dictionary<string, bool>(state);
        Parent = parent;
        Cost = cost;
        Action = action;
        AvailableActions = new List<Action>(availableActions);
    }
}

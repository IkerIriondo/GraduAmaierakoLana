using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public Dictionary<string, bool> GoalState;
    public int Priority;
    public string name;

    public Goal(Dictionary<string, bool> state, int priority, string name)
    {
        GoalState = state;
        Priority = priority;
        this.name = name;
    }
}

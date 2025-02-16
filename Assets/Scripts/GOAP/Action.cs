using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public float cost = 1f;
    public Dictionary<string, bool> Preconditions;
    public Dictionary<string, bool> Effects;
    public GameObject target;
    public string name;

    public Action(GameObject obj, string name, float cost)
    {
        target = obj;
        Preconditions = new Dictionary<string, bool>();
        Effects = new Dictionary<string, bool>();
        this.name = name;
        this.cost = cost;
    }

    public abstract bool IsAchievable();

    public abstract bool IsDone();

    public abstract void ResetAction();

    public abstract bool PerformAction();

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaitAtNoiseLocationAction : Action
{
    private WorldState worldState;
    private NavMeshAgent agent;
    private bool isDone = false;
    private float elapsedTime = 0;
    private float waitTime = 3f;

    public WaitAtNoiseLocationAction(GameObject enemy, WorldState state, NavMeshAgent agent) : base(enemy, "WaitAtNoiseLocation", 0)
    {
        worldState = state;
        this.agent = agent;

        Preconditions.Add(WorldStateKeys.AtNoiseLocation, true);

        Effects.Add(WorldStateKeys.SearchedNoiseLocation, true);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.AtNoiseLocation);
    }

    public override bool IsDone()
    {
        return isDone;
    }

    public override void ResetAction()
    {
        isDone = false;
        elapsedTime = 0;
    }

    public override bool PerformAction()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= waitTime)
        {
            isDone = true;
            return true;
        }
        return false;
    }

}

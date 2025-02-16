using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToPredictedPositionAction : Action
{
    private WorldState worldState;
    private NavMeshAgent agent;
    private bool isDone = false;

    public MoveToPredictedPositionAction(GameObject enemy, WorldState worldState, NavMeshAgent agent) : base(enemy, "MoveToPredictedPosition", 3)
    {
        this.worldState = worldState;
        this.agent = agent;

        Preconditions.Add(WorldStateKeys.PredictedPosition, true);

        Effects.Add(WorldStateKeys.AtPredictedPosition, true);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.PredictedPosition);
    }

    public override bool IsDone()
    {
        return isDone;
    }

    public override void ResetAction()
    {
        isDone = false;
    }

    public override bool PerformAction()
    {
        if (target == null) return false;

        GOAPAgent goapAgent = target.GetComponent<GOAPAgent>();
        if(goapAgent != null )
        {
            if (goapAgent.HasReachedDestination())
            {
                isDone = true;
                return true;
            }
        }
        return false;
    }
}

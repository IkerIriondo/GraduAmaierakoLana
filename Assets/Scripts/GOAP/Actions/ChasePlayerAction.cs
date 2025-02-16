using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChasePlayerAction : Action
{
    private WorldState worldState;
    private NavMeshAgent agent;
    private bool isDone = false;
    private Transform player;

    public ChasePlayerAction(GameObject enemy, WorldState worldState, NavMeshAgent navMeshAgent) : base(enemy, "ChasePlayer", 5)
    {
        this.worldState = worldState;
        this.agent = navMeshAgent;

        Preconditions.Add(WorldStateKeys.PlayerVisible, true);

        Effects.Add(WorldStateKeys.PlayerCaught, true);

    }

    public override bool IsAchievable()
    {
        return worldState.HasState(WorldStateKeys.PlayerVisible);
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
        agent.speed = 7;
        if (target == null) return false;

        if (player == null) player = GameObject.FindWithTag("Player").transform;

        if (player == null) return false;

        agent.SetDestination(player.position);

        if (GOAPTacticalAI.Instance.coordinated)
        {
            GOAPTacticalAI.Instance.LastKnownPlayerPosition = player.position;
        }

        if (Vector3.Distance(agent.transform.position, player.position) < 3f)
        {
            GameManager.Instance.GameLost();
            isDone = true;
        }

        return true;
    }
}

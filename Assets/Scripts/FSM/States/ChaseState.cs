using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "FSM/ChaseState")]
public class ChaseState : State
{
    public float chaseSpeed = 9.0f;

    private Transform player;

    public override void EnterState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} entering Chase State...");
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        player = GameObject.FindWithTag("Player").transform;

        agent.speed = chaseSpeed;
        if (!FSMTacticalAI.Instance.coordinated)
        {
            FSMTacticalAI.Instance.StartCoordination(player.position, enemy);
        }
    }

    public override void UpdateState(EnemyFSM enemy)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        FieldOfView fieldOfView = enemy.GetComponent<FieldOfView>();
        EnemyHearing enemyHearing = enemy.GetComponent<EnemyHearing>();

        if (enemy.IsFacingPlayer(enemy.transform, player.position, 3f))
        {
            GameManager.Instance.GameLost();
            return;
        }

        if (fieldOfView.canSeePlayer)
        {
            enemy.SetLastKnownPosition(player.position);
            Transform playerObj = player.transform.Find("PlayerObj");
            Vector3 worldForward = playerObj.transform.TransformDirection(Vector3.forward);
            enemy.SetLastKnownForward(worldForward);

            agent.SetDestination(player.position);

            FSMTacticalAI.Instance.SetLastKnownPlayerPosition(player.position);
        }
        else if (enemyHearing.hasHeardNoise)
        {
            Debug.Log($"{enemy.name} heard noise. Investigating...");
            enemy.SetLastKnownPosition(enemyHearing.lastHeardPosition);
            agent.SetDestination(enemyHearing.lastHeardPosition);
            enemyHearing.hasHeardNoise = false;
        }
        else
        {
            Debug.Log($"{enemy.name} lost the player. Predicting movement...");
            enemy.SetLastKnownPosition(player.transform.position);
            enemy.SetSearchZone(EnviromentManager.Instance.playerCurrentZone);
            enemy.PredictPlayerMovement();

            enemy.SwitchState(StatesManager.Instance.searchState);
        }

    }

    public override void ExitState(EnemyFSM enemy)
    {
        Debug.Log($"{enemy.name} leaving Chase State...");
    }

}

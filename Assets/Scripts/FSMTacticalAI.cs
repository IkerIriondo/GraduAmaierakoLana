using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMTacticalAI : TacticalAI
{
    private static FSMTacticalAI instance;
    public static FSMTacticalAI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FSMTacticalAI>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FSMTacticalAI");
                    instance = go.AddComponent<FSMTacticalAI>();
                }
            }
            return instance;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ActiveInstance = this;
    }

    protected override void AssignEnemiesForCoordination()
    {
        Zone playerZone = EnviromentManager.Instance.playerCurrentZone;
        foreach (var zone in playerZone.connectedZones)
        {
            if (zone.enemiesInZone.Count > 0)
            {
                coordinatedEnemies.Add(zone.enemiesInZone[0]);
            }
        }
    }

    protected override void ApplySurroundStrategy(MonoBehaviour enemy, Waypoint waypoint)
    {
        EnemyFSM fsmEnemy = enemy as EnemyFSM;
        if (fsmEnemy != null)
        {
            fsmEnemy.SetSurroundPoint(waypoint);
            fsmEnemy.SwitchState(StatesManager.Instance.surroundPlayerState);
        }
    }

}

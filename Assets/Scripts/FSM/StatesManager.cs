using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesManager : MonoBehaviour
{
    public static StatesManager Instance { get; private set; }

    public PatrolState patrolState;
    public ChaseState chaseState;
    public InvestigateState investigateState;
    public ReturnState returnState;
    public SearchState searchState;
    public SurroundPlayerState surroundPlayerState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


}

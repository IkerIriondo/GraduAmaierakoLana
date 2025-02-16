using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : ScriptableObject
{
    // Egoerara sartzerakoan exekutatuko den funtzioa
    public abstract void EnterState(EnemyFSM enemy);

    // Egoera aktibo dagoenean Update bakoitzean deituko den funtzioa
    public abstract void UpdateState(EnemyFSM enemy);

    // Egoeratik ateratzerakoan deituko zaion funtzioa
    public abstract void ExitState(EnemyFSM enemy);
}

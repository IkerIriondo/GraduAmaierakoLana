using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TestAction : Action
{
    Dictionary<string, bool> worldState;

    public TestAction(string name, Dictionary<string, bool> preconditions, Dictionary<string, bool> effects, float cost, Dictionary<string, bool> worldState)
            : base(null, name, cost)
        {
            this.Preconditions = preconditions;
            this.Effects = effects;
            this.worldState = worldState;
        }

        public override bool IsAchievable()
        {
            foreach(var precondition in Preconditions)
            {
                if(!worldState.ContainsKey(precondition.Key) || worldState[precondition.Key] != precondition.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool IsDone()
        {
            return false;
        }

        public override void ResetAction()
        {
            
        }

        public override bool PerformAction()
        {
            return true;
        }
}

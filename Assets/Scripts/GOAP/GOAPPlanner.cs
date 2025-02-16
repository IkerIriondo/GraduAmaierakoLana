using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GOAPPlanner
{
    public Queue<Action> Plan(GameObject agent, List<Action> availableActions, Dictionary<string, bool> worldState, Dictionary<string, bool> goal)
    {
        List<PlannerNode> frontier = new List<PlannerNode>();
        PlannerNode start = new PlannerNode(worldState, null, 0, null, availableActions);
        frontier.Add(start);

        while (frontier.Count > 0)
        {
            PlannerNode current = frontier.OrderBy(n => n.Cost + Heuristic(n.State, goal)).First();
            frontier.Remove(current);

            if (GoalAchieved(current.State, goal))
            {
                return ReconstructPlan(current);
            }

            foreach (var action in current.AvailableActions.ToList())
            {
                if (!PreconditionsMet(action.Preconditions, current.State))
                    continue;

                Dictionary<string, bool> newState = ApplyEffects(current.State, action.Effects);

                List<Action> newAvailableActions = new List<Action>(current.AvailableActions);
                newAvailableActions.Remove(action);

                PlannerNode child = new PlannerNode(newState, current, current.Cost + action.cost, action, newAvailableActions);

                frontier.Add(child);
            }
            
        }
        return null;
    }

    private float Heuristic(Dictionary<string, bool> currentState, Dictionary<string, bool> goal)
    {
        float unsatisfied = 0;
        foreach (var kvp in goal)
        {
            if (!currentState.ContainsKey(kvp.Key) || currentState[kvp.Key] != kvp.Value)
                unsatisfied++;
        }
        return unsatisfied;
    }

    private bool GoalAchieved(Dictionary<string, bool> state, Dictionary<string, bool> goal)
    {
        foreach (var kvp in goal)
        {
            if (!state.ContainsKey(kvp.Key) || state[kvp.Key] != kvp.Value)
                return false;
        }
        return true;
    }

    private bool PreconditionsMet(Dictionary<string, bool> preconditions, Dictionary<string, bool> state)
    {
        foreach (var kvp in preconditions)
        {
            if (!state.ContainsKey(kvp.Key) || state[kvp.Key] != kvp.Value)
                return false;
        }
        return true;
    }

    private Dictionary<string, bool> ApplyEffects(Dictionary<string, bool> state, Dictionary<string, bool> effects)
    {
        Dictionary<string, bool> newState = new Dictionary<string, bool>(state);
        foreach (var kvp in effects)
        {
            newState[kvp.Key] = kvp.Value;
        }
        return newState;
    }

    private Queue<Action> ReconstructPlan(PlannerNode node)
    {
        Stack<Action> stack = new Stack<Action>();
        while (node != null)
        {
            if (node.Action != null)
            {
                stack.Push(node.Action);
            }
            node = node.Parent;
        }

        Queue<Action> plan = new Queue<Action>();
        while (stack.Count > 0)
        {
            plan.Enqueue(stack.Pop());
        }
        return plan;
    }
}

using System.Drawing;

namespace WinterChallenge2024;

internal sealed class Action
{
    internal int OrganismId;

    internal ActionType ActionType;
    internal int OrganId;
    internal Point TargetPosition;
    internal OrganType? OrganType;
    internal OrganDirection? OrganDirection;    

    internal GoalType GoalType;
    internal ProteinType GoalProteinType;
    internal OrganType GoalOrganType;
    internal int TurnsToGoal;

    internal int Score = 0;
    internal string Source = string.Empty;
    internal bool BlockC = false;
    internal bool BlockD = false;

    public override string ToString()
    {
        if (ActionType == ActionType.GROW)
        {
            string action = $"GROW {OrganId} {TargetPosition.X} {TargetPosition.Y} {OrganType.ToString()}";

            if (OrganDirection != null)
            {
                action += $" {OrganDirection.ToString()}";
            }

            return action;
        }
        else if (ActionType == ActionType.SPORE)
        {
            return $"SPORE {OrganId} {TargetPosition.X} {TargetPosition.Y}";
        }
        else if (ActionType == ActionType.WAIT)
        {
            return "WAIT";
        }

        return string.Empty;
    }
}
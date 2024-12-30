using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace WinterChallenge2024;

internal sealed class Action
{
    internal int OrganismId;

    internal ActionType ActionType;
    internal int BaseOrganId;
    internal Point TargetPosition;
    internal OrganType? OrganType;
    internal OrganDirection? OrganDirection;

    internal ActionResult Goal;
    internal List<ActionResult> SideEffects;

    internal int TurnsToGoal;

    public override string ToString()
    {
        if (ActionType == ActionType.GROW)
        {
            string action = $"GROW {BaseOrganId} {TargetPosition.X} {TargetPosition.Y} {OrganType.ToString()}";

            if (OrganDirection != null)
            {
                action += $" {OrganDirection.ToString()}";
            }

            return action;
        }
        else if (ActionType == ActionType.SPORE)
        {
            return $"SPORE {BaseOrganId} {TargetPosition.X} {TargetPosition.Y}";
        }
        else if (ActionType == ActionType.WAIT)
        {
            return "WAIT";
        }

        return string.Empty;
    }
}
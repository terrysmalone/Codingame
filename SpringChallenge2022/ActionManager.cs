using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SpringChallenge2022;

public class ActionManager
{
    private readonly bool _player1;
    List<PossibleAction> _possibleActions = new List<PossibleAction>();

    public ActionManager(bool player1)
    {
        _player1 = player1;
    }

    internal void ClearPossibleActions()
    {
        _possibleActions.Clear();
    }

    internal void AddPossibleAction(int heroId, int priority, ActionType actionType, EntityType entityType, int? targetId, int? targetXPos, int? targetYPos)
    {
        _possibleActions.Add(new PossibleAction(heroId, priority, actionType, entityType, targetId, targetXPos, targetYPos));
    }

    internal string[] GetBestActions()
    {
        var actions = new string[3];

        var playerOffset = _player1 ? 0 : 3;

        Debugger.DisplayPossibleAction(_possibleActions, playerOffset);

        for (var i = 0; i < 3; i++)
        {
            var bestAction = _possibleActions.Where(a => a.HeroId == i + playerOffset)
                                             .OrderByDescending(a => a.Priority)
                                             .FirstOrDefault();

            if (bestAction != null)
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append($"{GetActionType(bestAction.ActionType)} ");

                if (bestAction.ActionType == ActionType.ShieldSpell
                    || bestAction.ActionType == ActionType.ControlSpell)
                {
                    stringBuilder.Append($"{bestAction.TargetId} ");
                }

                if (bestAction.ActionType == ActionType.Move
                    || bestAction.ActionType == ActionType.ControlSpell
                    || bestAction.ActionType == ActionType.WindSpell)
                {
                    stringBuilder.Append($"{bestAction.TargetXPos} {bestAction.TargetYPos}");
                }

                actions[i] = stringBuilder.ToString();
            }
            else
            {
                actions[i] = "WAIT";
            }
        }

        return actions;
    }

    private static string GetActionType(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Move:
                return "MOVE";
            case ActionType.ControlSpell:
                return "SPELL CONTROL";
            case ActionType.ShieldSpell:
                return "SPELL SHIELD";
            case ActionType.WindSpell:
                return "SPELL WIND";
            default:
                return "Incorrect action type";
        }
    }
}

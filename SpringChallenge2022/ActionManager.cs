using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SpringChallenge2022Tests")]

namespace SpringChallenge2022;

public class ActionManager
{
    private readonly bool _player1;
    private readonly List<PossibleAction> _possibleActions = new List<PossibleAction>();

    private int _mana;

    public ActionManager(bool player1)
    {
        _player1 = player1;
    }

    internal string[] GetBestActions()
    {
        var actions = new string[3];

        PerformManaChecks();

        // Don't control the same monster/enemy

        var playerOffset = _player1 ? 0 : 3;

        for (var i = 0; i < 3; i++)
        {
            var bestAction = _possibleActions.Where(a => a.HeroId == i + playerOffset)
                                             .OrderByDescending(a => a.Priority)
                                             .FirstOrDefault();

            actions[i] = GetActionString(bestAction);
        }

        return actions;
    }
    private void PerformManaChecks()
    {
        var manaLeft = _mana;

        var allHeroActions = new List<PossibleAction>[3];

        // Split actions into different heroes
        var playerOffset = _player1 ? 0 : 3;

        Debugger.DisplayPossibleAction(_possibleActions, playerOffset);

        for (var i = 0; i < 3; i++)
        {
            allHeroActions[i] = _possibleActions.Where(a => a.HeroId == i + playerOffset)
                                                .OrderByDescending(a => a.Priority)
                                                .ToList();
        }

        // Work out how many spells we have to get rid of
        var possibleSpellCount = 3;

        if (manaLeft < 10)
        {
            possibleSpellCount = 0;
        }
        else if (manaLeft < 20)
        {
            possibleSpellCount = 1;
        }
        else if (manaLeft < 30)
        {
            possibleSpellCount = 2;
        }

        if (possibleSpellCount != 3)
        {
            var numberOfSpellsAsFirstChoice = 0;

            var currentMax = new int[3];
            var nonSpellMax = new int[3];
            var canRemoveSpell = new bool[3];

            for (var i = 0; i < 3; i++)
            {
                var allPossibleActions = allHeroActions[i];

                var highestPriorityAction = allPossibleActions.First();

                currentMax[i] = highestPriorityAction.Priority;
                canRemoveSpell[i] = highestPriorityAction.ActionType != ActionType.Move;

                if (canRemoveSpell[i])
                {
                    nonSpellMax[i] = allPossibleActions.Where(a => a.ActionType == ActionType.Move)
                                                       .OrderByDescending(a => a.Priority)
                                                       .First()
                                                       .Priority;

                    numberOfSpellsAsFirstChoice++;
                }
            }

            // Remove spells
            Console.Error.WriteLine($"numberOfSpellsAsFirstChoice:{numberOfSpellsAsFirstChoice}");
            Console.Error.WriteLine($"possibleSpellCount:{possibleSpellCount}");

            while (numberOfSpellsAsFirstChoice > possibleSpellCount)
            {
                var highestIndex = 0;
                var highestValue = int.MinValue;

                if (canRemoveSpell[0])
                {
                    var total = nonSpellMax[0] + currentMax[1] + currentMax[2];

                    if (total > highestValue)
                    {
                        highestValue = total;
                        highestIndex = 0;
                    }
                }

                if (canRemoveSpell[1])
                {
                    var total =  currentMax[0] + nonSpellMax[1] + currentMax[2];

                    if (total > highestValue)
                    {
                        highestValue = total;
                        highestIndex = 1;
                    }
                }

                if (canRemoveSpell[2])
                {
                    var total =  currentMax[0] + currentMax[1] + nonSpellMax[2];

                    if (total > highestValue)
                    {
                        highestValue = total;
                        highestIndex = 2;
                    }
                }

                currentMax[highestIndex] = nonSpellMax[highestIndex];
                canRemoveSpell[highestIndex] = false;

                _possibleActions.RemoveAll(a => a.HeroId == highestIndex + playerOffset
                                                             && a.ActionType != ActionType.Move);

                numberOfSpellsAsFirstChoice--;
            }
        }

        Debugger.DisplayPossibleAction(_possibleActions, playerOffset);
    }

    private static string GetActionString(PossibleAction? bestAction)
    {
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

            return stringBuilder.ToString();
        }
        else
        {
            return "WAIT";
        }
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

    internal void ClearPossibleActions()
    {
        _possibleActions.Clear();
    }

    internal void AddPossibleAction(int heroId, int priority, ActionType actionType, EntityType entityType, int? targetId, int? targetXPos, int? targetYPos)
    {
        _possibleActions.Add(new PossibleAction(heroId, priority, actionType, entityType, targetId, targetXPos, targetYPos));
    }

    public void SetMana(int playerMana)
    {
        _mana = playerMana;
    }
}

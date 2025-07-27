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
        string[] actions = new string[3];

        PerformManaChecks();

        int playerOffset = _player1 ? 0 : 3;

        int idOfEntityBeingControlled = -1;





        for (int i = 0; i < 3; i++)
        {
            PossibleAction? bestAction = _possibleActions.Where(a => a.HeroId == i + playerOffset)
                                             .OrderByDescending(a => a.Priority)
                                             .FirstOrDefault();

            if (bestAction != null)
            {
                // This is a very crude attempt at not controlling the same entity twice
                if (idOfEntityBeingControlled != -1)
                {
                    if (bestAction.ActionType == ActionType.ControlSpell && bestAction.TargetId == idOfEntityBeingControlled)
                    {
                        _possibleActions.Remove(bestAction);

                        bestAction = _possibleActions.Where(a => a.HeroId == i + playerOffset)
                                                     .OrderByDescending(a => a.Priority)
                                                     .FirstOrDefault();
                    }
                }
                else
                {
                    if (bestAction.ActionType == ActionType.ControlSpell)
                    {
                        idOfEntityBeingControlled = bestAction.TargetId.Value;
                    }
                }

                actions[i] = GetActionString(bestAction);
            }
            else
            {
                actions[i] = "WAIT";
            }
        }

        return actions;
    }

    private void PerformManaChecks()
    {
        int manaLeft = _mana;

        List<PossibleAction>[] allHeroActions = new List<PossibleAction>[3];

        // Split actions into different heroes
        int playerOffset = _player1 ? 0 : 3;

        Debugger.DisplayPossibleAction(_possibleActions, playerOffset);

        for (int i = 0; i < 3; i++)
        {
            allHeroActions[i] = _possibleActions.Where(a => a.HeroId == i + playerOffset)
                                                .OrderByDescending(a => a.Priority)
                                                .ToList();
        }

        // Work out how many spells we have to get rid of
        int possibleSpellCount = 3;

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
            int numberOfSpellsAsFirstChoice = 0;

            int[] currentMax = new int[3];
            int[] nonSpellMax = new int[3];
            bool[] canRemoveSpell = new bool[3];

            for (int i = 0; i < 3; i++)
            {
                List<PossibleAction> allPossibleActions = allHeroActions[i];

                PossibleAction highestPriorityAction = allPossibleActions.First();

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
                int highestIndex = 0;
                int highestValue = int.MinValue;

                if (canRemoveSpell[0])
                {
                    int total = nonSpellMax[0] + currentMax[1] + currentMax[2];

                    if (total > highestValue)
                    {
                        highestValue = total;
                        highestIndex = 0;
                    }
                }

                if (canRemoveSpell[1])
                {
                    int total =  currentMax[0] + nonSpellMax[1] + currentMax[2];

                    if (total > highestValue)
                    {
                        highestValue = total;
                        highestIndex = 1;
                    }
                }

                if (canRemoveSpell[2])
                {
                    int total =  currentMax[0] + currentMax[1] + nonSpellMax[2];

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
        StringBuilder stringBuilder = new StringBuilder();

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

using NUnit.Framework;
using SpringChallenge2022;

namespace SpringChallenge2022Tests;

public class ActionManagerTests
{
    [Test]
    public void CorrectNumberOfActionsAreReturned()
    {
        ActionManager actionManager = new ActionManager(true);

        actionManager.AddPossibleAction(0, 10, ActionType.Move, EntityType.None, null, null, null);
        actionManager.AddPossibleAction(0, 10, ActionType.Move, EntityType.None, null, null, null);
        actionManager.AddPossibleAction(1, 10, ActionType.Move, EntityType.None, null, null, null);
        actionManager.AddPossibleAction(1, 10, ActionType.Move, EntityType.None, null, null, null);
        actionManager.AddPossibleAction(2, 10, ActionType.Move, EntityType.None, null, null, null);
        actionManager.AddPossibleAction(2, 10, ActionType.Move, EntityType.None, null, null, null);
        actionManager.AddPossibleAction(2, 10, ActionType.Move, EntityType.None, null, null, null);

        Assert.That(actionManager.GetBestActions().Length, Is.EqualTo(3));
    }

    [Test]
    public void BestActionIsChosen()
    {
    }

    [Test]
    public void PerformManaCheck_NothingIsRemovedIfThereAreNoSpells()
    {






    }

    [Test]
    public void PlayerTwoOffsetsHeroes()
    {

    }
}

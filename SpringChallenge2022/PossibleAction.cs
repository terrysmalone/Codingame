namespace SpringChallenge2022;

internal sealed class PossibleAction
{
    internal int HeroId { get; }
    internal int Priority { get; }
    internal ActionType ActionType { get; }
    internal EntityType EntityType { get; }
    internal int? TargetId { get; }
    internal int? TargetXPos { get; }
    internal int? TargetYPos { get; }

    internal PossibleAction(int heroId, int priority, ActionType actionType, EntityType entityType, int? targetId, int? targetXPos, int? targetYPos)
    {
        HeroId = heroId;
        Priority = priority;
        ActionType = actionType;
        EntityType = entityType;
        TargetId = targetId;
        TargetXPos = targetXPos;
        TargetYPos = targetYPos;
    }
}

namespace SpringChallenge2022;

internal sealed class PossibleAction
{
    internal ActionType ActionType { get; }
    internal EntityType EntityType { get; }
    internal int? TargetId { get; }
    internal int? TargetXPos { get; }
    internal int? TargetYPos { get; }

    internal PossibleAction(ActionType actionType, EntityType entityType, int? targetId, int? targetXPos, int? targetYPos)
    {
        ActionType = actionType;
        EntityType = entityType;
        TargetId = targetId;
        TargetXPos = targetXPos;
        TargetYPos = targetYPos;
    }
}

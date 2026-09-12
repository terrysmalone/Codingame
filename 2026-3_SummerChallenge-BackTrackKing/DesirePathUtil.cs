namespace BackTrackKing;

internal static class DesirePathUtil
{
    // Check if completing a path is worthwhile. If the opponent already owns most of it, there's no point
    // pursuing it
    internal static bool IsCompletionWorthwhile(DesirePath desirePath)
    {
        // Use: NetAdvantageAfterCompletion = (MyExistingCellsInPath + desirePath.RemainingPathCount) - OpponentExistingCellsInPath

        int ntAdvantage = (desirePath.MyTracksOnPathCount + desirePath.RemainingPathCount) - desirePath.OpponentTracksOnPathCount;

        if (ntAdvantage < 1)
        {
            return false;
        }

        return true;
    }
}

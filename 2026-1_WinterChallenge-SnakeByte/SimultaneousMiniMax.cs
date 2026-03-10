namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class SimultaneousMiniMax
{
    private int _maxDepth = 5;

    internal MoveSet FindBestMoveSet(GameState state)
    {
        List<MoveSet> myMoveSets = state.GetMyMoveSets();
        // Logger.MoveSets("Possible move sets for me:", myMoveSets);
        Console.Error.WriteLine($"Found {myMoveSets.Count} move sets for me.");

        List<MoveSet> opponentMoveSets = state.GetOpponentMoveSets();
        Console.Error.WriteLine($"Found {opponentMoveSets.Count} move sets for opponent.");

        int bestScore = int.MinValue;
        MoveSet bestMoveSet = myMoveSets[0];

        foreach (var myMoveSet in myMoveSets)
        {
            int worstOutcome = int.MaxValue;

            foreach (var opponentMoveSet in opponentMoveSets)
            {
                GameState nextState = state.Simulate(myMoveSet, opponentMoveSet);

                int score = Search(nextState, 1);

                if (score < worstOutcome)
                    worstOutcome = score;
            }

            if (worstOutcome > bestScore)
            {
                bestScore = worstOutcome;
                bestMoveSet = myMoveSet;
            }
        }

        return bestMoveSet;
    }

    private int Search(GameState state, int depth)
    {
        if (depth >= _maxDepth || state.IsTerminal())
            return state.Evaluate();

        List<MoveSet> myMoveSets = state.GetMyMoveSets();
        List<MoveSet> enemyMoveSets = state.GetOpponentMoveSets();

        int bestScore = int.MinValue;

        foreach (var myMoveSet in myMoveSets)
        {
            int worstOutcome = int.MaxValue;

            foreach (var enemyMoveSet in enemyMoveSets)
            {
                GameState nextState = state.Simulate(myMoveSet, enemyMoveSet);

                int score = Search(nextState, depth + 1);

                if (score < worstOutcome)
                {
                    worstOutcome = score;
                }
            }

            if (worstOutcome > bestScore)
            {
                bestScore = worstOutcome;
            }
        }

        return bestScore;
    }


}


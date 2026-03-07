using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using static System.Formats.Asn1.AsnWriter;

namespace _2023_2_FallChallenge_SeabedSecurity;

internal class ScoreCalculator
{
    private Game _game;

    public ScoreCalculator(Game game)
    {
        _game = game;
    }

    // Calculates what my score would be if I scanned both drones now, then 
    // calculates the total score the enemy could possibly get if they later scanned everything possible.
    // TODO: At the moment this doesn't care if creatures are unavailable to be scanned
    internal (int mine, int possibleEnemyScore) CalculateScanScores()
    {
        int score = 0;

        Console.Error.WriteLine($"My current score:{_game.MyScore}");

        HashSet<int> scannedCreatureIDs = _game.GetMyScannedCreatureIds();

        HashSet<int> myStoredIds = _game.MyStoredCreatureIds;

        HashSet<int> enemyStoredIds = _game.EnemyStoredCreatureIds;

        HashSet<int> cloneMyStoredIds = new HashSet<int>(myStoredIds);

        // Go through all possible scorings. If it's in the clone and not in the stored add it to the score. If it's not in the enemies stored double it.
        foreach (int scannedId in scannedCreatureIDs)
        {
            // Add points for scans

            Creature creature = _game.GetCreature(scannedId);

            int typeScore = 0;
            switch (creature.Type)
            {
                case 0:
                    typeScore = 1;
                    break;
                case 1:
                    typeScore = 2;
                    break;
                case 2:
                    typeScore += 3;
                    break;
            }

            // Double points if no one has scanned that type yet
            if (!ContainsType(myStoredIds, creature.Type, creature.Color))
            {
                score += typeScore;

                if (!ContainsType(enemyStoredIds, creature.Type, creature.Color))
                {

                    score += typeScore;
                }
            }

            cloneMyStoredIds.Add(scannedId);
        }

        // First to scan all of each colour 0, 1, 2, 3
        score += CalculateColourScore(0, cloneMyStoredIds, myStoredIds, enemyStoredIds);
        score += CalculateColourScore(1, cloneMyStoredIds, myStoredIds, enemyStoredIds);
        score += CalculateColourScore(2, cloneMyStoredIds, myStoredIds, enemyStoredIds);
        score += CalculateColourScore(3, cloneMyStoredIds, myStoredIds, enemyStoredIds);

        // First to scan all of each type 0, 1, 2
        score += CalculateTypeScore(0, cloneMyStoredIds, myStoredIds, enemyStoredIds);
        score += CalculateTypeScore(1, cloneMyStoredIds, myStoredIds, enemyStoredIds);
        score += CalculateTypeScore(2, cloneMyStoredIds, myStoredIds, enemyStoredIds);

        var enemyScore = 0;
        HashSet<int> cloneEnemyStoredIds = new HashSet<int>(myStoredIds);
        // Create a list of all creatures that are not stored or in clone
        foreach (Creature creature in _game.GetAllCreatures())
        {
            // Add points for the enemy scanning this creature
            int typeScore = 0;
            switch (creature.Type)
            {
                case 0:
                    typeScore = 1;
                    break;
                case 1:
                    typeScore = 2;
                    break;
                case 2:
                    typeScore += 3;
                    break;
            }
            // Double points if no one has scanned that type yet
            if (!ContainsType(enemyStoredIds, creature.Type, creature.Color))
            {
                enemyScore += typeScore;

                if (!ContainsType(cloneMyStoredIds, creature.Type, creature.Color))
                {
                    enemyScore += typeScore;
                }
            }


            cloneEnemyStoredIds.Add(creature.Id);
        }

        // First to scan all of each colour 0, 1, 2, 3
        enemyScore += CalculateColourScore(0, cloneEnemyStoredIds, enemyStoredIds, cloneMyStoredIds);
        enemyScore += CalculateColourScore(1, cloneEnemyStoredIds, enemyStoredIds, cloneMyStoredIds);
        enemyScore += CalculateColourScore(2, cloneEnemyStoredIds, enemyStoredIds, cloneMyStoredIds);
        enemyScore += CalculateColourScore(3, cloneEnemyStoredIds, enemyStoredIds, cloneMyStoredIds);

        // First to scan all of each type 0, 1, 2
        enemyScore += CalculateTypeScore(0, cloneEnemyStoredIds, enemyStoredIds, cloneMyStoredIds);
        enemyScore += CalculateTypeScore(1, cloneEnemyStoredIds, enemyStoredIds, cloneMyStoredIds);
        enemyScore += CalculateTypeScore(2, cloneEnemyStoredIds, enemyStoredIds, cloneMyStoredIds);

        return (_game.MyScore + score, _game.EnemyScore + enemyScore);
    }

    private int CalculateColourScore(int colour, HashSet<int> cloneMyStoredIds, HashSet<int> myStoredIds, HashSet<int> enemyStoredIds)
    {
        var score = 0;

        if (ContainsAllOfColour(cloneMyStoredIds, colour) && !ContainsAllOfColour(myStoredIds, colour))
        {
            score += 3;

            if (!ContainsAllOfColour(enemyStoredIds, colour))
            {
                score += 3;
            }
        }

        return score;
    }

    private bool ContainsAllOfColour(HashSet<int> ids, int colour)
    {
        int colourCount = 0;

        foreach (int id in ids)
        {
            Creature creature = _game.GetCreature(id);
            if (creature.Color == colour)
            {
                colourCount++;
            }
        }

        return colourCount == 3;
    }

    private int CalculateTypeScore(int type, HashSet<int> cloneMyStoredIds, HashSet<int> myStoredIds, HashSet<int> enemyStoredIds)
    {
        var score = 0;

        if (ContainsAllOfType(cloneMyStoredIds, type) && !ContainsAllOfType(myStoredIds, type))
        {
            score += 4;

            if (!ContainsAllOfType(enemyStoredIds, type))
            {
                score += 4;
            }
        }

        return score;
    }

    private bool ContainsAllOfType(HashSet<int> ids, int type)
    {
        int typeCount = 0;

        foreach (int id in ids)
        {
            Creature creature = _game.GetCreature(id);
            if (creature.Type == type)
            {
                typeCount++;
            }
        }

        return typeCount == 4;
    }

    internal object CalculateEnemyScanScore()
    {
        return 0;
    }

    private bool ContainsType(HashSet<int> storedIds, int type, int color)
    {
        foreach (int storedId in storedIds)
        {
            Creature creature = _game.GetCreature(storedId);
            if (creature.Type == type && creature.Color == color)
            {
                return true;
            }
        }

        return false;
    }
}




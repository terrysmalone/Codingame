using System;
using System.Collections.Generic;

namespace BackTrackKing;

public class Game
{
    private int myId;

    private Map map;

    private List<Town> towns;

    private int myScore;
    private int opponentScore;

    public Game(int myId)
    {
        this.myId = myId;

        towns = new List<Town>();
    }

    internal void SetMap(Map map)
    {
        this.map = map;
    }

    internal void SetMyScore(int myScore)
    {
        this.myScore = myScore;
    }

    internal void SetOpponentScore(int foeScore)
    {
        this.opponentScore = foeScore;
    }

    internal void SetTowns(List<Town> towns)
    {
        this.towns = towns;
    }

    internal string CalculateActions()
    {
        Logger.TypeMap(map);

        return "WAIT";
    }
}

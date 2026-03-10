using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class Game
{
    private int _width;
    private int _height;

    private Level _level;

    internal List<SnakeBot> MySnakeBots { get; set; }
    internal List<SnakeBot> OpponentSnakeBots { get; set; }

    public Game(int width, int height, bool[,] platforms)
    {
        _width = width;
        _height = height;

        _level = new Level(width, height, platforms);

        MySnakeBots = new List<SnakeBot>();
        OpponentSnakeBots = new List<SnakeBot>();
    }

    internal void MarkAllSnakesForRemoval()
    {
       foreach (var snakeBot in MySnakeBots)
        {
            snakeBot.Remove = true;
        }

        foreach (var snakeBot in OpponentSnakeBots)
        {
            snakeBot.Remove = true;
        }
    }

    internal void RemoveMarkedSnakes()
    {

        // Iterate through the snakes backwards to safely remove any where Remove == true
        for (int i = MySnakeBots.Count - 1; i >= 0; --i)
        {
            if (MySnakeBots[i].Remove)
            {
                MySnakeBots.RemoveAt(i);
            }
        }

        for (int i = OpponentSnakeBots.Count - 1; i >= 0; --i)
        {
            if (OpponentSnakeBots[i].Remove)
            {
                OpponentSnakeBots.RemoveAt(i);
            }
        }
    }

    internal void AddMySnake(SnakeBot snakeBot)
    {
        MySnakeBots.Add(snakeBot);
    }

    internal void AddOpponentSnake(SnakeBot snakeBot)
    {
        OpponentSnakeBots.Add(snakeBot);
    }

    internal SnakeBot GetSnake(int snakebotId)
    {
        return MySnakeBots.FirstOrDefault(s => s.Id == snakebotId) ?? OpponentSnakeBots.FirstOrDefault(s => s.Id == snakebotId);
    }

    internal string[] GetActions()
    {
        Logger.EntireGame(_level.Platforms, MySnakeBots, OpponentSnakeBots, _level.PowerSources);

        return new string[] { "WAIT" };
    }

    internal void RemoveAllPowerSources()
    {
        _level.PowerSources.Clear();
    }

    internal void AddPowerSource(int x, int y)
    {
        _level.PowerSources.Add(new Point(x, y));
    }
}

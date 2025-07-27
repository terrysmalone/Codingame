namespace Fall2020Challenge;

internal sealed class Inventory
{
    public int[] Ingredients { get; }
    internal int Score { get; }
    
    internal Inventory(int[] ingredients,
        int score)
    {
        Ingredients = ingredients;
        Score = score;
    }
}
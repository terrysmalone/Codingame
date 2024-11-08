namespace Fall2020Challenge;

internal struct Spell
{
    internal const string ActionType = "CAST";
    
    internal int Id { get; }
    public int[] IngredientsChange { get; }
    internal bool Castable { get; set; }
    internal bool Repeatable { get; set; }

    internal Spell(int id,
        int[] ingredientsChange,
        bool castable,
        bool repeatable)
    {
        Id = id;
        IngredientsChange = ingredientsChange;
        Castable = castable;
        Repeatable = repeatable;

    }
}
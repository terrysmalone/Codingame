namespace Fall2020Challenge;

internal struct Spell
{
    internal const string ActionType = "CAST";
    
    internal int Id { get; }
    public int[] IngredientsChange { get; }
    internal bool Castable { get; set; }

    internal Spell(int id,
        int[] ingredientsChange,
        bool castable)
    {
        Id = id;
        IngredientsChange = ingredientsChange;
        Castable = castable;

    }
}
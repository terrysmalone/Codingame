namespace Fall2020Challenge
{
    internal sealed class Spell
    {
        internal int Id { get; }
        public int[] IngredientsChange { get; }
        internal bool Castable { get; }

        internal Spell(int id, int[] ingredientsChange, bool castable)
        {
            Id = id;
            IngredientsChange = ingredientsChange;
            Castable = castable;

        }
    }
}

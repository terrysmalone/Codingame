namespace Fall2020Challenge
{
    internal sealed class Recipe
    {
        public const string ActionType = "BREW";
        
        internal int Id { get; }
        public int[] Ingredients { get; }
        internal int Price { get; }
        
        internal Recipe(int id,
            int[] ingredients,
            int price)
        {
            Id = id;
            Ingredients = ingredients;
            Price = price;
        }

        
    }
}
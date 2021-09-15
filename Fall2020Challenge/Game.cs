using System;
using System.Collections.Generic;
using System.Linq;

namespace Fall2020Challenge
{
    internal sealed class Game
    {   
        internal List<Recipe> Recipes { get; private set; }
        internal List<Spell> Spells { get; private set; }
        internal Inventory PlayerInventory { get; private set; }
        internal Inventory OpponentInventory { get; private set; }

        private Recipe _targetRecipe = null;
        private Queue<string> _listOfActions = new Queue<string>();
        
        public Game()
        {
            Recipes = new List<Recipe>();
        }
        
        public void SetRecipes(List<Recipe> recipes)
        {
            Recipes = recipes;
        }
        
        public void SetSpells(List<Spell> spells)
        {
            Spells = spells;
        }
        
        public void SetPlayerInventory(Inventory inventoryItems)
        {
            PlayerInventory = inventoryItems;
        }
        
        public void SetOpponentInventory(Inventory inventoryItems)
        {
            OpponentInventory = inventoryItems;
        }
        public string GetAction()
        {
            if(_targetRecipe == null || !Recipes.Exists(r => r.Id == _targetRecipe.Id))
            {
                // Pick the most expensive recipe
                _targetRecipe = Recipes.OrderByDescending(s => s.Price).First();
                _listOfActions = new Queue<string>();

                if (CanRecipeBeMade(_targetRecipe))
                {
                    return $"BREW {_targetRecipe.Id}";
                }
            }

            Console.Error.Write("Recipe ingredients");
            DisplayIngredients(_targetRecipe.Ingredients);

            Console.Error.Write("Current ingredients");
            DisplayIngredients(PlayerInventory.Ingredients);

            DisplaySpellIngredients();

            if(!_listOfActions.Any())
            {
                // Make list of actions
                _listOfActions = MakeActionsList(_targetRecipe.Ingredients);

                return _listOfActions.Dequeue();
            }
            else
            {
                return _listOfActions.Dequeue();
            }
        }

        private Queue<string> MakeActionsList(int[] targetRecipeIngredients)
        {
            var actions = new Queue<string>();

            var rootNode = new TreeNode(null, Spells.ConvertAll(s => new Spell(s.Id, s.IngredientsChange, s.Castable)).ToList(), PlayerInventory.Ingredients, string.Empty, null);

            var recipeMade = false;

            while (!recipeMade)
            {

                //if(PlayerInventory)
            //}

            actions.Enqueue("CAST 79");

            actions.Enqueue("REST");
            actions.Enqueue("CAST 79");

            return actions;
        }

        private bool CanSpellBeCast(int[] spellIngredientsChange)
        {
            var inventoryIngredients = PlayerInventory.Ingredients;
            
            return    inventoryIngredients[0] + spellIngredientsChange[0] >= 0
                      && inventoryIngredients[1] + spellIngredientsChange[1] >= 0
                      && inventoryIngredients[2] + spellIngredientsChange[2] >= 0
                      && inventoryIngredients[3] + spellIngredientsChange[3] >= 0;

        }

        private bool CanRecipeBeMade(Recipe recipe)
        {
            for (var i = 0; i < 4; i++)
            {
                if(PlayerInventory.Ingredients[i] < recipe.Ingredients[i])
                {
                    return false;
                }
            }

            return true;
        }

        private int[] CalculateNeededIngredients(int[] targetIngredients)
        {
            return CalculateNeededIngredientsAfterChange(targetIngredients, new int[] { 0, 0, 0, 0 });
        }
        
        private int[] CalculateNeededIngredientsAfterChange(IReadOnlyList<int> targetIngredients, IReadOnlyList<int> ingredientsChange)
        {
            var blueDiff = targetIngredients[0] - (PlayerInventory.Ingredients[0] + ingredientsChange[0]);
            var greenDiff = targetIngredients[1] - (PlayerInventory.Ingredients[1] + ingredientsChange[1]);
            var orangeDiff = targetIngredients[2] - (PlayerInventory.Ingredients[2] + ingredientsChange[2]);
            var yellowDiff = targetIngredients[3] - (PlayerInventory.Ingredients[3] + ingredientsChange[3]);
            
            var blueNeeds = blueDiff >= 0 ? blueDiff : 0;
            var greenNeeds = greenDiff >= 0 ? greenDiff : 0;
            var orangeNeeds = orangeDiff >= 0 ? orangeDiff : 0;
            var yellowNeeds = yellowDiff >= 0 ? yellowDiff : 0;

            return new []
            {
                blueNeeds,
                greenNeeds,
                orangeNeeds,
                yellowNeeds
            };
        }

        private int[] CalculateSpareIngredients(int[] neededIngredients)
        {
            var spareIngredients = new int[4];

            for (var i = 0; i < 4; i++)
            {
                spareIngredients[i] = 0;

                if(PlayerInventory.Ingredients[i] > neededIngredients[i])
                {
                    spareIngredients[i] = PlayerInventory.Ingredients[i] - neededIngredients[i];
                }
            }

            return spareIngredients;
        }

        private void DisplayRecipes()
        {
            Console.Error.WriteLine("Recipes");
            
            foreach (var recipe in Recipes)
            {
                Console.Error.WriteLine("actionId: " + recipe.Id);
                Console.Error.WriteLine("blueIngredients:   " + recipe.Ingredients[0]);   
                Console.Error.WriteLine("greenIngredients:  " + recipe.Ingredients[1]);   
                Console.Error.WriteLine("orangeIngredients: " + recipe.Ingredients[2]);   
                Console.Error.WriteLine("yellowIngredients: " + recipe.Ingredients[3]);   
                Console.Error.WriteLine("Price: " + recipe.Price);  
                Console.Error.WriteLine();                    
            }   
        }

        private static void DisplayIngredients(int[] ingredients)
        {
            Console.Error.WriteLine($"[{ingredients[0]},{ingredients[1]},{ingredients[2]},{ingredients[3]}]");
        }

        private void DisplaySpells()
        {
            Console.Error.WriteLine("Spells");
            
            foreach (var spell in Spells)
            {  
                Console.Error.WriteLine("actionId: " + spell.Id);
                Console.Error.WriteLine("Blue Ingredients change:   " + spell.IngredientsChange[0]);   
                Console.Error.WriteLine("Green Ingredients change:  " + spell.IngredientsChange[1]);   
                Console.Error.WriteLine("Orange Ingredients change: " + spell.IngredientsChange[2]);   
                Console.Error.WriteLine("Yellow Ingredients change: " + spell.IngredientsChange[3]);
                Console.Error.WriteLine();        
            }   
        }

        private void DisplaySpellIngredients()
        {
            Console.Error.WriteLine("SpellIngredients");

            foreach (var spell in Spells)
            {
                Console.Error.WriteLine($"[{spell.IngredientsChange[0]},{spell.IngredientsChange[1]},{spell.IngredientsChange[2]},{spell.IngredientsChange[3]}]");
            }
        }
    }

    internal class TreeNode
    {
        private readonly int[] _change;
        public TreeNode Parent { get; }
        public List<Spell> CurrentSpells { get; }
        public int[] PlayerIngredients { get; }

        public string Action { get; }

        private List<TreeNode> _children;

        public TreeNode(TreeNode parent, List<Spell> currentSpells, int[] playerIngredients, string action, int[] change)
        {
            _change = change;
            Parent = parent;
            CurrentSpells = currentSpells;
            PlayerIngredients = playerIngredients;
            Action = action;

            _children = new List<TreeNode>();
        }

        internal void AddChild(TreeNode child)
        {
            _children.Add(child);
        }
    }
}

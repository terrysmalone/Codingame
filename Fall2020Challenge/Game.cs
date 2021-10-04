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
        private List<string> _listOfActions = new List<string>();

        private const int _maxSearchDepth = 15;

        private int _nodesChecked = 0;
        
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
            //Console.Error.WriteLine("Recipes");

            // foreach (var recipe in Recipes)
            // {
            //     Console.Error.WriteLine($"ID:{recipe.Id}");
            // }

            if(_targetRecipe == null || !Recipes.Exists(r => r.Id == _targetRecipe.Id))
            {

                // Pick the most expensive recipe
                _targetRecipe = Recipes.OrderBy(s => s.Price).First();
                _listOfActions = new List<string>();

                if (CanRecipeBeMade(_targetRecipe))
                {
                    return $"BREW {_targetRecipe.Id}";
                }
            }

            //Console.Error.Write("Recipe ingredients");
            //DisplayIngredients(_targetRecipe.Ingredients);

            //Console.Error.Write("Current ingredients");
            //DisplayIngredients(PlayerInventory.Ingredients);

            //DisplaySpellIngredients();

            Console.Error.WriteLine($"Actions count: {_listOfActions.Count}");
            if(!_listOfActions.Any() || _listOfActions.Count == 0)
            {

                // Make list of actions
                _nodesChecked = 0;
                _listOfActions = MakeActionsList(_targetRecipe.Ingredients);
                Console.Error.WriteLine($"Nodes checked:{_nodesChecked}");

                _listOfActions.Add($"BREW {_targetRecipe.Id}");

                var action = _listOfActions[0];
                _listOfActions.RemoveAt(0);
                return action;
            }
            else
            {
                var action = _listOfActions[0];
                _listOfActions.RemoveAt(0);
                return action;
            }
        }

        private List<string> MakeActionsList(int[] targetRecipeIngredients)
        {
            //var rootNode = new TreeNode(null, Spells.ConvertAll(s => new Spell(s.Id, s.IngredientsChange, s.Castable)).ToList(), PlayerInventory.Ingredients, string.Empty, null);

            var availableSpells = Spells.ConvertAll(s => new Spell(s.Id, s.IngredientsChange, s.Castable)).ToList();
            var playerIngredients = GetDeepCopy(PlayerInventory.Ingredients);

            Console.Error.WriteLine("Got spells");
            DisplayIngredients(playerIngredients);

            var head = new TreeNode<GameState>(new GameState(availableSpells, playerIngredients, new List<string>()));

            Console.Error.WriteLine("Building tree");
            // Build the tree
            AddChildren(head, 0);
            Console.Error.WriteLine("Tree built");

            //DisplayTreeNode(head);

            // Search the tree
            var actionsList = GetActionsList(head, targetRecipeIngredients, 0);

            return actionsList;
        }

        private static int[] GetDeepCopy(IReadOnlyList<int> playerInventoryIngredients)
        {
            var copy = new int[playerInventoryIngredients.Count];

            for (var i = 0; i < playerInventoryIngredients.Count; i++)
            {
                copy[i] = playerInventoryIngredients[i];
            }

            return copy;
        }

        private static void AddChildren(TreeNode<GameState> currentNode, int currentDepth)
        {
            if(currentDepth >= _maxSearchDepth)
            {
                return;
            }

            var children = GetChildrenStates(currentNode);

            foreach (var child in children)
            {
                var childNode = currentNode.AddChild(child);

                AddChildren(childNode, currentDepth+1);
            }
        }

        private static List<GameState> GetChildrenStates(TreeNode<GameState> currentNode)
        {
            var children = new List<GameState>();

            var currentIngredients = currentNode.Value.PlayerIngredients;
            var currentSpells = currentNode.Value.AvailableSpells;
            var currentActions = currentNode.Value.Actions;

            // Can I make each spell
            foreach (var currentSpell in currentSpells)
            {
                // If the spell is castable
                if (currentSpell.Castable)
                {
                    var combinedIngredients = CombineIngredients(currentIngredients, currentSpell.IngredientsChange);

                    // if I have the ingredients to make the spell and making it doesn't put me above 10
                    if(    combinedIngredients.All(ingredient => ingredient >= 0)
                       &&  combinedIngredients.Sum() <= 10)
                    {
                        var spells = currentSpells.ConvertAll(s => new Spell(s.Id, s.IngredientsChange, false)).ToList();

                        var actions = new List<string>(currentActions);
                        actions.Add($"CAST {currentSpell.Id}");

                        children.Add(new GameState(spells, combinedIngredients, actions));
                    }
                }
            }

            // Can I rest
            if(currentSpells.Any(s => s.Castable == false))
            {
                var spells = currentSpells.ConvertAll(s => new Spell(s.Id, s.IngredientsChange, true)).ToList();
                var actions = new List<string>(currentActions);
                actions.Add("REST");

                children.Add(new GameState(spells, GetDeepCopy(currentIngredients), actions));
            }

            return children;
        }

        private static int[] CombineIngredients(IReadOnlyList<int> ingredients, IReadOnlyList<int> addedIngredients)
        {
            var combined = new int[4];

            combined[0] = ingredients[0] + addedIngredients[0];
            combined[1] = ingredients[1] + addedIngredients[1];
            combined[2] = ingredients[2] + addedIngredients[2];
            combined[3] = ingredients[3] + addedIngredients[3];

            return combined;
        }

        private List<string> GetActionsList(TreeNode<GameState> head, int[] targetRecipeIngredients, int depth)
        {
            var actionsList = new List<string>();

            foreach (var child in head.Children)
            {
                // Add action
                actionsList.Add(child.Value.Actions[^1]);

                // check for win
                if(CheckNode(child, targetRecipeIngredients, depth+1, actionsList))
                {
                    return actionsList;
                }

                //remove action
                actionsList.RemoveAt(actionsList.Count-1);
            }

            return actionsList;
        }
        private bool CheckNode(TreeNode<GameState> node, int[] targetRecipeIngredients, int depth, List<string> actionsList)
        {
            // if node is a winner return
            if(CanRecipeBeMadeWithIngredients(targetRecipeIngredients, node.Value.PlayerIngredients))
            {
                // Console.Error.WriteLine("---------------------");
                // foreach (var actn in actionsList)
                // {
                //     Console.Error.WriteLine(actn);
                // }

                return true;
            }

            // get children

            foreach (var child in node.Children)
            {
                // Add action
                actionsList.Add(child.Value.Actions[^1]);

                // check for win
                if(CheckNode(child, targetRecipeIngredients, depth+1, actionsList))
                {
                    return true;
                }

                //remove action
                actionsList.RemoveAt(actionsList.Count-1);
            }

            return false;
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

        private bool CanRecipeBeMadeWithIngredients(int[] targetIngredients, int[] currentIngredients)
        {
            _nodesChecked++;
            for (var i = 0; i < 4; i++)
            {
                if(targetIngredients[i] > currentIngredients[i])
                {
                    return false;
                }
            }

            Console.Error.WriteLine("---------------------");

            Console.Error.WriteLine("Target ingredients: " + string.Join(",", targetIngredients));
            Console.Error.WriteLine("Current ingredients: " + string.Join(",", currentIngredients));

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

        private void DisplayTreeNode(TreeNode<GameState> head, string indent = "-")
        {
            if(head.Value.Actions.Any())
            {
                Console.Error.WriteLine(indent + head.Value.Actions[^1]);
            }
            else
            {
                Console.Error.WriteLine(indent + "ROOT");
            }

            foreach (var child in head.Children)
            {
                DisplayTreeNode(child, $"-{indent}");
            }
        }
    }
}

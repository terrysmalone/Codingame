/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
  This hasn't been put in a namespace to allow for class 
  name duplicates.
***************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

    internal sealed class Game
    {   
        internal List<Recipe> Recipes { get; private set; }
        internal List<Spell> Spells { get; private set; }
        internal Inventory PlayerInventory { get; private set; }
        internal Inventory OpponentInventory { get; private set; }

        private Recipe _targetRecipe = null;
        private List<string> _listOfActions = new List<string>();

        private const int _maxSearchDepth = 20;
        
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
                _targetRecipe = Recipes.OrderBy(s => s.Price).First();
                _listOfActions = new List<string>();

                if (CanRecipeBeMade(_targetRecipe))
                {
                    return $"BREW {_targetRecipe.Id}";
                }

                Console.Error.WriteLine($"Target:{_targetRecipe.Id}");
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
                _listOfActions = MakeActionsList(_targetRecipe.Ingredients);

                _listOfActions.Add($"BREW {_targetRecipe.Id}");

                Console.Error.WriteLine("---------------------");
                foreach (var actn in _listOfActions)
                {
                    Console.Error.WriteLine(actn);
                }

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

            var head = new TreeNode<GameState>(new GameState(availableSpells, playerIngredients, new List<string>()));

            // Build the tree
            AddChildren(head, 0);

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
                Console.Error.WriteLine("---------------------");
                foreach (var actn in actionsList)
                {
                    Console.Error.WriteLine(actn);
                }

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

    internal class GameState
    {
        public List<Spell> AvailableSpells { get; }
        public int[] PlayerIngredients { get; }
        public List<string> Actions { get; }

        public GameState(List<Spell> availableSpells, int[] playerIngredients, List<string> actions)
        {
            AvailableSpells = availableSpells;
            PlayerIngredients = playerIngredients;
            Actions = actions;
        }
    }

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

    internal sealed class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();
            
            // game loop
            while (true)
            {
                ParseActions(game);

                game.SetPlayerInventory(GetInventoryItems());
                game.SetOpponentInventory(GetInventoryItems());
                
                //DisplayInventory(game.PlayerInventory, true);
                //DisplayInventory(game.OpponentInventory, false);

                var action = game.GetAction();
                Console.WriteLine(action);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // in the first league: BREW <id> | WAIT; later: BREW <id> | CAST <id> [<times>] | LEARN <id> | REST | WAIT
                //Console.WriteLine(actionType + " " + recipeId);
            }
        }

        private static void ParseActions(Game game)
        {        
            var actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play
            
            var recipes = new List<Recipe>();
            var spells = new List<Spell>();
            

            for (var i = 0; i < actionCount; i++)
            {               
                var input = Console.ReadLine();
                var inputs = input.Split(' ');
                
                if(inputs[1] == "BREW")
                {
                    recipes.Add(new Recipe(int.Parse(inputs[0]),
                                           new int[] { Math.Abs(int.Parse(inputs[2])),
                                                       Math.Abs(int.Parse(inputs[3])),
                                                       Math.Abs(int.Parse(inputs[4])),
                                                       Math.Abs(int.Parse(inputs[5]))},
                                           int.Parse(inputs[6])));   
                }
                else if(inputs[1] == "CAST")
                {
                    spells.Add(new Spell(int.Parse(inputs[0]),
                                         new int[] { int.Parse(inputs[2]),
                                                     int.Parse(inputs[3]),
                                                     int.Parse(inputs[4]),
                                                     int.Parse(inputs[5])},
                                         int.Parse(inputs[9]) == 1));   
                }       
            }
            
            game.SetRecipes(recipes);
            game.SetSpells(spells);
        }
        
        private static Inventory GetInventoryItems()
        {
            var input = Console.ReadLine();
            var inputs = input.Split(' ');
            
            var inventory = new Inventory(new int[] {Math.Abs(int.Parse(inputs[0])),
                                          Math.Abs(int.Parse(inputs[1])),
                                          Math.Abs(int.Parse(inputs[2])),
                                          Math.Abs(int.Parse(inputs[3]))},
                                          int.Parse(inputs[4]));

            return inventory;
        }

        private static void DisplayInventory (Inventory inventory, bool mine)
        {
            Console.Error.WriteLine(mine ? "My inventory" : "Opponent inventory");
            Console.Error.WriteLine("-----------");
            
            Console.Error.WriteLine("blueIngredients: " + inventory.Ingredients[0]);   
            Console.Error.WriteLine("greenIngredients: " + inventory.Ingredients[1]);   
            Console.Error.WriteLine("orangeIngredients: " + inventory.Ingredients[2]);   
            Console.Error.WriteLine("yellowIngredients: " + inventory.Ingredients[3]); 
        }
    }
    
    // for (int i = 0; i < actionCount; i++)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int actionId = int.Parse(inputs[0]); // the unique ID of this spell or recipe
//     string actionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
//     int delta0 = int.Parse(inputs[2]); // tier-0 ingredient change
//     int delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
//     int delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
//     int delta3 = int.Parse(inputs[5]); // tier-3 ingredient change
//     int price = int.Parse(inputs[6]); // the price in rupees if this is a potion
//     int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax; For brews, this is the value of the current urgency bonus
//     int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell; For brews, this is how many times you can still gain an urgency bonus
//     bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
//     bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell
// }
// for (int i = 0; i < 2; i++)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
//     int inv1 = int.Parse(inputs[1]);
//     int inv2 = int.Parse(inputs[2]);
//     int inv3 = int.Parse(inputs[3]);
//     int score = int.Parse(inputs[4]); // amount of rupees
// }

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

    public class TreeNode<T>
    {
        private readonly T _value;
        private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();

        public TreeNode(T value)
        {
            _value = value;
        }

        public TreeNode<T> this[int i]
        {
            get { return _children[i]; }
        }

        public TreeNode<T> Parent { get; private set; }

        public T Value { get { return _value; } }

        public ReadOnlyCollection<TreeNode<T>> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) {Parent = this};
            _children.Add(node);
            return node;
        }

        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }

        public IEnumerable<T> Flatten()
        {
            return new[] {Value}.Concat(_children.SelectMany(x => x.Flatten()));
        }
    }

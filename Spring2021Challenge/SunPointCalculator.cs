using System;
using System.Collections.Generic;
using System.Linq;

namespace Spring2021Challenge
{
    internal sealed class SunPointCalculator
    {
        private readonly List<Cell> _boardCells;
        private readonly List<Tree> _trees;
        private readonly int _sunDirection;

        private bool[] _inSpookyShadow;

        internal SunPointCalculator(List<Cell> boardCells, List<Tree> trees, int sunDirection)
        {
            _boardCells = boardCells;
            _trees = trees;
            _sunDirection = sunDirection;

            _inSpookyShadow = new bool[37];
        }

        internal Tuple<int, int> CalculateSunPoints()
        {
            CalculateShadowedCells();

            return CalculatePoints();
        }
        
        private void CalculateShadowedCells()
        {
            // foreach tree calculate it's shadow
            foreach (var tree in _trees)
            {
                if (tree.Size > 0)
                {
                    // Calculate first shadow
                    var treeCell = _boardCells.Find(c => c.Index == tree.CellIndex);
                    
                    if (treeCell == null) { continue; }
                    
                    var shadowIndex = treeCell.Neighbours[_sunDirection];

                    if (IsTreeInSpookyShadow(tree.Size, shadowIndex))
                    {
                        _inSpookyShadow[shadowIndex] = true;
                    }

                    // If size is 2 calculate 2nd shadow
                    if (tree.Size > 1)
                    {
                        var shadowCell = _boardCells.Find(c => c.Index == shadowIndex);

                        if (shadowCell == null) { continue; }
                        
                        shadowIndex = shadowCell.Neighbours[_sunDirection];

                        if (IsTreeInSpookyShadow(tree.Size, shadowIndex))
                        {
                            _inSpookyShadow[shadowIndex] = true;
                        }
                    }

                    // If size is 3 calculate 3rd shadow
                    if (tree.Size > 2)
                    {
                        var shadowCell = _boardCells.Find(c => c.Index == shadowIndex);
                        
                        if (shadowCell == null) { continue; }
                        
                        shadowIndex = shadowCell.Neighbours[_sunDirection];

                        if (IsTreeInSpookyShadow(tree.Size, shadowIndex))
                        {
                            _inSpookyShadow[shadowIndex] = true;
                        }
                    }
                }
            }
        }
        
        private bool IsTreeInSpookyShadow(int castingTreeSize, int shadowedTreeIndex)
        {
            // If the tree casts a shadow on a tree that's smaller than or equal to it, then it's spooky
            if (_trees.Find(t => t.CellIndex == shadowedTreeIndex) != null
                && _trees.Find(t => t.CellIndex == shadowedTreeIndex).Size <= castingTreeSize)
            {
                return true;
            }

            return false;
        }
        
        private Tuple<int, int> CalculatePoints()
        {
            var mySunPoints = 0;
            var opponentSunPoints = 0;
            
            foreach (var tree in _trees.Where(tree => !_inSpookyShadow[tree.CellIndex]))
            {
                //Console.Error.WriteLine("============================================");
                
                if (tree.IsMine)
                {
                    //Console.Error.WriteLine("My tree");
                    //Console.Error.WriteLine($"tree.CellIndex: {tree.CellIndex}");
                    //Console.Error.WriteLine($"tree.Size: {tree.Size}");

                    mySunPoints += tree.Size;
                }
                else
                {
                    //Console.Error.WriteLine("Opponents tree");
                    //Console.Error.WriteLine($"tree.CellIndex: {tree.CellIndex}");
                    //Console.Error.WriteLine($"tree.Size: {tree.Size}"); 
                    opponentSunPoints += tree.Size;
                }
            }

            return new Tuple<int, int>(mySunPoints, opponentSunPoints);
        }
        
        Action _lastAction;
        Tree _lastRemovedTree;
        Tree _lastSeededTree;

        internal void DoAction(Action action)
        {
            _lastAction = action;
            
            if(action.Type == "COMPLETE")
            {
                _lastRemovedTree = _trees.Find(t => t.CellIndex == action.TargetCellIdx);        // We probably need to deep copy here
                
                _trees.Remove(_lastRemovedTree);
            }
            if(action.Type == "SEED")
            {
                // We have to assume it's not
                _lastSeededTree = new Tree(action.TargetCellIdx, 0, true, false);
                   
                _trees.Add(_lastSeededTree);
            }
            else if (action.Type == "GROW")
            {
                var tree = _trees.Find(t => t.CellIndex == action.TargetCellIdx);

                tree.Size++;
            }
        }
        
        internal void UndoLastAction()
        {
            if(_lastAction.Type == "COMPLETE")
            {
                _trees.Add(_lastRemovedTree);
            }
            if(_lastAction.Type == "SEED")
            {
                _trees.Remove(_lastSeededTree);
            }
            else if (_lastAction.Type == "GROW")
            {
                var tree = _trees.Find(t => t.CellIndex == _lastAction.TargetCellIdx);

                tree.Size--;
            }
        }
    }
}
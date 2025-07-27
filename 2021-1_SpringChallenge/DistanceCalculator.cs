using System.Collections.Generic;

namespace Spring2021Challenge
{
    internal sealed class DistanceCalculator
    {
        private bool[] _hasBeenChecked;
        private readonly List<Cell> _cells;
    
        internal DistanceCalculator(List<Cell> cells)
        {
            _cells = cells;
    
            _hasBeenChecked = new bool[38];
        }
    
        internal int GetDistanceFromCentre(int cellIndex)
        {
            return GetDistanceBetweenCells(0, cellIndex);
        }
        
    
        internal int GetDistanceBetweenCells(int index1, int index2)
        {
            if(index1 == index2)
            {
                return 0;
            }
    
            _hasBeenChecked = new bool[38];

            Cell cell1 = _cells.Find(c => c.Index == index1);
    
            _hasBeenChecked[cell1.Index] = true;

            List<int> toCheck = new List<int>
            {
                cell1.Index
            };
    
            int distance = 1;
            
    
            while (distance <= 6)
            {
                // get ones to check 
                List<int> neighbouringIndexes = GetNeighbouringIndexes(toCheck);
    
                // check them
                if(neighbouringIndexes.Contains(index2))
                {
                    return distance;
                }
    
                // mark them as checked
                foreach(int index in neighbouringIndexes)
                {              
                    _hasBeenChecked[index] = true;
                }
    
                // update 
                toCheck = neighbouringIndexes;
    
                distance++;
            }
    
            // We should never get here
            return 7;
        }
    
        private List<int> GetNeighbouringIndexes(List<int> indexes)
        {
            List<int> neighbouringIndexes = new List<int>();
    
            foreach(int index in indexes)
            {
                int[] neighbourIndexes = _cells.Find(c => c.Index == index).Neighbours;
    
                foreach(int neighbourIndex in neighbourIndexes)
                {
                    if(neighbourIndex != -1 && !_hasBeenChecked[neighbourIndex] && !neighbouringIndexes.Contains(neighbourIndex))
                    {
                        neighbouringIndexes.Add(neighbourIndex);
                    }
                }        
            }
    
            return neighbouringIndexes;
        }
    }
}
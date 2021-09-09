namespace Spring2021Challenge
{
    internal sealed class Action
    {
        public string Type { get; }
        public int SourceCellIdx { get; }
        public int TargetCellIdx { get; }

        private Action(string type, int sourceCellIdx, int targetCellIdx)
        {
            Type = type;
            SourceCellIdx = sourceCellIdx;
            TargetCellIdx = targetCellIdx;
        }

        private Action(string type, int targetCellIdx)
            : this(type, 0, targetCellIdx)
        {
        }

        private Action(string type)
            : this(type, 0, 0)
        {
        }
        
        public static Action Parse(string action)
        {
            var parts = action.Split(" ");
            
            switch (parts[0])
            {
                case "WAIT":
                    return new Action("WAIT");
                case "SEED":
                    return new Action("SEED", int.Parse(parts[1]), int.Parse(parts[2]));
                case "GROW":
                case "COMPLETE":
                default:
                    return new Action(parts[0], int.Parse(parts[1]));
            }
        }
    
        public override string ToString()
        {
            switch (Type)
            {
                case "WAIT":
                    return "WAIT";
                case "SEED":
                    return $"SEED {SourceCellIdx} {TargetCellIdx}";
                default:
                    return $"{Type} {TargetCellIdx}";
            }
        }
    }
}
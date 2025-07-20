namespace SummerChallenge2025_SoakOverflow;

// Scores how much cover affects the given position of all 
// positions on the map
public class CoverMapGenerator
{
    private int _width, _height;
    private int[,] _cover;

    public CoverMapGenerator(int[,] cover)
    {
        _width = cover.GetLength(0);
        _height = cover.GetLength(1);
        _cover = cover;
    }

    public double[,] CreateCoverMap(int xPos, int yPos)
    {
        var coverMap = new double[_width, _height];

        // Populate all elements with 1.0
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                coverMap[i, j] = 1.0;
            }
        }

        // Set the current position to 0.0
        coverMap[xPos, yPos] = 0.0; 

        // Check if north is protected
        if (yPos - 2 >= 0 && _cover[xPos, yPos - 1] > 0)
        {
            var fillValue = GetCoverProtectionValue(_cover[xPos, yPos - 1]);
                
            // Fill all values to the north
            for (int y = 0; y <= yPos - 2; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    coverMap[x, y] = fillValue;
                }
            }

            // Set adjacent tiles back to 1.0
            if (xPos - 1 >= 0)
            {
                coverMap[xPos - 1, yPos - 2] = 1.0;
            }
            coverMap[xPos, yPos - 2] = 1.0;
            if (xPos + 1 < _width)
            {
                coverMap[xPos + 1, yPos - 2] = 1.0;
            }
        }

        // Check if south is protected
        if (yPos + 2 <= _height-1 && _cover[xPos, yPos + 1] > 0)
        {
            var fillValue = GetCoverProtectionValue(_cover[xPos, yPos + 1]);
            // Fill all values to the south
            for (int y = yPos + 2; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    coverMap[x, y] = fillValue;
                }
            }

            // Set adjacent tiles back to 1.0
            if (xPos - 1 >= 0)
            {
                coverMap[xPos - 1, yPos + 2] = 1.0;
            }
            coverMap[xPos, yPos + 2] = 1.0;
            if (xPos + 1 < _width)
            {
                coverMap[xPos + 1, yPos + 2] = 1.0;
            }
        }

        // Check if east is protected
        if (xPos + 2 < _width && _cover[xPos + 1, yPos] > 0)
        {
            var fillValue = GetCoverProtectionValue(_cover[xPos + 1, yPos]);
            // Fill all values to the east
            for (int x = xPos + 2; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // Don't fill it if it's already covered by large cover
                    if (coverMap[x, y] != 0.25)
                    {
                        coverMap[x, y] = fillValue;
                    }
                }
            }
            // Set adjacent tiles back to 1.0
            if (yPos - 1 >= 0)
            {
                coverMap[xPos + 2, yPos - 1] = 1.0;
            }
            coverMap[xPos + 2, yPos] = 1.0;
            if (yPos + 1 < _height)
            {
                coverMap[xPos + 2, yPos + 1] = 1.0;
            }
        }

        // Check if west is protected
        if (xPos - 2 >= 0 && _cover[xPos - 1, yPos] > 0)
        {
            var fillValue = GetCoverProtectionValue(_cover[xPos - 1, yPos]);
            // Fill all values to the west
            for (int x = 0; x <= xPos - 2; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // Don't fill it if it's already covered by large cover
                    if (coverMap[x, y] != 0.25)
                    {
                        coverMap[x, y] = fillValue;
                    }
                }
            }
            // Set adjacent tiles back to 1.0
            if (yPos - 1 >= 0)
            {
                coverMap[xPos - 2, yPos - 1] = 1.0;
            }
            coverMap[xPos - 2, yPos] = 1.0;
            if (yPos + 1 < _height)
            {
                coverMap[xPos - 2, yPos + 1] = 1.0;
            }
        }

        return coverMap;
    }

    private double GetCoverProtectionValue(int coverType)
    {
        if (coverType == 1)
        {
            return 0.5;
        }
        else if (coverType == 2)
        {
            return 0.25;
        }

        return 1.0;
    }
}
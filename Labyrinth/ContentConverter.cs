namespace Labyrinth
{
    internal static class ContentConverter
    {
        internal static char[,] ToCharacterGrid(Content[,] worldGrid)
        {
            var characterGrid = new char[worldGrid.GetLength(0), worldGrid.GetLength(1)];

            for (var y = 0; y < worldGrid.GetLength(1); y++)
            {
                for (var x = 0; x < worldGrid.GetLength(0); x++)
                {
                    characterGrid[x, y] = ToCharacter(worldGrid[x, y]);
                }
            }

            return characterGrid;
        }

        private static char ToCharacter(Content content)
        {
            return content switch
            {
                Content.Unknown       => '?',
                Content.Wall          => '#',
                Content.StartPosition => 'T',
                Content.Hollow        => '.',
                Content.ControlRoom   => 'C',
                _                     => '?'
            };
        }

        internal static Content ToContent(char contentChar)
        {
            return contentChar switch
            {
                '?' => Content.Unknown,
                '#' => Content.Wall,
                'T' => Content.StartPosition,
                '.' => Content.Hollow,
                'C' => Content.ControlRoom,
                _   => Content.Unknown
            };
        }
    }
}

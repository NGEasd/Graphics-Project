namespace Labirintus_projekt
{
    class LabirintMap
    {
        private int[,] map;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public LabirintMap()
        {
            GenerateMaze();
        }

        private void GenerateMaze()
        {

            string[] hardcodedMap = new string[] {
                "113311111199111119911",
                "110010011100111110009",
                "110010009000100010709",
                "110010011111111010011",
                "110007000000000000009",
                "110000000000000000009",
                "111100191111991001111",
                "110000000000000000009",
                "100000000070000000009",
                "900101001000111010011",
                "900191001107000000001",
                "111101001100000000211",
                "111191111199111119911"
            };

            Height = hardcodedMap.Length;
            Width = hardcodedMap[0].Length;

            map = new int[Height, Width];

            for (int x = 0; x < Height; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    int value = int.Parse(hardcodedMap[x][y].ToString());
                    map[x, y] = value;
                }
            }
        }

        public int Get(int x, int y)
        { 
            return map[x, y];
        }

        public int[] GetStartingPosition()
        {
            for (int x = 0; x < Height; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    if (map[x, y] == 2)
                    {
                        return new int[] { x, y};
                    }
                }
            }

            return new int[] { -1, -1 };
        }

        public bool IsWall(int x, int y)
        {
            return Get(x, y) == 1;
        }

        public bool IsWindow(int x, int y)
        {
            return Get(x, y) == 9;
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Height && y >= 0 && y < Width;
        }

        public void PrintToConsole()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Console.Write(map[y, x] == 1 ? "#" : " ");
                }
                Console.WriteLine();
            }
        }
        public int[] getLabirintCenter()
        {
            return new int[] { Width / 2, Height / 2 };
        }
    }
}

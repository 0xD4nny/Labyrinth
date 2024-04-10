namespace Labyrinth
{
    /// <summary>
    /// Stores the given coordinates and those of all four neighbors, indicating their accessibility with a boolean.
    /// </summary>
    /// <remarks>
    /// This class captures both the coordinates of a specified location and the coordinates of its four
    /// adjacent neighbors. Each neighbor's accessibility is represented by a boolean flag, providing essential
    /// information for spatial navigation and pathfinding algorithms.
    /// </remarks>
    class Location
    {
        public readonly int X;
        public readonly int Y;
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        public readonly (int X, int Y, bool IsReachable)[] Neighbors = new (int X, int Y, bool IsReachable)[4];

        public Location(int x, int y, char[,] map)
        {

            X = x;
            Y = y;
            _mapWidth = map.Length / map.GetLength(0);
            _mapHeight = map.GetLength(0);

            for (int i = 0; i < 4; i++)
            {
                if (x + Direction.DirX[i] < _mapWidth && y + Direction.DirY[i] < _mapHeight && x + Direction.DirX[i] > 0 && y + Direction.DirY[i] > 0)
                {
                    if (map[y + Direction.DirY[i], x + Direction.DirX[i]] is not 'W' &&
                        map[y + Direction.DirY[i], x + Direction.DirX[i]] is not '\0' &&
                        map[y + Direction.DirY[i], x + Direction.DirX[i]] is not '.')
                        Neighbors[i] = (X + Direction.DirX[i], Y + Direction.DirY[i], true);
                    else
                        Neighbors[i] = (X + Direction.DirX[i], Y + Direction.DirY[i], false);
                }
            }
        }

    }
}
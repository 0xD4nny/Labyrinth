namespace Labyrinth
{
    class BotAlgorithm
    {
        private readonly Stack<(int X, int Y)> _breadKrumelPath = new Stack<(int X, int Y)>();
        private (bool tDetected, int x, int y) _target;

        public readonly Dictionary<(int x, int y), Location> Graph = new Dictionary<(int x, int y), Location>();

        /// <summary>
        /// Invokes every private method in this class to generate a list containing the best path, 
        /// represented as a series of command strings
        /// </summary>
        public List<string> Run(Location currentPosition, char[,] map, bool[,] reachedLocations, ref bool gameWon)
        {
            Location target = GetTarget(currentPosition, map, reachedLocations);
            List<(int x, int y)> Path = Pathfinding(target, currentPosition, map, ref gameWon);
            while (PathFilter(Path));
            return ParseCordsToDirs(Path, reachedLocations);
        }

        /// <summary>
        /// This method iterates along the edge of the field of view once and returns the first empty field found at the edge.
        /// </summary>
        private Location GetTarget(Location currentLocation, char[,] map, bool[,] reachedLocations)
        {
            if (currentLocation is null)
                throw new ArgumentNullException(nameof(currentLocation));

            // + 5 to start in corner botton right (Rechts unten gewichtet)
            int searchPointX = currentLocation.X + 5;
            int searchPointY = currentLocation.Y + 5;

            if (_target.tDetected && IsReachable(new Location(_target.x, _target.y, map), map))
                return new Location(_target.x, _target.y, map);

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) && reachedLocations[searchPointY, searchPointX] is false)
                    return new Location(searchPointX, searchPointY, map);

                searchPointX--;
            }

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) && reachedLocations[searchPointY, searchPointX] is false)
                    return new Location(searchPointX, searchPointY, map);

                searchPointY--;
            }

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) && reachedLocations[searchPointY, searchPointX] is false)
                    return new Location(searchPointX, searchPointY, map);

                searchPointX++;
            }

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) && reachedLocations[searchPointY, searchPointX] is false)
                    return new Location(searchPointX, searchPointY, map);

                searchPointY++;
            }

            // Ein Brotkrümelpfad könnte auch hier der waytogo sein, um wieder solange zurück zu laufen bis wir wieder ein undefine field finden.
            for(int i = 0; i < 20; i++)
                _breadKrumelPath.Pop();

            (int x, int y) krumel = _breadKrumelPath.Pop();
            return new Location(krumel.x, krumel.y, map);

            throw new Exception("No Target found");


        }

        /// <summary>
        /// This method helps getTarget determine if it is reachable.
        /// Note: we search with this method to, if we found the target'T' and take there location and set a flag on true. 
        /// </summary>
        private bool IsReachable(Location current, char[,] map)
        {
            HashSet<(int X, int Y)> reached = new HashSet<(int X, int Y)>();
            Queue<Location> queue = new Queue<Location>();

            queue.Enqueue(current);

            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                reached.Add((current.X, current.Y));

                for (int i = 0; i < 4; i++)
                    if (current.Neighbors[i].IsReachable && !reached.Contains((current.Neighbors[i].X, current.Neighbors[i].Y)))
                    {
                        queue.Enqueue(new Location(current.Neighbors[i].X, current.Neighbors[i].Y, map));
                        reached.Add((current.Neighbors[i].X, current.Neighbors[i].Y));
                    }
                if (map[current.Y, current.X] is 'T')
                    _target = (true, current.X, current.Y);


                if (map[current.Y, current.X] is 'P')
                    return true;
            }
            return false;
        }

        /// <summary>
        /// This pathfinding algorithm method returns a queue with the (int X, int Y) from the start point to the target point.
        /// </summary>
        private List<(int X, int Y)> Pathfinding(Location target, Location currentPosition, char[,] map, ref bool gameWon)
        {
            if (currentPosition is null)
                throw new ArgumentNullException("_currentPosition can't be null.");

            LinkedList<(int X, int Y)> breadkrumel = new LinkedList<(int X, int Y)>();
            HashSet<(int X, int Y)> reached = new HashSet<(int X, int Y)>();
            List<(int X, int Y)> path = new List<(int X, int Y)>();
            Queue<Location> queue = new Queue<Location>();

            Location current = new Location(currentPosition.X, currentPosition.Y, map);

            queue.Enqueue(current);
            path.Add((current.X, current.Y));
            breadkrumel.AddFirst((current.X, current.Y));

            while ((current.X, current.Y) != (target.X, target.Y) || map[current.Y, current.X] is 'T')
            {
                current = queue.Dequeue();
                reached.Add((current.X, current.Y));

                if (map[current.Y, current.X] is 'T')
                {
                    gameWon = true;
                    break;
                }

                for (int i = 0; i < 4; i++)
                    if (!reached.Contains((current.Neighbors[i].X, current.Neighbors[i].Y)) && current.Neighbors[i].IsReachable)
                    {
                        breadkrumel.AddFirst((current.Neighbors[i].X, current.Neighbors[i].Y));
                        queue.Enqueue(new Location(current.Neighbors[i].X, current.Neighbors[i].Y, map));
                        path.Add((current.Neighbors[i].X, current.Neighbors[i].Y));
                        break;
                    }

                if (queue.Count == 0)
                {
                    (int X, int Y) = breadkrumel.First.Next.Value;
                    breadkrumel.RemoveFirst();
                    queue.Enqueue(new Location(X, Y, map));
                    path.Add((X, Y));
                }

            }
            path.Add((target.X, target.Y));
            return path;
        }

        /// <summary>
        /// This method filters out dead ends from the path, based on their doublicates.
        /// </summary>
        private bool PathFilter(List<(int x, int y)> path)
        {
            List<(int index, int x, int y)> indexes = new List<(int index, int x, int y)>();
            List<(int index, int x, int y)> indexes2 = new List<(int index, int x, int y)>();

            // whit this loop, we search doublicates;
            for (int i = 0; i < path.Count; i++)
                for (int j = 1 + i; j < path.Count; j++)
                    if (path[i] == path[j])
                        indexes.Add((i, path[i].x, path[i].y));

            // with this loop, we search how much doublicates ever doublicate has.
            for (int i = 0; i < indexes.Count; i++)
                for (int j = 0; j < path.Count; j++)
                    if (path[j] == (indexes[i].x, indexes[i].y))
                        indexes2.Add((j, path[j].x, path[j].y));

            if (indexes2.Count <= 0)
                return false;

            path.RemoveRange(indexes2[0].index, indexes2[1].index - indexes2[0].index);

            return true;
        }

        /// <summary>
        /// This methode is to parse the result of the pathfinding method in strings for the responses.
        /// </summary>
        private List<string> ParseCordsToDirs(List<(int X, int Y)> path, bool[,] reachedLocations)
        {
            string[] _dirs = ["RIGHT", "DOWN", "LEFT", "UP"];
            List<string> dirs = new List<string>();
            (int x, int y) current = path[0];

            path.Remove(current);
            reachedLocations[current.y, current.x] = true;
            _breadKrumelPath.Push((current.x, current.y));

            while (path.Count > 0)
            {
                (int x, int y) next = path[0];
                path.Remove(next);
                for (int i = 0; i < 4; i++)
                {
                    if (current.x + Direction.DirX[i] == next.x && current.y + Direction.DirY[i] == next.y)
                    {
                        _breadKrumelPath.Push((next.x, next.y));
                        reachedLocations[next.y, next.x] = true;
                        dirs.Add(_dirs[i]);
                        current = next;
                    }
                }
            }
            return dirs;
        }

        //"DOWN" = Y++;
        //"Right = X++;
        //"LEFT" = X--;
        //"UP" = Y--;

        //private double GetHeristic(Location current, Location target)
        //{
        //    return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        //}

    }
}
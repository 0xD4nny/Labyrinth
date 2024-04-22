using System.Diagnostics;

namespace Labyrinth
{
    class BotAlgorithm
    {
        public readonly Dictionary<(int x, int y), Location> Graph = new Dictionary<(int x, int y), Location>();
        private readonly Stack<Location> _reachableHoles = new Stack<Location>();
        private (bool tDetected, int x, int y) _target;

        /// <summary>
        /// Invokes every private method in this class to generate a list thats containing the best path, 
        /// represented as a series of command strings
        /// </summary>
        public List<string> Run(Location currentPosition, char[,] map, bool[,] reachedLocations, ref bool gameWon)
        {
            GetHoles(currentPosition, map, reachedLocations);
            Location location = _reachableHoles.Pop();
            
            while ((location.X, location.Y) == (currentPosition.X, currentPosition.Y))
                location = _reachableHoles.Pop();


            List<Location> path = Pathfinding(currentPosition, location, map, ref gameWon);
            List<string> dirs = ParseCordsToDirs(path, reachedLocations);

            return dirs;
        }

        public long getHoles = 0;
        public long pathFinding = 0;
        public long parseChords = 0;
        public long allTicks = 0;

        public List<string> RunBenchmark(Location currentPosition, char[,] map, bool[,] reachedLocations, ref bool gameWon)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            GetHoles(currentPosition, map, reachedLocations);
            Location location = _reachableHoles.Pop();

            while ((location.X, location.Y) == (currentPosition.X, currentPosition.Y))
                location = _reachableHoles.Pop();

            stopwatch.Stop();
            getHoles += stopwatch.ElapsedTicks;
            stopwatch.Reset();

            stopwatch.Start();
            List<Location> Path = AStar(location, currentPosition, map, ref gameWon);
            stopwatch.Stop();
            pathFinding += stopwatch.ElapsedTicks;
            stopwatch.Reset();

            stopwatch.Start();
            List<string> dirs = ParseCordsToDirs(Path, reachedLocations);
            stopwatch.Stop();
            parseChords += stopwatch.ElapsedTicks;
            stopwatch.Reset();

            return dirs;
        }

        private bool IsAnyNeigborUndefine(Location location, char[,] map)
        {
            for (int i = 0; i < 4; i++)
                if (map[location.Neighbors[i].Y, location.Neighbors[i].X] == '\0')
                    return true;

            return false;
        }

        /// <summary>
        /// This method iterates along the edge of the field of view once and returns the first empty field found at the edge.
        /// </summary>
        private void GetHoles(Location currentLocation, char[,] map, bool[,] reachedLocations)
        {
            if (_target.tDetected && IsReachable(new Location(_target.x, _target.y, map), map))
            {
                _reachableHoles.Push(new Location(_target.x, _target.y, map));
                return;
            }

            if (currentLocation is null)
                throw new ArgumentNullException(nameof(currentLocation));

            // + 5 to start in corner botton right (Rechts unten gewichtet)
            int searchPointX = currentLocation.X + 5;
            int searchPointY = currentLocation.Y + 5;

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) &&
                    reachedLocations[searchPointY, searchPointX] is false && !_reachableHoles.Contains(new Location(searchPointX, searchPointY, map)))
                    _reachableHoles.Push(new Location(searchPointX, searchPointY, map));

                searchPointX--;
            }

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) &&
                    reachedLocations[searchPointY, searchPointX] is false && !_reachableHoles.Contains(new Location(searchPointX, searchPointY, map)))
                    _reachableHoles.Push(new Location(searchPointX, searchPointY, map));

                searchPointY--;
            }

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) &&
                    reachedLocations[searchPointY, searchPointX] is false && !_reachableHoles.Contains(new Location(searchPointX, searchPointY, map)))
                    _reachableHoles.Push(new Location(searchPointX, searchPointY, map));

                searchPointX++;
            }

            for (int i = 0; i < 10; i++)
            {
                if (Graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, map), map) &&
                    reachedLocations[searchPointY, searchPointX] is false && !_reachableHoles.Contains(new Location(searchPointX, searchPointY, map)))
                    _reachableHoles.Push(new Location(searchPointX, searchPointY, map));

                searchPointY++;
            }
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
        private List<Location> Pathfinding(Location target, Location currentPosition, char[,] map, ref bool gameWon)
        {
            if (currentPosition is null)
                throw new ArgumentNullException("_currentPosition can't be null.");

            LinkedList<(int X, int Y)> breadkrumel = new LinkedList<(int X, int Y)>();
            HashSet<(int X, int Y)> reached = new HashSet<(int X, int Y)>();
            List<Location> path = new List<Location>();
            Queue<Location> queue = new Queue<Location>();

            Location current = new Location(currentPosition.X, currentPosition.Y, map);

            queue.Enqueue(current);
            path.Add(current);
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
                        path.Add(new Location(current.Neighbors[i].X, current.Neighbors[i].Y, map));
                        break;
                    }

                if (queue.Count == 0)
                {
                    (int X, int Y) = breadkrumel.First.Next.Value;
                    breadkrumel.RemoveFirst();
                    queue.Enqueue(new Location(X, Y, map));
                    path.Add(new Location(X,Y, map));
                }

            }
            path.Add(new Location(target.X, target.Y,map));
            return path;
        }

        /// <summary>
        /// This methode is to parse the result of the pathfinding method in strings for the responses.
        /// </summary>
        private List<string> ParseCordsToDirs(List<Location> path, bool[,] reachedLocations)
        {
            string[] _dirs = ["RIGHT", "DOWN", "LEFT", "UP"];
            List<string> dirs = new List<string>();
            Location current = path[0];

            path.Remove(current);
            reachedLocations[current.Y, current.X] = true;

            while (path.Count > 0)
            {
                Location next = path[0];
                path.Remove(next);
                for (int i = 0; i < 4; i++)
                {
                    if (current.X + Direction.DirX[i] == next.X && current.Y + Direction.DirY[i] == next.Y)
                    {
                        reachedLocations[next.Y, next.X] = true;
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

        /// <summary>
        /// Alternative Algorithm
        /// </summary>
        
        public List<Location> AStar(Location start, Location goal, char[,] map, ref bool gameWon)
        {
            Dictionary<Location, int> costSoFar = new Dictionary<Location, int>();
            List<Location> path = new List<Location>();
            PriorityQueue<Location, int> frontier = new PriorityQueue<Location, int>();

            frontier.Enqueue(start, 0);
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                Location current = frontier.Dequeue();
                path.Add(current);

                if ((current.X, current.Y) == (goal.X, goal.Y))
                    return path;

                for (int i = 0; i < 4; i++)
                {
                    Location next = new Location(current.Neighbors[i].X, current.Neighbors[i].Y, map);
                    int newCost = costSoFar[current];
                    if (map[next.Y, next.X] is not 'W')
                    {
                        costSoFar[next] = newCost;
                        frontier.Enqueue(next, newCost + GetHeuristic(next, goal));
                    }
                }
            }

            throw new Exception("pathfinding has failed.");
        }

        private int GetHeuristic(Location current, Location target)
        {
            return Math.Abs(current.X - target.X) + Math.Abs(current.Y - target.Y);
        }
    }
}

///// <summary>
///// This method filters out dead ends from the path, based on their doublicates.
///// </summary>
//private bool PathFilter(List<(int x, int y)> path)
//{
//    List<(int index, int x, int y)> indexes = new List<(int index, int x, int y)>();
//    List<(int index, int x, int y)> indexes2 = new List<(int index, int x, int y)>();

//    // whit this loop, we search doublicates;
//    for (int i = 0; i < path.Count; i++)
//        for (int j = 1 + i; j < path.Count; j++)
//            if (path[i] == path[j])
//                indexes.Add((i, path[i].x, path[i].y));

//    // with this loop, we search how much doublicates ever doublicate has.
//    for (int i = 0; i < indexes.Count; i++)
//        for (int j = 0; j < path.Count; j++)
//            if (path[j] == (indexes[i].x, indexes[i].y))
//                indexes2.Add((j, path[j].x, path[j].y));

//    if (indexes2.Count <= 0)
//        return false;

//    path.RemoveRange(indexes2[0].index, indexes2[1].index - indexes2[0].index);

//    return true;
//} Pathfilter
﻿using System.Diagnostics;

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

            List<(int x, int y)> Path = AStar(location, currentPosition, map, ref gameWon);
            return ParseCordsToDirs(Path, reachedLocations);
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
            List<(int x, int y)> Path = Pathfinding(location, currentPosition, map, ref gameWon);
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
        /// This methode is to parse the result of the pathfinding method in strings for the responses.
        /// </summary>
        private List<string> ParseCordsToDirs(List<(int X, int Y)> path, bool[,] reachedLocations)
        {
            string[] _dirs = ["RIGHT", "DOWN", "LEFT", "UP"];
            List<string> dirs = new List<string>();
            (int x, int y) current = path[0];

            path.Remove(current);
            reachedLocations[current.y, current.x] = true;

            while (path.Count > 0)
            {
                (int x, int y) next = path[0];
                path.Remove(next);
                for (int i = 0; i < 4; i++)
                {
                    if (current.x + Direction.DirX[i] == next.x && current.y + Direction.DirY[i] == next.y)
                    {
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

        /// <summary>
        /// Alternative Algorithm
        /// </summary>
        private List<(int X, int Y)> AStar(Location target, Location currentPosition, char[,] map, ref bool gameWon)
        {
            if (currentPosition is null)
                throw new ArgumentNullException("_currentPosition can't be null.");

            LinkedList<(int X, int Y)> breadkrumel = new LinkedList<(int X, int Y)>();
            HashSet<(int X, int Y)> reached = new HashSet<(int X, int Y)>();
            List<(int X, int Y)> path = new List<(int X, int Y)>();
            PriorityQueue<Location, double> queue = new PriorityQueue<Location, double>();

            Location current = new Location(currentPosition.X, currentPosition.Y, map);

            queue.Enqueue(current, 0);
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
                        queue.Enqueue(new Location(current.Neighbors[i].X, current.Neighbors[i].Y, map), GetHeristic(new Location(current.Neighbors[i].X, current.Neighbors[i].Y, map), target));
                        path.Add((current.Neighbors[i].X, current.Neighbors[i].Y));
                        break;
                    }

                if (queue.Count == 0)
                {
                    (int X, int Y) = breadkrumel.First.Next.Value;
                    breadkrumel.RemoveFirst();
                    queue.Enqueue(new Location(X, Y, map), GetHeristic(new Location(X, Y, map), target));
                    path.Add((X, Y));
                }

            }
            path.Add((target.X, target.Y));
            return path;
        }

        private double GetHeristic(Location current, Location target)
        {
            return Math.Abs(current.X - target.X) + Math.Abs(current.Y - target.Y);
        }
    }
}
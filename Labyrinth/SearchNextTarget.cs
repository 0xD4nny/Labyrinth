using System.Diagnostics;

namespace Labyrinth;

class SearchNextTarget
{
    private Stack<Node> _next = new Stack<Node>();

    private HashSet<Node> _reachable = new HashSet<Node>();

    private (bool tDetected, Node goal) _tTarget;

    private readonly Map _map;

    public SearchNextTarget(Map map)
    {
        _map = map;
    }


    /// <summary>
    /// This method returns a target with a undefine neigbor.
    /// </summary>
    public Node GetTarget(Node current, ref bool gameWon)
    {
        CheckReachability(current,ref gameWon);
        CollectTargets(current, gameWon);

        Node target = new Node(0, 0);

        if (_next.Count > 0)
            target = _next.Pop();

        if (gameWon)
            return target;

        while (!_map.HasUndefineNeigbor(target) && _next.Count > 0)
            target = _next.Pop();

        if (_next.Count < 1)
            return FloodToNextWithUndefineNeigbor(current, ref gameWon);

        return target;
    }

    /// <summary>
    /// This method helps the class determine if the next target is reachable.
    /// Note: we search with this method to, if we found the target'T' and take there location and set a flag on true. 
    /// </summary>
    private void CheckReachability(Node current, ref bool _gameWon)
    {
        HashSet<Node> reached = new HashSet<Node>();
        Queue<Node> queue = new Queue<Node>();

        Node start = current;

        queue.Enqueue(current);

        while (queue.Count > 0)
        {
            current = queue.Dequeue();
            reached.Add(current);

            foreach (Node dir in _map.DIRS)
            {
                Node next = new Node(current.X + dir.X, current.Y + dir.Y);
                if (_map.InPView(next, start) && !reached.Contains(next) && _map.NoWall(next))
                {
                    queue.Enqueue(next);
                    reached.Add(next);
                    _reachable.Add(next);
                }
                if (_map.InBounds(next) && _map.MapGrid[next.Y, next.X] is 'T')
                {
                    _next.Push(new Node(next.X, next.Y));
                    _gameWon = true;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// This method iterates along the edge of the field of view once and put them in a Stack if they are reachable.
    /// </summary>
    private void CollectTargets(Node currentNode, bool gameWon)
    {
        if (gameWon)
            return;

        Node[] searchDIRS = [new Node(-1, 0), new Node(0, -1), new Node(1, 0), new Node(0, 1)];
        Node searchPoint = new Node(currentNode.X + 5, currentNode.Y + 5);

        for (int s = 0; s < 4; s++)
            for (int i = 0; i < 10; i++)
            {
                if (_reachable.Contains(searchPoint) && !_map.IsReached(searchPoint) && !_next.Contains(searchPoint))
                    _next.Push(searchPoint);

                searchPoint = new Node(searchPoint.X + searchDIRS[s].X, searchPoint.Y + searchDIRS[s].Y);
            }
    }

    /// <summary>
    /// This method is implemented to address a specific bug where targets that were previously uncovered but not collected due to visibility issues are not collected when they become reachable at a later time. 
    /// The bug occurs when a previously visible area has not been accessed yet, leading to a situation where, even though it becomes accessible later, the target isn't collected as it should be. 
    /// This method performs a flood fill from the current point until it reaches a node that has an undefined neighbor, specifically to handle this scenario.
    /// </summary>
    private Node FloodToNextWithUndefineNeigbor(Node current, ref bool gameWon)
    {
        HashSet<Node> reached = new HashSet<Node>();
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(current);

        while (queue.Count > 0)
        {
            current = queue.Dequeue();
            reached.Add(current);

            foreach (Node dir in _map.DIRS)
            {
                Node next = new Node(current.X + dir.X, current.Y + dir.Y);
                if (!reached.Contains(next) && _map.NoWall(next))
                {
                    queue.Enqueue(next);
                    reached.Add(next);
                }
                if (_map.InBounds(next) && _map.MapGrid[next.Y, next.X] is 'T')
                {
                    _next.Push(new Node(next.X, next.Y));
                    gameWon = true;
                    return next;

                }
                if (_map.HasUndefineNeigbor(next) && _map.NoWall(next))
                    return next;
            }
        }
        throw new Exception("failed");
    }

    public Node BenchmarkGetTarget(Node current, ref bool gameWon, ref long checkReachability, ref long collectTargets, ref long checkForUndefineNeigbor, ref long floodFillToNext, ref int floodfillUseCount)
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        CheckReachability(current, ref gameWon);
        stopwatch.Stop();
        checkReachability += stopwatch.ElapsedTicks;
        stopwatch.Reset();


        stopwatch.Start();
        CollectTargets(current, gameWon);
        stopwatch.Stop();
        collectTargets += stopwatch.ElapsedTicks;
        stopwatch.Reset();


        Node target = new Node(0, 0);

        if (_next.Count > 0)
            target = _next.Pop();

        if (gameWon)
            return target;


        stopwatch.Start();
        while (!_map.HasUndefineNeigbor(target) && _next.Count > 0)
            target = _next.Pop();
        stopwatch.Stop();
        checkForUndefineNeigbor += stopwatch.ElapsedTicks;
        stopwatch.Reset();


        stopwatch.Start();
        if (_next.Count <= 0)
        {
            ++floodfillUseCount;
            return FloodToNextWithUndefineNeigbor(current, ref gameWon);
        }
        stopwatch.Stop();
        floodFillToNext += stopwatch.ElapsedTicks;
        stopwatch.Reset();

        return target;
    }

}
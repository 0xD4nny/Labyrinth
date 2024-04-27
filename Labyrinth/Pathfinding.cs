namespace Labyrinth;

/// <summary>
/// We use A* to find the best path from player to target
/// </summary>
class Pathfinding
{
    private readonly Map _map;

    public Pathfinding(Map map)
    {
        _map = map;
    }


    /// <summary>
    /// Runs the a* algorithm and returns a list with the best path from start to goal.
    /// </summary>
    public List<Node> Run(Node start, Node goal)
    {
        Dictionary<Node, Node> _bredKrumelPath = new Dictionary<Node, Node>();
        Dictionary<Node, double> _lastCost = new Dictionary<Node, double>();
        PriorityQueue<Node, double> pQueue = new PriorityQueue<Node, double>();
        
        pQueue.Enqueue(start, 0);

        _bredKrumelPath[start] = start;
        _lastCost[start] = 0;

        while (pQueue.Count > 0)
        {
            Node current = pQueue.Dequeue();
            _map.ReachedNodes.Add(current);

            if ((current.X, current.Y) == (goal.X, goal.Y))
                break;

            foreach (Node dir in _map.DIRS)
            {
                Node next = new Node(current.X + dir.X, current.Y + dir.Y);
                if (_map.InBounds(next) && _map.NoWall(next))
                {
                    double newCost = _lastCost[current];
                    if (!_lastCost.ContainsKey(next) || newCost < _lastCost[next])
                    {
                        _lastCost[next] = newCost;
                        double priority = newCost + Heuristic(next, goal);
                        pQueue.Enqueue(next, priority);
                        _bredKrumelPath[next] = current;
                    }
                }
            }
        }

        List<Node> path = new List<Node>();
        Node currentFromGoal = goal;

        while ((currentFromGoal.X, currentFromGoal.Y) != (start.X, start.Y))
        {
            path.Add(currentFromGoal);
            currentFromGoal = _bredKrumelPath[currentFromGoal];
        }
        path.Add(start);
        path.Reverse();

        return path;
    }

    /// <summary>
    /// We use manhattan-distance for the heuristic.
    /// </summary>
    private double Heuristic(Node start, Node goal)
    {
        return Math.Abs(start.X - goal.X) + Math.Abs(start.Y - goal.Y);
    }

}
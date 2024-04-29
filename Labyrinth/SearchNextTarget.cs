namespace Labyrinth;

//Todo: wir übersehen gelegentlich lücken weil wir zu weit springen oder durch zuvor aufgedeckte bereiche können sie nicht abgespeichert werden können.
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
    public Node GetTarget(Node current, ref bool _gameWon)
    {
        CheckReachability(current);
        CollectTargets(current, ref _gameWon);

        Node target = _next.Pop();

        if (_gameWon is true)
            return target;

        while (!_map.HasUnreachedNeigbor(target))
            target = _next.Pop();

        return target;
    }

    /// <summary>
    /// This method helps the class determine if the next target is reachable.
    /// Note: we search with this method to, if we found the target'T' and take there location and set a flag on true. 
    /// </summary>
    private void CheckReachability(Node current)
    {
        HashSet<Node> reached = new HashSet<Node>();
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(current);

        while (queue.Count > 0)
        {
            current = queue.Dequeue();
            reached.Add(current);
            _reachable.Add(current);

            foreach (Node dir in _map.DIRS)
            {
                Node next = new Node(current.X + dir.X, current.Y + dir.Y);
                if (!reached.Contains(next) && _map.NoWall(next) && _map.InPView(next, current))
                {
                    queue.Enqueue(next);
                    reached.Add(next);
                    _reachable.Add(next);
                }
                if (_map.InBounds(next) && _map.MapGrid[next.Y, next.X] is 'T')
                    _tTarget = (true, new Node(next.X, next.Y));
            }
        }
    }

    /// <summary>
    /// This method iterates along the edge of the field of view once and put them in a Stack if they are reachable.
    /// </summary>
    private void CollectTargets(Node currentNode, ref bool gameWon)
    {
        Node[] searchDIRS = [new Node(-1, 0), new Node(0, -1), new Node(1, 0), new Node(0, 1)];
        if (_tTarget.tDetected && _reachable.Contains(currentNode))
        {
            _next.Push(new Node(_tTarget.goal.X, _tTarget.goal.Y));
            gameWon = true;
            return;
        }

        Node searchPoint = new Node(currentNode.X + 5, currentNode.Y + 5);
        for(int s = 0; s < 4; s++)
            for (int i = 0; i < 10; i++)
            {
                if (_reachable.Contains(searchPoint) && !_map.IsReached(searchPoint) && !_next.Contains(searchPoint))
                    _next.Push(searchPoint);

                searchPoint = new Node (searchPoint.X + searchDIRS[s].X, searchPoint.Y + searchDIRS[s].Y);
            }
    }

}
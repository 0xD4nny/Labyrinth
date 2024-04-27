namespace Labyrinth;

//Todo: we collect to much targets atm, fix it.
class SearchNextTarget
{
    public Stack<Node> Next = new Stack<Node>();

    private HashSet<Node> _reachable = new HashSet<Node>();

    private (bool tDetected, Node goal) _tTarget;

    private readonly Map _map;

    public SearchNextTarget(Map map)
    {
        _map = map;
    }

    /// <summary>
    /// This method helps the class determine if the next target is reachable.
    /// Note: we search with this method to, if we found the target'T' and take there location and set a flag on true. 
    /// </summary>
    public void CheckReachability(Node current)
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
                if (_map.InBounds(next) && !reached.Contains(next) && _map.NoWall(next) && _map.InPView(next, current))
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
    public void CollectTargets(Node currentNode, ref bool gameWon)
    {
        if (_tTarget.tDetected && _reachable.Contains(currentNode))
        {
            Next.Push(new Node(_tTarget.goal.X, _tTarget.goal.Y));
            gameWon = true;
            return;
        }

        // + 5 to start in corner botton right.
        int searchPointX = currentNode.X + 5;
        int searchPointY = currentNode.Y + 5;

        for (int i = 0; i < 10; i++)
        {
            Node leftSearch = new Node(searchPointX, searchPointY);
            if (_reachable.Contains(leftSearch) && _map.NoWall(leftSearch) && !_map.IsReached(leftSearch) && !Next.Contains(leftSearch))
                Next.Push(leftSearch);
            searchPointX--;
        }

        for (int i = 0; i < 10; i++)
        {
            Node upSearch = new Node(searchPointX, searchPointY);
            if (_reachable.Contains(upSearch) && _map.NoWall(upSearch) && !_map.IsReached(upSearch) && _map.InBounds(upSearch) && !Next.Contains(upSearch))
                Next.Push(upSearch);
            searchPointY--;
        }

        for (int i = 0; i < 10; i++)
        {
            Node rightSearch = new Node(searchPointX, searchPointY);
            if (_reachable.Contains(rightSearch) && _map.NoWall(rightSearch) && !_map.IsReached(rightSearch) && _map.InBounds(rightSearch) && !Next.Contains(rightSearch))
                Next.Push(rightSearch);
            searchPointX++;
        }

        for (int i = 0; i < 10; i++)
        {
            Node downSearch = new Node(searchPointX, searchPointY);
            if (_reachable.Contains(downSearch) && _map.NoWall(downSearch) && !_map.IsReached(downSearch) && _map.InBounds(downSearch) && !Next.Contains(downSearch))
                Next.Push(downSearch);
            searchPointY++;
        }

    }

}
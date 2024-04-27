namespace Labyrinth;

//Todo: reWrite this class and reverse his strategy.
class SearchNextGoal
{
    public Stack<Node> Targets = new Stack<Node>();

    private (bool tDetected, int x, int y) _tTarget;

    private readonly Map _map;

    public SearchNextGoal(Map map)
    {
        _map = map;
    }


    /// <summary>
    /// This method iterates along the edge of the field of view once and returns the first empty field found at the edge.
    /// </summary>
    public void Run(Node currentNode, ref bool gameWon)
    {
        if (_tTarget.tDetected && IsReachable(new Node(_tTarget.x, _tTarget.y), currentNode))
        {
            Targets.Push(new Node(_tTarget.x, _tTarget.y));
            gameWon = true;
            return;
        }

        // + 5 to start in corner botton right (Rechts unten gewichtet)
        int searchPointX = currentNode.X + 5;
        int searchPointY = currentNode.Y + 5;

        for (int i = 0; i < 10; i++)
        {
            Node leftSearch = new Node(searchPointX, searchPointY);
            if (_map.NoWall(leftSearch) && !_map.IsReached(leftSearch) && !Targets.Contains(leftSearch) && IsReachable(leftSearch, currentNode))
                    Targets.Push(leftSearch);
            searchPointX--;
        }

        for (int i = 0; i < 10; i++)
        {
            Node upSearch = new Node(searchPointX, searchPointY);
            if (_map.NoWall(upSearch) && !_map.IsReached(upSearch) && _map.InBounds(upSearch) && !Targets.Contains(upSearch) && IsReachable(upSearch, currentNode))
                Targets.Push(upSearch);
            searchPointY--;
        }

        for (int i = 0; i < 10; i++)
        {
            Node rightSearch = new Node(searchPointX, searchPointY);
            if (_map.NoWall(rightSearch) && !_map.IsReached(rightSearch) && _map.InBounds(rightSearch) && !Targets.Contains(rightSearch) && IsReachable(rightSearch, currentNode))
                Targets.Push(rightSearch);
            searchPointX++;
        }

        for (int i = 0; i < 10; i++)
        {
            Node downSearch = new Node(searchPointX, searchPointY);
            if (_map.NoWall(downSearch) && !_map.IsReached(downSearch) && _map.InBounds(downSearch) && !Targets.Contains(downSearch) && IsReachable(downSearch, currentNode))
                Targets.Push(downSearch);
            searchPointY++;
        }

    }

    /// <summary>
    /// This method helps the class determine if the next goal is reachable.
    /// Note: we search with this method to, if we found the target'T' and take there location and set a flag on true. 
    /// </summary>
    private bool IsReachable(Node current,Node pPos)
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
                if (_map.InBounds(next) && !reached.Contains(next) && _map.NoWall(next) && _map.InPView(next,pPos))// need debug here 
                {
                    queue.Enqueue(next);
                    reached.Add(next);
                }
                if (_map.InBounds(next) && _map.MapGrid[next.Y, next.X] is 'T')
                    _tTarget = (true, next.X, next.Y);

                if (_map.InBounds(next) && _map.MapGrid[next.Y, next.X] is 'P')
                    return true;

            }
        }
        return false;
    }

}
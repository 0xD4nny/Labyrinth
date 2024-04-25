namespace Labyrinth;

class SearchTargets
{
    private readonly Grid _grid;

    public SearchTargets(Grid grid)
    {
        _grid = grid;
    }

    public Stack<Node> Targets = new Stack<Node>();

    private (bool tDetected, int x, int y) _tTarget;

    //Schreib am Besten beide Methoden um und dreh sie um, so dass zuerst der Flootfill startet und dann fahren wir den rand ab und sammeln nodes ein. 


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
            if (_grid.NoWall(leftSearch) && !_grid.IsReached(leftSearch) && !Targets.Contains(leftSearch) && IsReachable(leftSearch, currentNode))
                    Targets.Push(leftSearch);
            searchPointX--;
        }

        for (int i = 0; i < 10; i++)
        {
            Node upSearch = new Node(searchPointX, searchPointY);
            if (_grid.NoWall(upSearch) && !_grid.IsReached(upSearch) && _grid.InBounds(upSearch) && !Targets.Contains(upSearch) && IsReachable(upSearch, currentNode))
                Targets.Push(upSearch);
            searchPointY--;
        }

        for (int i = 0; i < 10; i++)
        {
            Node rightSearch = new Node(searchPointX, searchPointY);
            if (_grid.NoWall(rightSearch) && !_grid.IsReached(rightSearch) && _grid.InBounds(rightSearch) && !Targets.Contains(rightSearch) && IsReachable(rightSearch, currentNode))
                Targets.Push(rightSearch);
            searchPointX++;
        }

        for (int i = 0; i < 10; i++)
        {
            Node downSearch = new Node(searchPointX, searchPointY);
            if (_grid.NoWall(downSearch) && !_grid.IsReached(downSearch) && _grid.InBounds(downSearch) && !Targets.Contains(downSearch) && IsReachable(downSearch, currentNode))
                Targets.Push(downSearch);
            searchPointY++;
        }

    }

    /// <summary>
    /// This method helps getTarget determine if it is reachable.
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

            foreach (Node dir in _grid.DIRS)
            {
                Node next = new Node(current.X + dir.X, current.Y + dir.Y);
                if (!reached.Contains(next) && _grid.NoWall(next) && _grid.InPView(next,pPos))// need debug here 
                {
                    queue.Enqueue(next);
                    reached.Add(next);
                }
                if (_grid.InBounds(next) && _grid._map[next.Y, next.X] is 'T')
                    _tTarget = (true, next.X, next.Y);

                if (_grid.InBounds(next) && _grid._map[next.Y, next.X] is 'P')
                    return true;

            }
        }
        return false;
    }
}
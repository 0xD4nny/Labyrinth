namespace Labyrinth;

using System.Text;

/// <summary>
/// This class holds all information from the server that we need for the algorithms and the map view, and it provides numerous functions for managing these algorithms.
/// </summary>
class Map
{
    public readonly HashSet<Node> ReachedNodes = new HashSet<Node>();

    public readonly Node[] DIRS = [new Node(1, 0), new Node(0, 1), new Node(-1, 0), new Node(0, -1)];

    public Node? CurrentNode;

    public readonly char[,] MapGrid;

    private int _width, _height;

    public Map(int width, int height)
    {
        _width = width;
        _height = height;
        MapGrid = new char[height, height];
    }

    /// <summary>
    /// Returns true if the specified node is within the defined bounds..
    /// </summary>
    public bool InBounds(Node node)
    {
        return 0 <= node.X && node.X <= _width && 0 <= node.Y && node.Y <= _height;
    }

    /// <summary>
    /// Returns true if the specified node is Reached.
    /// </summary>
    public bool IsReached(Node node)
    {
        return ReachedNodes.Contains(node);
    }

    /// <summary>
    /// Returns true if the specified node is not: 'W', '\0', '.'.
    /// </summary>
    public bool NoWall(Node node)
    {
        return !(MapGrid[node.Y, node.X] == 'W' || MapGrid[node.Y, node.X] == '\0' || MapGrid[node.Y, node.X] == '.');
    }

    /// <summary>
    /// Returns true if the specified node is within the player's view range, extending +5 units in all directions.
    /// </summary>
    public bool InPView(Node next, Node start)
    {
        return start.X - 5 <= next.X && next.X <= start.X + 5 && start.Y - 5 <= next.Y && next.Y <= start.Y + 5;
    }

    public bool HasUndefineNeigbor(Node node)
    {
        foreach (Node dir in DIRS)
            if (MapGrid[node.Y + dir.Y, node.X + dir.X] is '\0')
                return true;
        return false;
    }

    public void UpdateCurrentNode(ServerResponse responseCoordinates)
    {
        if (responseCoordinates is null)
            throw new ArgumentNullException("_coordinates can't be null. Disconnected?");

        int x = 5 + int.Parse(responseCoordinates.Message.Substring(responseCoordinates.Message.IndexOf("X:") + 2, responseCoordinates.Message.IndexOf(";Y") - responseCoordinates.Message.IndexOf("X:") - 2));
        int y = 5 + int.Parse(responseCoordinates.Message.Substring(responseCoordinates.Message.IndexOf("Y:") + 2, responseCoordinates.Message.IndexOf(";Z") - responseCoordinates.Message.IndexOf("Y:") - 2));

        CurrentNode = new Node(x, y);
    }

    public void UpdateMap(ServerResponse[] responseMap)
    {
        if (CurrentNode is null)
            throw new NullReferenceException("_currentNode can't be null. Disconnected?");

        for (int y = 0; y < 11; y++)
            for (int x = 0; x < 11; x++)
                MapGrid[CurrentNode.Y + y - 5, CurrentNode.X + x - 5] = responseMap[y].Message[x];
    }

    public void PrintMap()
    {
        if (CurrentNode is null)
            throw new NullReferenceException("_currentNode can't be null. Disconnected?");

        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (ReachedNodes.Contains(new Node(x, y)))
                    MapGrid[y, x] = '*';

        MapGrid[CurrentNode.Y, CurrentNode.X] = 'P';

        int width = Console.BufferWidth;
        int height = Console.WindowHeight - 3;

        StringBuilder stringBuilder = new StringBuilder();
        for (int h = CurrentNode.Y - height / 2; h < CurrentNode.Y + height / 2; h++)
        {
            for (int w = CurrentNode.X - width / 2; w < CurrentNode.X + width / 2; w++)
            {
                if (h < _height - 1 && w < _width - 1 && h > 0 && w > 0)
                    switch (MapGrid[h, w])
                    {
                        case 'W':
                            stringBuilder.Append('█');
                            break;
                        case '\0':
                            stringBuilder.Append('?');
                            break;
                        case '.':
                            stringBuilder.Append('░');
                            break;
                        default:
                            stringBuilder.Append(MapGrid[h, w]);
                            break;
                    }
                else
                    stringBuilder.Append('░');
            }

            stringBuilder.Append('\n');
        }
        stringBuilder.AppendLine($"X:{CurrentNode.X},Y:{CurrentNode.Y}");
        Console.WriteLine(stringBuilder.ToString());
        MapGrid[CurrentNode.Y, CurrentNode.X] = '*';
    }

}
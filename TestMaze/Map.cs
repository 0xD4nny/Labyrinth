using System.Text;

namespace TestMaze;

class Map
{
    public readonly HashSet<Node> ReachedNodes = new HashSet<Node>();

    public readonly Node[] DIRS = [new Node(1, 0), new Node(0, 1), new Node(-1, 0), new Node(0, -1)];

    private int _width = 13, _height = 13;

    public readonly char[,] MapGrid = new char[13, 13]{
    //0    1    2    3    4    5    6    7    8    9    10   11   12   13
    {'?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?'}, // 0
    {'?', 'W', ' ', 'W', 'W', 'W', ' ', 'W', 'W', ' ', 'W', 'W', '?'}, // 1
    {'?', 'W', ' ', ' ', 'W', ' ', ' ', ' ', 'W', ' ', 'W', 'W', '?'}, // 2
    {'?', 'W', ' ', ' ', 'W', ' ', 'W', ' ', ' ', ' ', ' ', 'W', '?'}, // 3
    {'?', 'W', ' ', ' ', 'W', ' ', ' ', ' ', 'W', ' ', ' ', 'W', '?'}, // 4
    {'?', 'W', ' ', ' ', 'W', ' ', ' ', ' ', 'W', 'W', ' ', 'W', '?'}, // 5
    {'?', 'W', ' ', ' ', 'W', ' ', 'P', ' ', 'W', ' ', ' ', ' ', '?'}, // 6
    {'?', 'W', ' ', ' ', 'W', ' ', ' ', ' ', 'W', ' ', ' ', 'W', '?'}, // 7
    {'?', 'W', ' ', ' ', 'W', ' ', ' ', ' ', 'W', ' ', ' ', 'W', '?'}, // 8
    {'?', 'W', ' ', ' ', 'W', ' ', ' ', ' ', 'W', ' ', ' ', 'W', '?'}, // 9
    {'?', 'W', ' ', ' ', ' ', ' ', 'W', ' ', ' ', ' ', 'W', 'W', '?'}, // 10
    {'?', 'W', 'W', 'W', 'W', 'W', 'W', ' ', 'W', ' ', 'W', 'W', '?'}, // 11
    {'?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?', '?'}, // 12
    };

    public Node? _currentNode = new Node(6, 6);

    public void PrintMap()
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (ReachedNodes.Contains(new Node(x, y)))
                    MapGrid[y, x] = '*';

        MapGrid[_currentNode!.Y, _currentNode.X] = 'P';


        StringBuilder stringBuilder = new StringBuilder();
        for (int h = 0; h < 13; h++)
        {
            for (int w = 0; w < 13; w++)
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
        Console.WriteLine(stringBuilder.ToString());
    }

    public bool InBounds(Node nextNode)
    {
        return 0 <= nextNode.X && nextNode.X < _width && 0 <= nextNode.Y && nextNode.Y < _height;
    }

    public bool IsReached(Node nextNode)
    {
        return ReachedNodes.Contains(nextNode);
    }

    public bool NoWall(Node nextNode)
    {
        return !(MapGrid[nextNode.Y, nextNode.X] == 'W' || MapGrid[nextNode.Y, nextNode.X] == '\0' || MapGrid[nextNode.Y, nextNode.X] == '.');
    }

    public bool InPView(Node nextNode, Node currentNode)
    {
        return currentNode.X - 5 < nextNode.X && nextNode.X < currentNode.X + 5 && currentNode.Y - 5 < nextNode.Y && nextNode.Y < currentNode.Y + 5;
    }

}

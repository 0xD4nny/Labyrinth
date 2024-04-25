using System.Text;

namespace Labyrinth;

class Grid
{
    public readonly Node[] DIRS = [new Node(1, 0), new Node(0, 1), new Node(-1, 0), new Node(0, -1)];

    public readonly HashSet<Node> Walls = new HashSet<Node>();
    public readonly HashSet<Node> ReachedNodes = new HashSet<Node>();

    public char[,] _map;

    private readonly int _width, _height;

    public Grid(int width, int height)
    {
        _width = width + 10;
        _height = height + 10;
        _map = new char[_height,_width];
    }

    public bool InBounds(Node id)
    {
        return 0 <= id.X && id.X < _width && 0 <= id.Y && id.Y < _height;
    }

    public bool IsReached(Node id)
    {
        return _map[id.Y, id.X] == '*';
    }

    public bool NoWall(Node id)
    {
        return !(_map[id.Y, id.X] == 'W'|| _map[id.Y, id.X] == '\0' || _map[id.Y, id.X] == '.');
    }

    public bool InPView(Node id, Node currentNode)
    {
        return currentNode.X - 5 <= id.X && id.X < currentNode.X + 5 && currentNode.Y - 5 <= id.Y && id.Y < currentNode.Y + 5;
    }

    public void PrintStaticMap(Node currentNode)
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (ReachedNodes.Contains(new Node(x,y)))
                    _map[y, x] = '*';

        _map[currentNode.Y, currentNode.X] = 'P';


        StringBuilder stringBuilder = new StringBuilder();
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                switch (_map[y, x])
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
                        stringBuilder.Append(_map[y, x]);
                        break;
                }
            }

            stringBuilder.Append('\n');
        }

        Console.WriteLine(stringBuilder.ToString());
    }

    public void PrintDynamicMap(Node currentNode)
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (ReachedNodes.Contains(new Node(x, y)))
                    _map[y, x] = '*';

        _map[currentNode!.Y, currentNode.X] = 'P';


        int width = Console.BufferWidth;
        int height = Console.WindowHeight - 3;

        StringBuilder stringBuilder = new StringBuilder();
        for (int h = currentNode.Y - height / 2; h < currentNode.Y + height / 2; h++)
        {
            for (int w = currentNode.X - width / 2; w < currentNode.X + width / 2; w++)
            {
                if (h < _height && w < _width && h > 0 && w > 0)
                    switch (_map[h, w])
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
                            stringBuilder.Append(_map[h, w]);
                            break;
                    }
                else
                    stringBuilder.Append('░');
            }

            stringBuilder.Append('\n');
        }

        Console.WriteLine(stringBuilder.ToString());
    }

}
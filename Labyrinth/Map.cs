using System.Text;

namespace Labyrinth
{
    class Map
    {
        public readonly HashSet<Node> ReachedNodes = new HashSet<Node>();

        public readonly Node[] DIRS = [new Node(1, 0), new Node(0, 1), new Node(-1, 0), new Node(0, -1)];

        private int _width, _height;

        public readonly char[,] MapGrid;

        public Node? _currentNode;

        public Map(int width, int height)
        {
            _width = width;
            _height = height;
            MapGrid = new char[height, height];
        }


        public bool InBounds(Node node)
        {
            return 0 <= node.X && node.X < _width && 0 <= node.Y && node.Y < _height;
        }

        public bool IsReached(Node node)
        {
            return MapGrid[node.Y, node.X] == '*';
        }

        public bool NoWall(Node node)
        {
            return !(MapGrid[node.Y, node.X] == 'W' || MapGrid[node.Y, node.X] == '\0' || MapGrid[node.Y, node.X] == '.');
        }

        public bool InPView(Node node, Node currentNode)
        {
            return currentNode.X - 5 < node.X && node.X < currentNode.X + 5 && currentNode.Y - 5 < node.Y && node.Y < currentNode.Y + 5;
        }

        public void UpdateCurrentNode(ServerResponse responseCoordinates)
        {
            if (responseCoordinates is null)
                throw new ArgumentNullException("_coordinates can't be null. Disconnected?");

            int x = 5 + int.Parse(responseCoordinates.Message.Substring(responseCoordinates.Message.IndexOf("X:") + 2, responseCoordinates.Message.IndexOf(";Y") - responseCoordinates.Message.IndexOf("X:") - 2));
            int y = 5 + int.Parse(responseCoordinates.Message.Substring(responseCoordinates.Message.IndexOf("Y:") + 2, responseCoordinates.Message.IndexOf(";Z") - responseCoordinates.Message.IndexOf("Y:") - 2));

            Console.WriteLine($"X: {x} Y: {y}");
            _currentNode = new Node(x, y);
        }

        public void UpdateMap(ServerResponse[] responseMap)
        {
            if (_currentNode is null)
                throw new NullReferenceException("_currentNode can't be null. Disconnected?");

            for (int y = 0; y < 11; y++)
                for (int x = 0; x < 11; x++)
                    MapGrid[_currentNode.Y + y - 5, _currentNode.X + x - 5] = responseMap![y].Message[x];
        }

        public void PrintMap()
        {
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    if (ReachedNodes.Contains(new Node(x, y)))
                        MapGrid[y, x] = '*';

            MapGrid[_currentNode!.Y, _currentNode.X] = 'P';


            int width = Console.BufferWidth;
            int height = Console.WindowHeight - 3;

            StringBuilder stringBuilder = new StringBuilder();
            for (int h = _currentNode.Y - height / 2; h < _currentNode.Y + height / 2; h++)
            {
                for (int w = _currentNode.X - width / 2; w < _currentNode.X + width / 2; w++)
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

    }
}

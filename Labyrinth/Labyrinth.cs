namespace Labyrinth;

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

class Labyrinth
{
    private readonly int _width;
    private readonly int _height;
    private bool tInRange = false;// atm not settet

    private readonly TcpClient _client;

    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    private readonly char[,] _mapArray;
    private readonly bool[,] _reachedMapAreas;

    public Labyrinth(int width, int height)
    {
        _width = width;
        _height = height;

        _client = new TcpClient("labyrinth.ctrl-s.de", 50000);

        _reader = new StreamReader(_client.GetStream());
        _writer = new StreamWriter(_client.GetStream());

        _client.NoDelay = true;
        _writer.AutoFlush = false;


        _mapArray = new char[width + 10, height + 10];
        _reachedMapAreas = new bool[width + 10, height + 10];

        InitializeGame();

    }

    private void InitializeGame()
    {

        _writer.WriteLine($"WIDTH {_width}\nHEIGHT {_height}\nDEPTH 1\nSTART");
        _writer.Flush();

        ServerResponse response;
        do
        {
            response = ServerResponse.ParseResponse(_reader.ReadLine());
        }
        while (response.Message != "READY.");

        Enter(); // To get a Coordinate-Message
        Print(); // To get a Map-Message

        GetServerResponse();
        ProcessResponse();
    }

    public void GameLoop()
    {
        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            if (tInRange)
            {
                Location TLocation = GetTarget();
                List<(int x, int y)> Path2 = Pathfinding(TLocation);
                while (PathFilter(Path2)) ;
                Queue<string> QueueToT = ParseCordsToDirs(Path2);
                SendBotCommands(QueueToT);

                GetServerResponse();
                break;
            }


            Location Target = GetTarget();

            List<(int x, int y)> Path = Pathfinding(Target);
            while (PathFilter(Path)) ;
            Queue<string> DirectionStringList = ParseCordsToDirs(Path);

            SendBotCommands(DirectionStringList);

            GetServerResponse();

            ProcessResponse();
        }
    }


    #region Commands
    private void SendManualCommand(string KeyToString)
    {
        switch (KeyToString)
        {
            case "Right":
                Right();
                Print();
                break;
            case "Down":
                Down();
                Print();
                break;
            case "Left":
                Left();
                Print();
                break;
            case "Up":
                Up();
                Print();
                break;
            case "Enter":
                Enter();
                break;
            default:
                throw new Exception("no Valid command found");
        }
    }

    private void SendBotCommands(Queue<string> commands)
    {
        int count = commands.Count;

        if (count < 1)
            throw new Exception("The overgiven Queue \"paths\" is empty");

        foreach (string cmd in commands)
        {
            switch (cmd)
            {
                case "RIGHT":
                    Right();
                    break;
                case "DOWN":
                    Down();
                    break;
                case "LEFT":
                    Left();
                    break;
                case "UP":
                    Up();
                    break;
                case "ENTER":
                    Enter();
                    break;
                default:
                    throw new Exception("no valid command found");
            }
        }

        Print();
        ServerResponse? response;

        while (!(count == 1))
        {
            response = ServerResponse.ParseResponse(_reader.ReadLine());
            count--;
        }

    }

    private void Right()
    {
        _writer.WriteLine("RIGHT");
        _writer.Flush();
    }

    private void Down()
    {
        _writer.WriteLine("DOWN");
        _writer.Flush();
    }

    private void Left()
    {
        _writer.WriteLine("LEFT");
        _writer.Flush();
    }

    private void Up()
    {
        _writer.WriteLine("UP");
        _writer.Flush();
    }

    private void Enter()
    {
        _writer.WriteLine("ENTER");
        _writer.Flush();
    }

    private void Print()
    {
        _writer.WriteLine("PRINT");
        _writer.Flush();
    }
    #endregion


    #region Get Server Response
    private ServerResponse? _coordinatesMesssage;
    public readonly ServerResponse[]? _mapMassageArray = new ServerResponse[11];

    private void GetServerResponse()
    {
        _reader.ReadLine(); // we dont need the first line, so we read it just out without save.

        GetCoordinateMessage();

        GetMapMessages();
    }

    private void GetCoordinateMessage()
    {
        _coordinatesMesssage = ServerResponse.ParseResponse(_reader.ReadLine());
        if (_coordinatesMesssage is null)
            throw new NullReferenceException("coordinatesMesssage can't be null. Disconnected?");
    }

    private void GetMapMessages()
    {
        if (_mapMassageArray is null)
            throw new NullReferenceException("mapMassageArray can't be null. Disconnected?");

        for (int i = 0; i < _mapMassageArray.Length; i++)
        {
            _mapMassageArray[i] = ServerResponse.ParseResponse(_reader.ReadLine());
            if (_mapMassageArray[i] is null)
                throw new NullReferenceException("mapMassageArray[{i}] can't be null. Disconnected?");
        }
    }
    #endregion


    #region Process Server Response
    private Location? _currentPosition;
    private bool _gameWon;

    private void ProcessResponse()
    {
        UpdateCoordinates();

        UpdateMap();

        PrintMap();

        UpdateCoordinates(); // We call this Method a second time to get the informations from map to(infos like 'W')

        UpdateGraph();
    }

    private void UpdateCoordinates()
    {
        if (_coordinatesMesssage.Message is null)
            throw new NullReferenceException("coordinatesMesssage.Message can't be null. Disconnected?");


        int x = 5 + int.Parse(_coordinatesMesssage.Message.Substring(_coordinatesMesssage.Message.IndexOf("X:") + 2, _coordinatesMesssage.Message.IndexOf(";Y") - _coordinatesMesssage.Message.IndexOf("X:") - 2));
        int y = 5 + int.Parse(_coordinatesMesssage.Message.Substring(_coordinatesMesssage.Message.IndexOf("Y:") + 2, _coordinatesMesssage.Message.IndexOf(";Z") - _coordinatesMesssage.Message.IndexOf("Y:") - 2));

        _currentPosition = new Location(x, y, _mapArray);
        Console.WriteLine($"X: {x} Y: {y}");

    }

    private void UpdateMap()
    {
        if (_currentPosition is null || _mapMassageArray is null)
            throw new NullReferenceException("_currentPosition or _mapMassageArray can't be null. Disconnected?");

        for (int y = 0; y < 11; y++)
            for (int x = 0; x < 11; x++)
                _mapArray[_currentPosition.Y + y - 5, _currentPosition.X + x - 5] = _mapMassageArray[y].Message[x];

    }

    private void PrintMap()
    {
        //Just for debug//
        for (int y = 0; y < _height +10 ; y++)
            for (int x = 0; x < _width +10; x++)
                if (_reachedMapAreas[y, x] is true)
                    _mapArray[y, x] = '*';

        if (_currentPosition is null)
            throw new NullReferenceException("_currentPosition can't be null.");

        _mapArray[_currentPosition.Y, _currentPosition.X] = 'P';

        StringBuilder stringBuilder = new StringBuilder();
        for (int y = 0; y < _height + 10; y++)
        {
            for (int x = 0; x < _width + 10; x++)
            {
                switch (_mapArray[y, x])
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
                        stringBuilder.Append(_mapArray[y, x]);
                        break;
                }
            }
            stringBuilder.Append('\n');
        }
        Console.WriteLine(stringBuilder.ToString());
        stringBuilder.Clear();
    }

    #endregion


    #region Bot Algorithm
    private readonly Dictionary<(int, int), Location> _graph = new Dictionary<(int, int), Location>();
    public void UpdateGraph()
    {
        if (_currentPosition is null)
            throw new NullReferenceException("_currentPosition can't be null. Disconnected?");

        int currentX, currentY;

        for (int y = 0; y < 11; y++)
        {
            for (int x = 0; x < 11; x++)
            {
                currentX = _currentPosition.X + x - 5;
                currentY = _currentPosition.Y + y - 5;
                if (currentX < _width + 10 && currentY < _height + 10 && currentX > 0 && currentY > 0)
                {
                    if (_mapArray[currentY, currentX] is not 'W' && _mapArray[currentY, currentX] is not '\0' && _mapArray[currentY, currentX] is not '.')
                        _graph[(currentX, currentY)] = new Location(currentX, currentY, _mapArray);
                }
            }
        }
    }

    /// <summary>
    /// This method iterates along the edge of the field of view once and returns the first empty field found at the edge.
    /// </summary>
    private Location GetTarget()
    {
        if (_currentPosition is null)
            throw new NullReferenceException("_currentPosition can't be null. Disconnected?");

        // + 5 to start in corner botton right (Rechts unten gewichtet)
        int searchPointX = _currentPosition.X + 5;
        int searchPointY = _currentPosition.Y + 5;

        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)) && _reachedMapAreas[searchPointY, searchPointX] is not true)
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointX--;
        }

        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)) && _reachedMapAreas[searchPointY, searchPointX] is not true)
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointY--;
        }

        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)) && _reachedMapAreas[searchPointY, searchPointX] is not true)
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointX++;
        }

        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)) && _reachedMapAreas[searchPointY, searchPointX] is not true)
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointY++;
        }



        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)))
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointX--;
        }

        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)))
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointY--;
        }

        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)))
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointX++;
        }

        for (int i = 0; i < 10; i++)
        {
            if (_graph.ContainsKey((searchPointX, searchPointY)) && IsReachable(new Location(searchPointX, searchPointY, _mapArray)))
                return new Location(searchPointX, searchPointY, _mapArray);

            searchPointY++;
        }


        throw new Exception("No Target found");
    }

    /// <summary>
    /// This method helps getTarget determine if it is reachable.
    /// </summary>
    private bool IsReachable(Location current)
    {
        HashSet<(int X, int Y)> reached = new HashSet<(int X, int Y)>();
        Queue<Location> queue = new Queue<Location>();

        queue.Enqueue(current);

        while (queue.Count > 0)
        {
            current = queue.Dequeue();
            reached.Add((current.X, current.Y));

            for (int i = 0; i < 4; i++)
                if (current.Neighbors[i].IsReachable && !reached.Contains((current.Neighbors[i].X, current.Neighbors[i].Y)))
                {
                    queue.Enqueue(new Location(current.Neighbors[i].X, current.Neighbors[i].Y, _mapArray));
                    reached.Add((current.Neighbors[i].X, current.Neighbors[i].Y));
                }
            if (_mapArray[current.Y, current.X] is 'P')
                return true;
        }
        return false;
    }


    /// <summary>
    /// This pathfinding algorithm method returns a queue with the (int X, int Y) from the start point to the target point.
    /// </summary>
    public List<(int X, int Y)> Pathfinding(Location target)
    {
        HashSet<(int X, int Y)> reached = new HashSet<(int X, int Y)>();
        LinkedList<(int X, int Y)> breadkrumel = new LinkedList<(int X, int Y)>();
        List<(int X, int Y)> path = new List<(int X, int Y)>();
        Queue<Location> queue = new Queue<Location>();

        Location current = new Location(_currentPosition.X, _currentPosition.Y, _mapArray);
        queue.Enqueue(current);
        path.Add((current.X, current.Y));
        breadkrumel.AddFirst((current.X, current.Y));

        while ((current.X, current.Y) != (target.X, target.Y))
        {
            current = queue.Dequeue();
            reached.Add((current.X, current.Y));

            for (int i = 0; i < 4; i++)
                if (!reached.Contains((current.Neighbors[i].X, current.Neighbors[i].Y)) && current.Neighbors[i].IsReachable)
                {
                    breadkrumel.AddFirst((current.Neighbors[i].X, current.Neighbors[i].Y));
                    queue.Enqueue(new Location(current.Neighbors[i].X, current.Neighbors[i].Y, _mapArray));
                    path.Add((current.Neighbors[i].X, current.Neighbors[i].Y));
                    break;
                }

            if (queue.Count == 0)
            {
                (int X, int Y) = breadkrumel.First.Next.Value;
                breadkrumel.RemoveFirst();
                queue.Enqueue(new Location(X, Y, _mapArray));
                path.Add((X, Y));
            }
        }
        return path;
    }

    public bool PathFilter(List<(int x, int y)> path)
    {
        List<(int index, int x, int y)> indexes = new List<(int index, int x, int y)>();
        List<(int index, int x, int y)> indexes2 = new List<(int index, int x, int y)>();

        // whit this loop, we search doublicates;
        for (int i = 0; i < path.Count; i++)
            for (int j = 1 + i; j < path.Count; j++)
                if (path[i] == path[j])
                    indexes.Add((i, path[i].x, path[i].y));

        // with this loop, we search how much doublicates ever doublicate has.
        for (int i = 0; i < indexes.Count; i++)
            for (int j = 0; j < path.Count; j++)
                if (path[j] == (indexes[i].x, indexes[i].y))
                    indexes2.Add((j, path[j].x, path[j].y));

        if (indexes2.Count <= 0)
            return false;

        path.RemoveRange(indexes2[0].index, indexes2[1].index - indexes2[0].index);

        return true;
    }

    /// <summary>
    /// This methode is to parse the result of the pathfinding method in strings for the responses.
    /// </summary>
    private Queue<string> ParseCordsToDirs(List<(int X, int Y)> path)
    {
        string[] _dirs = ["RIGHT", "DOWN", "LEFT", "UP"];
        Queue<string> dirs = new Queue<string>();
        (int x, int y) current = path[0];
        path.Remove(current);

        while (path.Count > 0)
        {
            (int x, int y) next = path[0];
            path.Remove(next);
            for (int i = 0; i < 4; i++)
            {
                if (current.x + Direction.DirX[i] == next.x && current.y + Direction.DirY[i] == next.y)
                {
                    _reachedMapAreas[next.y, next.x] = true;
                    dirs.Enqueue(_dirs[i]);
                    current = next;
                }
            }
        }
        return dirs;
    }

    #endregion

}
//"DOWN" = Y++;
//"Right = X++;
//"LEFT" = X--;
//"UP" = Y--;


///// <summary>
///// In this method, we use the Euclidean distance formula to get the heuristic from the start point to the target point.
///// </summary>
//private double GetHeristic(Location current, Location target)
//{
//    return Math.Sqrt(Math.Pow(current.X - target.X, 2) + Math.Pow(current.Y - target.Y, 2));
//}
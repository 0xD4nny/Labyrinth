namespace Labyrinth;

using System;
using System.Collections.Generic;
using System.Net.Sockets;

class Labyrinth
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    private readonly int _width, _height;

    private bool _gameWon;

    private Grid _grid;
    private readonly SearchTargets _getTargets;
    private readonly AStar _algorithm;

    public Labyrinth(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new Grid(_width, _height);
        _algorithm = new AStar(_grid);
        _getTargets = new SearchTargets(_grid);

        _client = new TcpClient("labyrinth.ctrl-s.de", 50000);
        _reader = new StreamReader(_client.GetStream());
        _writer = new StreamWriter(_client.GetStream());
        _client.NoDelay = true;
        _writer.AutoFlush = false;

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

        GetResponse();

        UpdateCurrentNode();
        UpdateMap();

        _grid.PrintDynamicMap(_currentNode);
    }

    private void GetWinnerMessage()
    {
        Enter(); // If we come to this point, we sit on 'T', thats why we send enter here to the server. To get the last message and end the game.
        ServerResponse? response = ServerResponse.ParseResponse(_reader.ReadLine());
        Console.WriteLine(response.Message);
    }

    public void GameLoop()
    {
        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            _getTargets.Run(_currentNode, ref _gameWon);
            Node goal = _getTargets.Targets.Pop(); // needs fix!
            List<Node> bestPath = _algorithm.Run(_currentNode, goal);

            List<string> commands = ParseNodeToDirString(bestPath);
            SendCommands(commands);

            GetResponse();

            UpdateCurrentNode();
            UpdateMap();

            _grid.PrintDynamicMap(_currentNode);
        }
        
        GetWinnerMessage();
    }


    #region Commands
    /// <summary>
    /// This method sends a list of commands to the server and then prints the updated map on the screen.
    /// After this, it reads out every response for each command until the last one
    /// </summary>
    private void SendCommands(List<string> commands)
    {
        int count = commands.Count;

        if (count < 1)
            throw new Exception("The overgiven list \"commands\" is empty");

        foreach (string cmd in commands)
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

        Print();

        while (!(count-- == 1))
            _reader.ReadLine();
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

    /// <summary>
    ///You can use the KeyToString method as an argument, to play the game with the Arrow Keys manually.    
    ///</summary>
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
    private string KeyToString()
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        switch (keyInfo.Key)
        {
            case ConsoleKey.RightArrow:
                return "Right";
            case ConsoleKey.DownArrow:
                return "Down";
            case ConsoleKey.LeftArrow:
                return "Left";
            case ConsoleKey.UpArrow:
                return "Up";
            case ConsoleKey.E:
                return "Enter";
            default:
                throw new Exception("Wrong Key!");
        }
    }
    #endregion


    private ServerResponse? _coordinatesResponse;
    private ServerResponse[]? _mapResponse = new ServerResponse[11];
    private void GetResponse()
    {
        _reader.ReadLine(); // we dont need the first line, so we read it just out.

        _coordinatesResponse = ServerResponse.ParseResponse(_reader.ReadLine());

        for (int i = 0; i < _mapResponse!.Length; i++)
            _mapResponse[i] = ServerResponse.ParseResponse(_reader.ReadLine());
    }

    private Node _currentNode;
    private void UpdateCurrentNode()
    {
        if (_coordinatesResponse is null)
            throw new NullReferenceException("_coordinates can't be null. Disconnected?");

        int x = 5 + int.Parse(_coordinatesResponse.Message.Substring(_coordinatesResponse.Message.IndexOf("X:") + 2, _coordinatesResponse.Message.IndexOf(";Y") - _coordinatesResponse.Message.IndexOf("X:") - 2));
        int y = 5 + int.Parse(_coordinatesResponse.Message.Substring(_coordinatesResponse.Message.IndexOf("Y:") + 2, _coordinatesResponse.Message.IndexOf(";Z") - _coordinatesResponse.Message.IndexOf("Y:") - 2));

        _currentNode = new Node(x, y);
        Console.WriteLine($"X: {x} Y: {y}");
    }
    private void UpdateMap()
    {
        for (int y = 0; y < 11; y++)
            for (int x = 0; x < 11; x++)
                _grid._map[_currentNode.Y + y - 5, _currentNode.X + x - 5] = _mapResponse![y].Message[x];
    }

    /// <summary>
    /// This methode is to parse the result of the pathfinding method in strings for the responses.
    /// </summary>
    private List<string> ParseNodeToDirString(List<Node> path)
    {
        string[] _dirs = ["RIGHT", "DOWN", "LEFT", "UP"];
        List<string> dirs = new List<string>();
        Node current = path[0];

        path.Remove(current);

        while (path.Count > 0)
        {
            Node next = path[0];
            path.Remove(next);
            for (int i = 0; i < 4; i++)
            {
                if (current.X + _grid.DIRS[i].X == next.X && current.Y + _grid.DIRS[i].Y == next.Y)
                {
                    dirs.Add(_dirs[i]);
                    current = next;
                }
            }
        }
        return dirs;
    }

}
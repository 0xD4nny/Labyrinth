namespace Labyrinth;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

class Labyrinth
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    private readonly int _width, _height;
    private readonly char[,] _mapArray;
    private readonly bool[,] _reachedLocations;

    private readonly BotAlgorithm _botAlgorithm;
    private bool _gameWon;

    public Labyrinth(int width, int height)
    {
        _width = width;
        _height = height;

        _mapArray = new char[height + 10, width + 10];
        _reachedLocations = new bool[height + 10, width + 10];

        _client = new TcpClient("labyrinth.ctrl-s.de", 50000);
        _reader = new StreamReader(_client.GetStream());
        _writer = new StreamWriter(_client.GetStream());
        _client.NoDelay = true;
        _writer.AutoFlush = false;

        _botAlgorithm = new BotAlgorithm();

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

        ProcessResponse();
    }

    public void GameLoop()
    {
        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            SendCommands(_botAlgorithm.Run(_currentLocation!, _mapArray, _reachedLocations, ref _gameWon));

            GetResponse();

            ProcessResponse();
        }

        //the following three lines are the winningprocess. The game and the program ends after this process.
        Enter();
        ServerResponse? response = ServerResponse.ParseResponse(_reader.ReadLine());
        Console.WriteLine(response.Message);
    }

    public void Benchmark()
    {
        Stopwatch stopwatch = new Stopwatch();
        (long console, long botAlgorithm, long getResponse, long processResponse) times = (0, 0, 0, 0);

        while (!_gameWon)
        {
            stopwatch.Start();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            stopwatch.Stop();
            times.console += stopwatch.ElapsedTicks;
            stopwatch.Reset();

            stopwatch.Start();
            SendCommands(_botAlgorithm.Run(_currentLocation!, _mapArray, _reachedLocations, ref _gameWon));
            stopwatch.Stop();
            times.botAlgorithm += stopwatch.ElapsedTicks;
            stopwatch.Reset();

            stopwatch.Start();
            GetResponse();
            stopwatch.Stop();
            times.getResponse += stopwatch.ElapsedTicks;
            stopwatch.Reset();

            stopwatch.Start();
            ProcessResponse();
            stopwatch.Stop();
            times.processResponse += stopwatch.ElapsedTicks;
            stopwatch.Reset();
        }

        Enter();
        ServerResponse? response = ServerResponse.ParseResponse(_reader.ReadLine());
        Console.WriteLine(response.Message);
        Console.WriteLine($"ConsoleSettings:\t{times.console/1000}ms\nAlgorithm:\t\t{times.botAlgorithm/1000}ms\nGetResponse:\t\t{times.getResponse/1000}ms\nProcessResponse:\t{times.processResponse/1000}ms\n");
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
            ServerResponse.ParseResponse(_reader.ReadLine());

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


    #region Get Server Response
    private ServerResponse? _coordinatesResponse;
    private ServerResponse[]? _mapResponse = new ServerResponse[11];

    private void GetResponse()
    {
        _reader.ReadLine(); // we dont need the first line, so we read it just out, without saveing.

        _coordinatesResponse = ServerResponse.ParseResponse(_reader.ReadLine());

        for (int i = 0; i < _mapResponse!.Length; i++)
            _mapResponse[i] = ServerResponse.ParseResponse(_reader.ReadLine());
    }
    #endregion


    #region Process Server Response
    private Location? _currentLocation;
    private void ProcessResponse()
    {
        UpdateCurrentLocation();
        UpdateMap();
        UpdateGraph();

        PrintStaticMap();
        //PrintDynamicMap();
    }

    private void UpdateCurrentLocation()
    {
        if (_coordinatesResponse is null)
            throw new NullReferenceException("_coordinates can't be null. Disconnected?");

        int x = 5 + int.Parse(_coordinatesResponse.Message.Substring(_coordinatesResponse.Message.IndexOf("X:") + 2, _coordinatesResponse.Message.IndexOf(";Y") - _coordinatesResponse.Message.IndexOf("X:") - 2));
        int y = 5 + int.Parse(_coordinatesResponse.Message.Substring(_coordinatesResponse.Message.IndexOf("Y:") + 2, _coordinatesResponse.Message.IndexOf(";Z") - _coordinatesResponse.Message.IndexOf("Y:") - 2));

        _currentLocation = new Location(x, y, _mapArray);
        //Console.WriteLine($"X: {x} Y: {y}");
    }

    private void UpdateMap()
    {
        if (_currentLocation is null)
            throw new NullReferenceException("_currentLocation can't be null. Disconnected?");

        for (int y = 0; y < 11; y++)
            for (int x = 0; x < 11; x++)
                _mapArray[_currentLocation!.Y + y - 5, _currentLocation.X + x - 5] = _mapResponse![y].Message[x];

    }

    private void UpdateGraph()
    {
        if (_currentLocation is null)
            throw new NullReferenceException("_currentLocation can't be null. Disconnected?");

        int currentX, currentY;

        for (int y = 0; y < 11; y++)
            for (int x = 0; x < 11; x++)
            {
                currentX = _currentLocation.X + x - 5;
                currentY = _currentLocation.Y + y - 5;
                if (!_botAlgorithm.Graph.ContainsKey((currentX, currentY)) &&
                    currentX < _width + 10 && currentY < _height + 10 && currentX > 0 && currentY > 0 &&
                    _mapArray[currentY, currentX] is not 'W' && _mapArray[currentY, currentX] is not '\0' && _mapArray[currentY, currentX] is not '.')
                {
                    _botAlgorithm.Graph[(currentX, currentY)] = new Location(currentX, currentY, _mapArray);
                }
            }
    }

    private void PrintStaticMap()
    {
        //Just for debug//
        for (int y = 0; y < _height + 10; y++)
            for (int x = 0; x < _width + 10; x++)
                if (_reachedLocations[y, x] is true)
                    _mapArray[y, x] = '*';


        _mapArray[_currentLocation!.Y, _currentLocation.X] = 'P';

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
    }

    private void PrintDynamicMap()
    {
        //Just for debug//
        for (int y = 0; y < _height + 10; y++)
            for (int x = 0; x < _width + 10; x++)
                if (_reachedLocations[y, x] is true)
                    _mapArray[y, x] = '*';

        _mapArray[_currentLocation!.Y, _currentLocation.X] = 'P';

        int width = Console.BufferWidth;
        int height = Console.BufferHeight - 2;

        StringBuilder stringBuilder = new StringBuilder();
        for (int h = _currentLocation.Y - height / 2; h < _currentLocation.Y + height / 2; h++)
        {
            for (int w = _currentLocation.X - width / 2; w < _currentLocation.X + width / 2; w++)
            {
                if (h < _height + 10 && w < _width + 10 && h > 0 && w > 0)
                    switch (_mapArray[h, w])
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
                            stringBuilder.Append(_mapArray[h, w]);
                            break;
                    }
                else
                    stringBuilder.Append('░');
            }

            stringBuilder.Append('\n');
        }

        Console.WriteLine(stringBuilder.ToString());
    }
    #endregion

}
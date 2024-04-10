namespace Labyrinth;

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

class Labyrinth
{
    private readonly int _width;
    private readonly int _height;

    private readonly TcpClient _client;

    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    private readonly char[,] _mapArray;
    private readonly bool[,] _reachedLocations;

    private readonly BotAlgorithm _botAlgorithm;

    public Labyrinth(int width, int height)
    {
        _width = width;
        _height = height;

        _client = new TcpClient("labyrinth.ctrl-s.de", 50000);

        _reader = new StreamReader(_client.GetStream());
        _writer = new StreamWriter(_client.GetStream());

        _client.NoDelay = true;
        _writer.AutoFlush = false;


        _mapArray = new char[height + 10, width + 10];
        _reachedLocations = new bool[height + 10, width + 10];


        InitializeGame();

        if (_currentPosition is null)
            throw new NullReferenceException("_currentPossition can't be null.");

        _botAlgorithm = new BotAlgorithm();
        UpdateGraph();

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
        UpdateCoordinates();

        UpdateMap();

        PrintMap();

        UpdateCoordinates(); // We call this Method a second time to get the informations from map to(infos like 'W') 
    }

    public void GameLoop()
    {
        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            SendBotCommands(_botAlgorithm.Run(_currentPosition, _mapArray, _reachedLocations, _gameWon));

            GetServerResponse();

            ProcessResponse();
        }

        Enter();
        ServerResponse? response;
        response = ServerResponse.ParseResponse(_reader.ReadLine());
        Console.WriteLine(response.Message);
    }


    #region Commands
    /// <summary>
    ///You can use the KeyToString method of the ManualControl class as an argument, to play the game with the Arrow Keys manually.    
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

    private void SendBotCommands(List<string> commands)
    {
        int count = commands.Count;

        if (count < 1)
            throw new Exception("The overgiven list \"commands\" is empty");

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
    private readonly ServerResponse[]? _mapMassageArray = new ServerResponse[11];

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
        for (int y = 0; y < _height + 10; y++)
                for (int x = 0; x < _width + 10; x++)
                    if (_reachedLocations[y, x] is true)
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
                        _botAlgorithm.Graph[(currentX, currentY)] = new Location(currentX, currentY, _mapArray);
                }
            }
        }
    }
    #endregion


}
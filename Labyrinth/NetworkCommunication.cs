using System.Net.Sockets;

namespace Labyrinth;
/// <summary>
/// Manages the network communication with the server and handles the commands.
/// </summary>
class NetworkCommunication
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    private ServerResponse[]? _mapResponse = new ServerResponse[11];

    private readonly int _width, _height;

    public NetworkCommunication(int width, int height)
    {
        _client = new TcpClient("labyrinth.ctrl-s.de", 50000);
        _reader = new StreamReader(_client.GetStream());
        _writer = new StreamWriter(_client.GetStream());

        _client.NoDelay = true;
        _writer.AutoFlush = false;

        _width = width;
        _height = height;

        InizializeCommunikation();
    }


    /// <summary>
    /// This method configer all important settings and reads all messages, allowing us to directly play the game.
    /// </summary>
    private void InizializeCommunikation()
    {
        _writer.WriteLine($"WIDTH {_width}\nHEIGHT {_height}\nDEPTH 1\nSTART");
        _writer.Flush();

        ServerResponse? response;
        do
        {
            response = ServerResponse.ParseResponse(_reader.ReadLine());
        }
        while (response.Message != "READY.");

        Enter(); // To get a Coordinate-Message
        Print(); // To get a Map-Message

    }

    /// <summary>
    /// This methods reads out the player coordinates and stores it in our "ServerResponse-Class".
    /// </summary>
    public ServerResponse GetCoordinateResponse()
    {
        _reader.ReadLine(); // we dont need the first line, so we read it just out.
        return ServerResponse.ParseResponse(_reader.ReadLine());
    }

    /// <summary>
    /// This methods collects the Labyrinth-Map and stores it in our "ServerResponse-Class"-Type-Array.
    /// </summary>
    public ServerResponse[] GetMapResponse()
    {
        for (int i = 0; i < _mapResponse!.Length; i++)
            _mapResponse[i] = ServerResponse.ParseResponse(_reader.ReadLine());

        return _mapResponse;
    }

    /// <summary>
    /// This method is used when we find 'T' on the map and sit on it, to send the last command and read the final message with the winning text.
    /// </summary>
    public void GetWinnerMessage()
    {
        Enter();
        ServerResponse? response = ServerResponse.ParseResponse(_reader.ReadLine());
        Console.WriteLine(response.Message);
    }


    #region Commands
    /// <summary>
    /// This method is to parse a list of coordinates to a list of directions strings like "UP" and "DOWN".
    /// </summary>
    private List<string> ParseNodeToCommand(List<Node> path, Map map)
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
                if (current.X + map.DIRS[i].X == next.X && current.Y + map.DIRS[i].Y == next.Y)
                {
                    dirs.Add(_dirs[i]);
                    current = next;
                }
            }
        }
        return dirs;
    }

    /// <summary>
    /// This method sends a list of commands to the server and then prints the updated map on the screen.
    /// After this, it reads out every response for each command until the last one
    /// </summary>
    public void SendCommands(List<Node> path, Map map)
    {
        List<string> commands = ParseNodeToCommand(path, map);
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
    #endregion

}

using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Labyrinth;

/// <summary>
/// Manages the network communication with the server and handles the commands.
/// </summary>
class Network
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    private ServerResponse[]? _mapResponse = new ServerResponse[11];

    private readonly int _width, _height;

    public Network(int width, int height)
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

        _writer.WriteLine("ENTER");
        _writer.Flush();
        _writer.WriteLine("PRINT");
        _writer.Flush();

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
        _writer.WriteLine("ENTER");
        _writer.Flush();
        ServerResponse? response = ServerResponse.ParseResponse(_reader.ReadLine());
        Console.WriteLine(response.Message);
    }

    /// <summary>
    /// This method is to parse a list of coordinates to a list of directions strings like "UP" and "DOWN".
    /// </summary>
    private List<string> ParseNodeToCommand(List<Node> nodePath, Map map)
    {
        string[] _dirs = ["RIGHT", "DOWN", "LEFT", "UP"];
        List<string> stringPath = new List<string>();
        Node current = nodePath[0];

        nodePath.Remove(current);

        while (nodePath.Count > 0)
        {
            Node next = nodePath[0];
            nodePath.Remove(next);
            for (int i = 0; i < 4; i++)
            {
                if (current.X + map.DIRS[i].X == next.X && current.Y + map.DIRS[i].Y == next.Y)
                {
                    map.ReachedNodes.Add(next);
                    stringPath.Add(_dirs[i]);
                    current = next;
                }
            }
        }
        return stringPath;
    }

    /// <summary>
    /// This method sends a list of commands to the server and then prints the updated map on the screen.
    /// After this, it reads out every response for each command until the last one
    /// </summary>
    public void SendCommands(List<Node> bestPath, Map map)
    {
        StringBuilder stringBuilder = new StringBuilder();

        List<string> commands = ParseNodeToCommand(bestPath, map);

        int count = commands.Count;

        if (count < 1)
            throw new Exception("The given list \"commands\" is empty");


        foreach (string cmd in commands)
            stringBuilder.AppendLine(cmd);

        stringBuilder.Append("PRINT");
        _writer.WriteLine(stringBuilder.ToString());
        _writer.Flush();
        

        while (count-- > 1)
            _reader.ReadLine();
    }

    public void BenchmarkSendCommands(List<Node> bestPath, Map map, ref long parseCoordsToCmd, ref long sendCmd, ref long readout)
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        StringBuilder stringBuilder = new StringBuilder();
        List<string> commands = ParseNodeToCommand(bestPath, map);
        int count = commands.Count;
        
        if (count < 1)
            throw new Exception("The overgiven list \"commands\" is empty");

        foreach (string cmd in commands)
            stringBuilder.AppendLine(cmd);
        
        stringBuilder.Append("PRINT");
        stopwatch.Stop();
        parseCoordsToCmd += stopwatch.ElapsedTicks;
        stopwatch.Reset();


        stopwatch.Start();
        _writer.WriteLine(stringBuilder.ToString());
        _writer.Flush();
        stopwatch.Stop();
        sendCmd += stopwatch.ElapsedTicks;
        stopwatch.Reset();

        stopwatch.Start();
        while (count-- > 1)
            _reader.ReadLine();
        stopwatch.Stop();
        readout += stopwatch.ElapsedTicks;
        stopwatch.Stop();

    }
}

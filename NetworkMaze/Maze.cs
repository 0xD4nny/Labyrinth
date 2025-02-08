namespace Labyrinth;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/// <summary>
/// This class manages most other classes needed to run the Labyrinth automatically. It uses the SearchNextTarget class to find all possible exits from the player's view and stores them in a stack.
/// Then, we pop a target from the stack and pass it to the pathfinding class, which provides the best path as a list.
/// /// </summary>
class Maze
{
    private bool _gameWon;

    private readonly int _width, _height;
    private readonly Map _map;

    private readonly Network _networkCommunication;
    
    private readonly SearchNextTarget _searchNextTarget;

    private readonly Pathfinding _aStar;

    public Maze(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(_width + 10, _height + 10);

        _networkCommunication = new Network(_width,_height);
        _searchNextTarget = new SearchNextTarget(_map);
        _aStar = new Pathfinding(_map);
    }


    public void InitializeGame()
    {
        _map.UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
        _map.UpdateMap(_networkCommunication.GetMapResponse());
        Console.WriteLine("Run has started...");
        _map.PrintMap();
    }

    public void GameLoop()
    {
        if (_map.CurrentNode is null)
            throw new NullReferenceException("_currentNode can't be null. Disconnected?");

        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            Node target = _searchNextTarget.GetTarget(_map.CurrentNode, ref _gameWon);
            List<Node> bestPath = _aStar.Run(_map.CurrentNode, target);
            _networkCommunication.SendCommands(bestPath, _map);
 
            _map.UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
            _map.UpdateMap(_networkCommunication.GetMapResponse());
            _map.PrintMap();
        }

        _networkCommunication.GetWinnerMessage();
    }

    public void BenchmarkGameLoop()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        int totalSendetCommands = 0, runningLoops = 0, floodFillUseCount = 0;
        long allTicks = 0, getTarget = 0, aStar = 0, sendCommands = 0, updateNode = 0, updateMap = 0;
        long allTicksGetTarget = 0, checkReachability = 0, collectTargets = 0, checkForUndefineNeigbor = 0, floodFillToNext = 0;
        long allTicksNetworkCommunikation = 0, parseCoordsToCmd = 0, sendCmd = 0, readout = 0;

        if (_map.CurrentNode is null)
            throw new NullReferenceException("_currentNode can't be null. Disconnected?");

        while (!_gameWon)
        {
            stopwatch.Start();
            Node target = _searchNextTarget.BenchmarkGetTarget(_map.CurrentNode, ref _gameWon, ref checkReachability, ref collectTargets, ref checkForUndefineNeigbor, ref floodFillToNext,ref floodFillUseCount);
            stopwatch.Stop();
            getTarget += stopwatch.ElapsedTicks;
            stopwatch.Reset();


            stopwatch.Start();
            List<Node> bestPath = _aStar.Run(_map.CurrentNode, target);
            stopwatch.Stop();
            aStar += stopwatch.ElapsedTicks;
            stopwatch.Reset();

            totalSendetCommands += bestPath.Count;

            stopwatch.Start();
            _networkCommunication.BenchmarkSendCommands(bestPath, _map, ref parseCoordsToCmd, ref sendCmd, ref readout);
            stopwatch.Stop();
            sendCommands += stopwatch.ElapsedTicks;
            stopwatch.Reset();


            stopwatch.Start();
            _map.UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
            stopwatch.Stop();
            updateNode += stopwatch.ElapsedTicks;
            stopwatch.Reset();


            stopwatch.Start();
            _map.UpdateMap(_networkCommunication.GetMapResponse());
            stopwatch.Stop();
            updateMap += stopwatch.ElapsedTicks;
            stopwatch.Reset();


            runningLoops++;
        }

        _networkCommunication.GetWinnerMessage();

        allTicks = getTarget + aStar + sendCommands + updateNode + updateMap;
        allTicksGetTarget = checkReachability + collectTargets + checkForUndefineNeigbor + floodFillToNext;
        allTicksNetworkCommunikation = parseCoordsToCmd + sendCmd + readout;
        Console.WriteLine();
        Console.WriteLine($"Total Time:\t\t{allTicks / 10_000,10} ms");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"Get Target:\t\t{getTarget / 10_000,10} ms \t {GetPercent(allTicks, getTarget),0:F2}%");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"└─ Check Reachability:\t{checkReachability / 10_000,10} ms \t {GetPercent(allTicksGetTarget,checkReachability),0:F2}%");
        Console.WriteLine($"└─ Collect Targets:\t{collectTargets / 10_000,10} ms \t {GetPercent(allTicksGetTarget, collectTargets),0:F2}%");
        Console.WriteLine($"└─ Check for undefine:\t{checkForUndefineNeigbor / 10_000,10} ms \t {GetPercent(allTicksGetTarget, checkForUndefineNeigbor),0:F2}%");
        Console.WriteLine($"└─ Flood Fill To Next:\t{floodFillToNext / 10_000,10} ms \t {GetPercent(allTicksGetTarget, floodFillToNext),0:F2}%");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"A Star Class:\t\t{aStar / 10_000,10} ms \t {GetPercent(allTicks, aStar),0:F2}%");
        Console.WriteLine($"Send Commands:\t\t{sendCommands / 10_000,10} ms \t {GetPercent(allTicks, sendCommands),0:F2}%");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"└─ ParseToCmd:\t\t{parseCoordsToCmd / 10_000,10} ms \t {GetPercent(allTicksNetworkCommunikation, parseCoordsToCmd),0:F2}%");
        Console.WriteLine($"└─ Send commands:\t{sendCmd / 10_000,10} ms \t {GetPercent(allTicksNetworkCommunikation, sendCmd),0:F2}%");
        Console.WriteLine($"└─ Read out:\t\t{readout / 10_000,10} ms \t {GetPercent(allTicksNetworkCommunikation, readout),0:F2}%");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"Update Current:\t\t{updateNode / 10_000,10} ms \t {GetPercent(allTicks, updateNode),0:F2}%");
        Console.WriteLine($"Update Map:\t\t{updateMap / 10_000,10} ms \t {GetPercent(allTicks, updateMap),0:F2}%");
        Console.WriteLine();
        Console.ForegroundColor= ConsoleColor.White;
        Console.WriteLine($"Flood fill usecount:\t\t{floodFillUseCount}");
        Console.WriteLine($"Runned Loops:\t\t\t{runningLoops}");
        Console.WriteLine($"Total sended Commands:\t\t{totalSendetCommands}");

    }

    private decimal GetPercent(long allTickss, long item)
    {
        return (decimal)((decimal)item / (decimal)allTickss) * 100m;
    }

}
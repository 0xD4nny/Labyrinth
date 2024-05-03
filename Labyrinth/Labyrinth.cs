namespace Labyrinth;

using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// This class manages most other classes needed to run the Labyrinth automatically. It uses the SearchNextTarget class to find all possible exits from the player's view and stores them in a stack.
/// Then, we pop a target from the stack and pass it to the pathfinding class, which provides the best path as a list.
/// /// </summary>
class Labyrinth
{
    private bool _gameWon;

    private readonly int _width, _height;
    private readonly Map _map;

    private readonly NetworkCommunication _networkCommunication;
    
    private readonly SearchNextTarget _searchNextTarget;

    private readonly Pathfinding _aStar;

    public Labyrinth(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(_width + 10, _height + 10);

        _networkCommunication = new NetworkCommunication(_width,_height);
        _searchNextTarget = new SearchNextTarget(_map);
        _aStar = new Pathfinding(_map);
    }


    public void InitializeGame()
    {
        _map.UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
        _map.UpdateMap(_networkCommunication.GetMapResponse());
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

        long consoleSettings = 0, getTarget = 0, aStar = 0, sendCommands = 0, updateNode = 0, updateMap = 0, printMap = 0, allTicks = 0;
        int totalSendetCommands = 0, runningLoops = 0;
        if (_map.CurrentNode is null)
            throw new NullReferenceException("_currentNode can't be null. Disconnected?");

        while (!_gameWon)
        {
            runningLoops++;

            stopwatch.Start();
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;
            stopwatch.Stop();
            consoleSettings += stopwatch.ElapsedTicks;
            stopwatch.Reset();


            stopwatch.Start();
            Node target = _searchNextTarget.GetTarget(_map.CurrentNode, ref _gameWon);
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
            _networkCommunication.SendCommands(bestPath, _map);
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

            stopwatch.Start();
            _map.PrintMap();
            stopwatch.Stop();
            printMap += stopwatch.ElapsedTicks;
            stopwatch.Reset();

        }

        _networkCommunication.GetWinnerMessage();

        allTicks = consoleSettings + getTarget + aStar + sendCommands + updateNode + updateMap + printMap;

        Console.WriteLine($"Total Time:\t{allTicks / 10000,10:N} ms");
        Console.WriteLine($"Console Settings:{consoleSettings / 10000,9:N} ms \t {GetPercent(allTicks, consoleSettings),0:F2}%");
        Console.WriteLine($"Get Target:\t{getTarget / 10000,10:N} ms \t {GetPercent(allTicks, getTarget),0:F2}%");
        Console.WriteLine($"A Star:\t\t{aStar / 10000,10:N} ms\t {GetPercent(allTicks, aStar),0:F2}%");
        Console.WriteLine($"Send Commands:\t{sendCommands / 10000,10:N} ms\t {GetPercent(allTicks, sendCommands),0:F2}%");
        Console.WriteLine($"Update Current:\t{updateNode / 10000,10:N} ms\t {GetPercent(allTicks, updateNode),0:F2}%");
        Console.WriteLine($"Update Map:\t{updateMap / 10000,10:N} ms\t {GetPercent(allTicks, updateMap),0:F2}%");
        Console.WriteLine($"Print Map:\t{printMap / 10000,10:N} ms\t {GetPercent(allTicks, printMap),0:F2}%");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Runned Loops:\t\t{runningLoops}");
        Console.WriteLine($"Total sended Commands:\t{totalSendetCommands}");

    }

    private decimal GetPercent(long allTickss, long item)
    {
        return (decimal)((decimal)item / (decimal)allTickss) * 100;
    }

}
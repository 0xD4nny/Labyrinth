namespace Labyrinth;

using System;
using System.Collections.Generic;

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

}
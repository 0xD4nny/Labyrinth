namespace Labyrinth;

using System;
using System.Collections.Generic;

class Labyrinth
{
    private bool _gameWon;

    private readonly int _width, _height;
    private readonly Map _map;

    private readonly NetworkCommunication _networkCommunication;
    
    private readonly SearchNextTarget _searchNextTarget;
    private readonly AStar _aStar;

    public Labyrinth(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(_width + 10, _height + 10);

        _networkCommunication = new NetworkCommunication(_width,_height);
        _searchNextTarget = new SearchNextTarget(_map);
        _aStar = new AStar(_map);
    }


    public void InitializeGame()
    {
        _map.UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
        _map.UpdateMap(_networkCommunication.GetMapResponse());
        _map.PrintMap();
    }

    public void GameLoop()
    {
        if(_map._currentNode is null)
            throw new NullReferenceException("_currentNode can't be null. Disconnected?");

        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            _searchNextTarget.CheckReachability(_map._currentNode);
            _searchNextTarget.CollectTargets(_map._currentNode, ref _gameWon);
            
            Node target = _searchNextTarget.Next.Pop();
            List<Node> bestPath = _aStar.Run(_map._currentNode, target);

            _networkCommunication.SendCommands(bestPath, _map);
 
            _map.UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
            _map.UpdateMap(_networkCommunication.GetMapResponse());
            _map.PrintMap();
        }

        _networkCommunication.GetWinnerMessage();
    }

}
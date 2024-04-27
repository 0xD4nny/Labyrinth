namespace Labyrinth;

using System;
using System.Collections.Generic;

class Labyrinth
{
    private bool _gameWon;

    private readonly int _width, _height;
    private readonly Map _map;

    private readonly NetworkCommunication _networkCommunication;
    
    private readonly SearchNextGoal _searchNextGoal;
    private readonly AStar _aStar;

    public Labyrinth(int width, int height)
    {
        _width = width;
        _height = height;
        _map = new Map(_width + 10, _height + 10);

        _networkCommunication = new NetworkCommunication(_width,_height);
        _searchNextGoal = new SearchNextGoal(_map);
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

        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            _searchNextGoal.Run(_map._currentNode, ref _gameWon);
            Node goal = _searchNextGoal.Targets.Pop();
            List<Node> bestPath = _aStar.Run(_map._currentNode, goal);

            _networkCommunication.SendCommands(bestPath,_map);
 
            _map.UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
            _map.UpdateMap(_networkCommunication.GetMapResponse());
            _map.PrintMap();
        }

        _networkCommunication.GetWinnerMessage();
    }

}
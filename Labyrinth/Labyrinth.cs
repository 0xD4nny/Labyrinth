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

    private Node _currentNode;

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
        UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
        _map.UpdateMap(_currentNode, _networkCommunication.GetMapResponse());
        _map.PrintMap(_currentNode);
    }

    public void GameLoop()
    {
        while (!_gameWon)
        {
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;

            _searchNextGoal.Run(_currentNode, ref _gameWon);
            Node goal = _searchNextGoal.Targets.Pop();
            List<Node> bestPath = _aStar.Run(_currentNode, goal);

            _networkCommunication.SendCommands(bestPath,_map);
 
            UpdateCurrentNode(_networkCommunication.GetCoordinateResponse());
            _map.UpdateMap(_currentNode, _networkCommunication.GetMapResponse());
            _map.PrintMap(_currentNode);
        }

        _networkCommunication.GetWinnerMessage();
    }

    private void UpdateCurrentNode(ServerResponse responseCoordinates)
    {
        if (responseCoordinates is null)
            throw new NullReferenceException("_coordinates can't be null. Disconnected?");

        int x = 5 + int.Parse(responseCoordinates.Message.Substring(responseCoordinates.Message.IndexOf("X:") + 2, responseCoordinates.Message.IndexOf(";Y") - responseCoordinates.Message.IndexOf("X:") - 2));
        int y = 5 + int.Parse(responseCoordinates.Message.Substring(responseCoordinates.Message.IndexOf("Y:") + 2, responseCoordinates.Message.IndexOf(";Z") - responseCoordinates.Message.IndexOf("Y:") - 2));

        _currentNode = new Node(x, y);
        Console.WriteLine($"X: {x} Y: {y}");
    }

}
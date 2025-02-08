
namespace TestMaze;

class Program
{
    private static bool gameWon = false;
    static void Main()
    {
        Map map = new Map();
        SearchNextTarget searchNextTarget = new SearchNextTarget(map);
        AStar aStar = new AStar(map);

        map.PrintMap();
        searchNextTarget.Run(map._currentNode);
        searchNextTarget.CollectTargets(map._currentNode, ref gameWon);

        foreach (Node node in searchNextTarget.Next)
            Console.WriteLine($"node.X: {node.X}. node.Y: {node.Y}");
        Console.WriteLine();

        List<Node> bestPath = aStar.Run(map._currentNode, searchNextTarget.Next.Dequeue());

        map.PrintMap();

        foreach (Node node in bestPath)
            Console.WriteLine($"node.X: {node.X}. node.Y: {node.Y}");

    }
}
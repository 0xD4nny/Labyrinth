namespace Labyrinth;

class Program
{
    static void Main()
    {
        Labyrinth labyrinth = new Labyrinth(32, 32);
        labyrinth.InitializeGame();
        labyrinth.GameLoop();
    }

}
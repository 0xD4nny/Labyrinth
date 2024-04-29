namespace Labyrinth;

class Program
{
    static void Main()
    {
        Labyrinth labyrinth = new Labyrinth(512, 512);
        labyrinth.InitializeGame();
        labyrinth.GameLoop();

    }

}
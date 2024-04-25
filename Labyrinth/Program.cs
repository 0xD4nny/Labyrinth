namespace Labyrinth;

class Program
{
    static void Main()
    {
        Labyrinth Labyrinth = new Labyrinth(512, 512);
        Labyrinth.GameLoop();
    }
}
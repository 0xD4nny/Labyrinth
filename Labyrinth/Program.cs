namespace Labyrinth;

class Program
{
    static void Main()
    {
        Labyrinth Labyrinth = new Labyrinth(32, 32);
        Labyrinth.GameLoop();
    }
}
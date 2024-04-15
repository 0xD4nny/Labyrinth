namespace Labyrinth;

class Program
{
    static void Main()
    {
        Labyrinth Labyrinth = new Labyrinth(128, 32);
        //Labyrinth.GameLoop();
        Labyrinth.Benchmark();
    }
}
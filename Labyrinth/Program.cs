namespace Labyrinth;

class Program
{
    static void Main()
    {
        Labyrinth Labyrinth = new Labyrinth(128, 128);
        //Labyrinth.GameLoop();
        Labyrinth.BenchmarkGameLoop();
    }
}
namespace Labyrinth;

class Program
{
    static void Main()
    {
        // you can sett a range from 32/32 to 4096/4096.
        Labyrinth labyrinth = new Labyrinth(512, 512);
        labyrinth.InitializeGame();
        labyrinth.BenchmarkGameLoop();
    }

}
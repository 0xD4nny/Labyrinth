namespace Labyrinth;

class Program
{
    static void Main()
    {
        //for (int i = 0; i < 50; i++)
        //{
            Labyrinth labyrinth = new Labyrinth(32, 32);
            labyrinth.InitializeGame();
            labyrinth.BenchmarkGameLoop();
        //}

        //Console.WriteLine("Test was Succesfull.");
    }

}
namespace Labyrinth;

class Program
{
    static void Main()
    {
        //for (int i = 0; i < 50; i++)
        //{
            Labyrinth labyrinth = new Labyrinth(512, 512);
            labyrinth.InitializeGame();
            labyrinth.GameLoop();
        //}

        //Console.WriteLine("Test was Succesfull.");
    }

}
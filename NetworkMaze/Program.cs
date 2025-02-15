namespace Labyrinth;

class Program
{
    static void Main()
    {
        // you can set a range from 32/32 to 4096/4096.
        //for (int i = 0; i < 10; i++)
        //{
            Maze labyrinth = new Maze(512, 512);
            labyrinth.InitializeGame();
            labyrinth.GameLoop();
        //}

    }
}
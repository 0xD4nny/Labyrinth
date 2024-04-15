namespace Labyrinth;

class Program
{
    static void Main()
    {
        //Console.WriteLine($"DebugWidth: {Console.BufferWidth}\nDebugHeight:{Console.BufferHeight}\n");
        //Labyrinth Labyrinth = new Labyrinth(Console.BufferWidth - 10, Console.BufferHeight - 15);
        Labyrinth Labyrinth = new Labyrinth(128, 32);
        Labyrinth.GameLoop();
    }
}
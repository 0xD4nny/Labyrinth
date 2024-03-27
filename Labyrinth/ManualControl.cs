namespace Labyrinth
{
    class ManualControl
    {
        public string KeyToString()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.RightArrow:
                    return "Right";
                case ConsoleKey.DownArrow:
                    return "Down";
                case ConsoleKey.LeftArrow:
                    return "Left";
                case ConsoleKey.UpArrow:
                    return "Up";
                case ConsoleKey.E:
                    return "Enter";
                default:
                    throw new Exception("Wrong Key!");
            }
        }
    }
}

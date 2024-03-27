namespace Labyrinth
{
    /// <summary>
    /// You can use these 2 Arrays from this struct with a single Index in a for loop to walk in every direction.
    /// </summary>
    public readonly struct Direction
    {
        public static readonly sbyte[] DirX = [1, 0, -1, 0];
        public static readonly sbyte[] DirY = [0, 1, 0, -1];
    }
}
//"DOWN" = Y++;
//"Right = X++;
//"LEFT" = X--;
//"UP" = Y--;
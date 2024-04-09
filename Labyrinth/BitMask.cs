namespace Labyrinth
{
    struct BitMask
    {
        public const byte ReachableField    = 0b_0000_0001; // 1
        public const byte UndefineField     = 0b_0000_0010; // 2
        public const byte OutOfMap          = 0b_0000_0100; // 4
        public const byte W                 = 0b_0000_1000; // 8
        public const byte T                 = 0b_0001_0000; // 16
        public const byte D                 = 0b_0010_0000; // 32
        public const byte U                 = 0b_0100_0000; // 64
        public const byte Reached           = 0b_1000_0000; // 128

        public static byte ConvertCharToBitMask(char c)
        {
            switch (c)
            {
                case ' ':
                    return ReachableField;
                case '?':
                    return UndefineField;
                case '.':
                    return OutOfMap;
                case 'W':
                    return W;
                case 'T':
                    return T;
                case 'D':
                    return D;
                case 'U':
                    return U;
                case '*':
                    return Reached;
                default:
                    throw new Exception("Unkown Byte");
            }
        }
    }
}

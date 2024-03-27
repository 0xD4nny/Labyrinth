namespace Labyrinth
{
    struct BitMask
    {
        public const byte UndefineField = 0b0000;
        public const byte ReachableField = 0b0001;
        public const byte W = 0b0010;
        public const byte T = 0b0100;
        public const byte D = 0b1000;
        public const byte U = 0b0011;
        public const byte OutOfMap = 0b0111;
        public const byte Target = 0b1111;
    }
}

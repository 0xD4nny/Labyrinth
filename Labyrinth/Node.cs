namespace Labyrinth;

class Node
{
    public readonly int X, Y;
    public Node(int x, int y)
    {
        X = x; 
        Y = y;
    }


    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Node? other = obj as Node;
        
        if (other == null) 
            return false;

        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

}
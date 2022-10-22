using UnityEngine;


public struct Point     //I really have to be implementing basic XNA things!?!
{
    public int X;
    public int Y;

    public static Point Zero = new Point(0, 0);
    public static Point Up = new Point(0, 1);
    public static Point Down = new Point(0, -1);
    public static Point Left = new Point(-1, 0);
    public static Point Right = new Point(1, 0);

    public static bool operator ==(Point a, Point b)
    => a.X == b.X && a.Y == b.Y;

    public static bool operator !=(Point a, Point b)
    => a.X != b.X || a.Y != b.Y;

    public static Point operator +(Point a, Point b)
            => new Point(a.X + b.X, a.Y + b.Y);

    public static Point operator -(Point a, Point b)
        => new Point(a.X - b.X, a.Y - b.Y);

    public static Point operator *(Point a, Point b)
        => new Point(a.X * b.X, a.Y * b.Y);

    public static Point operator /(Point a, Point b)
        => new Point(a.X / b.X, a.Y / b.Y);

    public static Point operator *(Point a, int b)
    => new Point(a.X * b, a.Y * b);

    public static Point operator /(Point a, int b)
        => new Point(a.X / b, a.Y / b);

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point(int size)
    {
        X = size;
        Y = size;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Rectangle     //I really have to be implementing basic XNA things!?! Again!!??
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public readonly int Left;
    public readonly int Right;
    public readonly int Up;
    public readonly int Down;

    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;

        Left = x;
        Right = x + width;
        Up = y;
        Down = y + height;
    }

    public Rectangle(Point pos, Point size)
    {
        X = pos.X;
        Y = pos.Y;
        Width = size.X;
        Height = size.Y;

        Left = pos.X;
        Right = pos.X + size.X;
        Up = pos.Y;
        Down = pos.Y + size.Y;

    }

    public bool Intersects(Rectangle rect)
    {
        if (X > rect.Left && Right < rect.X && rect.Y > rect.Up && rect.Down < rect.Y)
            return true;

        return false;
    }

    public Point GetPos()
    {
        return new Point(X, Y);
    }

    public Point GetSize()
    {
        return new Point(Width, Height);
    }
}

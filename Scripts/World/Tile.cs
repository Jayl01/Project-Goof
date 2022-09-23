public struct Tile
{
    public const byte Air = 0;
    public const byte Dirt = 1;
    public const byte Wall = 2;

    public enum GenerationID
    {
        Undefined,
        Hallway,
        Room
    }

    public byte tileType;
    public Point tilePoint;
    public GenerationID generationID;

    public static Tile CreateTile(byte type, int x, int y)
    {
        Tile mewTileDataInstance = new()
        {
            tileType = type,
            tilePoint = new Point(x, y),
            generationID = GenerationID.Undefined
        };

        return mewTileDataInstance;
    }

    public static Tile CreateTile(byte type, Point point)
    {
        Tile mewTileDataInstance = new()
        {
            tileType = type,
            tilePoint = point,
            generationID = GenerationID.Undefined
        };

        return mewTileDataInstance;
    }

    public static Tile CreateTile(byte type, int x, int y, GenerationID generationID)
    {
        Tile mewTileDataInstance = new()
        {
            tileType = type,
            tilePoint = new Point(x, y),
            generationID = generationID
        };

        return mewTileDataInstance;
    }

    public static Tile CreateTile(byte type, Point point, GenerationID generationID)
    {
        Tile mewTileDataInstance = new()
        {
            tileType = type,
            tilePoint = point,
            generationID = generationID
        };

        return mewTileDataInstance;
    }
}

public struct GenerationLayer
{
    public int width;
    public int height;
    public Tile[,] layerTiles;
    public byte layerLevel;      //How high up the layer is

    public GenerationLayer(int width, int height, Tile[,]layerTiles, byte layerLevel)
    {
        this.width = width;
        this.height = height;
        this.layerTiles = layerTiles;
        this.layerLevel = layerLevel;
    }

    public GenerationLayer(Tile[,]layerTiles, byte layerLevel)
    {
        width = layerTiles.GetLength(0);
        height = layerTiles.GetLength(1);
        this.layerTiles = layerTiles;
        this.layerLevel = layerLevel;
    }

    public Tile CheckTileAbove(int x, int y)
    {
        if (y - 1 < 0)
            return layerTiles[0, 0];

        return layerTiles[x, y - 1];
    }

    public Tile CheckTileAbove(Point coordinates)
    {
        if (coordinates.Y - 1 < 0)
            return layerTiles[0, 0];

        return layerTiles[coordinates.X, coordinates.Y - 1];
    }

    public Tile CheckTileUnder(int x, int y)
    {
        if (y + 1 >= height)
            return layerTiles[0, 0];

        return layerTiles[x, y + 1];

    }
    public Tile CheckTileUnder(Point coordinates)
    {
        if (coordinates.Y + 1 >= height)
            return layerTiles[0, 0];

        return layerTiles[coordinates.X, coordinates.Y + 1];
    }

    public Tile CheckTileLeft(int x, int y)
    {
        if (x - 1 < 0)
            return layerTiles[0, 0];

        return layerTiles[x - 1, y];
    }

    public Tile CheckTileLeft(Point coordinates)
    {
        if (coordinates.X - 1 < 0)
            return layerTiles[0, 0];

        return layerTiles[coordinates.X - 1, coordinates.Y];
    }

    public Tile CheckTileRight(int x, int y)
    {
        if (x + 1 >= width)
            return layerTiles[0, 0];

        return layerTiles[x + 1, y];
    }

    public Tile CheckTileRight(Point coordinates)
    {
        if (coordinates.X + 1 >= width)
            return layerTiles[0, 0];

        return layerTiles[coordinates.X + 1, coordinates.Y];
    }
}

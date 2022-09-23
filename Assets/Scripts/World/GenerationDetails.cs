public struct GenerationDetails
{
    public byte[] tileTypes;
    public int[] tileChances;

    public GenerationDetails(byte[] tileTypes, int[] tileChances)
    {
        this.tileTypes = tileTypes;
        this.tileChances = tileChances;
    }

    public byte GetRandomTileType()
    {
        if (tileTypes.Length == 1)
            return tileTypes[0];

        int random = MapGenerator.worldRand.Next(1, 100 + 1);
        for (int i = 0; i < tileTypes.Length; i++)
        {
            if (random <= tileChances[i])
            {
                return tileTypes[i];
            }
            else
            {
                random -= tileChances[i];
            }
        }

        return tileTypes[0];
    }
}

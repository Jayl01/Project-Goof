using LiteNetLib.Utils;
using UnityEditor.VersionControl;
using static Packets;

public class PacketReader
{
    public static PacketReader packetReader;

    public static void ReceivePacket(NetDataReader packet)
    {
        PacketType packetType = (PacketType)packet.GetByte();
        byte sender = packet.GetByte();

        switch (packetType)
        {
            case PacketType.ObjectCreation:
                packetReader.HandleObjectCreation(packet, sender);
                break;

            case PacketType.WorldData:
                packetReader.HandleWorldData(packet, sender);
                break;
        }
    }

    private void HandleObjectCreation(NetDataReader packet, byte sender)
    {

    }

    private void HandleWorldData(NetDataReader packet, byte sender)
    {
        byte amountOfLayers = packet.GetByte();
        GenerationLayer[] generationLayers = new GenerationLayer[amountOfLayers];
        for (int i = 0; i < amountOfLayers; i++)
        {
            int layerWidth = packet.GetInt();
            int layerHeight = packet.GetInt();
            byte layerLevel = packet.GetByte();
            Tile[,] layerTiles = new Tile[layerWidth, layerHeight];
            for (int x = 0; x < generationLayers[i].width; x++)
            {
                for (int y = 0; y < generationLayers[i].height; y++)
                {
                    layerTiles[x, y] = Tile.CreateTile(packet.GetByte(), x, y);
                }
            }

            generationLayers[i] = new GenerationLayer(layerWidth, layerHeight, layerTiles, layerLevel);
        }

        MapGenerator.LoadWorld(generationLayers);
    }
}

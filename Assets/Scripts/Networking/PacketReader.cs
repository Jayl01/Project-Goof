using LiteNetLib.Utils;
using static Packets;
using static NetworkData;
using System.Collections.Generic;

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

            case PacketType.LobbyPlayerSync:
                packetReader.HandleLobbyPlayerSync(packet, sender);
                break;

            case PacketType.WorldData:
                packetReader.HandleWorldData(packet, sender);
                break;
        }
    }

    private void HandleObjectCreation(NetDataReader packet, byte sender)
    {

    }

    private void HandleLobbyPlayerSync(NetDataReader packet, byte sender)
    {
        LobbyManager.connectedPlayers = new Dictionary<string, PlayerData>();

        int amountOfPlayers = packet.GetByte();
        for (int i = 0; i < amountOfPlayers; i++)
        {
            PlayerData playerData = new PlayerData()
            {
                playerId = packet.GetByte(),
                playerName = packet.GetString()
            };
            if (playerData.playerName == LobbyManager.PlayerName)
                LobbyManager.self.clientID = playerData.playerId;
            LobbyManager.connectedPlayers.Add(playerData.playerName, playerData);
        }
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

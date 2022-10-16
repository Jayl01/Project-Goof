using LiteNetLib;
using LiteNetLib.Utils;
using System.Linq;
using UnityEngine;
using static Packets;

/// <summary>
/// A helper class for syncing in multiplayer.
/// </summary>
public static class SyncCall
{
    /// <summary>
    /// Unimplemented. Does not work.
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="objectPosition"></param>
    /// <param name="objectRotation"></param>
    /*public static void SyncObjectCreation(byte objectType, Vector3 objectPosition, Vector3 objectRotation)
    {
        NetDataWriter message = new NetDataWriter(true, 27);
        message.Put((byte)PacketType.ObjectCreation);
        message.Put(LobbyManager.self.clientID);
        message.Put(objectType);
        message.Put(objectPosition.x);
        message.Put(objectPosition.y);
        message.Put(objectPosition.z);
        message.Put(objectRotation.x);
        message.Put(objectRotation.y);
        message.Put(objectRotation.z);

        SendMessageToAllOthers(message, DeliveryMethod.ReliableUnordered);
    }*/

    /// <summary>
    /// Brings the entire lobby in sync with each other in terms of the IDs, names, and amount of people in the lobby. Should only ever be called by the server owner.
    /// </summary>
    public static void SyncLobbyPlayers()
    {
        NetDataWriter message = new NetDataWriter(true);
        message.Put((byte)PacketType.LobbyPlayerSync);
        message.Put(LobbyManager.self.clientID);

        message.Put((byte)LobbyManager.connectedPlayers.Count);
        string[] playerKeys = LobbyManager.connectedPlayers.Keys.ToArray();
        for (int i = 0; i < LobbyManager.connectedPlayers.Count; i++)
        {
            message.Put((byte)i);
            message.Put(playerKeys[i]);
        }
        SendMessageToAllOthers(message);
    }

    /// <summary>
    /// Syncs the world with all other players.
    /// </summary>
    /// <param name="generationLayers"></param>
    public static void SyncWorld(GenerationLayer[] generationLayers)
    {
        NetDataWriter message = new NetDataWriter(true);
        message.Put((byte)PacketType.WorldData);
        message.Put(LobbyManager.self.clientID);

        message.Put((byte)generationLayers.Length);
        for (int i = 0; i < generationLayers.Length; i++)
        {
            message.Put(generationLayers[i].width);
            message.Put(generationLayers[i].height);
            message.Put(generationLayers[i].layerLevel);
            for (int x = 0; x < generationLayers[i].width; x++)
            {
                for (int y = 0; y < generationLayers[i].height; y++)
                {
                    message.Put(generationLayers[i].layerTiles[x, y].tileType);
                }
            }
        }
        SendMessageToAllOthers(message);
    }

    /// <summary>
    /// Syncs a scene switch with all other players. The scene name input into the sceneName parameter will be what the scene switches to.
    /// </summary>
    /// <param name="sceneName">The scene to switch to.</param>
    public static void SyncSceneSwitch(string sceneName)
    {
        NetDataWriter message = new NetDataWriter(true, 10);
        message.Put((byte)PacketType.GlobalSceneSwitch);
        message.Put(LobbyManager.self.clientID);

        message.Put(sceneName);
        SendMessageToAllOthers(message);
    }

    public static void SyncSpawnPoints(Point[] spawnPoints)
    {
        NetDataWriter message = new NetDataWriter(true, 10);
        message.Put((byte)PacketType.SpawnPoints);
        message.Put(LobbyManager.self.clientID);

        message.Put(spawnPoints.Length);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            message.Put(spawnPoints[i].X);
            message.Put(spawnPoints[i].Y);
        }
        SendMessageToAllOthers(message);
    }

    public static void SyncMovement(Vector3 movement)
    {
        NetDataWriter message = new NetDataWriter(true, 10);
        message.Put((byte)PacketType.SpawnSyncMovement);
        message.Put(LobbyManager.self.clientID);

        message.Put(movement.x);
        message.Put(movement.y);
        message.Put(movement.z);
        SendMessageToAllOthers(message);
    }

    /// <summary>
    /// Sends a packet to the passed in peer.
    /// </summary>
    /// <param name="peer">The peer to send the packet to.</param>
    /// <param name="writer">The packet data to send.</param>
    /// <param name="deliveryMethod">The reliablility of the data.</param>
    public static void SendMessageToPeer(NetPeer peer, NetDataWriter writer, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        peer.Send(writer, deliveryMethod);
    }

    /// <summary>
    /// Sends a packet to all connected peers.
    /// </summary>
    /// <param name="writer">The packet data to send.</param>
    /// <param name="deliveryMethod">The reliablility of the data.</param>
    public static void SendMessageToAllOthers(NetDataWriter writer, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        LobbyManager.self.netManager.SendToAll(writer.Data, deliveryMethod);
    }
}

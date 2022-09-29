using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using static Packets;

/// <summary>
/// A helper class for syncing in multiplayer.
/// </summary>
public static class SyncCall
{
    public static void SyncObjectCreation(byte objectType, Vector3 objectPosition, Vector3 objectRotation)
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
    }

    public static void SyncWorld(GenerationLayer[] generationLayers)
    {
        NetDataWriter message = new NetDataWriter(true, 27);
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
        LobbyManager.self.netManager.SendToAll(writer.Data, deliveryMethod, LobbyManager.self.netPeer);
    }
}

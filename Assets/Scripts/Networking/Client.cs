using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static Packets;

/// <summary>
/// A networking object that represents this network-able machine.
/// </summary>
public class Client
{
    public NetPeer netPeer;
    public NetManager netManager;
    public EventBasedNetListener netListener;
    public byte clientID;

    public Client()
    {
        Initalize();
    }

    public void Initalize()
    {
        netListener = new EventBasedNetListener();
        netListener.NetworkReceiveEvent += PacketReceived;
        netListener.PeerConnectedEvent += PeerConnected;
        netListener.PeerDisconnectedEvent += PeerDisconnected;
        netListener.ConnectionRequestEvent += ConnectionRequestReceived;
        PacketReader.packetReader = new PacketReader();
        netManager = new NetManager(netListener);
        netManager.NatPunchEnabled = false;
        netManager.UpdateTime = 15;
        netManager.EnableStatistics = true;
        netManager.Start();
    }

    public void Update()
    {
        netManager.PollEvents();
    }

    public static void AttemptJoinLobby(string ip, int port, string requestKey)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(requestKey);
        LobbyManager.self.netPeer = LobbyManager.self.netManager.Connect(ip, port, writer);
    }

    private void PacketReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        PacketReader.ReceivePacket(reader);
    }

    private void ConnectionRequestReceived(ConnectionRequest request)
    {
        if (request.Data.GetString() == LobbyManager.LobbyKey)
            request.Accept();
    }

    private void PeerConnected(NetPeer peer)
    {
        
    }

    private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        
    }
}

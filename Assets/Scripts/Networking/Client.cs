using LiteNetLib;
using LiteNetLib.Utils;
using static NetworkData;

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

    public static void AttemptJoinLobby(string ip, int port, string requestKey, string playerName)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(requestKey);
        writer.Put(playerName);
        LobbyManager.self.netPeer = LobbyManager.self.netManager.Connect(ip, port, writer);
    }

    /// <summary>
    /// Called when a packet is received. Calls PacketReader.ReceievePacket to interepet the data.
    /// </summary>
    private void PacketReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        PacketReader.ReceivePacket(reader);
    }

    /// <summary>
    /// Called when someone tries to join the lobby
    /// </summary>
    /// <param name="request"></param>
    private void ConnectionRequestReceived(ConnectionRequest request)
    {
        if (!LobbyManager.LobbyJoinable)
            request.Reject();

        if (request.Data.GetString() == LobbyManager.LobbyKey)
        {
            request.Accept();
            PlayerData newPlayerData = new PlayerData()
            {
                playerId = (byte)(LobbyManager.connectedPlayers.Count + 1),
                playerName = request.Data.GetString()
            };
            LobbyManager.PlayerJoined(newPlayerData);
        }
    }

    /// <summary>
    /// Called when a new player connects.
    /// </summary>
    private void PeerConnected(NetPeer peer)
    {

    }

    /// <summary>
    /// Called when a player disconnects.
    /// </summary>
    private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {

    }
}

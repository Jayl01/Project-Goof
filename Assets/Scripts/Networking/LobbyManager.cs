using System.Collections.Generic;
using UnityEngine;
using static NetworkData;

public class LobbyManager : MonoBehaviour
{
    public static Client self;
    public static Dictionary<string, PlayerData> connectedPlayers;
    public static string PlayerName;        //Your own name as you type it in the title screen
    public static string LobbyKey = "";
    public static bool LobbyJoinable = false;

    public void Start()
    {
        self = new Client();
        connectedPlayers = new Dictionary<string, PlayerData>();
        //Don't destroy on load
    }

    public void Update()
    {
        self.Update();
    }

    public static void CreateLobby(string lobbyKey)
    {

        PlayerData playerData = new PlayerData()
        {
            playerId = 0,
            playerName = PlayerName
        };
        connectedPlayers.Add(playerData.playerName, playerData);
        self.clientID = 1;
        LobbyKey = lobbyKey;
        LobbyJoinable = true;
    }

    public static void PlayerJoined(PlayerData playerData)
    {
        connectedPlayers.Add(playerData.playerName, playerData);
        SyncCall.SyncLobbyPlayers();
    }

    public static void PlayerLeft(PlayerData playerData)
    {
        connectedPlayers.Remove(playerData.playerName);
        SyncCall.SyncLobbyPlayers();
    }

    public static void AttemptJoin(string ip, string key)
    {
        Client.AttemptJoinLobby(ip, 12125, key, PlayerName);
    }
}

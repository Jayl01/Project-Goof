using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NetworkData;

public class LobbyManager : MonoBehaviour
{
    public static Client self;
    public static Dictionary<string, PlayerData> connectedPlayers;
    public static string PlayerName;        //Your own name as you type it in the title screen
    public static string LobbyKey = "";
    public static string CurrentLobbyIP = "127.0.0.1";
    public const int LobbyPort = 12125;
    public static bool LobbyJoinable = false;

    public void Start()
    {
        self = new Client();
        connectedPlayers = new Dictionary<string, PlayerData>();
        //Don't destroy on load
        DontDestroyOnLoad(this);
    }

    public void Update()
    {
        self.Update();
    }

    public static void CreateLobby(string lobbyKey)
    {
        self.ReInitalizeAsServer();
        PlayerData playerData = new PlayerData()
        {
            playerId = 0,
            playerName = PlayerName
        };
        connectedPlayers.Add(playerData.playerName, playerData);
        self.clientID = 1;
        self.inLobby = true;
        LobbyKey = lobbyKey;
        LobbyJoinable = true;
    }

    /// <summary>
    /// Returns the amount of playres in the lobby.
    /// </summary>
    /// <returns></returns>
    public static int GetAmountOfPlayersInLobby() => connectedPlayers.Count;
    /// <summary>
    /// Returns the names of all players in the lobby.
    /// </summary>
    /// <returns></returns>
    public static string[] GetAllLobbyMemberNames() => connectedPlayers.Keys.ToArray();

    /// <summary>
    /// A method called when someone joins the lobby.
    /// </summary>
    /// <param name="playerData">The data of the player who joined.</param>
    public static void PlayerJoined(PlayerData playerData)
    {
        connectedPlayers.Add(playerData.playerName, playerData);
        SyncCall.SyncLobbyPlayers();
    }

    /// <summary>
    /// A method called when someone leaves the lobby.
    /// </summary>
    /// <param name="playerData">The data of the player who left.</param>
    public static void PlayerLeft(PlayerData playerData)
    {
        connectedPlayers.Remove(playerData.playerName);
        SyncCall.SyncLobbyPlayers();
    }

    /// <summary>
    /// Attemps to join the given Lobby IP with the given Lobby Key. This method can be called freely but 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="key"></param>
    public static void AttemptJoin(string ip, string key)
    {
        Client.AttemptJoinLobby(ip, LobbyPort, key, PlayerName);
    }
}

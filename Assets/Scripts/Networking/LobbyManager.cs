using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NetworkData;

public class LobbyManager : MonoBehaviour
{
    public static Client self;
    public static Dictionary<string, PlayerData> connectedPlayers;
    /// <summary>
    /// Returns the GameObject of the player.
    /// </summary>
    public static GameObject[] playerObjects;
    /// <summary>
    /// Returns the GameObject
    /// </summary>
    public static Ragdoll[] playerRagdolls;
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
        self.clientID = 0;
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
    /// Returns the index of the player with the given name. Returns 0 if the search was unsuccessful.
    /// </summary>
    /// <param name="name">the name to get the index of.</param>
    /// <returns>The index of hte player with the given name.</returns>
    public static int GetIndexOfPlayerFromName(string name)
    {
        string[] playerKeys = connectedPlayers.Keys.ToArray();
        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            if (playerKeys[i] == name)
                return i;
        }

        return 0;
    }

    /// <summary>
    /// A method called when someone joins the lobby.
    /// </summary>
    /// <param name="playerData">The data of the player who joined.</param>
    public static void PlayerJoined(PlayerData playerData)
    {
        connectedPlayers.Add(playerData.playerName, playerData);
        ReorganizeLobbyIndexes();
        SyncCall.SyncLobbyPlayers();
    }

    /// <summary>
    /// A method called when someone leaves the lobby.
    /// </summary>
    /// <param name="playerData">The data of the player who left.</param>
    public static void PlayerLeft(PlayerData playerData)
    {
        connectedPlayers.Remove(playerData.playerName);
        ReorganizeLobbyIndexes();
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

    public static void ReorganizeLobbyIndexes()
    {
        string[] playerKeys = connectedPlayers.Keys.ToArray();
        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            PlayerData playerData = connectedPlayers[playerKeys[i]];
            playerData.playerId = (byte)i;
            connectedPlayers[playerKeys[i]] = playerData;
        }
    }
}

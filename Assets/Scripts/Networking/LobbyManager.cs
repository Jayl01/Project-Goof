using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkData;

public class LobbyManager : MonoBehaviour
{
    public static Client self;
    public static List<PlayerData> connectedPlayers;
    public static string LobbyKey = "";

    public void Start()
    {
        self = new Client();
        connectedPlayers = new List<PlayerData>();
    }

    public static void PlayerJoined(PlayerData playerDataData)
    {
        connectedPlayers.Add(playerDataData);
    }
    
    public static void PlayerLeft()
    {

    }
}

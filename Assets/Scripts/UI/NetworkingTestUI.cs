using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NetworkData;

public class NetworkingTestUI : MonoBehaviour
{
    public GameObject startButton;
    public TextMeshProUGUI playerText;

    private string insertedName;
    private string insertedKey = "P";
    private string insertedIP;

    public void Update()
    {
        if (!LobbyManager.self.inLobby)
            return;

        string testText = "Players: ";
        string[] playerKeys = LobbyManager.connectedPlayers.Keys.ToArray();
        for (int i = 0; i < LobbyManager.connectedPlayers.Count; i++)
        {
            testText += LobbyManager.connectedPlayers[playerKeys[i]].playerName + " (" + LobbyManager.connectedPlayers[playerKeys[i]].playerId + "), ";
        }
        playerText.text = testText;
    }

    public void LobbyButtonClicked()
    {
        LobbyManager.PlayerName = insertedName;
        LobbyManager.CreateLobby(insertedKey);
        startButton.transform.localPosition = new Vector3(260, -183, 0);
    }

    public void JoinButtonClicked()
    {
        LobbyManager.PlayerName = insertedName;
        LobbyManager.AttemptJoin(insertedIP, insertedKey);
    }

    public void DebugSoloClicked()
    {
        StartGame(true);
    }

    public void NameInsterted(string input)
    {
        insertedName = input;
    }

    public void LobbyKeyInserted(string input)
    {
        insertedKey = input;
    }

    public void IPInserted(string input)
    {
        insertedIP = input;
    }

    public void StartGame(bool solo = false)
    {
        if (solo)
        {
            LobbyManager.PlayerName = "Solo";
            LobbyManager.CreateLobby("Key");
        }
        SceneManager.LoadScene("MultiplayerTest");
        SyncCall.SyncSceneSwitch("MultiplayerTest");
    }
}

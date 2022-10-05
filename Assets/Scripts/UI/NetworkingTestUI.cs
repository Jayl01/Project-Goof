using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkingTestUI : MonoBehaviour
{
    public GameObject nameTextObj;
    public GameObject lobbyTextObj;
    public GameObject ipTextObj;

    public InputField nameText;
    public InputField lobbyKeyText;
    public InputField ipText;

    public void Start()
    {
        nameText = nameTextObj.GetComponent<InputField>();
        lobbyKeyText = lobbyTextObj.GetComponent<InputField>();
        ipText = ipTextObj.GetComponent<InputField>();
    }

    public void LobbyButtonClicked()
    {
        LobbyManager.PlayerName = nameText.text;
        LobbyManager.CreateLobby(lobbyKeyText.text);
    }

    public void JoinButtonClicked()
    {
        LobbyManager.PlayerName = nameText.text;
        LobbyManager.AttemptJoin(ipText.text, lobbyKeyText.text);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021

using NETWORK_ENGINE;
public class GameRoomButton : MonoBehaviour
{
    public NetworkCore MyCore;
    public LobbyManager MyLobby;
    public Text MyText;
    public string GameName;
    public int Players;

    // Start is called before the first frame update
    void Start()
    {
        Players = 0;
    }
    public void SetText()
    {
        MyCore = GameObject.FindObjectOfType<NetworkCore>();
        MyLobby = GameObject.FindObjectOfType<LobbyManager>();
        if (MyCore == null || MyLobby == null)
        {
            throw new System.Exception("ERROR: Could not find NetworkCore or Network Lobby");
        }
        MyText.text = "Game Name: "+GameName + "\nPlayers: " + Players.ToString() + "/" + MyCore.MaxConnections.ToString() + "\t\tGame ID:" + name;
    }
    public void JoinGame()
    {
        int portOffset = -1;
        try
        {
            portOffset = int.Parse(name);
            if(portOffset != -1)
            {
                //WE have to connect through the Lobby.   
                //Grr this is annoying.
                MyLobby.Send("JOIN#" + portOffset + "#" + MyLobby.LocalConnectionID+"\n", 0);
            }
        }
        catch
        {
            throw new System.Exception("Name of Button not Integer, did you forget to set it as the port offset?");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

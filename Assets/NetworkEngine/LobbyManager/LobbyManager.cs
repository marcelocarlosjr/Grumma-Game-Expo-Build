using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NETWORK_ENGINE;
using System.Text;


///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021


/// <summary>
/// GameRoom Class - Will hold all of the game information.
/// This will only be kept on the Lobby Manager server - AKA Master
/// </summary>
public class GameRoom
{
    public bool DidStart;
    public int CurrentNumberPlayers;
    public int Port;
    public int GameID;
    //Only on server
    public System.Diagnostics.Process MyProcess;
    public float ProcessTTL;
    public Coroutine KillTimer;
    public int ServerC;
    public int Creator;
    public string GameName = "DefaultGame";
    public GameRoom()
    {
        DidStart = false;
        CurrentNumberPlayers = 0;
        Port = 0;
        GameID = 0;
        ProcessTTL = -1;
    }
}

public class LobbyManager : GenericNetworkCore
{
    //You have to do this so you can connect to the correct one.
    public string PublicIP;
    public string FloridaPolyIP;

    //Variables to control different game states.
    public NetworkCore MyCore;

    public Button CreateGame;
    //IsMaster and IsAgent will be used to describe serer/client for lobby engine;
    //to reduce confusion between lobby and network engine.
    public bool IsMaster
    {
        get { return IsServer; }
    }
    public bool IsAgent
    {
        get { return IsClient; }
    }
    public int LocalGameID = -10;
    public int GameCounter = 0;
    public bool IsGameServer = false;

    //UI Variables
    ExclusiveDictionary<int, GameRoomButton> GameRoomButtons;
    ExclusiveDictionary<int, GameRoom> Lobbies;
    public Transform Content;//This variable holds the game room buttons.
    public string MyGameName = "Default Game";
    public int MaxGameTime = -1;
    public GameObject GameRoomPrefab;
    //Game and Lobby values;
    public int LowPort = 9001;
    public int HighPort = 10000;
    public int LastPlayers = 0;
    // Start is called before the first frame update
    /// <summary>
    /// IF command line args have passed in "MASTER" this code will create a Lobby Manager Server - referred to as Master
    /// If the command line arg involves a _Port this code will tell the NetCore to create a game server and connect as an Agent to the Lobby Manager
    /// Otherwise, this code will simply connect you as an agent to the Lobby manager.
    /// </summary>
    void Start()
    {
        UsingUDP = false;
        MyCore = GameObject.FindObjectOfType<NetworkCore>();
        if (MyCore == null)
        {
            throw new System.Exception("Could not find core!");
        }
        string[] args = System.Environment.GetCommandLineArgs();   
        foreach (string a in args)
        {
            try
            {
                if (a.StartsWith("PORT_"))
                {
                    string[] temp = a.Split('_');
                    int port = int.Parse(temp[1]);
                    LocalGameID = int.Parse(temp[3]);
                    GenericNetworkCore.Logger("The number of Max connections is: " + MyCore.MaxConnections);
                    MyCore.PortNumber = port;
                    IP = "127.0.0.1";
                    StartCoroutine(ClientStart());

                    //StartCoroutine(SlowAgentStart());
                    //MyCore.IP = this.IP;

                    StartCoroutine(SlowStart());
                }
            }
            catch (System.Exception e)
            {
                GenericNetworkCore.Logger("Exception caught starting the server: " + e.ToString());
            }

            if (a.Contains("MASTER"))
            {
                StartCoroutine(ServerStart());
            }
        }
        if (!IsConnected)
        {
            StartCoroutine(SlowAgentStart());
        }

        //Maintained on clients with same ID as Lobby.
        GameRoomButtons = new ExclusiveDictionary<int, GameRoomButton>();

        //Lobby only maintained on the server.
        Lobbies = new ExclusiveDictionary<int, GameRoom>();
        if (Content == null)
        {
            throw new System.Exception("Could not find container for game buttons!");
        }
    }
    /// <summary>
    /// This function will deal with delays of setting up an agent.
    /// </summary>
    /// <returns>IEnumerator to allow for delays.</returns>
    public IEnumerator SlowAgentStart()
    {
        bool UsePublic = false;
        bool UseFlorida = false;

        //Ping Public Ip address to see if we are external..........
        GenericNetworkCore.Logger("Trying Public IP Address: " + PublicIP.ToString());
        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        System.Net.NetworkInformation.PingOptions po = new System.Net.NetworkInformation.PingOptions();
        po.DontFragment = true;
        string data = "HELLLLOOOOO!";
        byte[] buffer = ASCIIEncoding.ASCII.GetBytes(data);
        int timeout = 500;
        System.Net.NetworkInformation.PingReply pr = ping.Send(PublicIP, timeout, buffer, po);
        yield return new WaitForSeconds(1.5f);
        Logger("Ping Return: " + pr.Status.ToString());
        if(pr.Status == System.Net.NetworkInformation.IPStatus.Success)
        {
            GenericNetworkCore.Logger("The public IP responded with a roundtrip time of: " + pr.RoundtripTime);
            UsePublic = true;
            IP = PublicIP;
        }
        else
        {
            GenericNetworkCore.Logger("The public IP failed to respond");
            UsePublic = false;
        }
        //-------------------If not public, ping Florida Poly for internal access.
        if(!UsePublic)
        {
            GenericNetworkCore.Logger("Trying Florida Poly Address: " + FloridaPolyIP.ToString());
            pr = ping.Send(FloridaPolyIP, timeout, buffer, po);
            yield return new WaitForSeconds(1.5f);
            Logger("Ping Return: " + pr.Status.ToString());
            if (pr.Status.ToString() == "Success")
            {
                GenericNetworkCore.Logger("The Florida Poly IP responded with a roundtrip time of: " + pr.RoundtripTime);
                UseFlorida = true;
                IP = FloridaPolyIP;
            }
            else
            {
                GenericNetworkCore.Logger("The Florida Poly IP failed to respond");
                UseFlorida = false;
            }
        }
        //Otherwise use local host, assume testing.
        if(!UsePublic && !UseFlorida)
        {
            IP = "127.0.0.1";
            GenericNetworkCore.Logger("Using Home Address!");
        }
        StartCoroutine(ClientStart()); ;
    }

    /// <summary>
    /// This function will allow delays to ensure the game room has started.  
    /// Once it has it will tell the Lobby Manager so other players can join.
    /// </summary>
    /// <returns></returns>
    public IEnumerator SlowStart()
    {
        yield return new WaitForSeconds(1);
        yield return new WaitUntil( () =>LocalConnectionID >=0);
        MyCore.UI_StartServer();
        yield return new WaitUntil( ()=>MyCore.IsServer);
        Send("ISSERVER#" + LocalGameID + "#" + LocalConnectionID + "\n", 0);
        IsGameServer = true;
    }

    /// <summary>
    /// Will deal with incoming traffic regarding 
    /// the management of different game rooms.
    /// </summary>
    /// <param name="responce">The string value of the message incoming</param>
    public override void OnHandleMessages(string responce)
    {
        try
        {
            if (responce.Contains("ISSERVER"))
            {
                RegisterGame(responce);
            }
            if (responce.StartsWith("CREATEROOM") && IsMaster)
            {
                CreateNewGame(responce);
            }
            if (responce.StartsWith("G_DISC"))
            {
                //StartCoroutine(DisconnectGameServer(responce));
            }
            if (responce.StartsWith("NEWGAME#"))
            {
                NotifyNewGame(responce);
            }

            if (responce.StartsWith("JOIN#"))
            {
                JoinAGame(responce);
            }
            if (responce.StartsWith("GAMESTART"))
            {
                RegisterGameStarted(responce);
            }
            if (responce.StartsWith("PLAYERS#"))
            {
                UpdatePlayers(responce);
            }
        }
        catch (System.Exception e)
        {
            GenericNetworkCore.Logger("Lobby Manager Caugh this error: " + e.ToString()+"\n "+responce);
        }

    }

    // ---------------------Helper funcions to clean up OnHandleMessages -----------------------------
    /// <summary>
    /// Will update the number of players in a game
    /// and send notifcation to the clients.
    /// </summary>
    /// <param name="g">String form of the command.</param>
    public void UpdatePlayers(string g)
    {
        if (g.StartsWith("PLAYERS#"))
        {
            //Logger("Recieved Players! "+g);
            //Update the number of players
            string[] args = g.Split('#');
            int gidTemp = int.Parse(args[1].Trim());
            int newPlayer = int.Parse(args[2].Trim());
            int MaxConnection = int.Parse(args[3].Trim());
            if (GameRoomButtons.ContainsKey(gidTemp))
            {
                //Logger("Inside set button! - New player count = "+newPlayer);
                GameRoomButtons[gidTemp].Players = newPlayer;
                MyCore.MaxConnections = MaxConnection;
                GameRoomButtons[gidTemp].SetText();
            }
            if (Lobbies.ContainsKey(gidTemp))
            {
                Lobbies[gidTemp].CurrentNumberPlayers = newPlayer;
            }
            if (IsMaster)
            {
                foreach (KeyValuePair<int, Connector2> con in Connections)
                {
                    Send(g, con.Key);
                }
            }
        }
    }
    /// <summary>
    /// Will notify all that a game has started
    /// Therefore you can no longer join.
    /// </summary>
    /// <param name="g">String form of the command.</param>
    public void RegisterGameStarted(string g)
    {
        int gameID = int.Parse(g.Split('#')[1]);
        if(IsMaster)
        {
            if(Lobbies.ContainsKey(gameID))
            {
                Lobbies[gameID].DidStart = true;
            }
            if(GameRoomButtons.ContainsKey(gameID))
            {
                Destroy(GameRoomButtons[gameID].gameObject);
                GameRoomButtons.Remove(gameID);
            }
            foreach(KeyValuePair<int,Connector2> x in Connections)
            {
                Send(g, x.Key);
            }
        }
        if(IsAgent)
        {
            //Remove from List.
            if(GameRoomButtons.ContainsKey(gameID))
            {
                Destroy(GameRoomButtons[gameID].gameObject);
                GameRoomButtons.Remove(gameID);
            }
        }
    }
    /// <summary>
    /// Due to a strange co-routine problem.
    /// The client must first requests to join a game 
    /// Have the Lobby Master return the value of the port
    /// and then the player can join.
    /// The client already has the port information but the coroutine to join 
    /// must be called within this function.
    /// 
    /// </summary>
    /// <param name="g">string form of the command.</param>
    public void JoinAGame(string g)
    {
       if(IsMaster)
        {
            GenericNetworkCore.Logger("Master received join: " + g);
            int gID = int.Parse(g.Split('#')[1]);
            int agentId = int.Parse(g.Split('#')[2]);
            if(Lobbies.ContainsKey(gID))
            {
                Send("JOIN#" + Lobbies[gID].Port + "\n", agentId);
            }
        }
        if(IsAgent)
        {
            GenericNetworkCore.Logger("Joining game ID " + g.Split('#')[1].Trim());
            MyCore.PortNumber = int.Parse(g.Split('#')[1].Trim());
            MyCore.UI_StartClient();
            if (MyCore.IsConnected && MyCore.IsClient)
            {
                StartCoroutine(MenuManager());
            }
        }
    }
  /// <summary>
  /// This function will be sent from the server to the clients
  /// The clients will keep track of open games with the GameButton prefabs
  /// This function will create the prefabs on their list.
  /// </summary>
  /// <param name="g"></param>
    public void NotifyNewGame(string g)
    {
        
        //"NEWGAME#" + GameID + "#" + Lobbies[GameID].GameName + "\n"
        int gameID = int.Parse(g.Split('#')[1]);
        if (!GameRoomButtons.ContainsKey(gameID))
        {
            string gameName = g.Split('#')[2];
            GameObject temp = Instantiate(GameRoomPrefab);
            temp.name = gameID.ToString();
            temp.GetComponent<GameRoomButton>().Players = 0;
            temp.GetComponent<GameRoomButton>().GameName = gameName;
            temp.GetComponent<GameRoomButton>().SetText();
            temp.transform.SetParent(Content);
            GameRoomButtons.Add(gameID, temp.GetComponent<GameRoomButton>());
        }
    }
    /// <summary>
    /// Called by a client to a server.
    /// The server will spawn a process with the next available port.
    /// This port will then be sent back to the client so they can join.
    /// </summary>
    /// <param name="g">String form of the command.</param>
    public void CreateNewGame(string g)
    {

        GenericNetworkCore.Logger("Creating room: " + g);
        //create new room instance....
        GameRoom temp = new GameRoom();
        temp.GameID = -1;
        int range = (HighPort - LowPort);
        for (int i = 0; i < range; i++)
        {
            if (Lobbies.ContainsKey(i) != true)
            {
                temp.GameID = i;
                temp.Port = i + LowPort;
                break;
            }
        }
        try
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = true;
            string[] args = System.Environment.GetCommandLineArgs();
            GenericNetworkCore.Logger("Starting new process " + args[0]);
            proc.StartInfo.FileName = args[0];
            proc.StartInfo.Arguments = "PORT_" + temp.Port + "_GAMEID_" + temp.GameID +" -batchmode -nographics >GameServer"+temp.GameID+"Log.txt";               
            temp.MyProcess = proc;
            temp.ProcessTTL = MaxGameTime;
            GenericNetworkCore.Logger("PORT_" + temp.Port + "_GAMEID_" + temp.GameID + "\nTTL " + temp.ProcessTTL);
            proc.Start();
            temp.Creator = int.Parse(g.Split('#')[2]);
            temp.GameName = g.Split('#')[1];
            
            Lobbies.Add(temp.GameID, temp); 
            GameCounter++;
            if (temp.ProcessTTL != -1)
            {     
                StartCoroutine(RoomKiller(Lobbies[temp.GameID].ProcessTTL, Lobbies[temp.GameID]));
            }
        }
        catch (System.Exception e)
        {
            GenericNetworkCore.Logger("EXCEPTION - in creating a game!!! - " + e.ToString());
        }   
    }
    /// <summary>
    /// This is a coroutine that will kill any room that is given a non -1 time to live (ttl)
    /// Once the time is up the room is destroyed.
    /// </summary>
    /// <param name="t">Time in seconds for the game to lasts</param>
    /// <param name="g">Game ID</param>
    /// <returns></returns>
    public IEnumerator RoomKiller(float t, GameRoom g )
    {
        if (IsMaster)
        {
            yield return new WaitForSecondsRealtime(t);

            yield return StartCoroutine(Disconnect(g.ServerC));
            Logger("Waiting for Game Server to Disconnect!");
            yield return new WaitForSeconds(5);
            foreach(KeyValuePair<int,Connector2> c in Connections)
            {
                Send("GAMESTART#" + LocalGameID.ToString() + "\n", c.Value.connectionID);
            }
            GenericNetworkCore.Logger(" -Trying to destroy the process!");    
            try
            {
                g.MyProcess.Kill(); 
                GenericNetworkCore.Logger(" - Removing game from dictionary\n");    
            }
            catch
            {
                Logger("Process already quit.");
            }
            if (Lobbies.ContainsKey(g.GameID))
            {
                Lobbies.Remove(g.GameID);
            }
            if(GameRoomButtons.ContainsKey(g.GameID))
            {
                GameRoomButtons.Remove(g.GameID);
            }
        }
    }
    /// <summary>
    /// This is the mmessagge a new game server will send to the lobby manager.
    /// It is specifying that it is a game server and it is ready to be played.
    /// </summary>
    /// <param name="g">String form of the command.</param>
    public void RegisterGame(string g)
    {
        GenericNetworkCore.Logger("We recieved ISServer: " + g);
        int GameID = int.Parse(g.Split('#')[1]);
        int ServerId = int.Parse(g.Split('#')[2]);
        if (IsMaster)
        {
            if (Lobbies.ContainsKey(GameID))
            {
                Lobbies[GameID].ServerC = ServerId;
                Send("JOIN#" + Lobbies[GameID].Port + "\n", Lobbies[GameID].Creator);
            }
            foreach (KeyValuePair<int, Connector2> con in Connections)
            {
                Send("NEWGAME#" + GameID + "#" + Lobbies[GameID].GameName + "\n", con.Key);
            }
        }
    }
    /// <summary>
    /// This function will be called by the game server 
    /// to tell the Lobby Manager the game has started and no one else
    /// can join.
    /// </summary>
    public void NotifyGameStarted()
    {
        if (IsGameServer)
        {
            Send("GAMESTART#" + LocalGameID.ToString() + "\n",0);
        }
    }
    /// <summary>
    /// A simple menu manager that will hide the Lobby and Netcore UI while the game is bieng played.
    /// Most likely should be overwritten.  Function is virtual.
    /// </summary>
    /// <returns>IENumerators to allow for delays.</returns>
    public override IEnumerator MenuManager()
    {
   
            
            this.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);
            this.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            yield return new WaitUntil(() => MyCore.IsConnected);
            yield return new WaitForSeconds(1);
            this.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
            yield return new WaitUntil(() => MyCore.IsConnected == false);
            yield return new WaitForSeconds(1);
            this.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
            this.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
            this.transform.GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(false);
    }

    //----------------------------------End of helper funcitons --------------------------------------

    /// <summary>
    /// Will allow the Lobby Manager to send all available room data to new agents that connect.
    /// </summary>
    /// <param name="id">The ID of the player connecting.</param>
    /// <returns>IENumerator - Allows for delays.</returns>
    public override IEnumerator OnClientConnect(int id)
    {
        yield return new WaitForSeconds(.1f);
        if (IsAgent && !IsGameServer)
        {
            MyCore.IP = IP;
        }
        if (IsMaster)
        {
            foreach (KeyValuePair<int, GameRoom> g in Lobbies)
            {
                if (!g.Value.DidStart)
                {
                    Send("NEWGAME#" + g.Value.GameID + "#" + g.Value.GameName + "\n", id);
                    Send("PLAYERS#" + g.Value.GameID + "#" + g.Value.CurrentNumberPlayers + "\n", id);
                }
            }
        }
        Logger("Agent Connected to lobby manager, looking for game!");
    }
    /// <summary>
    /// If an agent is being disconnected then this function cleans up the details.
    /// Such as disconnecting them from the game or closing the game if it is a game server.
    /// </summary>
    /// <param name="id">ID of the agent connection being closed.</param>
    /// <returns>IEnumerator to allow for delays.</returns>
    /// 
    public override void StartingDisconnect(int id)
    {  
        if (IsGameServer)
        {
            Logger("We are Disconnecting the Game Server!");
            if (MyCore.IsConnected)
            {
                StartCoroutine(MyCore.DisconnectServer());
            }
        }
    }
    public override IEnumerator OnClientDisconnect(int id)
    {    
        if (IsMaster)
        {
            int badGameID = -1;
            foreach(KeyValuePair<int,GameRoom> x in Lobbies)
            {
                if(x.Value.ServerC == id)
                {
                    badGameID = x.Key;
                    break;
                }
            }
            if (badGameID != -1)
            {
                GenericNetworkCore.Logger("Found game id that is being removed - " + badGameID);
                //Just in case the menu hasn't been cleared out.
                foreach (KeyValuePair<int, Connector2> x in Connections)
                {
                    if (x.Key != id)
                    {
                        Send("GAMESTART#" + badGameID.ToString() + "\n", x.Key);
                    }
                }
                if (Lobbies.ContainsKey(badGameID))
                {
                    Lobbies.Remove(badGameID);
                }
            }
        }
        if (IsGameServer)
        {
            Logger("We are Disconnecting the Game Server!");
            if (MyCore.IsConnected)
            {
                yield return StartCoroutine(MyCore.DisconnectServer());
            }
        }

        yield break;
    }
    /// <summary>
    /// This is called after the agents have been disconected.
    /// Game Servers are destroyed freeing up the port they were using.
    /// Clients arre sent back to the Default Return Scene.
    /// </summary>
    /// <param name="id">ID of the agent that was disconnected.</param>
    public override void OnClientDisconnectCleanup(int id)
    {
        //I should go through games first...

        if(!IsMaster)
        {
            if (MyCore.IsConnected)
            {
                MyCore.UI_Quit();
            }          
            SceneManager.LoadScene(MyCore.DefaultReturnScene);
        }
    }
    /// <summary>
    /// IF the program ends make sure we try to disconnect.
    /// </summary>
    private void OnApplicationQuit()
    {
        if (IsConnected)
        {
            UI_Quit();
        }
 
    }
    /// <summary>
    /// Slow update will check to see if it is a game server
    /// If it is a game server are the number of players different then before?
    /// If so then notify the Lobbby Manager.
    /// </summary>
    public override void OnSlowUpdate()
    {
        /*if(Console!= null)
        {
            Console.text = SystemLog;
        }*/
        if (IsAgent && CreateGame != null)
        {
            CreateGame.interactable = (MyGameName.Length > 1 && IsConnected);
        }
        if (IsGameServer)
        {
            if (MyCore.Connections.Count != LastPlayers)
            {
                LastPlayers = MyCore.Connections.Count;
                Send("PLAYERS#" + LocalGameID.ToString() + "#" + LastPlayers.ToString() +"#"+ MyCore.MaxConnections+ "\n",0);
            }
        }
    }


    //--------------------------------UI buttons -------------------------------------
    /// <summary>
    /// Used for a call back from the Unity UI.
    /// </summary>
    public void UI_CreateRoom()
    {
        if (!IsMaster && IsConnected && LocalConnectionID > -1)
        {
            Logger("Sending start game!");
            if(!Send("CREATEROOM#" + MyGameName +"#"+ LocalConnectionID + "\n", 0))
            {
                Logger("ERROR: Could not send message to server!");
            }
            else
            {
                this.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);
                this.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
        }
        //StartCoroutine(JoinGame());
    }
    /// <summary>
    /// Used as a call back from the Unity UI
    /// </summary>
    /// <param name="n">Name of the game you want to create.</param>
    public void UI_SetGameName(string n)
    {
        MyGameName = n;
        
    }
}

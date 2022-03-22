using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NETWORK_ENGINE;

///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021

public class NetworkCore     : GenericNetworkCore
{
    public bool UseMenuManager = true;
    
    //PanelControlVariables
    public GameObject DisconnectScreen;
    //Object variables
    public ExclusiveDictionary<int, NetworkID> NetObjs = new ExclusiveDictionary<int, NetworkID>();
    public int ObjectCounter = 0;
    public NetworkID[] SpawnPrefab;
    public GameObject NetworkPlayerManager;
    public object ObjLock = new object();

   
    public int LocalPlayerId
    {
        get { return LocalConnectionID; }
    }

    //Control Variables

    public ExclusiveString MasterMessage;
    public ExclusiveString UDPMasterMessage;
     
    public int ConCounter
    {
        get { return NetSystem.ConCounter; }
    }
    public int DefaultReturnScene = 0;

    /// <summary>
    /// Initializes the Network Core variables.
    /// </summary>
    void Start()
    {
        UDPMasterMessage = new ExclusiveString();
        MasterMessage = new ExclusiveString();
        UDPMasterMessage.SetData("");
        MasterMessage.SetData("");
        UsingUDP = false;
    }
    
    /// <summary>
    /// Helper function will parse out a vector 3 from 
    /// Unity's default Vector 3 ToString()
    /// </summary>
    /// <param name="v">The string form of the desired vector</param>
    /// <returns>The vector3 object of the string.</returns>
    public static Vector3 Vector3FromString(string v)
    {
        string raw = v.Trim('(').Trim(')');
        string [] args = raw.Split(',');
        return new Vector3(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()), float.Parse(args[2].Trim()));
    }
    /// <summary>
    /// This function will be called once a client is established
    /// This function will synchronize the new player with all 
    /// of the networked game objects.
    /// It will also spawn the network Player Manager.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override IEnumerator OnClientConnect(int id)
    {
        if (IsServer)
        {
            yield return new WaitForSeconds(1);
            foreach (KeyValuePair<int, NetworkID> entry in NetObjs)
            {
                while(!entry.Value.IsInit)
                {
                    yield return new WaitForSeconds(1);
                }
                string MSG = "CREATE#" + entry.Value.Type + "#" + entry.Value.Owner +
               "#" + entry.Value.NetId + "#" + entry.Value.gameObject.transform.position.ToString() + "#"
               + entry.Value.gameObject.transform.rotation.eulerAngles.ToString()+"\n";
                //Connections[ConCounter - 1].Send(Encoding.ASCII.GetBytes(MSG));
                Send(MSG, id);
            }
            yield return new WaitForSeconds(.1f);
            NetCreateObject(-1, id);
        }
        if(IsClient)
        {
            if(GameObject.FindObjectOfType<LobbyManager>()== null && UseMenuManager)
            {
                StartCoroutine(MenuManager());
            }
        }
    }
    /// <summary>
    /// This will remove all game objects for the player who is disconnecting.
    /// Also used by the server to disconnect all players when the server goes down.
    /// </summary>
    /// <param name="id">ID of the bad connection the server is removing</param>
    /// <returns>IEnumerator to delay for synchronization</returns>
    public override IEnumerator OnClientDisconnect(int id)
    {
        OnClientDisc(id);
        yield break;
    }
    
    /// <summary>
    /// IF you were a client in the last game.
    /// You will return to the Main Menu.UDPGameObjectMessages
    /// This is compatible with the Lobby Manager.
    /// </summary>
    /// <param name="id">ID of the client whos connection has been ended</param>
    public override void OnClientDisconnectCleanup(int id)
    {
        if (IsServer)
        {
            Logger("Number of Connections " + Connections.Count);
        }
        if(!IsConnected && !IsServer)
        {   
            if (GameObject.FindObjectOfType<LobbyManager>() == null)
            {
                SceneManager.LoadScene(DefaultReturnScene);
            }
        }
    }
    /// <summary>
    /// This will gather all messages that need to be sent form the NetworkIdentities
    /// Then either send it to the server or to all of the clients.
    /// This will also send the UDP messages as well.
    /// </summary>
    public override void OnSlowUpdate()
    {
        List<string> UDPMasterStringList = new List<string>();
        foreach (KeyValuePair<int, NetworkID> id in NetObjs)
        {
            //Add their message to the masterMessage (the one we send)
            MasterMessage.Append(id.Value.GameObjectMessages.ToString() + "\n");
            //UDPMasterStringList.Add(id.Value.UDPGameObjectMessages.ReadAndClear() + "\n");
        }
        //Send Master Message
        List<int> bad = new List<int>();

        string msgToSend = MasterMessage.ReadAndClear();
        foreach (KeyValuePair<int, Connector2> item in Connections)
        {
            try
            {
                //This will send all of the information to the client (or to the server if on a client).
                if (msgToSend.Trim() != "")
                {
                    Send(msgToSend, item.Key);
                }
                /*foreach (string msg in UDPMasterStringList)
                {
                    if (msg.Trim() != "")
                    {
                        Send(msg, item.Key, false);
                    }
                }*/
            }
            catch (System.Exception e)
            {
                GenericNetworkCore.Logger("Exception occured in slow update: " + e.ToString());
                bad.Add(item.Key);
            }
        }
        //MasterMessage.SetData("");//delete old values.
        foreach (int i in bad)
        {
            GenericNetworkCore.Logger("We are disconecting Connection " + i.ToString()+" from "+name+":"+this.GetType().ToString());
            this.Disconnect(i);
        }
        
    }
    /// <summary>
    /// This function get's called from TCP and UDP receive functions.
    /// Therefore, the game programmer does not have to worry about having two 
    /// seperate handle messages.
    /// </summary>
    /// <param name="commands">The string containing the received message.</param>
    public override void OnHandleMessages(string commands)
    {
        try
        {
            if (commands.Trim(' ') == "OK" && IsClient)
            {
                //Heartbeat
            }

            else if (commands.Contains("CREATE#"))
            {
                if (IsClient)
                {
                    string[] arg = commands.Split('#');
                    try
                    {
                        int o = int.Parse(arg[2]);
                        int n = int.Parse(arg[3]);
                        if (!NetObjs.ContainsKey(n))
                        {
                            Vector3 pos = NetworkCore.Vector3FromString(arg[4]);
                            Quaternion qtemp = Quaternion.Euler(NetworkCore.Vector3FromString(arg[5]));
                            int type = int.Parse(arg[1]);
                            GameObject Temp;
                            if (type != -1)
                            {
                                Temp = GameObject.Instantiate(SpawnPrefab[int.Parse(arg[1])].gameObject, pos, qtemp);
                            }
                            else
                            {
                                Temp = GameObject.Instantiate(NetworkPlayerManager, pos, qtemp);
                            }
                            Temp.GetComponent<NetworkID>().Owner = o;
                            Temp.GetComponent<NetworkID>().NetId = n;
                            Temp.GetComponent<NetworkID>().Type = type;
                            NetObjs.Add(n, Temp.GetComponent<NetworkID>());
                            Temp.GetComponent<NetworkID>().NotifyDirty();
                        }
                    }
                    catch(System.Exception e)
                    {
                        //Malformed packet.
                        GenericNetworkCore.Logger("Exception occured inside create! "+e.ToString());
                    }
                }
            }
            else if (commands.Contains("DELETE#"))
            {
                if (IsClient)
                {
                    try
                    {
                        string[] args = commands.Split('#');
                        if (NetObjs.ContainsKey(int.Parse(args[1])))
                        {
                            GameObject.Destroy(NetObjs[int.Parse(args[1])].gameObject);
                            NetObjs.Remove(int.Parse(args[1]));
                        }
                    }
                    catch (System.Exception e)
                    {
                        GenericNetworkCore.Logger("ERROR OCCURED: " + e);
                    }
                }
            }
            else if (commands.Contains("DIRTY#"))
            {
                if (IsServer)
                {
                    int id = int.Parse(commands.Split('#')[1]);
                    if (NetObjs.ContainsKey(id))
                    {
                        foreach (NetworkComponent n in NetObjs[id].gameObject.GetComponents<NetworkComponent>())
                        {
                            n.IsDirty = true;
                        }
                    }
                }
            }
            else if (commands.Contains("COMMAND#") || commands.Contains("UPDATE#"))
            {
                //We will assume it is Game Object specific message
                //string msg = "COMMAND#" + myId.netId + "#" + var + "#" + value;
                //Debug.Log("Raw command from network Core: " + commands);
                string[] args = commands.Trim().Split('#');
                int n = int.Parse(args[1]);
                if (NetObjs.ContainsKey(n))
                {
                    NetObjs[n].Net_Update(args[0], args[2], args[3]);
                }
            }
        }
        catch(System.FormatException)
        {
            GenericNetworkCore.Logger("Dropping malformed packet " + commands);
        }
        catch(System.Exception e)
        {
            GenericNetworkCore.Logger("Exception in handle message: "+e.ToString());
            GenericNetworkCore.Logger(commands);
        }
        
    }

    /// <summary>
    /// This will spawn a game object across the network.
    /// The prefab will be identified by the index = to type.
    /// Server Only
    /// </summary>
    /// <param name="type">Index on the spawn prefab array</param>
    /// <param name="ownMe">Which player owns the object, -1 for server.</param>
    /// <param name="initPos">The initial position for the new object.</param>
    /// <param name="rotation">The initial rotation for the desired game object.</param>
    /// <returns>This function returns a pointer to the new game object.  (Server only)</returns>
    public GameObject NetCreateObject(int type, int ownMe, Vector3 initPos = new Vector3(), Quaternion rotation = new Quaternion())
    {
        if (IsServer)
        {
            GameObject temp;
            lock (ObjLock)
            {
                if (type != -1)
                {
                    temp = GameObject.Instantiate(SpawnPrefab[type].gameObject, initPos, rotation);
                }
                else
                {
                    temp = GameObject.Instantiate(NetworkPlayerManager, initPos, rotation);
                }
                temp.GetComponent<NetworkID>().Owner = ownMe;
                temp.GetComponent<NetworkID>().NetId = ObjectCounter;
                temp.GetComponent<NetworkID>().Type = type;
                NetObjs.Add(ObjectCounter, temp.GetComponent<NetworkID>());
                string MSG = "CREATE#" + type + "#" + ownMe +
             "#" + (ObjectCounter) + "#" + initPos.ToString() + "#" +
             rotation.eulerAngles.ToString() + "\n";
                
                ObjectCounter++;
                if(ObjectCounter <0)
                {
                    ObjectCounter = 0;
                }
                while(NetObjs.ContainsKey(ObjectCounter))
                {
                    ObjectCounter++;
                }
                MasterMessage.Append(MSG);
                foreach (NetworkComponent n in temp.GetComponents<NetworkComponent>())
                {
                    //Force update to all clients.
                    n.IsDirty = true;
                }
            }
        return temp;
        }
        else
        {
            return null;
        }

    }

    /// <summary>
    /// This will destroy an object with a given ID 
    /// across the network.
    /// Serer Only.
    /// </summary>
    /// <param name="netIDBad">The net ID of the object to Destroy.</param>
    public void NetDestroyObject(int netIDBad)
    {
        if (IsServer)
        {
            try
            {
                if (NetObjs.ContainsKey(netIDBad))
                {
                    Destroy(NetObjs[netIDBad].gameObject);
                    NetObjs.Remove(netIDBad);
                    string msg = "DELETE#" + netIDBad + "\n";
                    MasterMessage.Append(msg);
                }
            }
            catch
            {
                //Already been destroyed.
            }

        }
    }
    /// <summary>
    /// This is a virtual function intended to be overriden.
    /// By default thsi function will destroy all objects that belong to a player
    /// that is disconnecting.  
    /// (Note disconnection may not have occured yet).
    /// 
    /// </summary>
    /// <param name="badConnection">The ID of the player who is leaving.</param>
    public virtual void OnClientDisc(int badConnection)
    {
        if (IsServer)
        {
            //Remove Connection from server
            List<int> badObjs = new List<int>();
            foreach (KeyValuePair<int, NetworkID> obj in NetObjs)
            {
                if (obj.Value.Owner == badConnection)
                {
                    badObjs.Add(obj.Key);
                    //I have to add the key to a temp list and delete
                    //it outside of this for loop
                }
            }
            //Now I can remove the netObjs from the dictionary.
            for (int i = 0; i < badObjs.Count; i++)
            {
                NetDestroyObject(badObjs[i]);
            }
        }
        if (IsClient)
        {
            foreach (KeyValuePair<int, NetworkID> obj in NetObjs)
            {
                Destroy(obj.Value.gameObject);
            }
            NetObjs.Clear();
            //LocalConnectionID = -10;
        }
    }
    /// <summary>
    /// This will request the lobby manager to remove the game from the available lists.
    /// Once your game logic says all players are ready, then start the game.
    /// The game manager on the server should call this function.
    /// </summary>
    public void NotifyGameStart()
    {
        if(IsServer)
        {
            if(GameObject.FindObjectOfType<LobbyManager>() != null)
            {
                GameObject.FindObjectOfType<LobbyManager>().NotifyGameStarted();
                OnGameStarted();
            }
            StopListening();
        }
    }

    public void StopListening()
    {
        
        this.NetSystem.Listener.Dispose();
        IsListening = false;
        if(!this.NetSystem.ThreadListener.Join(50))
        {
            this.NetSystem.ThreadListener.Abort();
        }
    }


    /// <summary>
    /// A virtual function to give the player a hook to insert custom code once a game has started.
    /// </summary>
    public virtual void OnGameStarted()
    {

    }

    public override void OnServerDisconnectCleanup()
    {
        if(IsServer)
        {
            Logger("Number of connections: " + Connections.Count);
        }
        if(GameObject.FindObjectOfType<LobbyManager>() == null && !IsServer)
        {
            SceneManager.LoadScene(0);
        }

    }

    public override IEnumerator MenuManager()
    {      
        yield return new WaitUntil(() => IsConnected);
        this.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }

    public override void StartingDisconnect(int id)
    {
        if (IsServer)
        {
            Logger("Number of connections: " + Connections.Count);
        }
        if(IsClient)
        {
            OnClientDisc(id);
        }
        if(DisconnectScreen != null && !IsServer)
        {
            DisconnectScreen.SetActive(true);
        }
    }

}

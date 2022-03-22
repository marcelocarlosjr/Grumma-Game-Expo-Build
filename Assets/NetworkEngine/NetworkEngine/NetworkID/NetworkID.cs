using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021

namespace NETWORK_ENGINE
{

    public class DictionaryStringBuilder
    {
        private ExclusiveDictionary<string, string> _raw;
        public DictionaryStringBuilder()
        {
            _raw = new ExclusiveDictionary<string, string>();
        }
        public override string ToString()
        {
            string returnData = "";
            foreach (KeyValuePair<string, string> x in _raw)
            {
                returnData += x.Value;
            }
            _raw.Clear();
            return returnData;
        }

        public void AddValue(string Flag, string Command)
        {
            if (_raw.ContainsKey(Flag))
            {
                _raw[Flag] = Command;
            }
            else
            {
                _raw.Add(Flag, Command);
            }
        }

    }
    public class NetworkID : MonoBehaviour
    {
        public int Type;
        public int Owner = -10;
        public int NetId = -10;
        public bool IsInit;
        public bool IsLocalPlayer;
        public bool IsServer
        {
            get { return MyCore.IsServer; }
        }
        public bool IsClient
        {
            get { return MyCore.IsClient; }
        }
        public bool IsConnected
        {
            get { return MyCore.IsConnected; }

        }
        public float UpdateFrequency = .1f;
        public NetworkCore MyCore;
        public DictionaryStringBuilder GameObjectMessages = new DictionaryStringBuilder();
        public ExclusiveString UDPGameObjectMessages = new ExclusiveString();
        public object _lock = new object();

        // Use this for initialization
        void Start()
        {
            //UDPGameObjectMessages = new ExclusiveString();
            //GameObjectMessages = new ExclusiveString(); 
            MyCore = GameObject.FindObjectOfType<NetworkCore>();
            if(MyCore == null)
            {
                throw new System.Exception("There is no network core in the scene!");
            }
            //IsServer = MyCore.IsServer;
            //IsClient = MyCore.IsClient;
    
            StartCoroutine(SlowStart());

        }
        IEnumerator SlowStart()
        {      

            if (!IsServer && !IsClient)
            {
                //This will ONLY be true if the object was in the scene before the connection
                yield return new WaitUntil(() => (MyCore.IsServer || MyCore.IsClient));
   
                yield return new WaitForSeconds(.1f);
            }
            yield return new WaitForSeconds(.1f);  //This should be here.
            if (IsClient)
            {
                //Then we know we need to destroy this object and wait for it to be re-created by the server
                if (NetId == -10)
                {   
                    Debug.Log("We are destroying the non-server networked objects");
                    GameObject.Destroy(this.gameObject);
                }
            }
            if (IsServer && NetId == -10)
            {
                //We need to add ourselves to the networked object dictionary
                Type = -1;
                for (int i = 0; i < MyCore.SpawnPrefab.Length; i++)
                {
                    if (MyCore.SpawnPrefab[i].gameObject.name == this.gameObject.name.Split('(')[0].Trim())
                    {
                        Type = i;              
                        break;
                    }
                }
                if (Type == -1)
                {
                    Debug.LogError("Game object not found in prefab list! Game Object name - " + this.gameObject.name.Split('(')[0].Trim());
                    throw new System.Exception("FATAL - Game Object not found in prefab list!");
                }
                else
                {
                    lock (MyCore.ObjLock)
                    {
                        NetId = MyCore.ObjectCounter;
                        MyCore.ObjectCounter++;
                        Owner = -1;
                        MyCore.NetObjs.Add(NetId, this);
                    }
                }
            }

            yield return new WaitUntil(() => (Owner != -10 && NetId != -10));
            if(Owner == MyCore.LocalPlayerId)
            {
                IsLocalPlayer = true;
            }
            else
            {
                IsLocalPlayer = false;
            }
            IsInit = true;
            if (IsClient)
            {
                NotifyDirty();
            }
        }
        public void AddMsg(string msg,string flag, bool useTcp=true)
        {        
            //if (useTcp)
            //{
                GameObjectMessages.AddValue(flag, msg + "\n");
            //}
            //else
            //{
               // UDPGameObjectMessages.Append(msg + "\n");
            //}    
        }


        public void Net_Update(string type, string var, string value)
        {
            //Get components for network behaviours
            //Destroy self if owner connection is done.
            try
            {
                if (MyCore.IsServer && MyCore.Connections.ContainsKey(Owner) == false 
                    && Owner != -1)
                {
                    MyCore.NetDestroyObject(NetId);
                }
            }
            catch (System.NullReferenceException)
            {
                //Has not been initialized yet.  Ignore.
            }
            try
            {
                if (MyCore == null)
                {
                    MyCore = GameObject.FindObjectOfType<NetworkCore>();
                }
                if ((MyCore.IsServer && type == "COMMAND")
                    || (MyCore.IsClient && type == "UPDATE"))
                {
                    NetworkComponent[] myNets = gameObject.GetComponents<NetworkComponent>();
                    for (int i = 0; i < myNets.Length; i++)
                    {
                        myNets[i].HandleMessage(var, value);
                    }
                }
            }
            catch (System.Exception)
            {
                //Debug.Log("Game Object " + name + " Caught Exception: " + e.ToString() + "\n" + type + "\n" + var +"\n" +value);
                //This can happen if myCore has not been set.  
                //I am not sure how this is possible, but it should be good for the next round.
            }
        }

        public void NotifyDirty()
        {
            this.AddMsg("DIRTY#" + NetId + "\n", "DIRTY");
        }
    }
}
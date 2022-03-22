using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///This code was written by Dr. Bradford A. Towle Jr.
///And is intended for educational use only.
///4/11/2021

namespace NETWORK_ENGINE
{


    [RequireComponent(typeof(NetworkID))]
    public abstract class NetworkComponent : MonoBehaviour
    {
        public NetworkCore MyCore;
        public NetworkID MyId;

        public bool IsDirty = false;
        public bool IsClient
        {
            get { return MyCore.IsClient; }
        }
        public bool IsServer
        {
            get { return MyCore.IsServer; }
        }
        public bool IsLocalPlayer
        {
            get { return MyId.IsLocalPlayer; }
        }
        public int Owner
        {
            get { return MyId.Owner; }
        }
        public int Type
        {
            get { return MyId.Type; }
        }
        public int NetId
        {
            get { return MyId.NetId; }
        }
        public bool IsConnected
        {
            get { return MyCore.IsConnected; }
        }
        // Start is called before the first frame update
        public void Awake()
        {
            MyId = gameObject.GetComponent<NetworkID>();
            MyCore = GameObject.FindObjectOfType<NetworkCore>();
            if(MyCore == null)
            {
                throw new System.Exception("ERROR: There is no network core on the scene.");
            }
            if(MyId == null)
            {
                throw new System.Exception("ERROR: There is no network ID on this object");
            }
            StartCoroutine(SlowStart());
        }
        void Start()
        {
         
        }
        IEnumerator SlowStart()
        {
            yield return new WaitUntil(() => MyId.IsInit);
            NetworkedStart();
            StartCoroutine(SlowUpdate());
        }
        public abstract IEnumerator SlowUpdate();
        public abstract void HandleMessage(string flag, string value);
        public abstract void NetworkedStart();
        public void SendCommand(string var, string value, bool useTcp = true)
        {
            var = var.Replace('#', ' ');
            var = var.Replace('\n', ' ');
            value = value.Replace('#', ' ');
            value = value.Replace('\n', ' ');
            if (MyCore != null && MyCore.IsClient && IsLocalPlayer)
            {    
                if (useTcp)
                {
                    string msg = "COMMAND#" + MyId.NetId + "#" + var + "#" + value;
                    MyId.AddMsg(msg,var, useTcp);
                }
                else if (!useTcp && MyId.UDPGameObjectMessages.Str.Contains(var) == false)
                {
                    string msg = "COMMAND#" + MyId.NetId + "#" + var + "#" + value;
                    MyId.AddMsg(msg,var, useTcp);
                }
            }
        }
        public void SendUpdate(string var, string value, bool useTcp = true)
        {
            var = var.Replace('#', ' ');
            var = var.Replace('\n', ' ');
            value = value.Replace('#', ' ');
            value = value.Replace('\n', ' ');
            if (MyCore != null && MyCore.IsServer )
            {
               
                if ( useTcp)
                {
                    string msg = "UPDATE#" + MyId.NetId + "#" + var + "#" + value;
                    MyId.AddMsg(msg,var, useTcp);
                }
                else if (!useTcp && MyId.UDPGameObjectMessages.Str.Contains(var) == false )
                {
                    string msg = "UPDATE#" + MyId.NetId + "#" + var + "#" + value;
                    MyId.AddMsg(msg,var, useTcp);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NPM : NetworkComponent
{
    public GameObject NPMcanvas;
    public string Name;
    public int Class;
    public bool Ready;

    public InputField NameInput;
    public Toggle ReadyInput;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "NAME")
        {
            Name = value;
            if (IsServer)
            {
                SendUpdate("NAME", Name);
            }
        }
        if (flag == "CLASS")
        {
            Class = int.Parse(value);
            if (IsServer)
            {
                SendUpdate("CLASS", Class.ToString());
            }
        }
        if (flag == "READY")
        {
            Ready = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("READY", Ready.ToString());
                if (Ready)
                {
                    SpawnPlayer();
                }
            }
        }
    }
    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsLocalPlayer)
            {
                if (name == "" || Class == 0)
                {
                    ReadyInput.interactable = false;
                }
                else
                {
                    ReadyInput.interactable = true;
                }
            }
            if (IsServer)
            {
                if (IsDirty)
                {
                    SendUpdate("CLASS", Class.ToString());
                    SendUpdate("NAME", Name);
                    SendUpdate("READY", Ready.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.5f);
        }
    }
    public override void NetworkedStart()
    {
        Class = 0;
        Name = "";

        NPMcanvas = FindObjectOfType<UIInputManager>().gameObject;
        NPMcanvas.GetComponent<UIInputManager>().LocalNPM = this.gameObject;
        NameInput = NPMcanvas.GetComponent<UIInputManager>().NameInput;
        ReadyInput = NPMcanvas.GetComponent<UIInputManager>().ReadyInput;
    }
    public void SpawnPlayer()
    {
        if (IsServer)
        {
            MyCore.NetCreateObject(Class - 1, Owner, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
    public void GetName(string value)
    {
        if (IsLocalPlayer)
        {
            Name = value;
            SendCommand("NAME", Name.ToString());
        }
    }
    public void GetClass(int value)
    {
        if (IsLocalPlayer)
        {
            Class = value;
            SendCommand("CLASS", Class.ToString());
        }
    }
    public void GetReady(bool value)
    {
        Ready = value;
        SendCommand("READY", Ready.ToString());
        if (Ready)
        {
            this.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
       
    }
}

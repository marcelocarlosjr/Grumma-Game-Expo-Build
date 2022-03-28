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
            if (IsLocalPlayer)
            {
                if (Ready)
                {
                    RemoveCanvas();
                    ClearUI();
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
        if (IsLocalPlayer)
        {
            NPMcanvas = FindObjectOfType<UIInputManager>().gameObject;
            NPMcanvas.GetComponent<UIInputManager>().LocalNPM = this.gameObject;
            NameInput = NPMcanvas.GetComponent<UIInputManager>().NameInput;
            ReadyInput = NPMcanvas.GetComponent<UIInputManager>().ReadyInput;

            ClearUI();
        }
    }
    public void ClearUI()
    {
        Class = 0;
        ReadyInput.isOn = false;
        ReadyInput.interactable = false;
        Ready = false;
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
        if (IsLocalPlayer)
        {
            Ready = value;
            SendCommand("READY", Ready.ToString());
        }
    }

    public void RemoveCanvas()
    {
        NPMcanvas.gameObject.SetActive(false);
    }
    public void ShowCanvas()
    {
        NPMcanvas.gameObject.SetActive(true);
    }

    private void Start()
    {
       
    }
}

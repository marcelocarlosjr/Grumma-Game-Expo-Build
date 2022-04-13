using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NPM : NetworkComponent
{
    public GameObject NPMcanvas;
    public GameObject NPMpanel;
    public GameObject TUTpanel;
    public string Name;
    public int Class;
    public bool Ready;
    public bool Begin;

    public InputField NameInput;
    public Toggle ReadyInput;
    public Button BeginButton;

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
        if (flag == "READY") //this is new functionality for tutorial screen
        {
            Ready = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("READY", Ready.ToString());
            }
            if (IsLocalPlayer)
            {
                if (Ready)
                {
                    ViewTutorial();
                    RemoveCanvas();
                }
            }
        }
        if (flag == "BEGIN") //this is old "READY" changed all Ready to Begin
        {
            Begin = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("BEGIN", Begin.ToString());
                if (Begin)
                {
                    SpawnPlayer();
                }
            }
            if (IsLocalPlayer)
            {
                if (Begin)
                {
                    RemoveTutorial();
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
                    SendUpdate("BEGIN", Begin.ToString());
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
            NPMpanel = NPMcanvas.transform.GetChild(0).gameObject; //changed this to reflect scene hierarchy changes
            TUTpanel = NPMcanvas.transform.GetChild(1).gameObject; //same thing as ^
            NPMcanvas.GetComponent<UIInputManager>().LocalNPM = this.gameObject;
            NameInput = NPMcanvas.GetComponent<UIInputManager>().NameInput;
            ReadyInput = NPMcanvas.GetComponent<UIInputManager>().ReadyInput;
            BeginButton = NPMcanvas.GetComponent<UIInputManager>().BeginButton;

            ClearUI();
        }
    }
    public void ClearUI()
    {
        Class = 0;
        ReadyInput.isOn = false;
        ReadyInput.interactable = false;
        Ready = false;
        Begin = false;
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
    public void GetBegin() //added functionality for tutorial screen
    {
        if (IsLocalPlayer)
        {
            Begin = true;
            BeginButton.interactable = false;
            SendCommand("BEGIN", Begin.ToString());
        }
    }

    public void ViewTutorial() //added functionality for tutorial screen
    {
        TUTpanel.gameObject.SetActive(true);
        TUTpanel.gameObject.transform.GetChild(Class).gameObject.SetActive(true);
    }
    public void RemoveTutorial()
    {
        TUTpanel.gameObject.SetActive(false);
    }
    public void RemoveCanvas()
    {
        NPMpanel.gameObject.SetActive(false);
    }
    public void ShowCanvas()
    {
        NPMpanel.gameObject.SetActive(true);
    }

    private void Start()
    {
       
    }
}

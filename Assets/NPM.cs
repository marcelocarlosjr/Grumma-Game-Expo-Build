using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NPM : NetworkComponent
{
    public string Name;
    public int CharSelected;
    public bool Ready;

    public override IEnumerator SlowUpdate()
    {
        throw new System.NotImplementedException();
    }

    public override void HandleMessage(string flag, string value)
    {
        throw new System.NotImplementedException();
    }

    public override void NetworkedStart()
    {
        throw new System.NotImplementedException();
    }
}

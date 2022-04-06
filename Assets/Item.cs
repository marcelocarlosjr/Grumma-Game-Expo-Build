using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Item : NetworkComponent
{
    public ItemObject item;

    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer)
            {
                if (IsDirty)
                {
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}

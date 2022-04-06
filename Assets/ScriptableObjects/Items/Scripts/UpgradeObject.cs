using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Upgrade Object", menuName = "Inventory System/Items/Upgrade")]
public class UpgradeObject : ItemObject
{
    public float statModifier;
    public void Awake()
    {
        type = ItemType.Upgrade;
    }
}

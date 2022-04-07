using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Default,
    Potion,
    Upgrade
}
public abstract class ItemObject : ScriptableObject
{
    public Sprite uiDisplay;
    public ItemType type;
    [TextArea(15,20)]
    public string description;
}

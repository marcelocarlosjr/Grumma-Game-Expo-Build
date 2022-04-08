using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Default,
    Potion,
    Upgrade
}

public enum Attribute
{
    Damage_Increase,
    Health_Increase,
    Regen_Increase,
    MoveSpeed_Increase,
    AttackSpeed_Increase,
    Stamina_Increase,
    EXP_Increase
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public abstract class ItemObject : ScriptableObject
{
    public int SpawnPrefabInt;
    public Sprite uiDisplay;
    public ItemType type;
    public Attribute attribute;
    public Rarity rarity;
    [TextArea(15,20)]
    public string description;

    public float amount;
}

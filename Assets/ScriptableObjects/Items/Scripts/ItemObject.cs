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
    public Sprite uiDisplay;
    public ItemType type;
    [TextArea(15,20)]
    public string description;
}

public class ItemBuff
{
    public Attribute attribute;
    public int value;
    public int min;
    public int max;

    public ItemBuff(int _min, int _max)
    {
        min = _min;
        max = _max;
    }

    public void GenerateValue()
    {
        value = UnityEngine.Random.Range(min, max);
    }
}

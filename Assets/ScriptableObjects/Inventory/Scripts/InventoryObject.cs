using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemDatabaseObject database;
    public List<InventorySlot> Container = new List<InventorySlot>();
    public void AddItem(ItemObject _item, int _amount, int _owner)
    {
        Container.Add(new InventorySlot(database.GetID[_item], _item, _amount));
        
        if (_owner != -99)
        {
            foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                if (pc.Owner == _owner)
                {
                    pc.AddStat(_item.attribute.ToString(), _item.rarity.ToString());

                    pc.UpdateInv(database.GetID[_item], _amount);
                }
            }
        }
    }
    public void AddItem(ItemObject _item, int _amount, int _owner, bool Add)
    {
        Container.Add(new InventorySlot(database.GetID[_item], _item, _amount));

        if (_owner != -99)
        {
            foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                if (pc.Owner == _owner)
                {
                    if (Add)
                    {
                        pc.AddStat(_item.attribute.ToString(), _item.rarity.ToString());
                    }

                    pc.UpdateInv(database.GetID[_item], _amount);
                }
            }
        }
    }

    public void RemoveItem(int _index, int _id, int _amount, int _owner, Vector3 position, Vector3 directionForward, Vector3 directionRight)
    {
        Container.RemoveAt(_index);

        if (FindObjectOfType<NetworkCore>().LocalConnectionID == -1)
        {
            foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                if (pc.Owner == _owner)
                {
                    pc.RemoveStat(database.GetItem[_id].attribute.ToString(), database.GetItem[_index].rarity.ToString());
                }
            }

            for (int i = 0; i < _amount; i++)
            {
                GameObject temp = FindObjectOfType<NetworkCore>().NetCreateObject(database.GetItem[_id].SpawnPrefabInt, -1, position, Quaternion.identity);
                temp.GetComponent<Item>().ThrowItem((position + (directionForward * 2) + (directionRight * Random.Range(-.8f, .8f))));
            }
        }

        return;
    }
    public void RemoveALLItem(int _index, int _id, int _amount, int _owner, Vector3 position)
    {
        Container.RemoveAt(_index);

        if (FindObjectOfType<NetworkCore>().LocalConnectionID == -1)
        {
            foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                if (pc.Owner == _owner)
                {
                    pc.RemoveStat(database.GetItem[_id].attribute.ToString(), database.GetItem[_index].rarity.ToString());
                }
            }

            for (int i = 0; i < _amount; i++)
            {
                GameObject temp = FindObjectOfType<NetworkCore>().NetCreateObject(database.GetItem[_id].SpawnPrefabInt, -1, position, Quaternion.identity);
                temp.GetComponent<Item>().ThrowItem(position + (new Vector3(0,1,0) * Random.Range(-1.5f, 1.5f)) + (new Vector3(1, 0, 0) * Random.Range(-1.5f, 1.5f)));
            }
        }

        return;
    }

    public void OnAfterDeserialize()
    {
        for (int i = 0; i < Container.Count; i++)
        {
            Container[i].item = database.GetItem[Container[i].ID];
        }
    }

    public void OnBeforeSerialize()
    {

    }
}

[System.Serializable]
public class InventorySlot
{
    public int ID;
    public ItemObject item;
    public int amount;
    public InventorySlot(int _id, ItemObject _item, int _amount)
    {
        ID = _id;
        item = _item;
        amount = _amount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }
}
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
        for(int i = 0; i < Container.Count; i++)
        {
            if(Container[i].item == _item)
            {
                Container[i].AddAmount(_amount);
                if (_owner != -99)
                {
                    foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
                    {
                        if (pc.Owner == _owner)
                        {
                            pc.UpdateInv(database.GetID[_item], _amount);
                        }
                    }
                }
                return;
            }
        }
        Container.Add(new InventorySlot(database.GetID[_item], _item, _amount));
        if (_owner != -99)
        {
            foreach (PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                if (pc.Owner == _owner)
                {
                    pc.UpdateInv(database.GetID[_item], _amount);
                }
            }
        }
    }

    public void RemoveItem(int _index, int _id, int _amount, Vector3 position, Vector3 directionForward, Vector3 directionRight)
    {
        Container.RemoveAt(_index);

        if(FindObjectOfType<NetworkCore>().LocalConnectionID == -1)
        {
            for (int i = 0; i < _amount; i++)
            {

                FindObjectOfType<NetworkCore>().NetCreateObject(database.GetItem[_id].SpawnPrefabInt, -1, (position + (directionForward * 2) + (directionRight * Random.Range(-1.5f, 1.5f))), Quaternion.identity);

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInventory : MonoBehaviour
{
    public InventoryObject Inventory;
    public bool InventoryLinked;
    Dictionary<InventorySlot, GameObject> ItemsDisplayed = new Dictionary<InventorySlot, GameObject>();

    private void Start()
    {
        InventoryLinked = false;
    }
    void Update()
    {
        if (!InventoryLinked)
        {
            foreach(PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                if(pc.Owner == FindObjectOfType<NetworkCore>().LocalConnectionID)
                {
                    Inventory = pc.Inventory;
                    CreateDisplay();
                    InventoryLinked = true;
                }
            }
        }

        if (InventoryLinked && FindObjectOfType<NetworkCore>().LocalConnectionID != -1)
        {
            UpdateDisplay();
        }
    }
    public void CreateDisplay()
    {
        for (int i = 0; i < Inventory.Container.Count; i++)
        {
            var obj = Instantiate(Inventory.Container[i].item.prefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponentInChildren<Text>().text = Inventory.Container[i].amount.ToString("n0");
            ItemsDisplayed.Add(Inventory.Container[i], obj);
        }
    }
    public void UpdateDisplay()
    {

        for (int i = 0; i < Inventory.Container.Count; i++)
        {
            if (ItemsDisplayed.ContainsKey(Inventory.Container[i]))
            {
                ItemsDisplayed[Inventory.Container[i]].GetComponentInChildren<Text>().text = Inventory.Container[i].amount.ToString("n0");
            }
            else
            {
                var obj = Instantiate(Inventory.Container[i].item.prefab, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponentInChildren<Text>().text = Inventory.Container[i].amount.ToString("n0");
                ItemsDisplayed.Add(Inventory.Container[i], obj);
            }
        }
    }
}

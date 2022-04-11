using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DisplayInventory : MonoBehaviour
{
    public MouseItem mouseItem = new MouseItem();

    public GameObject InventoryPrefab;
    public InventoryObject Inventory;
    public bool InventoryLinked;
    public bool DisplayCreated;
    Dictionary<InventorySlot, GameObject> ItemsDisplayed = new Dictionary<InventorySlot, GameObject>();

    public Vector2 MousePos;

    private void Start()
    {
        InventoryLinked = false;
    }
    void Update()
    {
        foreach(PlayerController pc in FindObjectsOfType<PlayerController>())
        {
            if(pc.Owner == FindObjectOfType<NetworkCore>().LocalConnectionID)
            {
                Inventory = pc.Inventory;
                if (!DisplayCreated)
                {
                    CreateDisplay();
                    DisplayCreated = true;
                }
                InventoryLinked = true;
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
            var obj = Instantiate(InventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.transform.GetChild(0).GetComponent<Image>().sprite = Inventory.Container[i].item.uiDisplay;
            obj.GetComponentInChildren<Text>().text = Inventory.Container[i].amount.ToString("n0") + " ";

            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });


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
                ItemsDisplayed[Inventory.Container[i]].GetComponent<UIItemData>().Amount = Inventory.Container[i].amount;
                foreach (UIItemData item in FindObjectsOfType<UIItemData>())
                {
                    item.index = item.gameObject.transform.GetSiblingIndex();
                }
            }
            else
            {
                var obj = Instantiate(InventoryPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.transform.GetChild(0).GetComponent<Image>().sprite = Inventory.Container[i].item.uiDisplay;

                obj.GetComponent<UIItemData>().index = obj.transform.GetSiblingIndex();
                obj.GetComponent<UIItemData>().Id = Inventory.Container[i].ID;
                obj.GetComponent<UIItemData>().Amount = Inventory.Container[i].amount;

                obj.GetComponentInChildren<Text>().text = Inventory.Container[i].amount.ToString("n0");

                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });

                ItemsDisplayed.Add(Inventory.Container[i], obj);
                foreach (UIItemData item in FindObjectsOfType<UIItemData>())
                {
                    item.index = item.gameObject.transform.GetSiblingIndex();
                }
            }
        }
    }

    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj)
    {
        if (obj.GetComponent<UIItemData>())
        {
            mouseItem.hoverObj = obj;
        }

    }
    public void OnExit(GameObject obj)
    {
        mouseItem.hoverObj = null;
    }
    public void OnDragStart(GameObject obj)
    {
        var mouseObject = new GameObject();
        var rt = mouseObject.AddComponent<RectTransform>();
        rt.sizeDelta = obj.GetComponent<RectTransform>().sizeDelta;
        mouseObject.transform.SetParent(transform.parent);
        if (obj.GetComponent<UIItemData>())
        {
            var img = mouseObject.AddComponent<Image>();
            img.sprite = obj.transform.GetChild(0).GetComponent<Image>().sprite;
            img.raycastTarget = false;
        }

        mouseItem.obj = mouseObject;
        mouseItem.item = obj;
    }
    public void OnDragEnd(GameObject obj)
    {
        if(mouseItem.hoverObj == null)
        {
            foreach(PlayerController pc in FindObjectsOfType<PlayerController>())
            {
                if(pc.Owner == FindObjectOfType<NetworkCore>().LocalConnectionID)
                {
                    UIItemData temp = obj.GetComponent<UIItemData>();
                    ItemsDisplayed.Remove(Inventory.Container[temp.index]);
                    pc.RemoveInv(temp.index, temp.Id, temp.Amount);
                    Destroy(obj);
                }
            }
        }
        Destroy(mouseItem.obj);
        mouseItem.item = null;
    }
    public void OnDrag(GameObject obj)
    {
        if(mouseItem.obj != null)
        {
            mouseItem.obj.GetComponent<RectTransform>().position = MousePos;
        }
    }

    public void GetMousePos(Vector2 context)
    {
        MousePos = context;
    }

    public class MouseItem
    {
        public GameObject obj;
        public GameObject item;
        public InventorySlot hoverItem;
        public GameObject hoverObj;
    }

}

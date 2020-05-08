using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory
{
    public int maxSlots;
    public Dictionary<int, InventorySlot> inventorySlots = new Dictionary<int, InventorySlot>();
    public Inventory(int maxSlots)
    {
        for(int i = 0; i < maxSlots; i++)
        {
            InventorySlot inventorySlot = new InventorySlot(i);
            inventorySlots.Add(i, inventorySlot);
        }
    }
    public void UpdateSlots()
    {
        foreach(InventorySlot slot in inventorySlots.Values)
        {
            slot.Update();
        }
    }
    public bool AddItem(InventorySlot slot, Item item, int amount)
    {
        bool didAdd = false;
        if (slot.ItemMatch(item))
        {
            didAdd = slot.AddAmount(amount);
        }
        else
        {
            didAdd = false;
        }
        return didAdd;
    }
    public bool RemoveItem(InventorySlot slot, Item item, int amount)
    {
        bool didRemove = false;
        if (slot.ItemMatch(item))
        {
            didRemove = slot.RemoveAmount(amount);
            if (didRemove)
            {
                UpdateSlots();
            }
        }
        else
        {
            didRemove = false;
        }

        return didRemove;
    }
    public Item GetItemInSlot(InventorySlot slot)
    {
        return slot.GetItem();
    }
    public int GetAmountInSlot(InventorySlot slot)
    {
        return slot.GetAmount();
    }
    public InventorySlot GetInventorySlot(int ID)
    {
        return inventorySlots[ID];
    }
    public void IncreaseCapacity(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            int ID = inventorySlots.Count + i;
            InventorySlot inventorySlot = new InventorySlot(ID);
            inventorySlots.Add(ID,inventorySlot);
        }
    }
}

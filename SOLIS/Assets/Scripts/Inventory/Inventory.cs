using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public InventorySlot heldSlot;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public Inventory(int numSlots)
    {
        heldSlot = new InventorySlot();
        for(int i = 0; i < numSlots; i++)
        {
            InventorySlot slot = new InventorySlot(new Item(ItemType.wood, 100), 125);
            inventorySlots.Add(slot);
        }
    }
    public void InventorySlotClick(InventorySlot slot, int key)
    {
        if (key == 0)
        {
            if (heldSlot.GetItem() != null)
            {
                if (slot.GetItem() != null && slot.GetItem().GetItemType() == heldSlot.GetItem().GetItemType())
                {
                    slot.AddAmount(heldSlot.GetAmount());
                    heldSlot.SetAmount(0);
                }
                else if(slot.GetItem() == null)
                {
                    slot.SetItem(heldSlot.GetItem());
                    slot.SetAmount(heldSlot.GetAmount());
                    heldSlot.SetAmount(0);
                }
            }
            else
            {
                heldSlot.SetItem(slot.GetItem());
                heldSlot.SetAmount(slot.GetAmount());
                slot.SetAmount(0);
            }
        }
    }
}

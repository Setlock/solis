using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
    int ID;
    Item item;
    int amount;
    public InventorySlot(int ID)
    {
        this.ID = ID;
        item = null;
        amount = 0;
    }
    public InventorySlot(int ID, Item item, int amount)
    {
        this.ID = ID;
        this.item = item;
        this.amount = amount;
    }
    public void Update()
    {
        if (IsEmpty())
        {
            item = null;
            amount = 0;
        }
    }
    public bool IsEmpty()
    {
        return amount <= 0 || item == null;
    }
    public bool ItemMatch(Item item)
    {
        return this.item.GetName().Equals(item.GetName());
    }
    public bool CanRemove(int sub)
    {
        return amount - sub >= 0;
    }
    public bool RemoveAmount(int sub)
    {
        if(this.amount-sub >= 0)
        {
            this.amount -= sub;
            return true;
        }
        return false;
    }
    public bool AddAmount(int add)
    {
        if(this.amount + add <= item.GetMaxStack())
        {
            this.amount += add;
            return true;
        }
        return false;
    }
    public void SetItem(Item item)
    {
        this.item = item;
    }
    public void SetAmount(int amt)
    {
        this.amount = amt;
    }
    public Item GetItem()
    {
        return item;
    }
    public int GetAmount()
    {
        return amount;
    }
}

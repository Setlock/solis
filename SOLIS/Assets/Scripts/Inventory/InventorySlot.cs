using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
    Item item;
    int amt;
    public InventorySlot(Item item, int amt)
    {
        this.item = item;
        this.amt = amt;
    }
    public InventorySlot()
    {
        this.item = null;
        this.amt = 0;
    }
    public void SetItem(Item item)
    {
        this.item = item;
    }
    public void SetAmount(int amt)
    {
        this.amt = amt;
        if(this.amt == 0)
        {
            item = null;
        }
    }
    public void AddAmount(int amt)
    {
        this.amt += amt;
    }
    public void SubtractAmount(int amt)
    {
        this.amt -= amt;
        if(this.amt == 0)
        {
            item = null;
        }
    }
    public int GetAmount()
    {
        return this.amt;
    }
    public Item GetItem()
    {
        return this.item;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    ItemType type;
    int maxAmt;
    public Item(ItemType type, int maxAmt)
    {
        this.type = type;
        this.maxAmt = maxAmt;
    }
    public ItemType GetItemType()
    {
        return type;
    }
    public int GetMaxAmount()
    {
        return maxAmt;
    }
}
public enum ItemType
{
    ore,wood
}

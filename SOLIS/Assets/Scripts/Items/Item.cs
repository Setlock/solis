using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    string name;
    int maxAmt;
    public Item(string name, int maxAmt)
    {
        this.name = name;
        this.maxAmt = maxAmt;
    }
    public string GetName()
    {
        return name;
    }
    public int GetMaxAmount()
    {
        return maxAmt;
    }
}

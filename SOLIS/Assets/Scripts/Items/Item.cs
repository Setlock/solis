using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    string name;
    int maxStack;
    public Item(string name, int maxStack)
    {
        this.name = name;
        this.maxStack = maxStack;
    }
    public string GetName()
    {
        return name;
    }
    public int GetMaxStack()
    {
        return maxStack;
    }
}

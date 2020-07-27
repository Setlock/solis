
using UnityEngine;

public class Tile : MonoBehaviour
{
    public ItemDrop emptyDrop;
    public ItemDrop defaultDrop;
    public TileType type;

    public bool remove = false;

    public void StartTile(TileType type)
    {
        this.type = type;

        emptyDrop = new ItemDrop(null, 0);
        defaultDrop = emptyDrop;
    }
    public void StartTile(TileType type, Item item, int amount)
    {
        this.type = type;

        emptyDrop = new ItemDrop(null, 0);
        defaultDrop = new ItemDrop(item, amount);
    }
    public virtual ItemDrop GetDrop()
    {
        return defaultDrop;
    }
    public TileType GetTileType()
    {
        return type;
    }
}
public struct ItemDrop 
{
    Item item;
    int amount;
    public ItemDrop(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
    public Item GetItem()
    {
        return this.item;
    }
    public int GetAmount()
    {
        return this.amount;
    }
}

public enum TileType
{
    ore,
}

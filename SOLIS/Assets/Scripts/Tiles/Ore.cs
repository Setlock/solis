using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore : Tile
{
    int totalOre;
    public void Start()
    {
        StartTile(TileType.ore, ItemList.ore, 1);
        totalOre = (int)(UnityEngine.Random.value * 3) + 1;
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.Equals(GetComponent<BoxCollider2D>()))
            {
                Debug.Log("Dig Event: Removed " + RemoveOre(1) + " Ore - " + totalOre + " Remaining");
                if(totalOre <= 0)
                {
                    this.gameObject.SetActive(false);
                    remove = true;
                }
            }
        }
    }
    public int RemoveOre(int amount)
    {
        int removedAmount = amount;
        if(totalOre - amount >= 0)
        {
            totalOre -= amount;
        }
        else if(totalOre > 0)
        {
            removedAmount = totalOre;
            totalOre = 0;
        }
        else
        {
            removedAmount = 0;
        }
        return removedAmount;
    }
    public override ItemDrop GetDrop()
    {
        if (totalOre > 0)
        {
            totalOre--;
            return new ItemDrop(defaultDrop.GetItem(), 1);
        }
        return emptyDrop;
    }
}

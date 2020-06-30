using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    Inventory inventory;
    public int moveSpeed = 10;
    private void Start()
    {
        inventory = new Inventory(5);
        GetComponent<InventoryUIHandler>().SetReferenceInventory(inventory);
    }
    void FixedUpdate()
    {
        Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if(direction.x < 0)
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }
        if(direction.x > 0)
        {
            if (GetComponent<SpriteRenderer>().flipX)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
        }
        GetComponent<Rigidbody2D>().velocity = direction * moveSpeed * Time.deltaTime;

    }
}

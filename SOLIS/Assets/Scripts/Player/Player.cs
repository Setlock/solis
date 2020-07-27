using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject cloud;

    Inventory inventory;
    public int moveSpeed = 10;

    Vector2 cloudOffset;
    private void Start()
    {
        inventory = new Inventory(25);
        GetComponent<InventoryUIHandler>().SetReferenceInventory(inventory);
        cloudOffset = new Vector2(0, 0);
    }
    void FixedUpdate()
    {
        Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if(direction.x < 0)
        {
            if (GetComponent<SpriteRenderer>().flipX)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
        }
        if(direction.x > 0)
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }
        GetComponent<Rigidbody2D>().velocity = direction * moveSpeed * Time.deltaTime;

        Vector2 cloudVal = (direction * Time.deltaTime) / 8f;
        cloudOffset += new Vector2(cloudVal.x/cloud.GetComponent<MeshFilter>().mesh.bounds.size.x, cloudVal.y / cloud.GetComponent<MeshFilter>().mesh.bounds.size.y);
        cloud.GetComponent<MeshRenderer>().material.SetVector("_MovementOffset", cloudOffset);
    }
}

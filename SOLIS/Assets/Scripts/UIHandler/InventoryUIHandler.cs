using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUIHandler : MonoBehaviour
{
    public GameObject heldSlot;
    public List<GameObject> slotUI = new List<GameObject>();
    Inventory refInventory;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            for(int i = 0; i < slotUI.Count; i++)
            {
                if(hit.collider != null && hit.collider.Equals(slotUI[i].GetComponent<BoxCollider2D>()))
                {
                    refInventory.InventorySlotClick(refInventory.inventorySlots[i],0);
                    break;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {

        }
        for(int i = 0; i < slotUI.Count; i++)
        {
            if (refInventory.inventorySlots[i].GetItem() != null)
            {
                slotUI[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = refInventory.inventorySlots[i].GetItem().GetName() + " " + refInventory.inventorySlots[i].GetAmount();
            }
            else
            {
                slotUI[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
        if(refInventory.heldSlot.GetItem() != null)
        {
            heldSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = refInventory.heldSlot.GetItem().GetName() + " " + refInventory.heldSlot.GetAmount();
        }
        else
        {
            heldSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
        heldSlot.transform.position = Input.mousePosition;
    }
    public void SetReferenceInventory(Inventory inventory)
    {
        this.refInventory = inventory;
    }
}

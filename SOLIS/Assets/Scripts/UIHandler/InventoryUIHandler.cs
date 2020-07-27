using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIHandler : MonoBehaviour
{
    public GameObject canvas;
    public GameObject slotPrefab;
    public GameObject heldSlot;

    public List<GameObject> slotUI;
    Inventory refInventory;

    public Vector2 startPosition;
    public Vector2 dimensions;
    public Vector2 spacing;

    private void Start()
    {
        slotUI = new List<GameObject>();

        for(int j = 0; j < (int)dimensions.y; j++)
        {
            for(int i = 0; i < (int)dimensions.x; i++)
            {
                Vector3 position = new Vector3(startPosition.x + i * spacing.x, startPosition.y - j * spacing.y, 0);
                GameObject slotObject = Instantiate(slotPrefab, canvas.transform);
                slotObject.transform.position = position;
                slotObject.transform.SetAsFirstSibling();
                slotUI.Add(slotObject);
            }
        }

        heldSlot.transform.SetAsLastSibling();
    }
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
                slotUI[i].transform.GetChild(0).gameObject.SetActive(true);
                slotUI[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = refInventory.inventorySlots[i].GetItem().GetItemType().ToString() + " x"+refInventory.inventorySlots[i].GetAmount();
            }
            else
            {
                slotUI[i].transform.GetChild(0).gameObject.SetActive(false);
                slotUI[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
        if(refInventory.heldSlot.GetItem() != null)
        {
            heldSlot.transform.GetChild(0).gameObject.SetActive(true);
            heldSlot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ""+refInventory.heldSlot.GetAmount();
        }
        else
        {
            heldSlot.transform.GetChild(0).gameObject.SetActive(false);
            heldSlot.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
        }
        heldSlot.transform.position = Input.mousePosition;
    }
    public void SetReferenceInventory(Inventory inventory)
    {
        this.refInventory = inventory;
    }
}

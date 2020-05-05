using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class Entity : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetLayer()
    {
        if(gameObject.GetComponent<SpriteRenderer>() != null)
        {
            float bottomY = transform.position.y - GetComponent<SpriteRenderer>().bounds.size.y/2;
            float yVal = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x,bottomY)).y;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = (int)(-yVal);
        }
    }
    public void SetChildLayer()
    {
        int children = transform.childCount;
        for(int i = 0; i < children; i++)
        {
            Transform childTransform = transform.GetChild(i);
            if (childTransform.GetComponent<SpriteRenderer>() != null)
            {
                float bottomY = childTransform.position.y - childTransform.GetComponent<SpriteRenderer>().bounds.size.y / 2;
                float yVal = Camera.main.WorldToScreenPoint(new Vector3(childTransform.position.x, bottomY)).y;
                childTransform.GetComponent<SpriteRenderer>().sortingOrder = (int)(-yVal);
            }
        }
    }
}
